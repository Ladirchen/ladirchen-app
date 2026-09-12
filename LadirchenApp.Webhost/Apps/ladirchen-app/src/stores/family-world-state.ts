import { createGuardianAvatarAppearance, normalizeAvatarAppearance } from '@/domain/avatar';
import { HOUSE_THEMES } from '@/domain/house-catalog';
import type { HouseStageLevel } from '@/domain/house';
import type { ContributionId, FamilyCurrency, FamilyMember, FamilyPet, GuardianGift, HouseLayoutPlacement, SubscriptionTier, ViewerRole } from '@/domain/types';
import { createContributions, createFamilyBalances, createFamilyMembers, createFamilyPets, createHouseAccessories, createHouseLayoutPlacements, createPromotions, createSavingGoals, createShopRewards, FAMILY_MEMBER_IDS, SAVING_GOAL_IDS } from '@/infrastructure/fixtures/family-world-fixtures';

const AUTH_STATE_KEY = 'ladirchen:auth-state';
const authIsActive = (): boolean => {
  if (typeof localStorage === 'undefined') {return true;}
  const stored = localStorage.getItem(AUTH_STATE_KEY);
  return stored === null || stored === 'authenticated';
};

export const normalizeFamilyMembers = (members: FamilyMember[]): FamilyMember[] => {
  const guardians = members.filter(member => member.role === 'guardian');
  const hasAdministrator = guardians.some(member => member.guardianAccess === 'admin');
  return members.map((member) => {
    if (member.role !== 'guardian') {
      return {
        ...member,
        participatesInWeeklyGoal: member.participatesInWeeklyGoal ?? true,
        ...(member.appearance ? { appearance: normalizeAvatarAppearance(member.appearance) } : {}),
      };
    }
    const isFirstGuardian = member.id === guardians[0]?.id;
    const fallbackAppearance = createGuardianAvatarAppearance(isFirstGuardian ? 'adult' : 'grandpa');
    return {
      ...member,
      guardianAccess: member.guardianAccess ?? (!hasAdministrator && isFirstGuardian ? 'admin' : 'supporter'),
      participatesInWeeklyGoal: member.participatesInWeeklyGoal ?? false,
      appearance: normalizeAvatarAppearance(member.appearance, fallbackAppearance),
    };
  });
};

export const normalizeFamilyPets = (pets: ReadonlyArray<FamilyPet>): FamilyPet[] =>
  pets.map(pet => ({ ...pet }));

const initialStateValue = <T>(value: T): T => value;

export const mergeHouseLayout = (stored: ReadonlyArray<HouseLayoutPlacement>): HouseLayoutPlacement[] => {
  const storedById = new Map(stored.map(placement => [placement.id, placement]));
  return createHouseLayoutPlacements().map((placement) => {
    const saved = storedById.get(placement.id);
    if (!saved) {return placement;}
    const isCharacter = placement.entityType === 'member' || placement.entityType === 'pet';
    const isLadi = placement.entityType === 'ladi';
    const savedLadiWasPerched = isLadi && saved.y < 62;
    const isWallDecoration = placement.entityType === 'furniture' &&
      (placement.entityId === 'wall-art' || placement.entityId === 'halloween-bat-garland');
    const savedYIsValid = isWallDecoration
      ? saved.y >= 12 && saved.y <= 46
      : isLadi
        ? (saved.y >= 22 && saved.y <= 38) || saved.y >= 62
        : saved.y >= (isCharacter ? 62 : 52);
    const coordinates = {
      scale: Math.max(.5, Math.min(1.35, saved.scale)),
      x: savedLadiWasPerched ? placement.x : Math.max(4, Math.min(96, saved.x)),
      y: savedYIsValid ? Math.max(8, Math.min(94, saved.y)) : placement.y,
      zoneId: saved.zoneId,
    };
    if (placement.entityType === 'furniture') {return { ...placement, ...coordinates };}
    if (placement.entityType === 'member') {return { ...placement, ...coordinates };}
    if (placement.entityType === 'pet') {return { ...placement, ...coordinates };}
    return {
      ...placement,
      ...coordinates,
    };
  });
};

export const createFamilyWorldState = () => {
  const viewerRole = initialStateValue<ViewerRole>('child');
  const familyCurrencyCode = initialStateValue<FamilyCurrency>('CHF');
  const simulatedEnergy = initialStateValue<number | null>(null);
  const rewardAnimation: { visible: boolean; contributionId: ContributionId | undefined; value: number; energy: number; multiplier: number; stars: number; title: string; version: number } =
    { visible: false, contributionId: undefined, value: 0, energy: 0, multiplier: 1, stars: 0, title: '', version: 0 };
  const guardianGiftAnimation: { visible: boolean; guardianName: string; goalTitle: string; destination: 'balance' | 'goal'; amount: number; version: number } =
    { visible: false, guardianName: '', goalTitle: '', destination: 'goal', amount: 0, version: 0 };
  const pendingGuardianGifts: GuardianGift[] = [];
  const houseLevel = initialStateValue<HouseStageLevel>(0);
  const subscriptionTier = initialStateValue<SubscriptionTier>('pro');
  const snackbarParams: Record<string, number | string> = {};
  return {
    isAuthenticated: authIsActive(),
    signedInMemberId: FAMILY_MEMBER_IDS.laura,
    viewerRole,
    activeChildId: createFamilyMembers().find(member => member.role === 'child')?.id ?? FAMILY_MEMBER_IDS.laura,
    activeGoalId: SAVING_GOAL_IDS.bike,
    onboardingCompleted: false,
    familySetupOpen: false,
    balances: createFamilyBalances(),
    familyCurrencyCode,
    ladirchenPerCurrencyUnit: 10,
    perfectRatingBonusPercent: 5,
    baseSavingsRatePercent: 1,
    streakBonusRate: 0.5,
    completionBonusRate: 3.5,
    ratingBonusRate: 2.5,
    maxSavingsRatePercent: 8,
    simulatedEnergy,
    piggyBankOpen: false,
    rewardAnimation,
    guardianGiftAnimation,
    pendingGuardianGifts,
    completedWeeklyStreak: 0,
    currentWeekDays: 4,
    currentWeekTarget: 7,
    houseLevel,
    houseThemeId: HOUSE_THEMES[0]?.id ?? 'sunny-dollhouse',
    ownedHouseThemeIds: HOUSE_THEMES.filter(theme => theme.ownedByDefault).map(theme => theme.id),
    subscriptionTier,
    houseLayout: createHouseLayoutPlacements(),
    homeCustomizationHydrated: false,
    familyAggregatesHydrated: false,
    revealVersion: 0,
    snackbar: { visible: false, messageKey: '', params: snackbarParams },
    members: normalizeFamilyMembers(createFamilyMembers()),
    pets: normalizeFamilyPets(createFamilyPets()),
    contributions: createContributions(),
    goals: createSavingGoals(),
    accessories: createHouseAccessories(),
    promotions: createPromotions(),
    shopRewards: createShopRewards(),
  };
};

export type FamilyWorldState = ReturnType<typeof createFamilyWorldState>;
