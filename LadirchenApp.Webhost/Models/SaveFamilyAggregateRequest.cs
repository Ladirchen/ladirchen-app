using System.Text.Json;

namespace LadirchenApp.Webhost.Models;

public sealed record SaveFamilyAggregateRequest(
  string AggregateType,
  long ExpectedRevision,
  JsonElement State,
  string UpdatedBy
);
