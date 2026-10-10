using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionReturned(FamilyId FamilyId, ContributionId ContributionId, DateTimeOffset At)
  : BusinessEvent(At.ToUniversalTime());