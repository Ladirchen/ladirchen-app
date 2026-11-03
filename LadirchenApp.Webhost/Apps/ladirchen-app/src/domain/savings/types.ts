import type { SavingGoalTranslationNamespaceKey } from "@/locales/translation-keys";
import type { BehaviorIncidentId, FamilyMemberId, GuardianGiftId, SavingGoalId } from "@/domain/shared/identifiers";

export type FamilyCurrency = "CHF" | "EUR" | "HUF";
export type GoalVisibility = "family" | "guardians" | "private";
export type SavingGoalOwnerId = FamilyMemberId | "family";
export type FamilyGoalParticipantScope = "family" | "children" | "selected";
export type SavingsVoucherStatus = "pending" | "redeemable" | "redeemed";
export type BehaviorLightLevel = "green" | "yellow" | "orange" | "red";

export interface ChildBehaviorStatus {
  childId: FamilyMemberId;
  level: BehaviorLightLevel;
  warningCount: number;
  tasksBlocked: boolean;
  lastReason?: string;
  updatedAtUtc?: string;
}

export interface BehaviorNotice {
  id: BehaviorIncidentId;
  childId: FamilyMemberId;
  guardianId: FamilyMemberId;
  level: BehaviorLightLevel;
  deductedAmount: number;
  reason: string;
  occurredAtUtc: string;
}

export interface GuardianGift {
  id: GuardianGiftId;
  childId: FamilyMemberId;
  guardianName: string;
  goalTitle: string;
  destination: "balance" | "goal";
  amount: number;
}

export interface SavingGoalCancellationRequest {
  requestedByMemberId: FamilyMemberId;
  consentedMemberIds: ReadonlyArray<FamilyMemberId>;
  pendingMemberIds: ReadonlyArray<FamilyMemberId>;
}

export interface SavingGoalCommandLimits {
  canApproveCancellation: boolean;
  canCancel: boolean;
  canRejectCancellation: boolean;
  canRequestCancellation: boolean;
  canWithdraw: boolean;
  maximumDepositAmount: number;
  maximumSupportAmount: number;
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
  createdAt?: string;
  interestEarned?: number;
  projectedInterestCredit?: number;
  remainingAmount?: number;
  withdrawableBalance?: number;
  visibleToMemberIds?: ReadonlyArray<FamilyMemberId>;
  participantIds?: ReadonlyArray<FamilyMemberId>;
  voucherId?: SavingGoalId;
  cancellationRequest?: SavingGoalCancellationRequest;
  commandLimits?: Readonly<Partial<Record<FamilyMemberId, SavingGoalCommandLimits>>>;
  visibility: GoalVisibility;
  shared: boolean;
  cheered: boolean;
}

export interface NewGoal {
  title: string;
  icon: string;
  target: number;
  visibility: GoalVisibility;
  participantScope?: FamilyGoalParticipantScope;
  participantIds?: ReadonlyArray<FamilyMemberId>;
}

export interface SavingsVoucher {
  id: SavingGoalId;
  goalId: SavingGoalId;
  amount: number;
  ownerId?: FamilyMemberId | null;
  participantIds: ReadonlyArray<FamilyMemberId>;
  consentedMemberIds: ReadonlyArray<FamilyMemberId>;
  status: SavingsVoucherStatus;
  redeemedAt?: string;
  visibleToMemberIds: ReadonlyArray<FamilyMemberId>;
  consentableByMemberIds: ReadonlyArray<FamilyMemberId>;
  redeemableByMemberIds: ReadonlyArray<FamilyMemberId>;
}
