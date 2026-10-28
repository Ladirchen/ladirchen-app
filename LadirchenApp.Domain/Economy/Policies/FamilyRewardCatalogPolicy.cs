using System;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed class FamilyRewardCatalogPolicy
{
  public bool CanOpen(FamilyMember member)
  {
    ArgumentNullException.ThrowIfNull(member);

    return member.Role == MemberRole.Child || member.IsAdministrator;
  }

  public bool IsValidNewReward(
    string icon,
    string title,
    string description,
    string conditions,
    decimal price,
    int quantity,
    DateOnly? start,
    DateOnly? end,
    bool scheduled,
    bool limited
  )
  {
    return !string.IsNullOrWhiteSpace(icon)
      && !string.IsNullOrWhiteSpace(title)
      && !string.IsNullOrWhiteSpace(description)
      && !string.IsNullOrWhiteSpace(conditions)
      && price >= 1
      && quantity >= 1
      && (!scheduled || start is not null)
      && (!limited || end is not null)
      && (start is null || end is null || start <= end);
  }
}
