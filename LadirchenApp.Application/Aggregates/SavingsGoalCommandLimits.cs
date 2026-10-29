namespace LadirchenApp.Application.Aggregates;

internal sealed record SavingsGoalCommandLimits(
  bool CanCancel,
  bool CanWithdraw,
  decimal MaximumDepositAmount,
  decimal MaximumSupportAmount
);
