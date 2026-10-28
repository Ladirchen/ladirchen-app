using System.Text.Json;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record SaveFamilyAggregateCommand(
  FamilyId FamilyId,
  string AggregateType,
  long ExpectedRevision,
  JsonElement State,
  FamilyMemberId UpdatedBy
);
