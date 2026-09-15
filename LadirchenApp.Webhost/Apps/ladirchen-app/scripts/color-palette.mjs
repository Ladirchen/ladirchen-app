import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

const projectRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const sourceRoot = path.join(projectRoot, 'src');
const allowedPaletteFiles = new Set([
  path.join(sourceRoot, 'theme', 'color-palette.ts'),
]);
const semanticTokenFile = path.join(sourceRoot, 'styles', 'main.scss');
const illustrationDirectories = [
  path.join(sourceRoot, 'shared', 'components', 'avatar', 'avatar-visuals'),
  path.join(sourceRoot, 'features', 'world', 'components', 'furniture-visuals'),
  path.join(sourceRoot, 'features', 'world', 'components', 'house-theme-visuals'),
];
const illustrationFiles = new Set([
  'src/app/components/AppNavigationIcon.vue',
  'src/app/components/GlobalLadiGuide.vue',
  'src/features/onboarding/components/LadirchenIntro.vue',
  'src/features/savings/components/AnimatedExchangeIcon.vue',
  'src/features/savings/components/AnimatedWishIcon.vue',
  'src/features/streaks/components/AnimatedStreakFlame.vue',
  'src/features/world/components/AnimatedEnergyStar.vue',
  'src/features/world/components/AnimatedHouseEnergy.vue',
  'src/features/world/components/DollhouseInterior.vue',
  'src/features/world/components/FamilyWorldScene.vue',
  'src/features/world/components/FurnitureStoragePanel.vue',
  'src/features/world/components/HouseFurniture.vue',
  'src/features/world/components/HouseLayoutEntity.vue',
  'src/features/world/components/HouseThemeDecoration.vue',
  'src/features/world/components/PixiRoomScene.vue',
  'src/features/world/composables/use-family-world-scene.ts',
  'src/shared/components/AnimatedCompletionMark.vue',
  'src/shared/components/AnimatedPiggyBank.vue',
  'src/shared/components/LadiMascot.vue',
  'src/shared/components/LadirchenCoin.vue',
  'src/shared/components/avatar/AvatarCategoryIcon.vue',
  'src/shared/components/avatar/AvatarFigure.vue',
  'src/shared/components/contributions/ContributionMetaIcon.vue',
  'src/shared/components/family/AnimatedPet.vue',
  'src/shared/components/house/RoomFurniture.vue',
  'src/shared/components/ui/AnimatedSectionIcon.vue',
].map(file => path.join(projectRoot, file)));
const checkedExtensions = new Set(['.css', '.scss', '.ts', '.vue']);
const styleColorPattern = /#[\da-f]{3,4}(?:[\da-f]{2}){0,2}(?![\da-f])|rgba?\(\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?(?:\s*,\s*(?:0|1|0?\.\d+|\d+(?:\.\d+)?%))?\s*\)|(?<![-\w])(?:black|white)(?![-\w])/iu;
const typescriptColorPattern = /(['"])#[\da-f]{3,4}(?:[\da-f]{2}){0,2}\1|rgba?\(\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?\s*,\s*\d+(?:\.\d+)?%?(?:\s*,\s*(?:0|1|0?\.\d+|\d+(?:\.\d+)?%))?\s*\)/iu;
const directPaletteReferencePattern = /var\(--lad-palette-[^)]+\)/u;

const walk = async directory => (await Promise.all((await readdir(directory, { withFileTypes: true })).map(entry => {
  const entryPath = path.join(directory, entry.name);
  return entry.isDirectory() ? walk(entryPath) : [entryPath];
}))).flat();

const files = (await walk(sourceRoot))
  .filter(file => checkedExtensions.has(path.extname(file)))
  .filter(file => !allowedPaletteFiles.has(file));
const violations = [];
const semanticTokenViolations = [];

const mayUseDirectPalette = file => file === semanticTokenFile
  || illustrationFiles.has(file)
  || illustrationDirectories.some(directory => file.startsWith(`${directory}${path.sep}`));

for (const file of files) {
  const contents = await readFile(file, 'utf8');
  const pattern = path.extname(file) === '.ts' ? typescriptColorPattern : styleColorPattern;
  if (pattern.test(contents)) {violations.push(path.relative(projectRoot, file));}
  if (!mayUseDirectPalette(file) && directPaletteReferencePattern.test(contents)) {
    semanticTokenViolations.push(path.relative(projectRoot, file));
  }
}

if (violations.length > 0) {
  console.error(`Direct color literals found outside the central palettes:\n${violations.sort().join('\n')}`);
  process.exitCode = 1;
}

if (semanticTokenViolations.length > 0) {
  console.error(`Direct palette references found outside semantic tokens or illustration renderers:\n${semanticTokenViolations.sort().join('\n')}`);
  process.exitCode = 1;
}
