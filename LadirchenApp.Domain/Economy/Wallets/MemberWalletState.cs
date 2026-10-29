using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record MemberWalletState(
  FamilyMemberId MemberId,
  decimal Balance,
  decimal Reserved
);
