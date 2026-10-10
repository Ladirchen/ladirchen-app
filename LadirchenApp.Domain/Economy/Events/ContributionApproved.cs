using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionApproved(FamilyId FamilyId, ContributionId ContributionId, FamilyMemberId ReviewerId,
  FamilyMemberId MemberId, StarRating Rating, ContributionRewardBreakdown Reward, DateTimeOffset At)
  : BusinessEvent(At.ToUniversalTime());