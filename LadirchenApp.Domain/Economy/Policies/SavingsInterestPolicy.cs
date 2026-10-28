using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Domain.Ladi;

namespace LadirchenApp.Domain.Economy;

public sealed class SavingsInterestPolicy
{
  public SavingsInterestPolicy(Percentage baseRate, Percentage streakBonus, Percentage completionBonus,
    Percentage ratingBonus, Percentage maximumRate)
  {
    ArgumentNullException.ThrowIfNull(baseRate);
    ArgumentNullException.ThrowIfNull(streakBonus);
    ArgumentNullException.ThrowIfNull(completionBonus);
    ArgumentNullException.ThrowIfNull(ratingBonus);
    ArgumentNullException.ThrowIfNull(maximumRate);
    BaseRate = baseRate;
    StreakBonus = streakBonus;
    CompletionBonus = completionBonus;
    RatingBonus = ratingBonus;
    MaximumRate = maximumRate;
  }

  public static SavingsInterestPolicy Default { get; } = new(new(1), new(0.5m), new(3.5m), new(2.5m), new(8));
  public Percentage BaseRate
  {
    get;
  }
  public Percentage StreakBonus
  {
    get;
  }
  public Percentage CompletionBonus
  {
    get;
  }
  public Percentage RatingBonus
  {
    get;
  }
  public Percentage MaximumRate
  {
    get;
  }

  public Percentage CalculateRate(InterestEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    var rate = BaseRate.Value + context.StreakDays * StreakBonus.Value
      + context.Completion.Value / 100 * CompletionBonus.Value + context.AverageRating / 5 * RatingBonus.Value;

    return new(decimal.Round(Math.Min(MaximumRate.Value, rate), 2, MidpointRounding.AwayFromZero));
  }

  public static Percentage CalculateRate(LadiStage stage, Percentage maximumRate)
  {
    ArgumentNullException.ThrowIfNull(stage);
    ArgumentNullException.ThrowIfNull(maximumRate);
    var rate = maximumRate.Value * stage.Position / LadiStage.Count;

    return new Percentage(decimal.Round(rate, 2, MidpointRounding.AwayFromZero));
  }

  public static decimal CalculateCredit(LadirchenAmount saved, LadirchenAmount target, Percentage rate)
  {
    ArgumentNullException.ThrowIfNull(saved);
    ArgumentNullException.ThrowIfNull(target);
    ArgumentNullException.ThrowIfNull(rate);

    if (saved.Value <= 0 || saved.Value >= target.Value)
    {

      return 0;
    }

    return Math.Min(target.Value - saved.Value, Math.Max(1, Rules.Round(saved.Value * (rate.Value / 100))));
  }
}
