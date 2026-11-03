using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record ChildBehaviorStatus(
  FamilyMemberId ChildId,
  BehaviorLightLevel Level,
  int WarningCount,
  bool TasksBlocked,
  string? LastReason,
  DateTimeOffset? UpdatedAtUtc
);
