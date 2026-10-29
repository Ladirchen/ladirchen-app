namespace LadirchenApp.Webhost.Models;

public sealed record SimulateSavingsInterestRequest(
  string MemberId,
  int StreakDays,
  decimal CompletionPercent,
  decimal AverageRating
);
