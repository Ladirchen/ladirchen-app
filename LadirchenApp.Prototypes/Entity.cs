using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LadirchenApp.Prototypes;

[SuppressMessage(
  "Major Code Smell",
  "S4035",
  Justification = "This abstract DDD base requires inheritance; equality compares exact runtime types."
)]
public abstract class Entity<TId> : IEquatable<Entity<TId>>
  where TId : notnull
{
  protected Entity(TId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    Id = id;
  }

  public TId Id
  {
    get;
  }

  public bool Equals(Entity<TId>? other)
  {

    return ReferenceEquals(this, other)
      || other is not null && GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
  }

  public override bool Equals(object? obj)
  {

    return obj is Entity<TId> other && Equals(other);
  }

  public override int GetHashCode()
  {

    return HashCode.Combine(GetType(), Id);
  }

  public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
  {

    return ReferenceEquals(left, right) || left is not null && left.Equals(right);
  }

  public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
  {

    return !(left == right);
  }
}