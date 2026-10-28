using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Ladi;

public sealed record LadiStage
{
  private static readonly string[] StageIds =
  [
    "idle-ladi",
    "starter-ladi",
    "garden-ladi",
    "sun-ladi",
    "aurora-ladi",
    "super-ladi",
  ];

  public LadiStage(string id, int position)
  {
    Rules.Require(position >= 1 && position <= StageIds.Length, DomainError.InvalidValue, "Invalid Ladi stage position.");
    Rules.Require(StageIds[position - 1] == id, DomainError.InvalidValue, "The Ladi stage identifier and position do not match.");
    Id = id;
    Position = position;
  }

  public string Id { get; }
  public int Position { get; }
  public static int Count
  {
    get
    {
      return StageIds.Length;
    }
  }

  public static LadiStage AtPosition(int position)
  {
    Rules.Require(position >= 1 && position <= StageIds.Length, DomainError.InvalidValue, "Invalid Ladi stage position.");

    return new LadiStage(StageIds[position - 1], position);
  }

  public static LadiStage FromId(string id)
  {
    var position = Array.IndexOf(StageIds, id) + 1;
    Rules.Require(position > 0, DomainError.InvalidValue, "Unknown Ladi stage.");

    return AtPosition(position);
  }
}
