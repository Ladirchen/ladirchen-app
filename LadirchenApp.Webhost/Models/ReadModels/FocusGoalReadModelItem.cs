using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FocusGoalReadModelItem(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] string Period,
  [property: JsonRequired] string StartDate,
  [property: JsonRequired] string EndDate,
  [property: JsonRequired] string AssigneeId,
  [property: JsonRequired] decimal CompletionBonus,
  [property: JsonRequired] bool BonusPaid,
  [property: JsonRequired] int ProgressPercent,
  [property: JsonRequired] int RemainingPercent,
  [property: JsonRequired] IReadOnlyList<string> PreparationContributionIds
);
