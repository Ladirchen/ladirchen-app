using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Family;

public sealed record FamilyState
{
  public FamilyState(
    FamilyId id,
    IanaTimeZone timeZone,
    FamilyCurrency currency,
    ExchangeRate exchangeRate,
    FamilyMemberId? activeChildId,
    bool onboardingCompleted,
    IEnumerable<FamilyMemberState> members,
    IEnumerable<FamilyPetState> pets
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(timeZone);
    ArgumentNullException.ThrowIfNull(exchangeRate);
    ArgumentNullException.ThrowIfNull(members);
    ArgumentNullException.ThrowIfNull(pets);
    Id = id;
    TimeZone = timeZone;
    Currency = Rules.Defined(currency);
    ExchangeRate = exchangeRate;
    ActiveChildId = activeChildId;
    OnboardingCompleted = onboardingCompleted;
    Members = members.ToArray();
    Pets = pets.ToArray();
  }

  public FamilyId Id { get; }
  public IanaTimeZone TimeZone { get; }
  public FamilyCurrency Currency { get; }
  public ExchangeRate ExchangeRate { get; }
  public FamilyMemberId? ActiveChildId { get; }
  public bool OnboardingCompleted { get; }
  public IReadOnlyList<FamilyMemberState> Members { get; }
  public IReadOnlyList<FamilyPetState> Pets { get; }
}
