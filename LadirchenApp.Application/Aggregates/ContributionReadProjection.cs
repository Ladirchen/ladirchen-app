using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record ContributionReadProjection(
  decimal ProjectedReward,
  IReadOnlyList<FamilyMemberId> PartnerableMemberIds
);
