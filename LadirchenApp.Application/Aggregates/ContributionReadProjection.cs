using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal sealed record ContributionReadProjection(
  decimal ProjectedReward,
  IReadOnlyList<FamilyMemberId> PartnerableMemberIds
);