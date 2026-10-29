using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record FamilyAggregateChange(
  FamilyId FamilyId,
  string AggregateType,
  long Revision,
  FamilyMemberId UpdatedBy,
  DateTimeOffset UpdatedAt
);
