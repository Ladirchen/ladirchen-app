namespace LadirchenApp.Application.Aggregates;

public sealed record SavingsGoalCommandLimits(
  bool CanCancel,
  bool CanRequestCancellation,
  bool CanApproveCancellation,
  bool CanRejectCancellation,
  bool CanWithdraw,
  decimal MaximumDepositAmount,
  decimal MaximumSupportAmount
);
