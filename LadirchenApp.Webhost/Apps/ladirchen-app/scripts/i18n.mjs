import { readFile } from "node:fs/promises";
import path from "node:path";
import process from "node:process";
import { fileURLToPath } from "node:url";

import { walkFiles } from "./file-system.mjs";

const projectRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const sourceRoot = path.join(projectRoot, "src");
const violations = [];

async function loadLocale(locale) {
  const relativeFilename = path.join("src", "locales", `${locale}.json`);
  const messages = JSON.parse(await readFile(path.join(projectRoot, relativeFilename), "utf8"));
  if (messages === null || typeof messages !== "object" || Array.isArray(messages)) {
    throw new Error(`${relativeFilename}: Locale must be a JSON object.`);
  }
  const keys = Object.keys(messages);
  const result = new Set();
  for (const key of keys) {
    if (typeof messages[key] === "string") {
      result.add(key);
    } else {
      violations.push(`${relativeFilename}: Locale must be flat with string values: ${key}`);
    }
  }
  const sortedKeys = [...keys].sort();
  const firstUnsorted = keys.findIndex((key, index) => key !== sortedKeys[index]);
  if (firstUnsorted >= 0) {
    violations.push(`${relativeFilename}: Keys must be sorted, first out of order: ${keys[firstUnsorted]}`);
  }
  const keySet = new Set(keys);
  for (const key of keys) {
    const segments = key.split(".");
    for (let length = 1; length < segments.length; length += 1) {
      const prefix = segments.slice(0, length).join(".");
      if (keySet.has(prefix)) {
        violations.push(`${relativeFilename}: Key ${prefix} conflicts with nested key ${key}`);
      }
    }
  }
  return result;
}

function stripVueTemplateMarkup(source) {
  let result = "";
  let quote = "";
  let inTag = false;
  let inInterpolation = false;
  for (let index = 0; index < source.length; index += 1) {
    const character = source[index];
    const pair = source.slice(index, index + 2);
    if (!inTag && !inInterpolation && pair === "{{") {
      inInterpolation = true;
      index += 1;
      result += "  ";
      continue;
    }
    if (inInterpolation && pair === "}}") {
      inInterpolation = false;
      index += 1;
      result += "  ";
      continue;
    }
    if (!inTag && !inInterpolation && character === "<") {
      inTag = true;
      result += " ";
      continue;
    }
    if (inTag) {
      if (quote) {
        if (character === quote) {
          quote = "";
        }
      } else if (character === '"' || character === "'") {
        quote = character;
      } else if (character === ">") {
        inTag = false;
      }
    }
    result += character === "\n" ? "\n" : " ";
  }
  return result;
}

const de = await loadLocale("de");
const en = await loadLocale("en");

for (const key of de.keys()) {
  if (!en.has(key)) {
    violations.push(`Missing English locale key: ${key}`);
  }
}
for (const key of en.keys()) {
  if (!de.has(key)) {
    violations.push(`Missing German locale key: ${key}`);
  }
}

const sourceFiles = (await walkFiles(sourceRoot)).filter(
  (file) => /\.(?:ts|vue)$/u.test(file) && !file.includes(`${path.sep}locales${path.sep}`),
);
const staticTranslationPattern = /\bt\(\s*(['"])([^'"]+)\1/gu;
const translatableAttributePattern = /(?:^|\s)(aria-label|alt|label|placeholder|title)\s*=\s*(['"])(.*?)\2/gu;

for (const file of sourceFiles) {
  const source = await readFile(file, "utf8");
  for (const match of source.matchAll(staticTranslationPattern)) {
    if (!de.has(match[2])) {
      const line = source.slice(0, match.index).split("\n").length;
      violations.push(`${path.relative(projectRoot, file)}:${line}: Unknown translation key ${match[2]}`);
    }
  }

  if (!file.endsWith(".vue")) {
    continue;
  }
  const templateStart = source.indexOf("<template");
  const scriptStart = source.indexOf("<script");
  if (templateStart < 0 || scriptStart < 0) {
    continue;
  }
  const templateContentStart = source.indexOf(">", templateStart) + 1;
  const templateSource = source.slice(templateContentStart, scriptStart);
  const templateStartLine = source.slice(0, templateContentStart).split("\n").length;
  for (const match of templateSource.matchAll(translatableAttributePattern)) {
    const text = match[3].trim();
    if (!text || !/[A-Za-zÄÖÜäöüß]/u.test(text)) {
      continue;
    }
    const line = templateStartLine + templateSource.slice(0, match.index).split("\n").length - 1;
    violations.push(`${path.relative(projectRoot, file)}:${line}: Untranslated ${match[1]} attribute: ${text}`);
  }
  const templateWithoutMarkup = stripVueTemplateMarkup(templateSource);
  templateWithoutMarkup.split("\n").forEach((rawText, index) => {
    const text = rawText.replace(/\s+/gu, " ").trim();
    if (!text || text === "L" || !/[A-Za-zÄÖÜäöüß]/u.test(text)) {
      return;
    }
    violations.push(
      `${path.relative(projectRoot, file)}:${templateStartLine + index}: Untranslated template text: ${text}`,
    );
  });
}

if (violations.length > 0) {
  console.error(`Internationalization checks failed:\n${violations.sort().join("\n")}`);
  process.exitCode = 1;
}
