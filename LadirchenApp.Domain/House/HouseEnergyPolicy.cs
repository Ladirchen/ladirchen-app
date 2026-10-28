using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Ladi;

namespace LadirchenApp.Domain.House;

public sealed class HouseEnergyPolicy
{
  public const int DefaultEnergy = 60;

  public int ContributionFor(LadiStage stage)
  {
    ArgumentNullException.ThrowIfNull(stage);

    return (stage.Position - 1) * 20;
  }

  public int Calculate(IEnumerable<LadiStage> participantStages)
  {
    ArgumentNullException.ThrowIfNull(participantStages);
    var contributions = participantStages.Select(ContributionFor).ToArray();

    if (contributions.Length == 0)
    {
      return DefaultEnergy;
    }

    return (int)Math.Round(contributions.Average(), MidpointRounding.AwayFromZero);
  }

  public int VisualLevel(int energy)
  {
    return energy switch
    {
      < 0 => throw new ArgumentOutOfRangeException(nameof(energy)),
      < 20 => 1,
      < 40 => 2,
      < 65 => 3,
      < 85 => 4,
      <= 100 => 5,
      _ => throw new ArgumentOutOfRangeException(nameof(energy)),
    };
  }

  public string Atmosphere(int energy)
  {
    return energy switch
    {
      < 0 => throw new ArgumentOutOfRangeException(nameof(energy)),
      < 40 => "rainy",
      < 70 => "cloudy",
      <= 100 => "sunny",
      _ => throw new ArgumentOutOfRangeException(nameof(energy)),
    };
  }
}
