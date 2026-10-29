using System;

using LadirchenApp.Domain.Economy;

namespace LadirchenApp.Application.Persistence;

public sealed record LoadedFamilyEconomy
{
  public LoadedFamilyEconomy(
    FamilyEconomy root,
    FamilyEconomyPresentationState presentation,
    long revision,
    AggregateAudit audit
  )
  {
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    Root = root ?? throw new ArgumentNullException(nameof(root));
    Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
    Revision = revision;
    Audit = audit ?? throw new ArgumentNullException(nameof(audit));
  }

  public FamilyEconomy Root { get; }
  public FamilyEconomyPresentationState Presentation { get; }
  public long Revision { get; }
  public AggregateAudit Audit { get; }
}
