import { resolveFamilyMemberAvatarAppearance } from '@/domain/avatar';
import { characterCollidesWithFurniture, DEFAULT_ROOM_DESIGNS, furnitureVisualDefinitionFor, HOUSE_LAYOUT_CONSTRAINTS, HOUSE_THEMES } from '@/domain/house';
import { clamp } from '@/domain/shared/numbers';
import type { HouseStageLevel, HouseZoneId, RoomDesignId } from '@/domain/house';
import type { FamilyMember, FamilyPet, SubscriptionTier, ViewerRole } from '@/domain/family/types';
import { DEFAULT_FAMILY_TIME_ZONE } from '@/domain/family/time-zone';
import type { HouseLayoutPlacement } from '@/domain/house/entities';
import type { FamilyCurrency, GuardianGift } from '@/domain/savings/types';
import type { ContributionId } from '@/domain/shared/identifiers';
import type { TranslationKey } from '@/locales/translation-keys';
import type { FamilyWorldInitialData, FamilyWorldInitialDataFactory } from '@/application/ports/family-world-initial-data';

let configuredInitialDataFactory: FamilyWorldInitialDataFactory | undefined;

export const configureFamilyWorldInitialData = (factory: FamilyWorldInitialDataFactory): void => {
  configuredInitialDataFactory = factory;
};

export const normalizeFamilyMembers = (members: FamilyMember[]): FamilyMember[] => {
  const guardians = members.filter(member => member.role === 'guardian');
  const hasAdministrator = guardians.some(member => member.guardianAccess === 'admin');
  return members.map((member) => {
    if (member.role !== 'guardian') {
      return {
        ...member,
        participatesInWeeklyGoal: member.participatesInWeeklyGoal ?? true,
        appearance: resolveFamilyMemberAvatarAppearance(member, members),
      };
    }
    const isFirstGuardian = member.id === guardians[0]?.id;
    return {
      ...member,
      guardianAccess: member.guardianAccess ?? (!hasAdministrator && isFirstGuardian ? 'admin' : 'supporter'),
      participatesInWeeklyGoal: member.participatesInWeeklyGoal ?? false,
      appearance: resolveFamilyMemberAvatarAppearance(member, members),
    };
  });
};

export const normalizeFamilyPets = (pets: ReadonlyArray<FamilyPet>): FamilyPet[] =>
  pets.map(pet => ({ ...pet }));

const initialStateValue = <T>(value: T): T => value;

const createDefaultSelectedRoomDesignIds = (): Partial<Record<HouseZoneId, RoomDesignId>> => {
  const selections: Partial<Record<HouseZoneId, RoomDesignId>> = {};
  for (const design of DEFAULT_ROOM_DESIGNS) { selections[design.zoneId] = design.id; }
  return selections;
};

export const mergeHouseLayout = (
  stored: ReadonlyArray<HouseLayoutPlacement>,
  initialData: FamilyWorldInitialData,
): HouseLayoutPlacement[] => {
  const defaultPlacements = initialData.houseLayout;
  const accessories = initialData.accessories;
  const storedById = new Map(stored.map(placement => [placement.id, placement]));
  const fixedAccessoryIds = new Set(accessories
    .filter(accessory => accessory.mobility === 'fixed')
    .map(accessory => accessory.id));
  const mergedPlacements = defaultPlacements.map((placement) => {
    if (placement.entityType === 'furniture' && fixedAccessoryIds.has(placement.entityId)) { return placement; }
    const saved = storedById.get(placement.id);
    if (!saved) {return placement;}
    const isCharacter = placement.entityType === 'member' || placement.entityType === 'pet';
    const isLadi = placement.entityType === 'ladi';
    const savedLadiWasPerched = isLadi && saved.y < HOUSE_LAYOUT_CONSTRAINTS.floorMinimumY;
    const accessory = placement.entityType === 'furniture'
      ? accessories.find(item => item.id === placement.entityId)
      : undefined;
    const visualDefinition = accessory?.visual ? furnitureVisualDefinitionFor(accessory.visual) : undefined;
    const minimumY = visualDefinition?.minimumY;
    const locksScale = visualDefinition?.locksScale ?? false;
    const savedYIsValid = minimumY !== undefined
      ? saved.y >= minimumY && saved.y <= (visualDefinition?.maximumY ?? HOUSE_LAYOUT_CONSTRAINTS.maximumY)
      : isLadi
        ? (saved.y >= HOUSE_LAYOUT_CONSTRAINTS.perch.persistedMinimumY &&
          saved.y <= HOUSE_LAYOUT_CONSTRAINTS.perch.persistedMaximumY) ||
          saved.y >= HOUSE_LAYOUT_CONSTRAINTS.floorMinimumY
        : saved.y >= (isCharacter || locksScale
          ? HOUSE_LAYOUT_CONSTRAINTS.floorMinimumY
          : HOUSE_LAYOUT_CONSTRAINTS.furnitureMinimumY);
    const coordinates = {
      scale: locksScale ? placement.scale : clamp(
        saved.scale,
        HOUSE_LAYOUT_CONSTRAINTS.minimumScale,
        HOUSE_LAYOUT_CONSTRAINTS.maximumScale,
      ),
      x: savedLadiWasPerched ? placement.x : clamp(
        saved.x,
        HOUSE_LAYOUT_CONSTRAINTS.minimumX,
        HOUSE_LAYOUT_CONSTRAINTS.maximumX,
      ),
      y: savedYIsValid
        ? clamp(saved.y, HOUSE_LAYOUT_CONSTRAINTS.minimumY, HOUSE_LAYOUT_CONSTRAINTS.maximumY)
        : placement.y,
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
  return mergedPlacements.map((placement) => {
    if (placement.entityType === 'furniture' || !characterCollidesWithFurniture(
      placement.id,
      placement.zoneId,
      placement.x,
      placement.y,
      mergedPlacements,
      accessories,
    )) { return placement; }
    return defaultPlacements.find(defaultPlacement => defaultPlacement.id === placement.id) ?? placement;
  });
};

export const createFamilyWorldState = (providedInitialData?: FamilyWorldInitialData) => {
  const initialData = providedInitialData ?? configuredInitialDataFactory?.create();
  if (!initialData) {throw new Error('Family world initial data must be configured before the store is created.');}
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
  const snackbar: { visible: boolean; messageKey: TranslationKey | ''; params: Record<string, number | string> } =
    { visible: false, messageKey: '', params: snackbarParams };
  return {
    currentTimeMilliseconds: Date.now(),
    isAuthenticated: true,
    signedInMemberId: initialData.signedInMemberId,
    viewerRole,
    activeChildId: initialData.activeChildId,
    activeGoalId: initialData.activeGoalId,
    onboardingCompleted: false,
    familySetupOpen: false,
    balances: initialData.balances,
    familyCurrencyCode,
    familyTimeZone: DEFAULT_FAMILY_TIME_ZONE,
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
    currentWeekTarget: 7,
    houseLevel,
    houseThemeId: HOUSE_THEMES[0]?.id ?? 'sunny-dollhouse',
    ownedHouseThemeIds: HOUSE_THEMES.filter(theme => theme.ownedByDefault).map(theme => theme.id),
    ownedRoomDesignIds: DEFAULT_ROOM_DESIGNS.map(design => design.id),
    selectedRoomDesignIds: createDefaultSelectedRoomDesignIds(),
    subscriptionTier,
    houseLayout: initialData.houseLayout,
    homeCustomizationHydrated: false,
    familyAggregatesHydrated: false,
    revealVersion: 0,
    snackbar,
    members: normalizeFamilyMembers(initialData.members),
    pets: normalizeFamilyPets(initialData.pets),
    contributions: initialData.contributions,
    goals: initialData.goals,
    accessories: initialData.accessories,
    promotions: initialData.promotions,
    shopRewards: initialData.shopRewards,
  };
};

export type FamilyWorldState = ReturnType<typeof createFamilyWorldState>;
