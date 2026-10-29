using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record GuardianGoalGiftState(
  GuardianGiftId? Id,
  FamilyMemberId OwnerId,
  FamilyMemberId? GuardianId,
  decimal Amount,
  SavingGoalId? GoalId,
  GiftDestination Destination
);
