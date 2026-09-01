import type { AvatarAppearance } from './avatar';
import type { FurniturePlacement, FurnitureVisual } from './house';

export type ViewerRole = 'child' | 'guardian';
export type FamilyCurrency = 'CHF' | 'EUR' | 'HUF';
export type ContributionKind = 'basic' | 'extra';
export type ContributionStatus = 'available' | 'pending' | 'approved';
export type GoalVisibility = 'family' | 'guardians' | 'private';
export type WorldEffect = 'lights' | 'flowers' | 'garden' | 'sparkle' | 'smoke';

export interface FamilyMember {
  id: string;
  name: string;
  nickname?: string;
  avatar: string;
  color: string;
  role: ViewerRole;
  weeklyStreak: number;
  email?: string;
  invitationPending?: boolean;
  appearance?: AvatarAppearance;
}

export interface FamilyPet {
  id: string;
  name: string;
  kind: string;
  avatar: string;
  color: string;
}

export interface Contribution {
  id: string;
  title: string;
  description: string;
  icon: string;
  area: string;
  kind: ContributionKind;
  status: ContributionStatus;
  reward: number;
  energy: number;
  assigneeId?: string;
  dueLabel: string;
  worldEffect?: WorldEffect;
  stars?: number;
  invitedChildIds?: string[];
}

export interface SavingGoal {
  id: string;
  title: string;
  icon: string;
  ownerId: string;
  target: number;
  saved: number;
  starterBonus?: number;
  interestEarned?: number;
  visibility: GoalVisibility;
  shared: boolean;
  cheered: boolean;
}

export interface HouseAccessory {
  id: string;
  title: string;
  description: string;
  icon: string;
  price: number;
  placement: 'inside' | 'outside';
  visual?: FurnitureVisual;
  scene?: FurniturePlacement;
  owned: boolean;
  equipped: boolean;
}

export interface Promotion {
  id: string;
  contributionId: string;
  title: string;
  multiplier: number;
  deadline: string;
  teamworkBonus: number;
  active: boolean;
}

export interface NewPromotion {
  contributionId: string;
  multiplier: number;
  deadline: string;
  teamworkBonus: number;
}

export type ShopRewardStatus = 'available' | 'requested' | 'redeemed';
export type ShopRewardCategory = 'time' | 'activity' | 'allowance' | 'gift' | 'privilege' | 'custom';

export interface ShopReward {
  id: string;
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  availableUntil?: string;
  status: ShopRewardStatus;
  requesterId?: string;
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
  assigneeId?: string;
}

export interface NewGoal {
  title: string;
  icon: string;
  target: number;
  visibility: GoalVisibility;
}
