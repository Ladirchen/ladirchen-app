import type { FurnitureSetDefinition, HouseAccessoryCategory, HouseRoomDefinition, HouseStageDefinition, HouseThemeDefinition } from './house';
import type { HouseAccessory } from './types';
import { houseRoomColorPalette, houseThemeColorPalette } from '@/theme/color-palette';

export const HOUSE_STAGES: HouseStageDefinition[] = [
  { id: 'starter-home', level: 0, nameKey: 'catalog.stages.starter-home', icon: '🏠', rooms: 2 },
  { id: 'family-home', level: 1, nameKey: 'catalog.stages.family-home', icon: '🏡', rooms: 3 },
  { id: 'garden-home', level: 2, nameKey: 'catalog.stages.garden-home', icon: '🏘️', rooms: 4 },
  { id: 'tower-home', level: 3, nameKey: 'catalog.stages.tower-home', icon: '🏰', rooms: 5 },
  { id: 'dream-home', level: 4, nameKey: 'catalog.stages.dream-home', icon: '✨🏡', rooms: 5 },
];

export const DEFAULT_HOUSE_STAGE = HOUSE_STAGES[0]!;

export const HOUSE_ROOMS: ReadonlyArray<HouseRoomDefinition> = [
  { id: 'living-room', nameKey: 'catalog.rooms.living-room', icon: '🛋️', minimumHouseLevel: 0, ...houseRoomColorPalette.livingRoom },
  { id: 'kitchen', nameKey: 'catalog.rooms.kitchen', icon: '🍳', minimumHouseLevel: 0, ...houseRoomColorPalette.kitchen },
  { id: 'children-room', nameKey: 'catalog.rooms.children-room', icon: '🧸', minimumHouseLevel: 2, ...houseRoomColorPalette.childrenRoom },
  { id: 'bedroom', nameKey: 'catalog.rooms.bedroom', icon: '🛏️', minimumHouseLevel: 3, ...houseRoomColorPalette.bedroom },
  { id: 'creative-room', nameKey: 'catalog.rooms.creative-room', icon: '🎨', minimumHouseLevel: 4, ...houseRoomColorPalette.creativeRoom },
];

export const FURNITURE_SETS: ReadonlyArray<FurnitureSetDefinition> = [
  { id: 'cozy-living-set', nameKey: 'catalog.sets.cozy-living-set.name', descriptionKey: 'catalog.sets.cozy-living-set.description', icon: '🛋️', price: 250, zoneId: 'living-room', minimumHouseLevel: 0, accessoryIds: ['cozy-sofa', 'round-rug', 'floor-lamp'] },
  { id: 'family-kitchen-set', nameKey: 'catalog.sets.family-kitchen-set.name', descriptionKey: 'catalog.sets.family-kitchen-set.description', icon: '🍳', price: 420, zoneId: 'kitchen', minimumHouseLevel: 0, accessoryIds: ['kitchen-counter', 'retro-fridge', 'family-dining-table'] },
  { id: 'adventure-kids-set', nameKey: 'catalog.sets.adventure-kids-set.name', descriptionKey: 'catalog.sets.adventure-kids-set.description', icon: '🧸', price: 310, zoneId: 'children-room', minimumHouseLevel: 2, accessoryIds: ['bunk-bed', 'play-table', 'bookshelf'] },
  { id: 'calm-bedroom-set', nameKey: 'catalog.sets.calm-bedroom-set.name', descriptionKey: 'catalog.sets.calm-bedroom-set.description', icon: '🛏️', price: 390, zoneId: 'bedroom', minimumHouseLevel: 3, accessoryIds: ['double-bed', 'wardrobe', 'bedside-table'] },
  { id: 'creative-studio-set', nameKey: 'catalog.sets.creative-studio-set.name', descriptionKey: 'catalog.sets.creative-studio-set.description', icon: '🎨', price: 280, zoneId: 'creative-room', minimumHouseLevel: 4, accessoryIds: ['art-desk', 'storage-cabinet', 'plant-corner'] },
  { id: 'pet-comfort-set', nameKey: 'catalog.sets.pet-comfort-set.name', descriptionKey: 'catalog.sets.pet-comfort-set.description', icon: '🐾', price: 260, zoneId: 'living-room', minimumHouseLevel: 0, accessoryIds: ['cat-tree', 'cat-bed', 'dog-blanket', 'pet-bowl'] },
  { id: 'summer-garden-set', nameKey: 'catalog.sets.summer-garden-set.name', descriptionKey: 'catalog.sets.summer-garden-set.description', icon: '🏊', price: 520, zoneId: 'garden', minimumHouseLevel: 1, accessoryIds: ['garden-pool', 'garden-chair', 'garden-table', 'garden-sunshade'] },
  { id: 'garden-play-set', nameKey: 'catalog.sets.garden-play-set.name', descriptionKey: 'catalog.sets.garden-play-set.description', icon: '🤸', price: 340, zoneId: 'garden', minimumHouseLevel: 2, accessoryIds: ['garden-trampoline', 'hammock', 'garden-lights'] },
  { id: 'halloween-exterior-set', nameKey: 'catalog.sets.halloween-exterior-set.name', descriptionKey: 'catalog.sets.halloween-exterior-set.description', icon: '🎃', price: 190, zoneId: 'garden', minimumHouseLevel: 0, accessoryIds: ['halloween-pumpkin-arch', 'halloween-bat-garland'] },
  { id: 'cotton-candy-exterior-set', nameKey: 'catalog.sets.cotton-candy-exterior-set.name', descriptionKey: 'catalog.sets.cotton-candy-exterior-set.description', icon: '🍭', price: 210, zoneId: 'garden', minimumHouseLevel: 0, accessoryIds: ['candy-cloud-bushes', 'candy-striped-fence'] },
];

export const HOUSE_THEMES: HouseThemeDefinition[] = [
  {
    id: 'sunny-dollhouse',
    nameKey: 'catalog.themes.sunny-dollhouse.name',
    descriptionKey: 'catalog.themes.sunny-dollhouse.description',
    icon: '🏡',
    price: 0,
    kind: 'standard',
    ownedByDefault: true,
    ...houseThemeColorPalette.sunnyDollhouse,
  },
  {
    id: 'halloween-night',
    nameKey: 'catalog.themes.halloween-night.name',
    descriptionKey: 'catalog.themes.halloween-night.description',
    icon: '🎃',
    price: 320,
    kind: 'seasonal',
    ownedByDefault: false,
    ...houseThemeColorPalette.halloweenNight,
  },
  {
    id: 'cotton-candy-dream',
    nameKey: 'catalog.themes.cotton-candy-dream.name',
    descriptionKey: 'catalog.themes.cotton-candy-dream.description',
    icon: '🍬',
    price: 360,
    kind: 'fantasy',
    ownedByDefault: false,
    ...houseThemeColorPalette.cottonCandyDream,
  },
  {
    id: 'starlight-palace',
    nameKey: 'catalog.themes.starlight-palace.name',
    descriptionKey: 'catalog.themes.starlight-palace.description',
    icon: '👑',
    price: 420,
    kind: 'fantasy',
    ownedByDefault: false,
    ...houseThemeColorPalette.starlightPalace,
  },
];

type HouseAccessoryFixture = Omit<HouseAccessory, 'category'> & { readonly category?: HouseAccessoryCategory };

export const createHouseAccessories = (): HouseAccessory[] => ([
  { id: 'flower-boxes', category: 'garden', title: '', description: '', icon: '🌺', price: 60, placement: 'outside', minimumHouseLevel: 0, visual: 'flower-box', owned: true, equipped: true },
  { id: 'garden-lights', category: 'garden', title: '', description: '', icon: '🏮', price: 120, placement: 'outside', setIds: ['garden-play-set'], minimumHouseLevel: 0, visual: 'string-lights', owned: false, equipped: false },
  { id: 'hammock', category: 'garden', title: '', description: '', icon: '🏖️', price: 160, placement: 'outside', setIds: ['garden-play-set'], minimumHouseLevel: 0, visual: 'hammock', owned: false, equipped: false },
  { id: 'telescope', category: 'garden', title: '', description: '', icon: '🔭', price: 260, placement: 'outside', minimumHouseLevel: 0, visual: 'telescope', owned: false, equipped: false },
  { id: 'round-rug', category: 'furniture', title: '', description: '', icon: '🟠', price: 70, placement: 'inside', roomId: 'living-room', setIds: ['cozy-living-set'], minimumHouseLevel: 0, visual: 'rug', scene: { x: 230, y: 253 }, owned: true, equipped: true },
  { id: 'cozy-sofa', category: 'furniture', title: '', description: '', icon: '🛋️', price: 140, placement: 'inside', roomId: 'living-room', setIds: ['cozy-living-set'], minimumHouseLevel: 0, visual: 'sofa', scene: { x: 295, y: 213 }, owned: true, equipped: true },
  { id: 'bookshelf', title: '', description: '', icon: '📚', price: 110, placement: 'inside', roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'bookshelf', scene: { x: 152, y: 193 }, owned: false, equipped: false },
  { id: 'floor-lamp', title: '', description: '', icon: '💡', price: 90, placement: 'inside', roomId: 'living-room', setIds: ['cozy-living-set'], minimumHouseLevel: 0, visual: 'lamp', scene: { x: 319, y: 198 }, owned: false, equipped: false },
  { id: 'bunk-bed', title: '', description: '', icon: '🛏️', price: 180, placement: 'inside', roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'bunk-bed', scene: { x: 167, y: 200 }, owned: false, equipped: false },
  { id: 'play-table', title: '', description: '', icon: '🧩', price: 95, placement: 'inside', roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'table', scene: { x: 225, y: 214 }, owned: false, equipped: false },
  { id: 'plant-corner', title: '', description: '', icon: '🪴', price: 55, placement: 'inside', roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 0, visual: 'plant', scene: { x: 183, y: 207 }, owned: false, equipped: false },
  { id: 'wall-art', title: '', description: '', icon: '🖼️', price: 45, placement: 'inside', roomId: 'living-room', minimumHouseLevel: 0, visual: 'wall-art', scene: { x: 163, y: 147 }, owned: false, equipped: false },
  { id: 'cat-tree', title: '', description: '', icon: '🐈', price: 135, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'cat-tree', scene: { x: 306, y: 210 }, owned: false, equipped: false },
  { id: 'cat-bed', title: '', description: '', icon: '🐾', price: 65, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'pet-bed', scene: { x: 270, y: 250 }, owned: false, equipped: false },
  { id: 'dog-blanket', title: '', description: '', icon: '🦴', price: 75, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'dog-blanket', scene: { x: 190, y: 250 }, owned: false, equipped: false },
  { id: 'pet-bowl', title: '', description: '', icon: '🥣', price: 50, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'food-bowl', scene: { x: 333, y: 246 }, owned: false, equipped: false },
  { id: 'kitchen-counter', title: '', description: '', icon: '🍳', price: 210, placement: 'inside', roomId: 'kitchen', setIds: ['family-kitchen-set'], minimumHouseLevel: 0, visual: 'kitchen-counter', owned: true, equipped: true },
  { id: 'retro-fridge', title: '', description: '', icon: '🧊', price: 150, placement: 'inside', roomId: 'kitchen', setIds: ['family-kitchen-set'], minimumHouseLevel: 0, visual: 'retro-fridge', owned: true, equipped: true },
  { id: 'family-dining-table', title: '', description: '', icon: '🍽️', price: 130, placement: 'inside', roomId: 'kitchen', setIds: ['family-kitchen-set'], minimumHouseLevel: 0, visual: 'dining-table', owned: true, equipped: true },
  { id: 'double-bed', title: '', description: '', icon: '🛏️', price: 190, placement: 'inside', roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'double-bed', owned: false, equipped: false },
  { id: 'wardrobe', title: '', description: '', icon: '🚪', price: 150, placement: 'inside', roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'wardrobe', owned: false, equipped: false },
  { id: 'bedside-table', title: '', description: '', icon: '🕯️', price: 80, placement: 'inside', roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'bedside-table', owned: false, equipped: false },
  { id: 'art-desk', title: '', description: '', icon: '🎨', price: 140, placement: 'inside', roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 4, visual: 'art-desk', owned: false, equipped: false },
  { id: 'storage-cabinet', title: '', description: '', icon: '🗄️', price: 120, placement: 'inside', roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 4, visual: 'storage-cabinet', owned: false, equipped: false },
  { id: 'garden-pool', title: '', description: '', icon: '🏊', price: 260, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'pool', owned: false, equipped: false },
  { id: 'garden-chair', title: '', description: '', icon: '🪑', price: 95, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'garden-chair', owned: false, equipped: false },
  { id: 'garden-table', title: '', description: '', icon: '🍹', price: 110, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'garden-table', owned: false, equipped: false },
  { id: 'garden-trampoline', title: '', description: '', icon: '🤸', price: 220, placement: 'outside', setIds: ['garden-play-set'], minimumHouseLevel: 2, visual: 'trampoline', owned: false, equipped: false },
  { id: 'garden-sunshade', title: '', description: '', icon: '⛱️', price: 105, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'sunshade', owned: false, equipped: false },
  { id: 'halloween-pumpkin-arch', category: 'special', title: '', description: '', icon: '🎃', price: 120, placement: 'outside', setIds: ['halloween-exterior-set'], minimumHouseLevel: 0, motion: 'glow', visual: 'pumpkin-arch', owned: false, equipped: false },
  { id: 'halloween-bat-garland', category: 'special', title: '', description: '', icon: '🦇', price: 95, placement: 'outside', setIds: ['halloween-exterior-set'], minimumHouseLevel: 0, motion: 'flutter', visual: 'bat-garland', owned: false, equipped: false },
  { id: 'candy-cloud-bushes', title: '', description: '', icon: '🍬', price: 125, placement: 'outside', setIds: ['cotton-candy-exterior-set'], minimumHouseLevel: 0, visual: 'candy-bush', owned: false, equipped: false },
  { id: 'candy-striped-fence', title: '', description: '', icon: '🍭', price: 110, placement: 'outside', setIds: ['cotton-candy-exterior-set'], minimumHouseLevel: 0, visual: 'candy-fence', owned: false, equipped: false },
  { id: 'family-flag', category: 'special', title: '', description: '', icon: '🚩', price: 180, placement: 'outside', minimumHouseLevel: 0, motion: 'wave', visual: 'family-flag', owned: false, equipped: false },
] satisfies HouseAccessoryFixture[]).map(accessory => ({
  ...accessory,
  translationKey: `catalog.accessories.${accessory.id}`,
  category: accessory.category ?? (accessory.placement === 'inside' ? 'furniture' : 'garden'),
}));
