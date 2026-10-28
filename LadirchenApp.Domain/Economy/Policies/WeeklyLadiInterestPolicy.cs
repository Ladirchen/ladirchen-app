using System;

using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed class WeeklyLadiInterestPolicy
{
  public Percentage RateFor(PersonalLadiEvaluation evaluation, Percentage maximumRate)
  {
    ArgumentNullException.ThrowIfNull(evaluation);

    return SavingsInterestPolicy.CalculateRate(evaluation.Stage, maximumRate);
  }
}
