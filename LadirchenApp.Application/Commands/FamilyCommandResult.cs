using System;
using System.Collections.Generic;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Application.Commands;

public sealed record FamilyCommandResult(
  IReadOnlyList<FamilyAggregateChange> ChangedAggregates,
  IReadOnlyList<FamilyUserNotification> UserNotifications
)
{
  public FamilyCommandResult(IReadOnlyList<FamilyAggregateChange> changedAggregates)
    : this(changedAggregates, Array.Empty<FamilyUserNotification>()) { }
}