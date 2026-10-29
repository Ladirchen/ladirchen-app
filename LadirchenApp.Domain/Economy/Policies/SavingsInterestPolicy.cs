using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Domain.Ladi;

namespace LadirchenApp.Domain.Economy;

public sealed class SavingsInterestPolicy
{
  public SavingsInterestPolicy(Percentage maximumRate)
  {
    ArgumentNullException.ThrowIfNull(maximumRate);
    Rules.Require(
      maximumRate.Value > 0,
      DomainError.InvalidValue,
      "The maximum savings interest rate must be above zero."
    );
    MaximumRate = maximumRate;
  }

  public static SavingsInterestPolicy Default { get; } = new(new(8));
  public Percentage MaximumRate
  {
    get;
  }

  public Percentage RateFor(PersonalLadiEvaluation evaluation)
  {
    ArgumentNullException.ThrowIfNull(evaluation);

    return CalculateRate(evaluation.Stage, MaximumRate);
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
