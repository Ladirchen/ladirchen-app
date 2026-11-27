using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal sealed record FocusAchievementPersistenceState(
  string FocusId,
  string ChildId,
  string Title,
  string Description,
  string Icon,
  DateOnly StartDate,
  DateOnly EndDate,
  DateTimeOffset AchievedAt,
  int PreparationCount,
  decimal EarnedLadirchen,
  decimal EarnedEnergy
);
