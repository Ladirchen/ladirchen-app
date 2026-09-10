import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

const projectRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const sourceRoot = path.join(projectRoot, 'src');
const allowedPaletteFiles = new Set([
  path.join(sourceRoot, 'theme', 'color-palette.ts'),
]);
const checkedExtensions = new Set(['.css', '.scss', '.ts', '.vue']);
const styleColorPattern = /#[\da-f]{3,4}(?:[\da-f]{2}){0,2}(?![\da-f])|rgba?\(\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?(?:\s*,\s*(?:0|1|0?\.\d+|\d+(?:\.\d+)?%))?\s*\)/iu;
const typescriptColorPattern = /(['"])#[\da-f]{3,4}(?:[\da-f]{2}){0,2}\1|rgba?\(\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?(?:\s*,\s*(?:0|1|0?\.\d+|\d+(?:\.\d+)?%))?\s*\)/iu;

const walk = async directory => (await Promise.all((await readdir(directory, { withFileTypes: true })).map(entry => {
  const entryPath = path.join(directory, entry.name);
  return entry.isDirectory() ? walk(entryPath) : [entryPath];
}))).flat();

const files = (await walk(sourceRoot))
  .filter(file => checkedExtensions.has(path.extname(file)))
  .filter(file => !allowedPaletteFiles.has(file));
const violations = [];

for (const file of files) {
  const contents = await readFile(file, 'utf8');
  const pattern = path.extname(file) === '.ts' ? typescriptColorPattern : styleColorPattern;
  if (pattern.test(contents)) {violations.push(path.relative(projectRoot, file));}
}

if (violations.length > 0) {
  console.error(`Direct color literals found outside the central palettes:\n${violations.sort().join('\n')}`);
  process.exitCode = 1;
}
