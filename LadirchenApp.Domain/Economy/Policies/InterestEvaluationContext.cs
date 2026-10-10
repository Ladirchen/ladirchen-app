using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record InterestEvaluationContext
{
  public InterestEvaluationContext(int streakDays, Percentage completion, decimal averageRating)
  {
    ArgumentNullException.ThrowIfNull(completion);
    ArgumentOutOfRangeException.ThrowIfNegative(streakDays);
    ArgumentOutOfRangeException.ThrowIfNegative(averageRating);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(averageRating, 5);
    StreakDays = streakDays;
    Completion = completion;
    AverageRating = averageRating;
  }
  public int StreakDays
  {
    get;
  }
  public Percentage Completion
  {
    get;
  }
  public decimal AverageRating
  {
    get;
  }
}