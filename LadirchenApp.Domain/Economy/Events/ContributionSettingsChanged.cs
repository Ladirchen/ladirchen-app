using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionSettingsChanged(
  FamilyId FamilyId,
  Percentage PerfectRatingBonus,
  DateTimeOffset At
) : BusinessEvent(At.ToUniversalTime());
