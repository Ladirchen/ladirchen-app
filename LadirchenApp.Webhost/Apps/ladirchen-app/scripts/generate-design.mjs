import { access, readFile, writeFile } from "node:fs/promises";
import { constants } from "node:fs";
import { fileURLToPath } from "node:url";

const appRoot = fileURLToPath(new URL("..", import.meta.url));
const changes = new Map();
const createdFiles = new Map();

const usage = `Usage:
  pnpm generate:design avatar <category> <id> [options]
  pnpm generate:design furniture [indoor|outdoor] <id> [options]
  pnpm generate:design house-theme <id> [options]

Avatar categories:
  face, face-shape, hair, outfit, accessory, fun-accessory, seasonal-accessory

Examples:
  pnpm generate:design avatar hair side-braid --label-en="Side braid" --label-de="Seitenzopf"
  pnpm generate:design avatar outfit raincoat
  pnpm generate:design furniture indoor reading-chair --room=living-room --price=90 --icon=🪑
  pnpm generate:design furniture outdoor bird-bath --price=120 --icon=🐦
  pnpm generate:design house-theme ocean-home --kind=fantasy --price=350 --icon=🌊

Common options:
  --label-en=<text> --label-de=<text> --dry-run

New badge options (furniture and house themes):
  --new --new-from=YYYY-MM-DD --new-until=YYYY-MM-DD --new-days=<number>

Furniture options:
  --placement=inside|outside --room=<room-id> --category=garden|special
  --motion=none|wave|glow|flutter
  --price=<number> --level=0..4 --icon=<text> --x=<number> --y=<number> --scale=<number>
  --set=<furniture-set-id> --description-en=<text> --description-de=<text>

Avatar options:
  --profile=all|child|guardian
  --head-clearance=true|false (accessories only)

House-theme options:
  --kind=standard|seasonal|fantasy --price=<number> --icon=<text>
  --description-en=<text> --description-de=<text>`;

const parseArguments = (arguments_) => {
  const positionals = [];
  const options = {};
  for (const argument of arguments_) {
    if (!argument.startsWith("--")) {
      positionals.push(argument);
      continue;
    }
    const [name, ...valueParts] = argument.slice(2).split("=");
    options[name] = valueParts.length > 0 ? valueParts.join("=") : true;
  }
  return { options, positionals };
};

const { options, positionals } = parseArguments(process.argv.slice(2));
if (options.help || positionals.length === 0) {
  console.log(usage);
  process.exit(0);
}

const kebabIdPattern = /^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$/u;
const assertKebabId = (id) => {
  if (!kebabIdPattern.test(id)) {
    throw new Error(`Design ID "${id}" must use lowercase kebab-case.`);
  }
  if (id === "none") {
    throw new Error("\"none\" is reserved and cannot be generated.");
  }
};

const titleCase = (value) => value.split("-").map(part => `${part[0].toUpperCase()}${part.slice(1)}`).join(" ");
const pascalCase = (value) => value.split("-").map(part => `${part[0].toUpperCase()}${part.slice(1)}`).join("");
const camelCase = (value) => {
  const [first, ...rest] = value.split("-");
  return `${first}${rest.map(part => `${part[0].toUpperCase()}${part.slice(1)}`).join("")}`;
};
const quote = (value) => `'${String(value).replaceAll("\\", "\\\\").replaceAll("'", "\\'")}'`;
const numberOption = (name, fallback) => {
  const value = options[name] === undefined ? fallback : Number(options[name]);
  if (!Number.isFinite(value)) {throw new Error(`--${name} must be a finite number.`);}
  return value;
};
const enumOption = (name, allowed, fallback) => {
  const value = options[name] === undefined ? fallback : String(options[name]);
  if (!allowed.includes(value)) {throw new Error(`--${name} must be one of: ${allowed.join(", ")}.`);}
  return value;
};
const booleanOption = (name, fallback) => {
  if (options[name] === undefined) {return fallback;}
  if (options[name] === true || options[name] === "true") {return true;}
  if (options[name] === "false") {return false;}
  throw new Error(`--${name} must be true or false.`);
};
const calendarDateOption = (name) => {
  if (options[name] === undefined) {return undefined;}
  const value = String(options[name]);
  const match = /^(\d{4})-(\d{2})-(\d{2})$/u.exec(value);
  if (!match) {throw new Error(`--${name} must use YYYY-MM-DD.`);}
  const [, yearText, monthText, dayText] = match;
  const year = Number(yearText);
  const month = Number(monthText);
  const day = Number(dayText);
  const parsed = new Date(Date.UTC(year, month - 1, day));
  if (parsed.getUTCFullYear() !== year || parsed.getUTCMonth() !== month - 1 || parsed.getUTCDate() !== day) {
    throw new Error(`--${name} must be a valid calendar date.`);
  }
  return value;
};
const toCalendarDate = (date) => [
  date.getUTCFullYear(),
  String(date.getUTCMonth() + 1).padStart(2, "0"),
  String(date.getUTCDate()).padStart(2, "0"),
].join("-");
const todayCalendarDate = () => {
  const today = new Date();
  return [
    today.getFullYear(),
    String(today.getMonth() + 1).padStart(2, "0"),
    String(today.getDate()).padStart(2, "0"),
  ].join("-");
};
const addCalendarDays = (date, days) => {
  const result = new Date(`${date}T00:00:00Z`);
  result.setUTCDate(result.getUTCDate() + days);
  return toCalendarDate(result);
};
const generatedNewBadge = () => {
  const scheduleOptions = ["new-from", "new-until", "new-days"];
  const requested = options.new !== undefined || scheduleOptions.some(name => options[name] !== undefined);
  if (!requested) {return "";}
  const enabled = booleanOption("new", true);
  if (!enabled) {
    if (scheduleOptions.some(name => options[name] !== undefined)) {
      throw new Error("--new=false cannot be combined with new badge schedule options.");
    }
    return "";
  }
  const from = calendarDateOption("new-from") ?? todayCalendarDate();
  const explicitUntil = calendarDateOption("new-until");
  const explicitDuration = options["new-days"] !== undefined;
  if (explicitUntil && explicitDuration) {throw new Error("Use either --new-until or --new-days, not both.");}
  const duration = numberOption("new-days", 14);
  if (!Number.isInteger(duration) || duration < 1) {throw new Error("--new-days must be a positive integer.");}
  const until = explicitUntil ?? addCalendarDays(from, duration - 1);
  if (until < from) {throw new Error("--new-until cannot be earlier than --new-from.");}
  return `, newBadge: { from: ${quote(from)}, until: ${quote(until)} }`;
};

const absolutePath = relativePath => `${appRoot}/${relativePath}`;
const exists = async (relativePath) => {
  try {
    await access(absolutePath(relativePath), constants.F_OK);
    return true;
  } catch {
    return false;
  }
};
const source = async (relativePath) => changes.get(relativePath) ?? readFile(absolutePath(relativePath), "utf8");
const update = async (relativePath, transform) => {
  const current = await source(relativePath);
  const next = transform(current);
  if (next === current) {throw new Error(`No change was produced for ${relativePath}.`);}
  changes.set(relativePath, next);
};
const insertAtMarker = async (relativePath, markerName, content) => {
  const marker = `// design-generator:${markerName}`;
  await update(relativePath, (current) => {
    const lines = current.split("\n");
    const markerLineIndex = lines.findIndex(line => line.trim() === marker);
    if (markerLineIndex < 0) {throw new Error(`Marker ${marker} was not found in ${relativePath}.`);}
    const indentation = lines[markerLineIndex].slice(0, lines[markerLineIndex].indexOf(marker));
    const insertedLines = content.split("\n").map(line => `${indentation}${line}`);
    lines.splice(markerLineIndex, 0, ...insertedLines);
    return lines.join("\n");
  });
};
const create = async (relativePath, content) => {
  if (await exists(relativePath) || createdFiles.has(relativePath)) {
    throw new Error(`${relativePath} already exists.`);
  }
  createdFiles.set(relativePath, content);
};
const ensureIdMissing = async (relativePath, markerName, id, label) => {
  const current = await source(relativePath);
  const markerIndex = current.indexOf(`// design-generator:${markerName}`);
  if (markerIndex < 0) {throw new Error(`ID marker ${markerName} was not found in ${relativePath}.`);}
  const sectionStart = current.lastIndexOf("[", markerIndex);
  if (sectionStart < 0) {throw new Error(`Could not find the ID list before marker ${markerName}.`);}
  if (current.slice(sectionStart, markerIndex).includes(`'${id}'`)) {throw new Error(`${label} already exists.`);}
};

const avatarCategories = {
  accessory: { catalogFile: "src/domain/avatar/accessory-catalog.ts", idMarker: "avatar-accessory-id", localeMarker: "avatar-accessory-locale", registryMarker: "avatar-accessory-visual" },
  face: { catalogFile: "src/domain/avatar/face-catalog.ts", idMarker: "avatar-face-id", localeMarker: "avatar-face-locale", registryMarker: "avatar-face-visual" },
  "face-shape": { catalogFile: "src/domain/avatar/face-catalog.ts", idMarker: "avatar-face-shape-id", localeMarker: "avatar-face-shape-locale", registryMarker: "avatar-face-shape-visual" },
  "fun-accessory": { catalogFile: "src/domain/avatar/accessory-catalog.ts", idMarker: "avatar-fun-accessory-id", localeMarker: "avatar-fun-accessory-locale", registryMarker: "avatar-fun-accessory-visual" },
  hair: { catalogFile: "src/domain/avatar/hair-catalog.ts", idMarker: "avatar-hair-id", localeMarker: "avatar-hair-locale", registryMarker: "avatar-hair-visual" },
  outfit: { catalogFile: "src/domain/avatar/outfit-catalog.ts", idMarker: "avatar-outfit-id", localeMarker: "avatar-outfit-locale", registryMarker: "avatar-outfit-visual" },
  "seasonal-accessory": { catalogFile: "src/domain/avatar/accessory-catalog.ts", idMarker: "avatar-seasonal-accessory-id", localeMarker: "avatar-seasonal-accessory-locale", registryMarker: "avatar-seasonal-accessory-visual" },
};

const avatarVisualTemplate = (category, id) => {
  const className = `generated-${category}-${id}`;
  if (category === "face-shape") {
    return `<template>\n  <path class="${className}" d="M80 20c27 0 43 18 43 49 0 29-18 51-43 56-25-5-43-27-43-56 0-31 16-49 43-49Z" />\n</template>\n\n<style scoped>\n.${className} { fill: var(--skin); stroke: var(--lad-palette-orange-600); stroke-width: 2.5; }\n</style>\n`;
  }
  if (category === "face") {
    return `<template>\n  <g class="${className}">\n    <ellipse cx="60" cy="70" rx="6.5" ry="8.5" /><ellipse cx="100" cy="70" rx="6.5" ry="8.5" />\n    <path d="M69 92q11 10 22 0" />\n  </g>\n</template>\n\n<style scoped>\n.${className} ellipse { fill: var(--lad-palette-text); }\n.${className} path { fill: none; stroke: var(--lad-palette-orange-650); stroke-linecap: round; stroke-width: 3; }\n</style>\n`;
  }
  const layers = category === "outfit" ? "'back' | 'head' | 'front'" : category === "hair" || category === "fun-accessory" ? "'back' | 'front'" : undefined;
  if (layers) {
    return `<template>\n  <g v-if="layer === 'front'" class="${className}">\n    <path d="M45 130q35-20 70 0l4 50H41Z" />\n  </g>\n</template>\n\n<script lang="ts" setup>\ndefineProps<{ layer: ${layers} }>();\n</script>\n\n<style scoped>\n.${className} { fill: var(--${category === "hair" ? "hair" : category === "outfit" ? "outfit" : "lad-palette-yellow"}); stroke: var(--lad-palette-muted-700); stroke-linejoin: round; stroke-width: 3; }\n</style>\n`;
  }
  return `<template>\n  <g class="${className}">\n    <circle cx="80" cy="32" r="18" />\n  </g>\n</template>\n\n<style scoped>\n.${className} { fill: var(--lad-palette-yellow); stroke: var(--lad-palette-amber-650); stroke-width: 3; }\n</style>\n`;
};

const generateAvatar = async () => {
  const [category, id] = positionals.slice(1);
  const configuration = avatarCategories[category];
  if (!configuration || !id) {throw new Error(`Avatar generation requires a category and ID.\n\n${usage}`);}
  assertKebabId(id);
  const labelEn = String(options["label-en"] ?? titleCase(id));
  const labelDe = String(options["label-de"] ?? labelEn);
  const componentName = `${pascalCase(category)}${pascalCase(id)}Visual`;
  const componentFile = `src/features/avatar/components/avatar-visuals/${componentName}.vue`;
  const registryFile = "src/features/avatar/components/avatar-visuals/index.ts";
  const profile = enumOption("profile", ["all", "child", "guardian"], "all");
  const profileArgument = profile === "all" ? "" : `, [${quote(profile)}]`;
  await ensureIdMissing(configuration.catalogFile, configuration.idMarker, id, `Avatar ${category} ID "${id}"`);
  await create(componentFile, avatarVisualTemplate(category, id));
  await insertAtMarker(configuration.catalogFile, configuration.idMarker, `defineAvatarPart(${quote(id)}${profileArgument}),`);
  await insertAtMarker(registryFile, "avatar-visual-import", `import ${componentName} from './${componentName}.vue';`);
  const registryEntry = category === "accessory"
    ? `${quote(id)}: { component: ${componentName}, requiresHeadClearance: ${booleanOption("head-clearance", true)} },`
    : `${quote(id)}: ${componentName},`;
  await insertAtMarker(registryFile, configuration.registryMarker, registryEntry);
  await insertAtMarker("src/locales/en.ts", configuration.localeMarker, `${quote(id)}: ${quote(labelEn)},`);
  await insertAtMarker("src/locales/de.ts", configuration.localeMarker, `${quote(id)}: ${quote(labelDe)},`);
};

const furnitureMotionStyles = (id, motion) => {
  if (motion === "none") {return "";}
  const className = `generated-${id}`;
  const animations = {
    flutter: `@keyframes ${className}-flutter { from { transform: translateY(0) scaleY(.82); } to { transform: translateY(-4px) scaleY(1.12); } }`,
    glow: `@keyframes ${className}-glow { from { filter: drop-shadow(0 0 0 transparent); opacity: .82; } to { filter: drop-shadow(0 0 9px var(--lad-palette-yellow)); opacity: 1; } }`,
    wave: `@keyframes ${className}-wave { from { transform: skewY(-2deg) scaleX(.97); } to { transform: skewY(4deg) scaleX(1.03); } }`,
  };
  return `\n.${className} { animation: ${className}-${motion} 1.8s ease-in-out infinite alternate; transform-box: fill-box; transform-origin: center; }\n${animations[motion]}\n@media (prefers-reduced-motion: reduce) { .${className} { animation: none; } }`;
};

const furnitureVisualTemplate = (id, motion) => `<template>\n  <g class="generated-${id}">\n    <rect x="-36" y="-26" width="72" height="52" rx="12" />\n    <path d="M-28 26v12m56-12v12" />\n  </g>\n</template>\n\n<style scoped>\n.generated-${id} rect { fill: var(--lad-palette-blue-250); stroke: var(--lad-palette-blue-600); stroke-width: 3; }\n.generated-${id} path { fill: none; stroke: var(--lad-palette-muted-700); stroke-linecap: round; stroke-width: 5; }${furnitureMotionStyles(id, motion)}\n</style>\n`;

const generateFurniture = async () => {
  const requestedArea = ["indoor", "outdoor"].includes(positionals[1]) ? positionals[1] : undefined;
  const id = requestedArea ? positionals[2] : positionals[1];
  if (!id) {throw new Error(`Furniture generation requires an ID.\n\n${usage}`);}
  assertKebabId(id);
  const areaPlacement = requestedArea === "outdoor" ? "outside" : "inside";
  const placement = requestedArea
    ? enumOption("placement", [areaPlacement], areaPlacement)
    : enumOption("placement", ["inside", "outside"], "inside");
  const room = enumOption("room", ["living-room", "kitchen", "children-room", "bedroom", "creative-room"], "living-room");
  const category = enumOption("category", ["garden", "special"], "garden");
  const motion = enumOption("motion", ["none", "wave", "glow", "flutter"], "none");
  const level = numberOption("level", 0);
  if (!Number.isInteger(level) || level < 0 || level > 4) {throw new Error("--level must be an integer from 0 to 4.");}
  const price = numberOption("price", 100);
  const x = numberOption("x", 50);
  const y = numberOption("y", placement === "inside" ? 66 : 68);
  const scale = numberOption("scale", .8);
  const icon = String(options.icon ?? "🪑");
  const labelEn = String(options["label-en"] ?? titleCase(id));
  const labelDe = String(options["label-de"] ?? labelEn);
  const descriptionEn = String(options["description-en"] ?? `${labelEn} for the family home.`);
  const descriptionDe = String(options["description-de"] ?? `${labelDe} für das Familienhaus.`);
  const setId = options.set === undefined ? undefined : String(options.set);
  if (setId && !kebabIdPattern.test(setId)) {throw new Error("--set must use lowercase kebab-case.");}
  const newBadge = generatedNewBadge();
  const componentName = `${pascalCase(id)}Visual`;
  const componentFile = `src/features/world/components/furniture-visuals/${componentName}.vue`;
  const registryFile = "src/features/world/components/furniture-visuals/index.ts";
  await ensureIdMissing("src/domain/house/types.ts", "house-accessory-id", id, `House accessory ID "${id}"`);
  await ensureIdMissing("src/domain/house/types.ts", "furniture-visual-id", id, `Furniture visual ID "${id}"`);
  await create(componentFile, furnitureVisualTemplate(id, motion));
  await insertAtMarker("src/domain/house/types.ts", "house-accessory-id", `'${id}',`);
  await insertAtMarker("src/domain/house/types.ts", "furniture-visual-id", `'${id}',`);
  await insertAtMarker(registryFile, "furniture-visual-import", `import ${componentName} from './${componentName}.vue';`);
  await insertAtMarker(registryFile, "furniture-visual", `${quote(id)}: ${componentName},`);
  const commonFields = `id: ${quote(id)}, icon: ${quote(icon)}, price: ${price}${setId ? `, setIds: [${quote(setId)}]` : ""}${level ? `, minimumHouseLevel: ${level}` : ""}${motion !== "none" ? `, motion: ${quote(motion)}` : ""}${newBadge}, visual: ${quote(id)}, defaultPlacement: { x: ${x}, y: ${y}, scale: ${scale} }`;
  const definition = placement === "inside"
    ? `defineIndoorFurniture({ ${commonFields}, roomId: ${quote(room)} }),`
    : `defineOutdoorAccessory({ ${commonFields}${category === "special" ? ", category: 'special'" : ""} }),`;
  await insertAtMarker("src/domain/house/catalog.ts", "house-accessory-definition", definition);
  await insertAtMarker("src/locales/en.ts", "house-accessory-locale", `${quote(id)}: { title: ${quote(labelEn)}, description: ${quote(descriptionEn)} },`);
  await insertAtMarker("src/locales/de.ts", "house-accessory-locale", `${quote(id)}: { title: ${quote(labelDe)}, description: ${quote(descriptionDe)} },`);
};

const themeVisualTemplate = id => `<template>\n  <g class="generated-${id}" aria-hidden="true">\n    <path d="m178 120 4 10 10 4-10 4-4 10-4-10-10-4 10-4Z" />\n    <path d="m294 132 3 8 8 3-8 3-3 8-3-8-8-3 8-3Z" />\n  </g>\n</template>\n\n<style scoped>\n.generated-${id} { fill: var(--house-trim); }\n</style>\n`;

const generateHouseTheme = async () => {
  const id = positionals[1];
  if (!id) {throw new Error(`House-theme generation requires an ID.\n\n${usage}`);}
  assertKebabId(id);
  const kind = enumOption("kind", ["standard", "seasonal", "fantasy"], "standard");
  const price = numberOption("price", 300);
  const icon = String(options.icon ?? "🏠");
  const labelEn = String(options["label-en"] ?? titleCase(id));
  const labelDe = String(options["label-de"] ?? labelEn);
  const descriptionEn = String(options["description-en"] ?? `${labelEn} house design.`);
  const descriptionDe = String(options["description-de"] ?? `${labelDe} als Hausdesign.`);
  const newBadge = generatedNewBadge();
  const paletteName = camelCase(id);
  const componentName = `${pascalCase(id)}ThemeVisual`;
  const componentFile = `src/features/world/components/house-theme-visuals/${componentName}.vue`;
  const registryFile = "src/features/world/components/house-theme-visuals/index.ts";
  await ensureIdMissing("src/domain/house/types.ts", "house-theme-id", id, `House theme ID "${id}"`);
  await create(componentFile, themeVisualTemplate(id));
  await insertAtMarker("src/domain/house/types.ts", "house-theme-id", `'${id}',`);
  await insertAtMarker("src/theme/color-palette.ts", "house-theme-palette", `${paletteName}: defineHexPalette({\n  door: '#75a982',\n  floor: '#f8d89b',\n  landscapeAccent: '#62bd77',\n  roof: '#ec6e66',\n  roofShade: '#c64f56',\n  trim: '#a96855',\n  wall: '#fff8df',\n  wallUpper: '#f2dcf2',\n  window: '#8ed5e6',\n}),`);
  await insertAtMarker("src/domain/house/catalog.ts", "house-theme-definition", `defineHouseTheme({ id: ${quote(id)}, icon: ${quote(icon)}, price: ${price}, kind: ${quote(kind)}${newBadge}, ...houseThemeColorPalette.${paletteName} }),`);
  await insertAtMarker(registryFile, "house-theme-visual-import", `import ${componentName} from './${componentName}.vue';`);
  await insertAtMarker(registryFile, "house-theme-visual", `${quote(id)}: ${componentName},`);
  await insertAtMarker("src/locales/en.ts", "house-theme-locale", `${quote(id)}: { name: ${quote(labelEn)}, description: ${quote(descriptionEn)} },`);
  await insertAtMarker("src/locales/de.ts", "house-theme-locale", `${quote(id)}: { name: ${quote(labelDe)}, description: ${quote(descriptionDe)} },`);
};

const generators = {
  avatar: generateAvatar,
  furniture: generateFurniture,
  "house-theme": generateHouseTheme,
};

const kind = positionals[0];
const generate = generators[kind];
if (!generate) {throw new Error(`Unknown design kind "${kind}".\n\n${usage}`);}

await generate();

const changedPaths = [...changes.keys(), ...createdFiles.keys()].sort();
if (options["dry-run"]) {
  console.log(`Dry run: ${changedPaths.length} files would change:\n${changedPaths.map(path => `  ${path}`).join("\n")}`);
  process.exit(0);
}

for (const [relativePath, content] of changes) {
  await writeFile(absolutePath(relativePath), content);
}
for (const [relativePath, content] of createdFiles) {
  await writeFile(absolutePath(relativePath), content);
}

console.log(`Generated ${kind} design. Updated ${changedPaths.length} files:\n${changedPaths.map(path => `  ${path}`).join("\n")}\n\nEdit the generated Vue visual, then run pnpm lint and pnpm type-check.`);
