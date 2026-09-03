import type { FurnitureSetDefinition, HouseRoomDefinition, HouseStageDefinition, HouseThemeDefinition } from '@/domain/house';
import type { HouseAccessory } from '@/domain/types';

export const HOUSE_STAGES: HouseStageDefinition[] = [
  { id: 'starter-home', level: 0, name: 'kleines Häuschen', icon: '🏠', rooms: 1 },
  { id: 'family-home', level: 1, name: 'gemütliches Familienhaus', icon: '🏡', rooms: 2 },
  { id: 'garden-home', level: 2, name: 'großes Gartenhaus', icon: '🏘️', rooms: 3 },
  { id: 'tower-home', level: 3, name: 'Haus mit Turm', icon: '🏰', rooms: 4 },
  { id: 'dream-home', level: 4, name: 'Traumhaus', icon: '✨🏡', rooms: 5 },
];

export const DEFAULT_HOUSE_STAGE = HOUSE_STAGES[0]!;

export const HOUSE_ROOMS: ReadonlyArray<HouseRoomDefinition> = [
  { id: 'living-room', name: 'Wohnzimmer', icon: '🛋️', minimumHouseLevel: 0, wall: '#fff3d2', floor: '#d9a574' },
  { id: 'kitchen', name: 'Küche', icon: '🍳', minimumHouseLevel: 1, wall: '#edf7e8', floor: '#c9b394' },
  { id: 'children-room', name: 'Kinderzimmer', icon: '🧸', minimumHouseLevel: 2, wall: '#e9f3ff', floor: '#d9b982' },
  { id: 'bedroom', name: 'Schlafzimmer', icon: '🛏️', minimumHouseLevel: 3, wall: '#f4eaff', floor: '#c89b7c' },
  { id: 'creative-room', name: 'Kreativzimmer', icon: '🎨', minimumHouseLevel: 4, wall: '#fff0e8', floor: '#d2a477' },
];

export const FURNITURE_SETS: ReadonlyArray<FurnitureSetDefinition> = [
  { id: 'cozy-living-set', name: 'Gemütliches Wohnzimmer', description: 'Sofa, Teppich und Sternenlampe als harmonisches Set.', icon: '🛋️', price: 250, zoneId: 'living-room', minimumHouseLevel: 0, accessoryIds: ['cozy-sofa', 'round-rug', 'floor-lamp'] },
  { id: 'family-kitchen-set', name: 'Familienküche', description: 'Eine vollständige Küche mit Arbeitszeile, Kühlschrank und Esstisch.', icon: '🍳', price: 420, zoneId: 'kitchen', minimumHouseLevel: 1, accessoryIds: ['kitchen-counter', 'retro-fridge', 'family-dining-table'] },
  { id: 'adventure-kids-set', name: 'Abenteuer-Kinderzimmer', description: 'Hochbett, Kreativtisch und Bücherregal für große Ideen.', icon: '🧸', price: 310, zoneId: 'children-room', minimumHouseLevel: 2, accessoryIds: ['bunk-bed', 'play-table', 'bookshelf'] },
  { id: 'calm-bedroom-set', name: 'Ruhiges Schlafzimmer', description: 'Doppelbett, Kleiderschrank und Nachttisch in warmen Farben.', icon: '🛏️', price: 390, zoneId: 'bedroom', minimumHouseLevel: 3, accessoryIds: ['double-bed', 'wardrobe', 'bedside-table'] },
  { id: 'creative-studio-set', name: 'Kreativatelier', description: 'Zeichentisch, Stauraum und Pflanzen für kreative Nachmittage.', icon: '🎨', price: 280, zoneId: 'creative-room', minimumHouseLevel: 4, accessoryIds: ['art-desk', 'storage-cabinet', 'plant-corner'] },
  { id: 'pet-comfort-set', name: 'Tierische Kuschelecke', description: 'Schlafplätze, Napf und Kletterplatz für eure Haustiere.', icon: '🐾', price: 260, zoneId: 'living-room', minimumHouseLevel: 0, accessoryIds: ['cat-tree', 'cat-bed', 'dog-blanket', 'pet-bowl'] },
  { id: 'summer-garden-set', name: 'Sommergarten', description: 'Pool, Liegestuhl, Gartentisch und Sonnenschirm für draußen.', icon: '🏊', price: 520, zoneId: 'garden', minimumHouseLevel: 1, accessoryIds: ['garden-pool', 'garden-chair', 'garden-table', 'garden-sunshade'] },
  { id: 'garden-play-set', name: 'Garten-Spielplatz', description: 'Trampolin, Hängematte und warme Gartenlichter.', icon: '🤸', price: 340, zoneId: 'garden', minimumHouseLevel: 2, accessoryIds: ['garden-trampoline', 'hammock', 'garden-lights'] },
];

export const HOUSE_THEMES: HouseThemeDefinition[] = [
  {
    id: 'sunny-dollhouse',
    name: 'Sonnenhaus',
    wall: '#fff8df',
    wallUpper: '#f2dcf2',
    floor: '#f8d89b',
    roof: '#ec6e66',
    roofShade: '#c64f56',
    trim: '#a96855',
  },
];

export const createHouseAccessories = (): HouseAccessory[] => [
  { id: 'flower-boxes', title: 'Bunte Blumenkästen', description: 'Blumen für die Fenster deiner Familienwelt.', icon: '🌺', price: 60, placement: 'outside', minimumHouseLevel: 0, visual: 'flower-box', owned: true, equipped: true },
  { id: 'garden-lights', title: 'Gartenlichter', description: 'Warme Lichter für gemütliche Abende.', icon: '🏮', price: 120, placement: 'outside', setIds: ['garden-play-set'], minimumHouseLevel: 0, visual: 'string-lights', owned: false, equipped: false },
  { id: 'hammock', title: 'Hängematte', description: 'Ein ruhiger Platz unter dem großen Baum.', icon: '🏖️', price: 160, placement: 'outside', setIds: ['garden-play-set'], minimumHouseLevel: 0, visual: 'hammock', owned: false, equipped: false },
  { id: 'telescope', title: 'Sternenfernrohr', description: 'Für klare Nächte auf der kleinen Terrasse.', icon: '🔭', price: 260, placement: 'outside', minimumHouseLevel: 0, visual: 'telescope', owned: false, equipped: false },
  { id: 'round-rug', title: 'Sonnen-Teppich', description: 'Macht schon das erste kleine Zimmer gemütlich.', icon: '🟠', price: 70, placement: 'inside', roomId: 'living-room', setIds: ['cozy-living-set'], minimumHouseLevel: 0, visual: 'rug', scene: { x: 230, y: 253 }, owned: true, equipped: true },
  { id: 'cozy-sofa', title: 'Wolken-Sofa', description: 'Ein weicher Platz für gemeinsame Geschichten.', icon: '🛋️', price: 140, placement: 'inside', roomId: 'living-room', setIds: ['cozy-living-set'], minimumHouseLevel: 0, visual: 'sofa', scene: { x: 295, y: 213 }, owned: false, equipped: false },
  { id: 'bookshelf', title: 'Buntes Bücherregal', description: 'Ein kleines Regal für große Ideen.', icon: '📚', price: 110, placement: 'inside', roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'bookshelf', scene: { x: 152, y: 193 }, owned: false, equipped: false },
  { id: 'floor-lamp', title: 'Sternenlampe', description: 'Leuchtet warm, wenn die Tagesenergie steigt.', icon: '💡', price: 90, placement: 'inside', roomId: 'living-room', setIds: ['cozy-living-set'], minimumHouseLevel: 0, visual: 'lamp', scene: { x: 319, y: 198 }, owned: false, equipped: false },
  { id: 'bunk-bed', title: 'Abenteuer-Hochbett', description: 'Ein stabiles Bett mit Stauraum und Leselicht.', icon: '🛏️', price: 180, placement: 'inside', roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'bunk-bed', scene: { x: 167, y: 200 }, owned: false, equipped: false },
  { id: 'play-table', title: 'Kreativtisch', description: 'Ein kleiner Tisch für Bauen, Malen und Familienprojekte.', icon: '🧩', price: 95, placement: 'inside', roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'table', scene: { x: 225, y: 214 }, owned: false, equipped: false },
  { id: 'plant-corner', title: 'Dschungelpflanze', description: 'Eine große Zimmerpflanze für eine lebendige Ecke.', icon: '🪴', price: 55, placement: 'inside', roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 0, visual: 'plant', scene: { x: 183, y: 207 }, owned: false, equipped: false },
  { id: 'wall-art', title: 'Familienbild', description: 'Ein gerahmtes Bild für die Wand eures Hauses.', icon: '🖼️', price: 45, placement: 'inside', roomId: 'living-room', minimumHouseLevel: 0, visual: 'wall-art', scene: { x: 163, y: 147 }, owned: false, equipped: false },
  { id: 'cat-tree', title: 'Katzenbaum', description: 'Ein Kletterplatz mit Aussicht für kleine Samtpfoten.', icon: '🐈', price: 135, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'cat-tree', scene: { x: 306, y: 210 }, owned: false, equipped: false },
  { id: 'cat-bed', title: 'Katzenkissen', description: 'Ein weiches Lieblingsplätzchen zum Schnurren.', icon: '🐾', price: 65, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'pet-bed', scene: { x: 270, y: 250 }, owned: false, equipped: false },
  { id: 'dog-blanket', title: 'Hundedecke', description: 'Eine kuschelige Decke für entspannte Pausen.', icon: '🦴', price: 75, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'dog-blanket', scene: { x: 190, y: 250 }, owned: false, equipped: false },
  { id: 'pet-bowl', title: 'Futternapf', description: 'Ein bunter Napf für hungrige Hausfreunde.', icon: '🥣', price: 50, placement: 'inside', roomId: 'living-room', setIds: ['pet-comfort-set'], minimumHouseLevel: 0, visual: 'food-bowl', scene: { x: 333, y: 246 }, owned: false, equipped: false },
  { id: 'kitchen-counter', title: 'Küchenzeile', description: 'Arbeitsplatte, Spüle und Herd als saubere Einheit.', icon: '🍳', price: 210, placement: 'inside', roomId: 'kitchen', setIds: ['family-kitchen-set'], minimumHouseLevel: 1, visual: 'kitchen-counter', owned: false, equipped: false },
  { id: 'retro-fridge', title: 'Retro-Kühlschrank', description: 'Ein geräumiger Kühlschrank mit Familienmagneten.', icon: '🧊', price: 150, placement: 'inside', roomId: 'kitchen', setIds: ['family-kitchen-set'], minimumHouseLevel: 1, visual: 'retro-fridge', owned: false, equipped: false },
  { id: 'family-dining-table', title: 'Familien-Esstisch', description: 'Ein robuster Holztisch mit Sitzplätzen für alle.', icon: '🍽️', price: 130, placement: 'inside', roomId: 'kitchen', setIds: ['family-kitchen-set'], minimumHouseLevel: 1, visual: 'dining-table', owned: false, equipped: false },
  { id: 'double-bed', title: 'Gemütliches Doppelbett', description: 'Ein gepolstertes Bett mit Decke und Kissen.', icon: '🛏️', price: 190, placement: 'inside', roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'double-bed', owned: false, equipped: false },
  { id: 'wardrobe', title: 'Großer Kleiderschrank', description: 'Ordentlicher Stauraum für die Familiengarderobe.', icon: '🚪', price: 150, placement: 'inside', roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'wardrobe', owned: false, equipped: false },
  { id: 'bedside-table', title: 'Nachttisch', description: 'Kleine Ablage mit warmer Leselampe.', icon: '🕯️', price: 80, placement: 'inside', roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'bedside-table', owned: false, equipped: false },
  { id: 'art-desk', title: 'Zeichentisch', description: 'Ein großer Arbeitstisch für Kunst und Projekte.', icon: '🎨', price: 140, placement: 'inside', roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 4, visual: 'art-desk', owned: false, equipped: false },
  { id: 'storage-cabinet', title: 'Materialschrank', description: 'Beschriftete Fächer halten Bastelsachen ordentlich.', icon: '🗄️', price: 120, placement: 'inside', roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 4, visual: 'storage-cabinet', owned: false, equipped: false },
  { id: 'garden-pool', title: 'Familienpool', description: 'Ein runder Pool mit Leiter für warme Sommertage.', icon: '🏊', price: 260, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'pool', owned: false, equipped: false },
  { id: 'garden-chair', title: 'Garten-Liegestuhl', description: 'Ein verstellbarer Holzstuhl mit weichem Polster.', icon: '🪑', price: 95, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'garden-chair', owned: false, equipped: false },
  { id: 'garden-table', title: 'Gartentisch', description: 'Ein wetterfester Tisch für gemeinsame Pausen.', icon: '🍹', price: 110, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'garden-table', owned: false, equipped: false },
  { id: 'garden-trampoline', title: 'Sicheres Trampolin', description: 'Mit Netz, Leiter und weichem Rand.', icon: '🤸', price: 220, placement: 'outside', setIds: ['garden-play-set'], minimumHouseLevel: 2, visual: 'trampoline', owned: false, equipped: false },
  { id: 'garden-sunshade', title: 'Großer Sonnenschirm', description: 'Spendet Schatten neben Pool und Gartentisch.', icon: '⛱️', price: 105, placement: 'outside', setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'sunshade', owned: false, equipped: false },
];
