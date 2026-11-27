using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record FocusAchievementPresentationState(
  ContributionId FocusId,
  FamilyMemberId ChildId,
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
