using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record LadirchenTransferred(FamilyId FamilyId, FamilyMemberId SenderId, FamilyMemberId? RecipientId,
  SavingGoalId? GoalId, decimal Amount, TransactionId OperationId, DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());