import type { HouseStageDefinition, HouseThemeDefinition } from '@/domain/house';
import type { HouseAccessory } from '@/domain/types';

export const HOUSE_STAGES: HouseStageDefinition[] = [
  { id: 'starter-home', level: 0, name: 'kleines Häuschen', icon: '🏠', rooms: 1 },
  { id: 'family-home', level: 1, name: 'gemütliches Familienhaus', icon: '🏡', rooms: 2 },
  { id: 'garden-home', level: 2, name: 'großes Gartenhaus', icon: '🏘️', rooms: 3 },
  { id: 'tower-home', level: 3, name: 'Haus mit Turm', icon: '🏰', rooms: 4 },
  { id: 'dream-home', level: 4, name: 'Traumhaus', icon: '✨🏡', rooms: 5 },
];

export const DEFAULT_HOUSE_STAGE = HOUSE_STAGES[0]!;

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
  { id: 'flower-boxes', title: 'Bunte Blumenkästen', description: 'Blumen für die Fenster deiner Familienwelt.', icon: '🌺', price: 60, placement: 'outside', owned: true, equipped: true },
  { id: 'garden-lights', title: 'Gartenlichter', description: 'Warme Lichter für gemütliche Abende.', icon: '🏮', price: 120, placement: 'outside', owned: false, equipped: false },
  { id: 'hammock', title: 'Hängematte', description: 'Ein ruhiger Platz unter dem großen Baum.', icon: '🏖️', price: 160, placement: 'outside', owned: false, equipped: false },
  { id: 'telescope', title: 'Sternenfernrohr', description: 'Für klare Nächte auf der kleinen Terrasse.', icon: '🔭', price: 260, placement: 'outside', owned: false, equipped: false },
  { id: 'round-rug', title: 'Sonnen-Teppich', description: 'Macht schon das erste kleine Zimmer gemütlich.', icon: '🟠', price: 70, placement: 'inside', visual: 'rug', scene: { x: 230, y: 253 }, owned: true, equipped: true },
  { id: 'cozy-sofa', title: 'Wolken-Sofa', description: 'Ein weicher Platz für gemeinsame Geschichten.', icon: '🛋️', price: 140, placement: 'inside', visual: 'sofa', scene: { x: 295, y: 213 }, owned: false, equipped: false },
  { id: 'bookshelf', title: 'Buntes Bücherregal', description: 'Ein kleines Regal für große Ideen.', icon: '📚', price: 110, placement: 'inside', visual: 'bookshelf', scene: { x: 152, y: 193 }, owned: false, equipped: false },
  { id: 'floor-lamp', title: 'Sternenlampe', description: 'Leuchtet warm, wenn die Tagesenergie steigt.', icon: '💡', price: 90, placement: 'inside', visual: 'lamp', scene: { x: 319, y: 198 }, owned: false, equipped: false },
  { id: 'bunk-bed', title: 'Abenteuer-Hochbett', description: 'Passt auch nach einem Stufenwechsel ins Hausinventar.', icon: '🛏️', price: 180, placement: 'inside', visual: 'bunk-bed', scene: { x: 167, y: 200 }, owned: false, equipped: false },
  { id: 'play-table', title: 'Kreativtisch', description: 'Ein kleiner Tisch für Bauen, Malen und Familienprojekte.', icon: '🧩', price: 95, placement: 'inside', visual: 'table', scene: { x: 225, y: 214 }, owned: false, equipped: false },
  { id: 'plant-corner', title: 'Dschungelpflanze', description: 'Eine große Zimmerpflanze für die gemütliche Spielecke.', icon: '🪴', price: 55, placement: 'inside', visual: 'plant', scene: { x: 183, y: 207 }, owned: false, equipped: false },
  { id: 'wall-art', title: 'Familienbild', description: 'Ein fröhliches Bild für die Wand eures Puppenhauses.', icon: '🖼️', price: 45, placement: 'inside', visual: 'wall-art', scene: { x: 163, y: 147 }, owned: false, equipped: false },
  { id: 'cat-tree', title: 'Katzenbaum', description: 'Ein Kletterplatz mit Aussicht für kleine Samtpfoten.', icon: '🐈', price: 135, placement: 'inside', visual: 'cat-tree', scene: { x: 306, y: 210 }, owned: false, equipped: false },
  { id: 'cat-bed', title: 'Katzenkissen', description: 'Ein weiches Lieblingsplätzchen zum Schnurren.', icon: '🐾', price: 65, placement: 'inside', visual: 'pet-bed', scene: { x: 270, y: 250 }, owned: false, equipped: false },
  { id: 'dog-blanket', title: 'Hundedecke', description: 'Eine kuschelige Decke für entspannte Pausen.', icon: '🦴', price: 75, placement: 'inside', visual: 'dog-blanket', scene: { x: 190, y: 250 }, owned: false, equipped: false },
  { id: 'pet-bowl', title: 'Futternapf', description: 'Ein bunter Napf für hungrige Hausfreunde.', icon: '🥣', price: 50, placement: 'inside', visual: 'food-bowl', scene: { x: 333, y: 246 }, owned: false, equipped: false },
];
