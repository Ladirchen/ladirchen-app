using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record ContributionPresentationState(
  ContributionId ContributionId,
  string Description,
  string Icon,
  bool RewardCelebrated,
  string? Category = null,
  DateOnly? DueDate = null,
  string Recurrence = "none",
  bool IsFocus = false,
  string? FocusPeriod = null,
  DateOnly? FocusStartDate = null,
  DateOnly? FocusEndDate = null,
  decimal FocusCompletionBonus = 0,
  string? FocusId = null,
  int? PreparationMinutes = null,
  bool FocusBonusPaid = false
);
