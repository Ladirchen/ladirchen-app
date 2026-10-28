using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Ladi;

public sealed record PersonalParticipation
{
  public PersonalParticipation(int streakDays, decimal effectiveCompletionPercent, decimal averageBasicRating)
  {
    Rules.Require(streakDays >= 0, DomainError.InvalidValue, "Streak days cannot be negative.");
    Rules.Require(
      effectiveCompletionPercent >= 0 && effectiveCompletionPercent <= 100,
      DomainError.InvalidValue,
      "Effective completion must be between zero and one hundred percent."
    );
    Rules.Require(
      averageBasicRating >= 0 && averageBasicRating <= 5,
      DomainError.InvalidValue,
      "Average rating must be between zero and five stars."
    );
    StreakDays = streakDays;
    EffectiveCompletionPercent = effectiveCompletionPercent;
    AverageBasicRating = averageBasicRating;
  }

  public int StreakDays { get; }
  public decimal EffectiveCompletionPercent { get; }
  public decimal AverageBasicRating { get; }
}
