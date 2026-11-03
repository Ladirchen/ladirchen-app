using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record PendingBehaviorNotice(
  BehaviorIncidentId Id,
  FamilyMemberId ChildId,
  FamilyMemberId GuardianId,
  BehaviorLightLevel Level,
  decimal DeductedAmount,
  string Reason,
  DateTimeOffset OccurredAtUtc
);
