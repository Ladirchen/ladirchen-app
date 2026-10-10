using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LadirchenApp.Prototypes;

[SuppressMessage(
  "Major Code Smell",
  "S4035",
  Justification = "This abstract DDD base requires inheritance; equality compares exact runtime types."
)]
public abstract class ValueObject : IEquatable<ValueObject>
{
  protected abstract IEnumerable<object?> GetEqualityComponents();

  public bool Equals(ValueObject? other)
  {
    return ReferenceEquals(this, other)
      || other is not null
        && GetType() == other.GetType()
        && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
  }

  public override bool Equals(object? obj)
  {
    return obj is ValueObject other && Equals(other);
  }

  public override int GetHashCode()
  {
    var hash = new HashCode();
    hash.Add(GetType());

    foreach (var component in GetEqualityComponents())
    {
      hash.Add(component);
    }

    return hash.ToHashCode();
  }

  public static bool operator ==(ValueObject? left, ValueObject? right)
  {
    return ReferenceEquals(left, right) || left is not null && left.Equals(right);
  }

  public static bool operator !=(ValueObject? left, ValueObject? right)
  {
    return !(left == right);
  }
}
