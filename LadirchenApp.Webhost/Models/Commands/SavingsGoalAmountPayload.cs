using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsGoalAmountPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] decimal Amount
);
