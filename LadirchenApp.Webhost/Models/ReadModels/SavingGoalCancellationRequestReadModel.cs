using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SavingGoalCancellationRequestReadModel(
  [property: JsonRequired] string RequestedByMemberId,
  [property: JsonRequired] IReadOnlyList<string> ConsentedMemberIds,
  [property: JsonRequired] IReadOnlyList<string> PendingMemberIds
);
