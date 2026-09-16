import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

import ts from 'typescript';

const projectRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const sourceRoot = path.join(projectRoot, 'src');

const loadLocale = async (locale) => {
  const filename = path.join(sourceRoot, 'locales', `${locale}.ts`);
  const sources = new Map();
  const loadSources = async directory => Promise.all((await readdir(directory, { withFileTypes: true })).map(async entry => {
    const entryPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {await loadSources(entryPath);}
    else if (entry.name.endsWith('.ts')) {sources.set(entryPath, await readFile(entryPath, 'utf8'));}
  }));
  await loadSources(path.join(sourceRoot, 'locales'));
  const modules = new Map();
  const execute = moduleFilename => {
    const normalizedFilename = moduleFilename.endsWith('.ts') ? moduleFilename : `${moduleFilename}.ts`;
    if (modules.has(normalizedFilename)) {return modules.get(normalizedFilename).exports;}
    const source = sources.get(normalizedFilename);
    if (source === undefined) {throw new Error(`Locale module not found: ${normalizedFilename}`);}
    const javascript = ts.transpileModule(source, {
      compilerOptions: { esModuleInterop: true, module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
    }).outputText;
    const module = { exports: {} };
    modules.set(normalizedFilename, module);
    const requireLocaleModule = request => execute(path.resolve(path.dirname(normalizedFilename), request));
    Function('exports', 'module', 'require', javascript)(module.exports, module, requireLocaleModule);
    return module.exports;
  };
  return execute(filename).default;
};

const flattenMessages = (value, prefix = '', result = new Map()) => {
  for (const [name, child] of Object.entries(value)) {
    const key = prefix ? `${prefix}.${name}` : name;
    if (child !== null && typeof child === 'object') {
      flattenMessages(child, key, result);
    } else {
      result.set(key, typeof child);
    }
  }
  return result;
};

const walk = async directory => (await Promise.all((await readdir(directory, { withFileTypes: true })).map(entry => {
  const entryPath = path.join(directory, entry.name);
  return entry.isDirectory() ? walk(entryPath) : [entryPath];
}))).flat();

const stripVueTemplateMarkup = (source) => {
  let result = '';
  let quote = '';
  let inTag = false;
  let inInterpolation = false;
  for (let index = 0; index < source.length; index += 1) {
    const character = source[index];
    const pair = source.slice(index, index + 2);
    if (!inTag && !inInterpolation && pair === '{{') {
      inInterpolation = true;
      index += 1;
      result += '  ';
      continue;
    }
    if (inInterpolation && pair === '}}') {
      inInterpolation = false;
      index += 1;
      result += '  ';
      continue;
    }
    if (!inTag && !inInterpolation && character === '<') {
      inTag = true;
      result += ' ';
      continue;
    }
    if (inTag) {
      if (quote) {
        if (character === quote) {quote = '';}
      } else if (character === '"' || character === "'") {
        quote = character;
      } else if (character === '>') {
        inTag = false;
      }
    }
    result += character === '\n' ? '\n' : ' ';
  }
  return result;
};

const de = flattenMessages(await loadLocale('de'));
const en = flattenMessages(await loadLocale('en'));
const violations = [];

for (const key of de.keys()) {
  if (!en.has(key)) {
    violations.push(`Missing English locale key: ${key}`);
  } else if (en.get(key) !== de.get(key)) {
    violations.push(`Locale value type differs: ${key}`);
  }
}
for (const key of en.keys()) {
  if (!de.has(key)) {violations.push(`Missing German locale key: ${key}`);}
}

const sourceFiles = (await walk(sourceRoot)).filter(file => /\.(?:ts|vue)$/u.test(file) && !file.includes(`${path.sep}locales${path.sep}`));
const staticTranslationPattern = /\bt\(\s*(['"])([^'"]+)\1/gu;
const translatableAttributePattern = /(?:^|\s)(aria-label|alt|label|placeholder|title)\s*=\s*(['"])(.*?)\2/gu;

for (const file of sourceFiles) {
  const source = await readFile(file, 'utf8');
  for (const match of source.matchAll(staticTranslationPattern)) {
    if (!de.has(match[2])) {
      const line = source.slice(0, match.index).split('\n').length;
      violations.push(`${path.relative(projectRoot, file)}:${line}: Unknown translation key ${match[2]}`);
    }
  }

  if (!file.endsWith('.vue')) {continue;}
  const templateStart = source.indexOf('<template');
  const scriptStart = source.indexOf('<script');
  if (templateStart < 0 || scriptStart < 0) {continue;}
  const templateContentStart = source.indexOf('>', templateStart) + 1;
  const templateSource = source.slice(templateContentStart, scriptStart);
  const templateStartLine = source.slice(0, templateContentStart).split('\n').length;
  for (const match of templateSource.matchAll(translatableAttributePattern)) {
    const text = match[3].trim();
    if (!text || !/[A-Za-zÄÖÜäöüß]/u.test(text)) {continue;}
    const line = templateStartLine + templateSource.slice(0, match.index).split('\n').length - 1;
    violations.push(`${path.relative(projectRoot, file)}:${line}: Untranslated ${match[1]} attribute: ${text}`);
  }
  const templateWithoutMarkup = stripVueTemplateMarkup(templateSource);
  templateWithoutMarkup.split('\n').forEach((rawText, index) => {
    const text = rawText.replace(/\s+/gu, ' ').trim();
    if (!text || text === 'L' || !/[A-Za-zÄÖÜäöüß]/u.test(text)) {return;}
    violations.push(`${path.relative(projectRoot, file)}:${templateStartLine + index}: Untranslated template text: ${text}`);
  });
}

if (violations.length > 0) {
  console.error(`Internationalization checks failed:\n${violations.sort().join('\n')}`);
  process.exitCode = 1;
}
