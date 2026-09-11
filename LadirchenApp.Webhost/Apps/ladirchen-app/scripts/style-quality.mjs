import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

const projectRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const sourceRoot = path.join(projectRoot, 'src');
const checkedExtensions = new Set(['.css', '.scss', '.vue']);
const maximumStyleLineLength = 160;
const embeddedStylePattern = /<style\b[^>]*>([\s\S]*?)<\/style>/gu;
const importantPattern = /!important/gu;
const allowedAccessibilityDeclarations = new Set([
  'animation: none !important',
  'animation-duration: 0.01ms !important',
  'animation-iteration-count: 1 !important',
  'scroll-behavior: auto !important',
  'transition-duration: 0.01ms !important',
]);

const walk = async directory => (await Promise.all((await readdir(directory, { withFileTypes: true })).map(entry => {
  const entryPath = path.join(directory, entry.name);
  return entry.isDirectory() ? walk(entryPath) : [entryPath];
}))).flat();

const findReducedMotionRanges = (contents) => {
  const ranges = [];
  const pattern = /@include\s+reduced-motion\s*\{/gu;
  for (const match of contents.matchAll(pattern)) {
    const start = match.index;
    let depth = 1;
    let cursor = start + match[0].length;
    while (cursor < contents.length && depth > 0) {
      if (contents[cursor] === '{') { depth += 1; }
      if (contents[cursor] === '}') { depth -= 1; }
      cursor += 1;
    }
    if (depth === 0) { ranges.push([start, cursor]); }
  }
  return ranges;
};

const files = (await walk(sourceRoot)).filter(file => checkedExtensions.has(path.extname(file)));
const violations = [];

for (const file of files) {
  const contents = await readFile(file, 'utf8');
  const styleSections = path.extname(file) === '.vue'
    ? [...contents.matchAll(embeddedStylePattern)].map(match => ({
      contents: match[1],
      startLine: contents.slice(0, match.index + match[0].indexOf(match[1])).split('\n').length - 1,
    }))
    : [{ contents, startLine: 0 }];
  for (const section of styleSections) {
    section.contents.split('\n').forEach((lineContents, lineIndex) => {
      if (lineContents.length <= maximumStyleLineLength) { return; }
      violations.push(`${path.relative(projectRoot, file)}:${section.startLine + lineIndex + 1} (style line exceeds ${maximumStyleLineLength} characters)`);
    });
  }
  const reducedMotionRanges = findReducedMotionRanges(contents);
  for (const match of contents.matchAll(importantPattern)) {
    const declarationStart = Math.max(
      contents.lastIndexOf(';', match.index),
      contents.lastIndexOf('{', match.index),
      contents.lastIndexOf('}', match.index),
      contents.lastIndexOf('\n', match.index),
    ) + 1;
    const declarationSource = contents.slice(declarationStart, match.index + match[0].length).trim();
    const colonIndex = declarationSource.indexOf(':');
    const declaration = colonIndex < 0
      ? declarationSource
      : `${declarationSource.slice(0, colonIndex).trim()}: ${declarationSource.slice(colonIndex + 1).trim()}`;
    const isAccessibilityOverride = allowedAccessibilityDeclarations.has(declaration)
      && reducedMotionRanges.some(([start, end]) => match.index >= start && match.index < end);
    if (isAccessibilityOverride) { continue; }

    const line = contents.slice(0, match.index).split('\n').length;
    violations.push(`${path.relative(projectRoot, file)}:${line} (${declaration})`);
  }
}

if (violations.length > 0) {
  console.error(`Style quality violations:\n${violations.join('\n')}`);
  process.exitCode = 1;
}
