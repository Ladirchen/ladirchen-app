using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record RefundableContributionState(
  FamilyMemberId MemberId,
  decimal Amount
);
