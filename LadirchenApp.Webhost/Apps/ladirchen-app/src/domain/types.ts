import type { AvatarAppearance } from './avatar';
import type { FurniturePlacement, FurnitureSetId, FurnitureVisualId, HexColor, HouseAccessoryCategory, HouseAccessoryId, HouseAccessoryMotion, HouseAccessoryPlacement, HouseLayoutEntityType, HouseRoomId, HouseStageLevel, HouseZoneId } from './house';

declare const domainIdBrand: unique symbol;

type DomainId<Kind extends string> = string & { readonly [domainIdBrand]: Kind };

export type FamilyMemberId = DomainId<'family-member'>;
export type FamilyId = DomainId<'family'>;
export type FamilyPetId = DomainId<'family-pet'>;
export type ContributionId = DomainId<'contribution'>;
export type SavingGoalId = DomainId<'saving-goal'>;
export type PromotionId = DomainId<'promotion'>;
export type ShopRewardId = DomainId<'shop-reward'>;
export type GuardianGiftId = DomainId<'guardian-gift'>;
export type HouseLayoutPlacementId = DomainId<'house-layout-placement'>;

const domainIdPattern = /^\w[\w.:-]{0,127}$/u;
const uuidPattern = /^[\da-f]{8}-(?:[\da-f]{4}-){3}[\da-f]{12}$/iu;
export const isDomainIdValue = (value: unknown): value is string =>
  typeof value === 'string' && domainIdPattern.test(value);
export const isUuidValue = (value: unknown): value is string =>
  typeof value === 'string' && uuidPattern.test(value);

const domainId = <Kind extends string>(value: string): DomainId<Kind> => {
  if (!isDomainIdValue(value)) {throw new TypeError(`Invalid ${value ? 'format' : 'empty value'} for a domain ID.`);}
  return value as DomainId<Kind>;
};

export const createDomainId = {
  contribution: (value: string): ContributionId => domainId<'contribution'>(value),
  family: (value: string): FamilyId => {
    if (!isUuidValue(value)) {throw new TypeError('A family ID must be a UUID.');}
    return domainId<'family'>(value);
  },
  familyMember: (value: string): FamilyMemberId => domainId<'family-member'>(value),
  familyPet: (value: string): FamilyPetId => domainId<'family-pet'>(value),
  guardianGift: (value: string): GuardianGiftId => domainId<'guardian-gift'>(value),
  houseLayoutPlacement: (value: string): HouseLayoutPlacementId => domainId<'house-layout-placement'>(value),
  promotion: (value: string): PromotionId => domainId<'promotion'>(value),
  savingGoal: (value: string): SavingGoalId => domainId<'saving-goal'>(value),
  shopReward: (value: string): ShopRewardId => domainId<'shop-reward'>(value),
} as const;

export type ViewerRole = 'child' | 'guardian';
export type SubscriptionTier = 'free' | 'pro';
export type GuardianAccessLevel = 'admin' | 'supporter';
export type FamilyCurrency = 'CHF' | 'EUR' | 'HUF';
export type ContributionKind = 'basic' | 'extra';
export type ContributionStatus = 'available' | 'pending' | 'approved';
export type GoalVisibility = 'family' | 'guardians' | 'private';
export type WorldEffect = 'lights' | 'flowers' | 'garden' | 'sparkle' | 'smoke';
export type FamilyPetKindId = 'cat' | 'dog' | 'rabbit' | 'bird' | 'other';
export type SavingGoalOwnerId = FamilyMemberId | 'family';

export interface FamilyMember {
  id: FamilyMemberId;
  name: string;
  nickname?: string;
  avatar: string;
  color: HexColor;
  role: ViewerRole;
  guardianAccess?: GuardianAccessLevel;
  participatesInWeeklyGoal?: boolean;
  weeklyStreak: number;
  email?: string;
  invitationPending?: boolean;
  appearance?: AvatarAppearance;
}

export interface FamilyPet {
  id: FamilyPetId;
  name: string;
  kind: FamilyPetKindId;
  kindLabel: string;
  avatar: string;
  color: HexColor;
}

export interface GuardianGift {
  id: GuardianGiftId;
  childId: FamilyMemberId;
  guardianName: string;
  goalTitle: string;
  destination: 'balance' | 'goal';
  amount: number;
}

export interface Contribution {
  id: ContributionId;
  translationKey?: string;
  title: string;
  description: string;
  icon: string;
  area: string;
  areaKey?: string;
  kind: ContributionKind;
  status: ContributionStatus;
  reward: number;
  energy: number;
  assigneeId?: FamilyMemberId;
  dueLabel: string;
  dueLabelKey?: string;
  worldEffect?: WorldEffect;
  stars?: number;
  invitedChildIds?: FamilyMemberId[];
  earnedReward?: number;
  earnedRatingBonus?: number;
  earnedPromotionMultiplier?: number;
  approvedAt?: string;
  rewardCelebrated?: boolean;
}

export interface SavingGoal {
  id: SavingGoalId;
  translationKey?: string;
  title: string;
  icon: string;
  ownerId: SavingGoalOwnerId;
  target: number;
  saved: number;
  starterBonus?: number;
  interestEarned?: number;
  visibility: GoalVisibility;
  shared: boolean;
  cheered: boolean;
}

export interface HouseAccessory {
  id: HouseAccessoryId;
  translationKey?: string;
  category?: HouseAccessoryCategory;
  title: string;
  description: string;
  icon: string;
  price: number;
  placement: HouseAccessoryPlacement;
  roomId?: HouseRoomId;
  setIds?: ReadonlyArray<FurnitureSetId>;
  minimumHouseLevel?: HouseStageLevel;
  motion?: HouseAccessoryMotion;
  visual?: FurnitureVisualId;
  scene?: FurniturePlacement;
  owned: boolean;
  equipped: boolean;
}

interface HouseLayoutPlacementBase {
  id: HouseLayoutPlacementId;
  entityType: HouseLayoutEntityType;
  zoneId: HouseZoneId;
  x: number;
  y: number;
  scale: number;
}

export type HouseLayoutPlacement =
  | (HouseLayoutPlacementBase & { entityType: 'furniture'; entityId: HouseAccessoryId })
  | (HouseLayoutPlacementBase & { entityType: 'member'; entityId: FamilyMemberId })
  | (HouseLayoutPlacementBase & { entityType: 'pet'; entityId: FamilyPetId })
  | (HouseLayoutPlacementBase & { entityType: 'ladi'; entityId: 'family-ladi' });

export interface Promotion {
  id: PromotionId;
  translationKey?: string;
  contributionId: ContributionId;
  title: string;
  titleKey?: string;
  multiplier: number;
  deadline: string;
  teamworkBonus: number;
  active: boolean;
}

export interface NewPromotion {
  contributionId: ContributionId;
  multiplier: number;
  deadline: string;
  teamworkBonus: number;
}

export type ShopRewardStatus = 'available' | 'requested' | 'redeemed';
export type ShopRewardCategory = 'time' | 'activity' | 'allowance' | 'gift' | 'privilege' | 'custom';

export interface ShopReward {
  id: ShopRewardId;
  translationKey?: string;
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  availableFrom?: string;
  availableUntil?: string;
  isVisible?: boolean;
  status: ShopRewardStatus;
  requesterId?: FamilyMemberId;
}

export interface NewShopReward {
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  availableFrom?: string;
  availableUntil?: string;
  isVisible?: boolean;
}

export interface NewContribution {
  title: string;
  description: string;
  icon: string;
  reward: number;
  energy: number;
  kind: ContributionKind;
  assigneeId?: FamilyMemberId;
}

export interface NewGoal {
  title: string;
  icon: string;
  target: number;
  visibility: GoalVisibility;
}
