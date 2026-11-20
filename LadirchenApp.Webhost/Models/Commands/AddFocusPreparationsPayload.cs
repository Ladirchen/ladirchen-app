using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AddFocusPreparationsPayload(
  [property: JsonRequired] string FocusId,
  [property: JsonRequired] IReadOnlyList<string> Ids,
  [property: JsonRequired] IReadOnlyList<string> Dates,
  [property: JsonRequired] int Minutes,
  [property: JsonRequired] decimal Reward
);
