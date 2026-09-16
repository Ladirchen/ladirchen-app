import type { AvatarAppearance } from './avatar';
import type { FurniturePlacement, FurnitureVisualId, HexColor, HouseAccessoryId, HouseAccessoryPlacement } from './house';

declare const domainIdBrand: unique symbol;

type DomainId<Kind extends string> = string & { readonly [domainIdBrand]: Kind };

export type FamilyMemberId = DomainId<'family-member'>;
export type FamilyPetId = DomainId<'family-pet'>;
export type ContributionId = DomainId<'contribution'>;
export type SavingGoalId = DomainId<'saving-goal'>;
export type PromotionId = DomainId<'promotion'>;
export type ShopRewardId = DomainId<'shop-reward'>;
export type GuardianGiftId = DomainId<'guardian-gift'>;

const domainId = <Kind extends string>(value: string): DomainId<Kind> => value as DomainId<Kind>;

export const createDomainId = {
  contribution: (value: string): ContributionId => domainId<'contribution'>(value),
  familyMember: (value: string): FamilyMemberId => domainId<'family-member'>(value),
  familyPet: (value: string): FamilyPetId => domainId<'family-pet'>(value),
  guardianGift: (value: string): GuardianGiftId => domainId<'guardian-gift'>(value),
  promotion: (value: string): PromotionId => domainId<'promotion'>(value),
  savingGoal: (value: string): SavingGoalId => domainId<'saving-goal'>(value),
  shopReward: (value: string): ShopRewardId => domainId<'shop-reward'>(value),
} as const;

export type ViewerRole = 'child' | 'guardian';
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

export interface Contribution {
  id: ContributionId;
  title: string;
  description: string;
  icon: string;
  area: string;
  kind: ContributionKind;
  status: ContributionStatus;
  reward: number;
  energy: number;
  assigneeId?: FamilyMemberId;
  dueLabel: string;
  worldEffect?: WorldEffect;
  stars?: number;
  invitedChildIds?: FamilyMemberId[];
}

export interface SavingGoal {
  id: SavingGoalId;
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
  title: string;
  description: string;
  icon: string;
  price: number;
  placement: HouseAccessoryPlacement;
  visual?: FurnitureVisualId;
  scene?: FurniturePlacement;
  owned: boolean;
  equipped: boolean;
}

export interface Promotion {
  id: PromotionId;
  contributionId: ContributionId;
  title: string;
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
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  availableUntil?: string;
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
  availableUntil?: string;
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
