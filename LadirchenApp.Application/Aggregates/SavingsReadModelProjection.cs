using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal sealed record SavingsReadModelProjection(
  IReadOnlyDictionary<FamilyMemberId, decimal> Balances,
  IReadOnlyDictionary<FamilyMemberId, decimal> AvailableBalances,
  IReadOnlyDictionary<SavingGoalId, SavingsGoalReadProjection> Goals,
  IReadOnlyDictionary<SavingGoalId, SavingsVoucherReadProjection> Vouchers,
  IReadOnlyDictionary<FamilyMemberId, decimal> SharedSavedBalances,
  IReadOnlySet<FamilyMemberId> VisibleBalanceMemberIds
);
