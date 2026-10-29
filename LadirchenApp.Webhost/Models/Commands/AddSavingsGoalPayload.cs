using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AddSavingsGoalPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string OwnerId,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] decimal Target,
  [property: JsonRequired] string Visibility,
  string? ParticipantScope,
  IReadOnlyList<string>? ParticipantIds
);
