using System;
using System.Globalization;

namespace LadirchenApp.Domain.Shared;

public sealed record AvailabilityWindow
{
  public AvailabilityWindow(DateOnly? from = null, DateOnly? until = null)
  {
    Rules.Require(from is null || until is null || from <= until, DomainError.InvalidValue, "The date window is reversed.");
    From = from;
    Until = until;
  }
  public DateOnly? From
  {
    get;
  }
  public DateOnly? Until
  {
    get;
  }
  public bool Contains(DateOnly date)
  {

    return (From is null || date >= From) && (Until is null || date <= Until);
  }
}