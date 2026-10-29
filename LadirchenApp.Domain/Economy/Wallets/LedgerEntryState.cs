using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record LedgerEntryState(
  Guid EntryId,
  TransactionId OperationId,
  FamilyMemberId MemberId,
  SavingGoalId? GoalId,
  LedgerKind Kind,
  decimal Amount,
  LedgerSourceState Source,
  string Reason,
  DateTimeOffset OccurredAtUtc,
  FamilyMemberId? InitiatorId
);
