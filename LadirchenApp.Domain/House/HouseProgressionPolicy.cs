using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

public sealed class HouseProgressionPolicy
{
  private static readonly string[] Stages = ["Starter Home", "Family Home", "Garden Home", "Tower Home", "Dream Home"];
  private static readonly int[] RoomCounts = [2, 4, 7, 10, 12];
  private static readonly IReadOnlyDictionary<string, int> RoomMinimumLevels = new Dictionary<string, int>
  {
    ["Living Room"] = 0,
    ["Kitchen"] = 0,
    ["Bathroom"] = 1,
    ["Children Room"] = 1,
    ["Play Room"] = 2,
    ["Nursery"] = 2,
    ["Pet Room"] = 2,
    ["Bedroom"] = 3,
    ["Wardrobe Room"] = 3,
    ["Gamer Room"] = 3,
    ["Creative Room"] = 4,
    ["Makeup Room"] = 4,
  };

  public int DisplayedLevel(int storedLevel)
  {
    ValidateLevel(storedLevel);

    return storedLevel + 1;
  }

  public string Stage(int storedLevel)
  {
    ValidateLevel(storedLevel);

    return Stages[storedLevel];
  }

  public int RoomCount(int storedLevel)
  {
    ValidateLevel(storedLevel);

    return RoomCounts[storedLevel];
  }

  public bool IsRoomAvailable(string room, int storedLevel)
  {
    ValidateLevel(storedLevel);

    return RoomMinimumLevels.TryGetValue(room, out var requiredLevel) && storedLevel >= requiredLevel;
  }

  public bool IsContentAvailable(int storedLevel, int requiredLevel)
  {
    ValidateLevel(storedLevel);

    return storedLevel >= requiredLevel;
  }

  public bool IsExtensionVisible(string extension, int storedLevel)
  {
    ValidateLevel(storedLevel);
    var requiredLevel = extension switch
    {
      "Upper Floor" => 1,
      "Side Extension" => 2,
      "Tower" => 3,
      "Front Balcony" => 4,
      _ => int.MaxValue,
    };

    return storedLevel >= requiredLevel;
  }

  private static void ValidateLevel(int level)
  {
    Rules.Require(level >= 0 && level < Stages.Length, DomainError.InvalidValue, "Stored house level must be between zero and four.");
  }
}
