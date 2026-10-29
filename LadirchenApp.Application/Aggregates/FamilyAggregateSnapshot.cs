using System;
using System.Text.Json;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record FamilyAggregateSnapshot(
  FamilyId FamilyId,
  string AggregateType,
  int SchemaVersion,
  long Revision,
  JsonElement State,
  FamilyMemberId UpdatedBy,
  DateTimeOffset UpdatedAt
);
