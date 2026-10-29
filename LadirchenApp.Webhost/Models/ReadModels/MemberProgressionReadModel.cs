using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record MemberProgressionReadModel(
  [property: JsonRequired] IReadOnlyList<string> ApprovedDates,
  [property: JsonRequired] decimal AverageBasicRating,
  [property: JsonRequired] decimal CompletionPercent,
  [property: JsonRequired] decimal EnergyContribution,
  [property: JsonRequired] string LadiStageId,
  [property: JsonRequired] int LadiStagePosition,
  [property: JsonRequired] decimal ParticipationValue,
  [property: JsonRequired] decimal SavingsInterestRate,
  [property: JsonRequired] int StreakDays
);
