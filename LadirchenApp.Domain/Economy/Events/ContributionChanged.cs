using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionChanged(FamilyId FamilyId, ContributionId ContributionId, string Change, DateTimeOffset At)
  : BusinessEvent(At.ToUniversalTime());