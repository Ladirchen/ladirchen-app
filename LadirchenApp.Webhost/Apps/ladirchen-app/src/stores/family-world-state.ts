import { resolveFamilyMemberAvatarAppearance } from "@/domain/avatar";
import type {
  HouseAtmosphere,
  HouseEnergyVisualLevel,
  HouseStageLevel,
  HouseThemeId,
  HouseZoneId,
  RoomDesignId,
} from "@/domain/house";
import type { FamilyMember, FamilyPet, SubscriptionTier, ViewerRole } from "@/domain/family/types";
import { DEFAULT_FAMILY_TIME_ZONE } from "@/domain/family/time-zone";
import type { FamilyCurrency, GuardianGift, SavingsVoucher } from "@/domain/savings/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import type { TranslationKey } from "@/locales/translation-keys";
import type { FamilyWorldInitialData } from "@/application/ports/family-world-initial-data";
import type { MemberProgression } from "@/application/contracts/family-progression-contract";
import type { HouseCatalogState } from "@/application/contracts/home-customization-contract";
import type { AvatarProfileCatalog } from "@/application/contracts/family-profile-contract";
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
  const houseCatalog = initialStateValue<HouseCatalogState | undefined>(undefined);
  const subscriptionTier = initialStateValue<SubscriptionTier>("free");
  const sharedSavedBalances: Partial<Record<FamilyMemberId, number>> = {};
  const todayEarnedByMember: Partial<Record<FamilyMemberId, number>> = {};
  const currentWeekEarnedByMember: Partial<Record<FamilyMemberId, number>> = {};
  const currentWeekApprovedContributionCountByMember: Partial<Record<FamilyMemberId, number>> = {};
  const avatarCatalog = initialStateValue<Readonly<Record<ViewerRole, AvatarProfileCatalog>> | undefined>(undefined);
  const houseAtmosphere = initialStateValue<HouseAtmosphere>("cloudy");
  const houseVisualLevel = initialStateValue<HouseEnergyVisualLevel>(1);
  const savingsVouchers: SavingsVoucher[] = [];
  const houseThemeId = initialStateValue<HouseThemeId>("sunny-dollhouse");
  const ownedHouseThemeIds: HouseThemeId[] = [];
  const ownedRoomDesignIds: RoomDesignId[] = [];
  const selectedRoomDesignIds: Partial<Record<HouseZoneId, RoomDesignId>> = {};
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
    isAuthenticated: false,
    signedInMemberId: initialData.signedInMemberId,
    viewerRole,
    activeChildId: initialData.activeChildId,
    activeGoalId: initialData.activeGoalId,
    onboardingCompleted: false,
    familySetupOpen: false,
    balances: initialData.balances,
    availableBalances: { ...initialData.balances },
    sharedSavedBalances,
    todayEarnedByMember,
    currentWeekEarnedByMember,
    currentWeekApprovedContributionCountByMember,
    familyCurrencyCode,
    familyTimeZone: DEFAULT_FAMILY_TIME_ZONE,
    ladirchenPerCurrencyUnit: 1,
    perfectRatingBonusPercent: 0,
    maxSavingsRatePercent: 0,
    avatarCatalog,
    memberProgressions,
    familyEnergyReadModel: 0,
    canCloseWeekSuccessfully: false,
    minimumSuccessfulWeekEnergy: 0,
    houseAtmosphere,
    houseVisualLevel,
    simulatedEnergy,
    piggyBankOpen: false,
    rewardAnimation,
    guardianGiftAnimation,
    pendingGuardianGifts,
    savingsVouchers,
    completedWeeklyStreak: 0,
    currentWeekTarget: 0,
    houseLevel,
    houseCatalog,
    houseThemeId,
    ownedHouseThemeIds,
    ownedRoomDesignIds,
    selectedRoomDesignIds,
    canArrangeHouseReadModel: false,
    subscriptionTier,
    houseLayout: initialData.houseLayout,
    homeCustomizationHydrated: false,
    familyReadModelsHydrated: false,
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
