using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SavingsVoucherReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string GoalId,
  [property: JsonRequired] decimal Amount,
  [property: JsonRequired, JsonIgnore(Condition = JsonIgnoreCondition.Never)] string? OwnerId,
  [property: JsonRequired] IReadOnlyList<string> ParticipantIds,
  [property: JsonRequired] IReadOnlyList<string> ConsentedMemberIds,
  [property: JsonRequired] string Status,
  string? RedeemedAt,
  [property: JsonRequired] IReadOnlyList<string> VisibleToMemberIds,
  [property: JsonRequired] IReadOnlyList<string> ConsentableByMemberIds,
  [property: JsonRequired] IReadOnlyList<string> RedeemableByMemberIds
);
