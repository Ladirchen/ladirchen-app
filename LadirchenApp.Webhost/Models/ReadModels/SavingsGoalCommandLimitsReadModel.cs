using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SavingsGoalCommandLimitsReadModel(
  [property: JsonRequired] bool CanApproveCancellation,
  [property: JsonRequired] bool CanCancel,
  [property: JsonRequired] bool CanRejectCancellation,
  [property: JsonRequired] bool CanRequestCancellation,
  [property: JsonRequired] bool CanWithdraw,
  [property: JsonRequired] decimal MaximumDepositAmount,
  [property: JsonRequired] decimal MaximumSupportAmount
);
