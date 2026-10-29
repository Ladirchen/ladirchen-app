import { resolveFamilyMemberAvatarAppearance } from "@/domain/avatar";
import { DEFAULT_ROOM_DESIGNS, HOUSE_THEMES } from "@/domain/house";
import type { HouseStageLevel, HouseZoneId, RoomDesignId } from "@/domain/house";
import type { FamilyMember, FamilyPet, SubscriptionTier, ViewerRole } from "@/domain/family/types";
import { DEFAULT_FAMILY_TIME_ZONE } from "@/domain/family/time-zone";
import type { FamilyCurrency, GuardianGift } from "@/domain/savings/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import type { TranslationKey } from "@/locales/translation-keys";
import type { FamilyWorldInitialData } from "@/application/ports/family-world-initial-data";
import type { MemberProgression } from "@/application/contracts/family-progression-contract";
import type { HouseCatalogState } from "@/application/contracts/home-customization-contract";
export function normalizeFamilyMembers(members: FamilyMember[]): FamilyMember[] {
  return members.map((member) => ({
    ...member,
    appearance: resolveFamilyMemberAvatarAppearance(member, members),
  }));
}
export function normalizeFamilyPets(pets: ReadonlyArray<FamilyPet>): FamilyPet[] {
  return pets.map((pet) => ({ ...pet }));
}
function initialStateValue<T>(value: T): T {
  return value;
}
function createDefaultSelectedRoomDesignIds(): Partial<Record<HouseZoneId, RoomDesignId>> {
  const selections: Partial<Record<HouseZoneId, RoomDesignId>> = {};

  for (const design of DEFAULT_ROOM_DESIGNS) {
    selections[design.zoneId] = design.id;
  }

  return selections;
}
export function createFamilyWorldState(initialData: FamilyWorldInitialData) {
  const viewerRole = initialStateValue<ViewerRole>("child");
  const familyCurrencyCode = initialStateValue<FamilyCurrency>("CHF");
  const simulatedEnergy = initialStateValue<number | null>(null);
  const rewardAnimation: {
    visible: boolean;
    contributionId: ContributionId | undefined;
    value: number;
    energy: number;
    multiplier: number;
    stars: number;
    title: string;
    version: number;
  } = {
    visible: false,
    contributionId: undefined,
    value: 0,
    energy: 0,
    multiplier: 1,
    stars: 0,
    title: "",
    version: 0,
  };
  const guardianGiftAnimation: {
    visible: boolean;
    guardianName: string;
    goalTitle: string;
    destination: "balance" | "goal";
    amount: number;
    version: number;
  } = { visible: false, guardianName: "", goalTitle: "", destination: "goal", amount: 0, version: 0 };
  const pendingGuardianGifts: GuardianGift[] = [];
  const houseLevel = initialStateValue<HouseStageLevel>(0);
  const memberProgressions: Partial<Record<FamilyMemberId, MemberProgression>> = {};
  const houseCatalog = undefined as HouseCatalogState | undefined;
  const subscriptionTier = initialStateValue<SubscriptionTier>("pro");
  const snackbarParams: Record<string, number | string> = {};
  const snackbar: {
    visible: boolean;
    messageKey: TranslationKey | "";
    params: Record<string, number | string>;
  } = {
    visible: false,
    messageKey: "",
    params: snackbarParams,
  };

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
    availableBalances: { ...initialData.balances },
    sharedSavedBalances: {} as Partial<Record<FamilyMemberId, number>>,
    todayEarnedByMember: {} as Partial<Record<FamilyMemberId, number>>,
    familyCurrencyCode,
    familyTimeZone: DEFAULT_FAMILY_TIME_ZONE,
    ladirchenPerCurrencyUnit: 10,
    perfectRatingBonusPercent: 5,
    maxSavingsRatePercent: 8,
    memberProgressions,
    familyEnergyReadModel: 60,
    houseVisualLevel: 3,
    simulatedEnergy,
    piggyBankOpen: false,
    rewardAnimation,
    guardianGiftAnimation,
    pendingGuardianGifts,
    completedWeeklyStreak: 0,
    currentWeekTarget: 7,
    houseLevel,
    houseCatalog,
    houseThemeId: HOUSE_THEMES[0]?.id ?? "sunny-dollhouse",
    ownedHouseThemeIds: HOUSE_THEMES.filter((theme) => theme.ownedByDefault).map((theme) => theme.id),
    ownedRoomDesignIds: DEFAULT_ROOM_DESIGNS.map((design) => design.id),
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
    rewardShopRedemptionOpen: false,
  };
}
export type FamilyWorldState = ReturnType<typeof createFamilyWorldState>;
