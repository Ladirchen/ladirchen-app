using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record GuardianGiftReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] string GuardianName,
  [property: JsonRequired] string GoalTitle,
  [property: JsonRequired] string Destination,
  [property: JsonRequired] decimal Amount
);
