using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal sealed record SavingsVoucherReadProjection(
  IReadOnlyList<FamilyMemberId> VisibleToMemberIds,
  IReadOnlyList<FamilyMemberId> ConsentableByMemberIds,
  IReadOnlyList<FamilyMemberId> RedeemableByMemberIds
);
