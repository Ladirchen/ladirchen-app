import { createDomainId } from '@/domain/types';
import type { Contribution, FamilyMember, FamilyPet, Promotion, SavingGoal, ShopReward } from '@/domain/types';
import { createGuardianAvatarAppearance } from '@/domain/avatar';

export { createHouseAccessories } from '@/features/world/data/house-catalog';

export const FAMILY_MEMBER_IDS = {
  adam: createDomainId.familyMember('adam'),
  daniel: createDomainId.familyMember('daniel'),
  hannes: createDomainId.familyMember('hannes'),
  laura: createDomainId.familyMember('laura'),
  mama: createDomainId.familyMember('mama'),
} as const;

export const FAMILY_PET_IDS = {
  anna: createDomainId.familyPet('anna'),
  elsa: createDomainId.familyPet('elsa'),
} as const;

export const CONTRIBUTION_IDS = {
  adamBreakfast: createDomainId.contribution('adam-breakfast'),
  adamShoes: createDomainId.contribution('adam-shoes'),
  danielCats: createDomainId.contribution('daniel-cats'),
  danielLaundry: createDomainId.contribution('daniel-laundry'),
  dishwasher: createDomainId.contribution('dishwasher'),
  feedPet: createDomainId.contribution('feed-pet'),
  roomAir: createDomainId.contribution('room-air'),
  setTable: createDomainId.contribution('set-table'),
  takeTrash: createDomainId.contribution('take-trash'),
  waterPlants: createDomainId.contribution('water-plants'),
} as const;

export const SAVING_GOAL_IDS = {
  bike: createDomainId.savingGoal('bike'),
  camera: createDomainId.savingGoal('camera'),
  lego: createDomainId.savingGoal('lego'),
  zoo: createDomainId.savingGoal('zoo'),
} as const;

export const createFamilyMembers = (): FamilyMember[] => [
  { id: FAMILY_MEMBER_IDS.laura, name: 'Laura', avatar: '👧', color: '#6f8df5', role: 'child', weeklyStreak: 4 },
  { id: FAMILY_MEMBER_IDS.adam, name: 'Adam', avatar: '👦', color: '#e79f45', role: 'child', weeklyStreak: 3 },
  { id: FAMILY_MEMBER_IDS.daniel, name: 'Daniel', avatar: '🧒', color: '#59a8d8', role: 'child', weeklyStreak: 2 },
  { id: FAMILY_MEMBER_IDS.mama, name: 'Mama (du)', avatar: '👩', color: '#dd7b91', role: 'guardian', guardianAccess: 'admin', weeklyStreak: 0, appearance: { ...createGuardianAvatarAppearance('adult'), hair: 'waves', outfit: 'blouse', outfitColorId: 'outfit-rose' } },
  { id: FAMILY_MEMBER_IDS.hannes, name: 'Hannes', avatar: '🧑', color: '#58aa82', role: 'guardian', guardianAccess: 'supporter', weeklyStreak: 0, appearance: { ...createGuardianAvatarAppearance('adult'), faceShape: 'angular', outfitColorId: 'outfit-mint' } },
];

export const createFamilyPets = (): FamilyPet[] => [
  { id: FAMILY_PET_IDS.anna, name: 'Anna', kind: 'cat', kindLabel: 'Katze', avatar: '🐈', color: '#efb767' },
  { id: FAMILY_PET_IDS.elsa, name: 'Elsa', kind: 'cat', kindLabel: 'Katze', avatar: '🐈‍⬛', color: '#8996ac' },
];

export const createContributions = (): Contribution[] => [
  {
    id: CONTRIBUTION_IDS.roomAir,
    title: 'Zimmer lüften',
    description: 'Fenster für zehn Minuten vollständig öffnen.',
    icon: '🌬️',
    area: 'Eigenes Zimmer',
    kind: 'basic',
    status: 'approved',
    reward: 5,
    energy: 15,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: 'Morgens',
    worldEffect: 'sparkle',
    stars: 4,
  },
  {
    id: CONTRIBUTION_IDS.setTable,
    title: 'Tisch decken',
    description: 'Teller, Besteck und Gläser für alle bereitstellen.',
    icon: '🥣',
    area: 'Esszimmer',
    kind: 'basic',
    status: 'approved',
    reward: 10,
    energy: 20,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: 'Vor dem Abendessen',
    worldEffect: 'flowers',
    stars: 5,
  },
  {
    id: CONTRIBUTION_IDS.feedPet,
    title: 'Anna und Elsa füttern',
    description: 'Den Katzen frisches Wasser und ihre Portion Futter geben.',
    icon: '🐈',
    area: 'Haustiere',
    kind: 'basic',
    status: 'approved',
    reward: 8,
    energy: 25,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: 'Nach der Schule',
    worldEffect: 'garden',
    stars: 4,
  },
  {
    id: CONTRIBUTION_IDS.dishwasher,
    title: 'Geschirrspüler einräumen',
    description: 'Schmutziges Geschirr und Besteck ordentlich einräumen.',
    icon: '🍽️',
    area: 'Küche',
    kind: 'basic',
    status: 'available',
    reward: 15,
    energy: 25,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: 'Bis 17:00 Uhr',
    worldEffect: 'lights',
  },
  {
    id: CONTRIBUTION_IDS.takeTrash,
    title: 'Müll runterbringen',
    description: 'Restmüll mitnehmen und einen neuen Beutel einsetzen.',
    icon: '🗑️',
    area: 'Küche',
    kind: 'basic',
    status: 'available',
    reward: 10,
    energy: 15,
    dueLabel: 'Heute',
    worldEffect: 'smoke',
  },
  {
    id: CONTRIBUTION_IDS.waterPlants,
    title: 'Pflanzen gießen',
    description: 'Die Pflanzen im Wohnzimmer und auf dem Balkon gießen.',
    icon: '🌱',
    area: 'Wohnzimmer',
    kind: 'extra',
    status: 'available',
    reward: 10,
    energy: 0,
    dueLabel: 'Freiwillig',
  },
  {
    id: CONTRIBUTION_IDS.adamBreakfast,
    title: 'Frühstückstisch abräumen',
    description: 'Geschirr in die Küche bringen und den Tisch abwischen.',
    icon: '🥛',
    area: 'Esszimmer',
    kind: 'basic',
    status: 'approved',
    reward: 8,
    energy: 60,
    assigneeId: FAMILY_MEMBER_IDS.adam,
    dueLabel: 'Morgens',
    worldEffect: 'lights',
  },
  {
    id: CONTRIBUTION_IDS.adamShoes,
    title: 'Schuhe ordentlich stellen',
    description: 'Die Schuhe im Eingangsbereich paarweise einsortieren.',
    icon: '👟',
    area: 'Eingang',
    kind: 'basic',
    status: 'available',
    reward: 6,
    energy: 40,
    assigneeId: FAMILY_MEMBER_IDS.adam,
    dueLabel: 'Heute',
    worldEffect: 'sparkle',
  },
  {
    id: CONTRIBUTION_IDS.danielCats,
    title: 'Katzenplätze aufräumen',
    description: 'Die Decken von Anna und Elsa ausschütteln und richten.',
    icon: '🐈‍⬛',
    area: 'Haustiere',
    kind: 'basic',
    status: 'approved',
    reward: 10,
    energy: 60,
    assigneeId: FAMILY_MEMBER_IDS.daniel,
    dueLabel: 'Nachmittags',
    worldEffect: 'garden',
  },
  {
    id: CONTRIBUTION_IDS.danielLaundry,
    title: 'Wäsche sortieren',
    description: 'Helle und dunkle Wäsche in die passenden Körbe legen.',
    icon: '🧺',
    area: 'Bad',
    kind: 'basic',
    status: 'available',
    reward: 9,
    energy: 40,
    assigneeId: FAMILY_MEMBER_IDS.daniel,
    dueLabel: 'Heute',
    worldEffect: 'flowers',
  },
];

export const createPromotions = (): Promotion[] => [
  {
    id: createDomainId.promotion('dishwasher-double'),
    contributionId: CONTRIBUTION_IDS.dishwasher,
    title: 'Nur heute: doppelte Ladirchen',
    multiplier: 2,
    deadline: '17:00',
    teamworkBonus: 10,
    active: true,
  },
];

export const createShopRewards = (): ShopReward[] => [
  { id: createDomainId.shopReward('allowance'), title: '10 CHF Taschengeld-Auszahlung', description: 'Sofortige Auszahlung durch eine Bezugsperson.', icon: '💵', price: 100, category: 'allowance', quantity: 3, conditions: 'Maximal einmal pro Woche einlösbar.', status: 'available' },
  { id: createDomainId.shopReward('gaming-time'), title: '2 Stunden Gamingzeit', description: 'Zwei zusätzliche Stunden am vereinbarten Tag.', icon: '🎮', price: 100, category: 'time', quantity: 2, conditions: 'Termin vorher mit einer Bezugsperson vereinbaren.', status: 'available' },
  { id: createDomainId.shopReward('mueller-trip'), title: 'Müller-Kurztrip', description: 'Gemeinsam stöbern und eine Kleinigkeit aussuchen.', icon: '🛍️', price: 180, category: 'activity', quantity: 1, conditions: 'Einlösbar an einem freien Nachmittag.', status: 'available' },
  { id: createDomainId.shopReward('movie-night'), title: 'Filmabend nach Wunsch', description: 'Du wählst Film und Familiensnack aus.', icon: '🍿', price: 75, category: 'activity', quantity: 2, conditions: 'Film muss für alle Teilnehmenden geeignet sein.', status: 'available' },
  { id: createDomainId.shopReward('bedtime'), title: '30 Minuten länger wach', description: 'Am Wochenende eine halbe Stunde später ins Bett.', icon: '🌙', price: 45, category: 'privilege', quantity: 4, conditions: 'Nur Freitag oder Samstag einlösbar.', status: 'available' },
];

export const createSavingGoals = (): SavingGoal[] => [
  { id: SAVING_GOAL_IDS.bike, title: 'Neues Fahrrad', icon: '🚲', ownerId: FAMILY_MEMBER_IDS.laura, target: 500, saved: 125, interestEarned: 8, visibility: 'family', shared: false, cheered: false },
  { id: SAVING_GOAL_IDS.lego, title: 'Großes Legoset', icon: '🧱', ownerId: FAMILY_MEMBER_IDS.adam, target: 200, saved: 80, interestEarned: 4, visibility: 'family', shared: false, cheered: false },
  { id: SAVING_GOAL_IDS.camera, title: 'Eigene Kamera', icon: '📷', ownerId: FAMILY_MEMBER_IDS.daniel, target: 350, saved: 90, interestEarned: 3, visibility: 'family', shared: false, cheered: false },
  { id: SAVING_GOAL_IDS.zoo, title: 'Gemeinsamer Zoobesuch', icon: '🦒', ownerId: 'family', target: 600, saved: 360, visibility: 'family', shared: true, cheered: false },
];
