using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record LedgerEntry(TransactionId OperationId, FamilyMemberId MemberId, SavingGoalId? GoalId,
  LedgerKind Kind, decimal Amount, DomainIdentifier SourceId, string Reason, DateTimeOffset OccurredAtUtc,
  FamilyMemberId? InitiatorId = null)
{
  public Guid EntryId { get; init; } = Guid.NewGuid();
  public FamilyMemberId ActorId
  {
    get
    {
      return InitiatorId ?? MemberId;
    }
  }
}
