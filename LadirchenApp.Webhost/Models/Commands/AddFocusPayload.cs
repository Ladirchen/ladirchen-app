using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AddFocusPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] string Period,
  [property: JsonRequired] string StartDate,
  [property: JsonRequired] string EndDate,
  [property: JsonRequired] string AssigneeId,
  [property: JsonRequired] decimal CompletionBonus
);
