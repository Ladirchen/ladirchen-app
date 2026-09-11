import type { SavingGoalTranslationNamespaceKey } from '@/locales/translation-keys';
import type { FamilyMemberId, GuardianGiftId, SavingGoalId } from '../shared/identifiers';

export type FamilyCurrency = 'CHF' | 'EUR' | 'HUF';
export type GoalVisibility = 'family' | 'guardians' | 'private';
export type SavingGoalOwnerId = FamilyMemberId | 'family';

export interface GuardianGift {
  id: GuardianGiftId;
  childId: FamilyMemberId;
  guardianName: string;
  goalTitle: string;
  destination: 'balance' | 'goal';
  amount: number;
}

export interface SavingGoal {
  id: SavingGoalId;
  translationKey?: SavingGoalTranslationNamespaceKey;
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

export interface NewGoal {
  title: string;
  icon: string;
  target: number;
  visibility: GoalVisibility;
}
