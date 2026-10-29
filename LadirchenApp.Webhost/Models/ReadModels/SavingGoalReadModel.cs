using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SavingGoalReadModel(
  [property: JsonRequired] string Id,
  string? TranslationKey,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] string OwnerId,
  [property: JsonRequired] decimal Target,
  [property: JsonRequired] decimal Saved,
  decimal? StarterBonus,
  string? CreatedAt,
  decimal? InterestEarned,
  decimal? ProjectedInterestCredit,
  decimal? RemainingAmount,
  decimal? WithdrawableBalance,
  IReadOnlyList<string>? VisibleToMemberIds,
  IReadOnlyList<string>? ParticipantIds,
  string? VoucherId,
  SavingGoalCancellationRequestReadModel? CancellationRequest,
  IReadOnlyDictionary<string, SavingsGoalCommandLimitsReadModel>? CommandLimits,
  [property: JsonRequired] string Visibility,
  [property: JsonRequired] bool Shared,
  [property: JsonRequired] bool Cheered
);
