using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed class GuardianGoalGift
{
  public GuardianGoalGift(FamilyMemberId ownerId, FamilyMemberId guardianId, decimal amount)
    : this(null, ownerId, guardianId, amount, null, GiftDestination.Goal)
  {
  }

  public GuardianGoalGift(
    GuardianGiftId? id,
    FamilyMemberId ownerId,
    FamilyMemberId? guardianId,
    decimal amount,
    SavingGoalId? goalId,
    GiftDestination destination
  )
  {
    ArgumentNullException.ThrowIfNull(ownerId);
    Rules.Require(amount > 0, DomainError.InvalidValue, "A pending gift amount must be positive.");
    Id = id;
    OwnerId = ownerId;
    GuardianId = guardianId;
    Amount = amount;
    GoalId = goalId;
    Destination = Rules.Defined(destination);
  }

  public GuardianGiftId? Id { get; }
  public FamilyMemberId OwnerId { get; }
  public FamilyMemberId? GuardianId { get; }
  public decimal Amount { get; }
  public SavingGoalId? GoalId { get; }
  public GiftDestination Destination { get; }
}
