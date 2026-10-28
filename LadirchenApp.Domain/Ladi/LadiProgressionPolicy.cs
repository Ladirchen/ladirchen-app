using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Ladi;

public sealed class LadiProgressionPolicy
{
  public const decimal DefaultMaximumParticipation = 8m;

  public ParticipationValue Calculate(PersonalParticipation participation, decimal maximum = DefaultMaximumParticipation)
  {
    ArgumentNullException.ThrowIfNull(participation);
    Rules.Require(maximum > 0, DomainError.InvalidValue, "Maximum participation must be positive.");
    var completionRatio = participation.EffectiveCompletionPercent / 100m;
    var ratingRatio = participation.AverageBasicRating / 5m;
    var uncapped =
      1m + participation.StreakDays * 0.5m + completionRatio * 3.5m + ratingRatio * 2.5m;

    return new ParticipationValue(uncapped, Math.Min(uncapped, maximum));
  }

  public decimal CalculateEffectiveCompletion(decimal assignedBasicEnergy, decimal approvedBasicEnergy, decimal approvedExtraEnergy)
  {
    Rules.Require(assignedBasicEnergy > 0, DomainError.InvalidValue, "Assigned basic energy must be positive.");
    Rules.Require(
      approvedBasicEnergy >= 0 && approvedExtraEnergy >= 0,
      DomainError.InvalidValue,
      "Approved energy cannot be negative."
    );
    var completion = (approvedBasicEnergy + approvedExtraEnergy) / assignedBasicEnergy * 100m;

    return Math.Min(completion, 100m);
  }

  public LadiStage DetermineStage(decimal cappedParticipation, decimal maximum = DefaultMaximumParticipation)
  {
    Rules.Require(maximum > 0, DomainError.InvalidValue, "Maximum participation must be positive.");
    Rules.Require(cappedParticipation >= 0, DomainError.InvalidValue, "Participation cannot be negative.");
    var bounded = Math.Min(cappedParticipation, maximum);
    var bandWidth = maximum / LadiStage.Count;
    var position = Math.Max(1, Math.Min(LadiStage.Count, (int)Math.Ceiling(bounded / bandWidth)));

    return LadiStage.AtPosition(position);
  }
}
