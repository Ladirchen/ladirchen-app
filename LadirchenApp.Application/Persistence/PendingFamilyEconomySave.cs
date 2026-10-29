using System;

using LadirchenApp.Domain.Economy;

namespace LadirchenApp.Application.Persistence;

public sealed record PendingFamilyEconomySave
{
  public PendingFamilyEconomySave(
    FamilyEconomy root,
    FamilyEconomyPresentationState presentation,
    long expectedRevision
  )
  {
    ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
    Root = root ?? throw new ArgumentNullException(nameof(root));
    Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
    ExpectedRevision = expectedRevision;
  }

  public FamilyEconomy Root { get; }
  public FamilyEconomyPresentationState Presentation { get; }
  public long ExpectedRevision { get; }
}
