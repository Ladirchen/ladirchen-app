using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionSubmitted(FamilyId FamilyId, ContributionId ContributionId, FamilyMemberId MemberId,
  DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());