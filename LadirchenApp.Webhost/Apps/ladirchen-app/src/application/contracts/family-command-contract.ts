import type { AvatarAppearance } from "@/domain/avatar";
import type { NewContribution, NewPromotion } from "@/domain/contributions/types";
import type { FamilyMember, FamilyPet, GuardianAccessLevel } from "@/domain/family/types";
import type { FurnitureSetId, HouseAccessoryId, HouseThemeId, HouseZoneId, RoomDesignId } from "@/domain/house";
import type { FamilyCurrency, NewGoal, SavingGoalOwnerId } from "@/domain/savings/types";
import type {
  ContributionId,
  FamilyMemberId,
  GuardianGiftId,
  HouseLayoutPlacementId,
  PromotionId,
  SavingGoalId,
  ShopRewardId,
} from "@/domain/shared/identifiers";
import type { NewShopReward } from "@/domain/shop/types";

type EmptyFamilyCommandPayload = Readonly<Record<string, never>>;
type IdFamilyCommandPayload<TId> = Readonly<{ id: TId }>;
type PercentageFamilyCommandPayload = Readonly<{ percent: number }>;
type FamilySetupMemberPayload = Readonly<Omit<FamilyMember, "permissions" | "weeklyStreak">>;
type FamilySetupPetPayload = Readonly<FamilyPet>;

export interface FamilyCommandPayloadMap {
  readonly "contributions.acknowledge-reward": IdFamilyCommandPayload<ContributionId>;
  readonly "contributions.add": Readonly<{ id: ContributionId } & NewContribution>;
  readonly "contributions.add-promotion": Readonly<{ id: PromotionId } & NewPromotion>;
  readonly "contributions.approve": Readonly<{ id: ContributionId; stars: number }>;
  readonly "contributions.assign": Readonly<{ id: ContributionId; memberId?: FamilyMemberId }>;
  readonly "contributions.claim": IdFamilyCommandPayload<ContributionId>;
  readonly "contributions.delete": IdFamilyCommandPayload<ContributionId>;
  readonly "contributions.delete-promotion": IdFamilyCommandPayload<PromotionId>;
  readonly "contributions.return": IdFamilyCommandPayload<ContributionId>;
  readonly "contributions.set-partners": Readonly<{
    id: ContributionId;
    partnerIds: ReadonlyArray<FamilyMemberId>;
  }>;
  readonly "contributions.set-perfect-rating-bonus": PercentageFamilyCommandPayload;
  readonly "contributions.submit": IdFamilyCommandPayload<ContributionId>;
  readonly "home.move-entity": Readonly<{
    placementId: HouseLayoutPlacementId;
    zoneId: HouseZoneId;
    x: number;
    y: number;
  }>;
  readonly "home.place-accessory": Readonly<{ id: HouseAccessoryId; zoneId: HouseZoneId }>;
  readonly "home.purchase-accessory": IdFamilyCommandPayload<HouseAccessoryId>;
  readonly "home.purchase-furniture-set": IdFamilyCommandPayload<FurnitureSetId>;
  readonly "home.purchase-room-design": IdFamilyCommandPayload<RoomDesignId>;
  readonly "home.purchase-theme": IdFamilyCommandPayload<HouseThemeId>;
  readonly "home.reset-entity": Readonly<{ placementId: HouseLayoutPlacementId }>;
  readonly "home.select-room-design": IdFamilyCommandPayload<RoomDesignId>;
  readonly "home.select-theme": IdFamilyCommandPayload<HouseThemeId>;
  readonly "home.store-accessory": IdFamilyCommandPayload<HouseAccessoryId>;
  readonly "home.toggle-accessory": IdFamilyCommandPayload<HouseAccessoryId>;
  readonly "profile.complete-setup": Readonly<{
    members: ReadonlyArray<FamilySetupMemberPayload>;
    pets: ReadonlyArray<FamilySetupPetPayload>;
  }>;
  readonly "profile.invite-guardian": Readonly<{
    id: FamilyMemberId;
    name: string;
    email: string;
    guardianAccess: GuardianAccessLevel;
  }>;
  readonly "profile.save-own-appearance": Readonly<{ appearance: AvatarAppearance }>;
  readonly "profile.set-guardian-access": Readonly<{
    memberId: FamilyMemberId;
    guardianAccess: GuardianAccessLevel;
  }>;
  readonly "profile.set-own-nickname": Readonly<{ nickname: string }>;
  readonly "profile.set-weekly-participation": Readonly<{
    memberId: FamilyMemberId;
    participates: boolean;
  }>;
  readonly "progression.complete-week": EmptyFamilyCommandPayload;
  readonly "progression.fail-week": EmptyFamilyCommandPayload;
  readonly "progression.refresh": EmptyFamilyCommandPayload;
  readonly "progression.set-maximum-savings-rate": PercentageFamilyCommandPayload;
  readonly "reward-shop.add": Readonly<{ id: ShopRewardId } & NewShopReward>;
  readonly "reward-shop.cancel": IdFamilyCommandPayload<ShopRewardId>;
  readonly "reward-shop.decide": Readonly<{ id: ShopRewardId; approved: boolean }>;
  readonly "reward-shop.delete": IdFamilyCommandPayload<ShopRewardId>;
  readonly "reward-shop.request": IdFamilyCommandPayload<ShopRewardId>;
  readonly "reward-shop.set-visibility": Readonly<{ id: ShopRewardId; isVisible: boolean }>;
  readonly "savings.add-goal": Readonly<{ id: SavingGoalId; ownerId: SavingGoalOwnerId } & NewGoal>;
  readonly "savings.cancel-goal": IdFamilyCommandPayload<SavingGoalId>;
  readonly "savings.consent-voucher": IdFamilyCommandPayload<SavingGoalId>;
  readonly "savings.consume-guardian-gift": Readonly<{ giftId: GuardianGiftId }>;
  readonly "savings.decide-goal-cancellation": Readonly<{ id: SavingGoalId; approved: boolean }>;
  readonly "savings.deposit": Readonly<{ id: SavingGoalId; amount: number }>;
  readonly "savings.gift-to-goal": Readonly<{
    id: SavingGoalId;
    amount: number;
    giftId?: GuardianGiftId;
    senderName?: string;
  }>;
  readonly "savings.gift-to-member": Readonly<{
    recipientId: FamilyMemberId;
    amount: number;
    giftId: GuardianGiftId;
    senderName?: string;
  }>;
  readonly "savings.redeem-voucher": IdFamilyCommandPayload<SavingGoalId>;
  readonly "savings.request-goal-cancellation": IdFamilyCommandPayload<SavingGoalId>;
  readonly "savings.set-currency": Readonly<{ currency: FamilyCurrency }>;
  readonly "savings.set-exchange-rate": Readonly<{ rate: number }>;
  readonly "savings.settle-weekly-interest": Readonly<{ year: number; week: number }>;
  readonly "savings.special-gift": Readonly<{
    childId: FamilyMemberId;
    amount: number;
    giftId: GuardianGiftId;
    reason: string;
    senderName?: string;
  }>;
  readonly "savings.support-goal": Readonly<{
    id: SavingGoalId;
    amount: number;
    giftId?: GuardianGiftId;
    senderName?: string;
  }>;
  readonly "savings.toggle-cheer": IdFamilyCommandPayload<SavingGoalId>;
  readonly "savings.update-goal": Readonly<{ id: SavingGoalId } & NewGoal>;
  readonly "savings.withdraw": Readonly<{ id: SavingGoalId; amount: number }>;
}

export type FamilyCommandType = keyof FamilyCommandPayloadMap;
export type FamilyCommandPayload<TCommandType extends FamilyCommandType> = FamilyCommandPayloadMap[TCommandType];

export type FamilyCommand = {
  readonly [TCommandType in FamilyCommandType]: Readonly<{
    commandType: TCommandType;
    payload: FamilyCommandPayloadMap[TCommandType];
  }>;
}[FamilyCommandType];
