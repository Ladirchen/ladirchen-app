using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyProgressionReadModelState(
  [property: JsonRequired] bool CanCloseWeekSuccessfully,
  [property: JsonRequired] int CompletedWeeklyStreak,
  [property: JsonRequired] int CurrentWeekTarget,
  [property: JsonRequired] decimal FamilyEnergy,
  [property: JsonRequired] string HouseAtmosphere,
  [property: JsonRequired] int HouseLevel,
  [property: JsonRequired] int HouseVisualLevel,
  [property: JsonRequired] decimal MaximumSavingsInterestRate,
  [property: JsonRequired] decimal MinimumSuccessfulWeekEnergy,
  [property: JsonRequired] IReadOnlyDictionary<string, MemberProgressionReadModel> MemberProgressions
);
