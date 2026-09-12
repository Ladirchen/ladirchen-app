import type { Contribution, Promotion } from '@/domain/contributions/types';
import type { FamilyMember, FamilyPet } from '@/domain/family/types';
import type { HouseLayoutPlacement } from '@/domain/house/entities';
import type { SavingGoal } from '@/domain/savings/types';
import { createDomainId } from '@/domain/shared/identifiers';
import type { FamilyMemberId } from '@/domain/shared/identifiers';
import type { ShopReward } from '@/domain/shop/types';
import { createGuardianAvatarAppearance } from '@/domain/avatar';
import { createHouseAccessories, createHouseAccessoryLayoutPlacements } from '@/domain/house/catalog';
import { familyMemberColorPalette } from '@/theme/color-palette';
import type { FamilyWorldInitialData } from '@/application/ports/family-world-initial-data';

export { createHouseAccessories };

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
  { id: FAMILY_MEMBER_IDS.laura, name: 'Laura', avatar: '👧', color: familyMemberColorPalette.laura, role: 'child', participatesInWeeklyGoal: true, weeklyStreak: 4 },
  { id: FAMILY_MEMBER_IDS.adam, name: 'Adam', avatar: '👦', color: familyMemberColorPalette.adam, role: 'child', participatesInWeeklyGoal: true, weeklyStreak: 3 },
  { id: FAMILY_MEMBER_IDS.daniel, name: 'Daniel', avatar: '🧒', color: familyMemberColorPalette.daniel, role: 'child', participatesInWeeklyGoal: true, weeklyStreak: 2 },
  { id: FAMILY_MEMBER_IDS.mama, name: 'Mama', avatar: '👩', color: familyMemberColorPalette.mama, role: 'guardian', guardianAccess: 'admin', participatesInWeeklyGoal: true, weeklyStreak: 0, appearance: { ...createGuardianAvatarAppearance('adult'), hair: 'waves', outfit: 'blouse', outfitColorId: 'outfit-rose' } },
  { id: FAMILY_MEMBER_IDS.hannes, name: 'Hannes', avatar: '🧑', color: familyMemberColorPalette.hannes, role: 'guardian', guardianAccess: 'supporter', participatesInWeeklyGoal: false, weeklyStreak: 0, appearance: { ...createGuardianAvatarAppearance('adult'), faceShape: 'angular', outfitColorId: 'outfit-mint' } },
];

export const createFamilyBalances = (): Record<FamilyMemberId, number> => ({
  [FAMILY_MEMBER_IDS.laura]: 340,
  [FAMILY_MEMBER_IDS.adam]: 220,
  [FAMILY_MEMBER_IDS.daniel]: 175,
});

export const createFamilyPets = (): FamilyPet[] => [
  { id: FAMILY_PET_IDS.anna, name: 'Anna', kind: 'cat', avatar: '🐈', color: familyMemberColorPalette.petAnna },
  { id: FAMILY_PET_IDS.elsa, name: 'Elsa', kind: 'cat', avatar: '🐈‍⬛', color: familyMemberColorPalette.petElsa },
];

export const createHouseLayoutPlacements = (): HouseLayoutPlacement[] => [
  ...createHouseAccessoryLayoutPlacements(),
  { id: createDomainId.houseLayoutPlacement('member-laura'), entityType: 'member', entityId: FAMILY_MEMBER_IDS.laura, zoneId: 'living-room', x: 42, y: 72, scale: .75 },
  { id: createDomainId.houseLayoutPlacement('member-adam'), entityType: 'member', entityId: FAMILY_MEMBER_IDS.adam, zoneId: 'living-room', x: 56, y: 72, scale: .75 },
  { id: createDomainId.houseLayoutPlacement('member-daniel'), entityType: 'member', entityId: FAMILY_MEMBER_IDS.daniel, zoneId: 'living-room', x: 67, y: 72, scale: .75 },
  { id: createDomainId.houseLayoutPlacement('member-mama'), entityType: 'member', entityId: FAMILY_MEMBER_IDS.mama, zoneId: 'kitchen', x: 28, y: 72, scale: .78 },
  { id: createDomainId.houseLayoutPlacement('member-hannes'), entityType: 'member', entityId: FAMILY_MEMBER_IDS.hannes, zoneId: 'kitchen', x: 72, y: 72, scale: .78 },
  { id: createDomainId.houseLayoutPlacement('pet-anna'), entityType: 'pet', entityId: FAMILY_PET_IDS.anna, zoneId: 'living-room', x: 18, y: 80, scale: .72 },
  { id: createDomainId.houseLayoutPlacement('pet-elsa'), entityType: 'pet', entityId: FAMILY_PET_IDS.elsa, zoneId: 'living-room', x: 86, y: 80, scale: .72 },
  { id: createDomainId.houseLayoutPlacement('family-ladi'), entityType: 'ladi', entityId: 'family-ladi', zoneId: 'living-room', x: 18, y: 30, scale: .78 },
];

export const createContributions = (): Contribution[] => [
  {
    id: CONTRIBUTION_IDS.roomAir,
    translationKey: 'seed.contributions.roomAir',
    title: '',
    description: '',
    icon: '🌬️',
    area: '',
    kind: 'basic',
    status: 'approved',
    reward: 5,
    energy: 15,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: '',
    worldEffect: 'sparkle',
    stars: 4,
  },
  {
    id: CONTRIBUTION_IDS.setTable,
    translationKey: 'seed.contributions.setTable',
    title: '',
    description: '',
    icon: '🥣',
    area: '',
    kind: 'basic',
    status: 'approved',
    reward: 10,
    energy: 20,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: '',
    worldEffect: 'flowers',
    stars: 5,
  },
  {
    id: CONTRIBUTION_IDS.feedPet,
    translationKey: 'seed.contributions.feedPet',
    title: '',
    description: '',
    icon: '🐈',
    area: '',
    kind: 'basic',
    status: 'approved',
    reward: 8,
    energy: 25,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: '',
    worldEffect: 'garden',
    stars: 4,
  },
  {
    id: CONTRIBUTION_IDS.dishwasher,
    translationKey: 'seed.contributions.dishwasher',
    title: '',
    description: '',
    icon: '🍽️',
    area: '',
    kind: 'basic',
    status: 'available',
    reward: 15,
    energy: 25,
    assigneeId: FAMILY_MEMBER_IDS.laura,
    dueLabel: '',
    worldEffect: 'lights',
  },
  {
    id: CONTRIBUTION_IDS.takeTrash,
    translationKey: 'seed.contributions.takeTrash',
    title: '',
    description: '',
    icon: '🗑️',
    area: '',
    kind: 'basic',
    status: 'available',
    reward: 10,
    energy: 15,
    dueLabel: '',
    worldEffect: 'smoke',
  },
  {
    id: CONTRIBUTION_IDS.waterPlants,
    translationKey: 'seed.contributions.waterPlants',
    title: '',
    description: '',
    icon: '🌱',
    area: '',
    kind: 'extra',
    status: 'available',
    reward: 10,
    energy: 0,
    dueLabel: '',
  },
  {
    id: CONTRIBUTION_IDS.adamBreakfast,
    translationKey: 'seed.contributions.adamBreakfast',
    title: '',
    description: '',
    icon: '🥛',
    area: '',
    kind: 'basic',
    status: 'approved',
    reward: 8,
    energy: 60,
    assigneeId: FAMILY_MEMBER_IDS.adam,
    dueLabel: '',
    worldEffect: 'lights',
  },
  {
    id: CONTRIBUTION_IDS.adamShoes,
    translationKey: 'seed.contributions.adamShoes',
    title: '',
    description: '',
    icon: '👟',
    area: '',
    kind: 'basic',
    status: 'available',
    reward: 6,
    energy: 40,
    assigneeId: FAMILY_MEMBER_IDS.adam,
    dueLabel: '',
    worldEffect: 'sparkle',
  },
  {
    id: CONTRIBUTION_IDS.danielCats,
    translationKey: 'seed.contributions.danielCats',
    title: '',
    description: '',
    icon: '🐈‍⬛',
    area: '',
    kind: 'basic',
    status: 'approved',
    reward: 10,
    energy: 60,
    assigneeId: FAMILY_MEMBER_IDS.daniel,
    dueLabel: '',
    worldEffect: 'garden',
  },
  {
    id: CONTRIBUTION_IDS.danielLaundry,
    translationKey: 'seed.contributions.danielLaundry',
    title: '',
    description: '',
    icon: '🧺',
    area: '',
    kind: 'basic',
    status: 'available',
    reward: 9,
    energy: 40,
    assigneeId: FAMILY_MEMBER_IDS.daniel,
    dueLabel: '',
    worldEffect: 'flowers',
  },
];

export const createPromotions = (): Promotion[] => [
  {
    id: createDomainId.promotion('dishwasher-double'),
    translationKey: 'seed.promotions.dishwasherDouble',
    contributionId: CONTRIBUTION_IDS.dishwasher,
    title: '',
    multiplier: 2,
    deadline: '17:00',
    teamworkBonus: 10,
    active: true,
  },
];

export const createShopRewards = (): ShopReward[] => [
  { id: createDomainId.shopReward('allowance'), translationKey: 'seed.rewards.allowance', title: '', description: '', icon: '💵', price: 100, category: 'allowance', quantity: 3, conditions: '', status: 'available' },
  { id: createDomainId.shopReward('gaming-time'), translationKey: 'seed.rewards.gamingTime', title: '', description: '', icon: '🎮', price: 100, category: 'time', quantity: 2, conditions: '', status: 'available' },
  { id: createDomainId.shopReward('mueller-trip'), translationKey: 'seed.rewards.muellerTrip', title: '', description: '', icon: '🛍️', price: 180, category: 'activity', quantity: 1, conditions: '', status: 'available' },
  { id: createDomainId.shopReward('movie-night'), translationKey: 'seed.rewards.movieNight', title: '', description: '', icon: '🍿', price: 75, category: 'activity', quantity: 2, conditions: '', status: 'available' },
  { id: createDomainId.shopReward('bedtime'), translationKey: 'seed.rewards.bedtime', title: '', description: '', icon: '🌙', price: 45, category: 'privilege', quantity: 4, conditions: '', status: 'available' },
];

export const createSavingGoals = (): SavingGoal[] => [
  { id: SAVING_GOAL_IDS.bike, translationKey: 'seed.goals.bike', title: '', icon: '🚲', ownerId: FAMILY_MEMBER_IDS.laura, target: 500, saved: 125, interestEarned: 8, visibility: 'family', shared: false, cheered: false },
  { id: SAVING_GOAL_IDS.lego, translationKey: 'seed.goals.lego', title: '', icon: '🧱', ownerId: FAMILY_MEMBER_IDS.adam, target: 200, saved: 80, interestEarned: 4, visibility: 'family', shared: false, cheered: false },
  { id: SAVING_GOAL_IDS.camera, translationKey: 'seed.goals.camera', title: '', icon: '📷', ownerId: FAMILY_MEMBER_IDS.daniel, target: 350, saved: 90, interestEarned: 3, visibility: 'family', shared: false, cheered: false },
  { id: SAVING_GOAL_IDS.zoo, translationKey: 'seed.goals.zoo', title: '', icon: '🦒', ownerId: 'family', target: 600, saved: 360, visibility: 'family', shared: true, cheered: false },
];

export const createFamilyWorldInitialData = (): FamilyWorldInitialData => ({
  activeChildId: FAMILY_MEMBER_IDS.laura,
  activeGoalId: SAVING_GOAL_IDS.bike,
  accessories: createHouseAccessories(),
  balances: createFamilyBalances(),
  contributions: createContributions(),
  goals: createSavingGoals(),
  houseLayout: createHouseLayoutPlacements(),
  members: createFamilyMembers(),
  pets: createFamilyPets(),
  promotions: createPromotions(),
  shopRewards: createShopRewards(),
  signedInMemberId: FAMILY_MEMBER_IDS.laura,
});
