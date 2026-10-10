using System;
using System.Collections.Generic;

namespace LadirchenApp.Prototypes;

public abstract class AggregateRoot<TId> : Entity<TId>
  where TId : notnull
{
  private readonly List<IDomainEvent> domainEvents = new();

  protected AggregateRoot(TId id)
    : base(id)
  {
    DomainEvents = domainEvents.AsReadOnly();
  }

  public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

  protected void AddDomainEvent(IDomainEvent domainEvent)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);
    domainEvents.Add(domainEvent);
  }

  public void ClearDomainEvents()
  {
    domainEvents.Clear();
  }
}
