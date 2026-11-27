using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FocusAchievementReadModelItem(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] string StartDate,
  [property: JsonRequired] string EndDate,
  [property: JsonRequired] string AchievedAt,
  [property: JsonRequired] int PreparationCount,
  [property: JsonRequired] decimal EarnedLadirchen,
  [property: JsonRequired] decimal EarnedEnergy
);
