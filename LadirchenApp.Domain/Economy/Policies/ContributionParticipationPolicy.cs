using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public static class ContributionParticipationPolicy
{
  public static Percentage Completion(IEnumerable<Contribution> contributions, FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(memberId);
    var required = contributions
      .Where(item => (item.Kind is ContributionKind.Basic or ContributionKind.School) && item.AssigneeId == memberId)
      .ToArray();
    var total = required.Sum(item => item.Energy.Value);
    var approvedBasic = required.Where(item => item.Status != ContributionStatus.Available).Sum(item => item.Energy.Value);
    var approvedExtra = contributions
      .Where(item => item.Kind == ContributionKind.Extra && item.AssigneeId == memberId && item.Status != ContributionStatus.Available)
      .Sum(item => item.Energy.Value);

    return EffectiveCompletion(total, approvedBasic, approvedExtra);
  }

  public static Percentage EffectiveCompletion(decimal assignedBasic, decimal approvedBasic, decimal approvedExtra)
  {
    Rules.Require(
      assignedBasic >= 0 && approvedBasic >= 0 && approvedExtra >= 0,
      DomainError.InvalidValue,
      "Contribution energy cannot be negative."
    );

    if (assignedBasic == 0)
    {
      return new Percentage(60);
    }

    return new Percentage(Math.Min(100, Rules.Round((approvedBasic + approvedExtra) / assignedBasic * 100)));
  }

  public static int ParticipationDays(IEnumerable<Contribution> contributions, FamilyMemberId memberId,
    IsoWeekPeriod period, IanaTimeZone zone)
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(memberId);
    ArgumentNullException.ThrowIfNull(period);
    ArgumentNullException.ThrowIfNull(zone);

    return contributions
      .Where(item => item.AssigneeId == memberId && item.Status != ContributionStatus.Available)
      .Select(item => item.SubmittedAt ?? item.ApprovedAt)
      .Where(item => item is not null)
      .Select(item => zone.LocalDate(item!.Value))
      .Where(period.Contains)
      .Distinct()
      .Count();
  }

  public static int ParticipationDaysThrough(
    IEnumerable<Contribution> contributions,
    FamilyMemberId memberId,
    DateOnly throughDate,
    IanaTimeZone zone
  )
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(memberId);
    ArgumentNullException.ThrowIfNull(zone);

    return contributions
      .Where(item => item.AssigneeId == memberId && item.Status != ContributionStatus.Available)
      .Select(item => item.SubmittedAt ?? item.ApprovedAt)
      .Where(item => item is not null)
      .Select(item => zone.LocalDate(item!.Value))
      .Where(date => date <= throughDate)
      .Distinct()
      .Count();
  }

  public static decimal AverageRating(IEnumerable<Contribution> contributions, FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(memberId);
    var ratings = contributions.Where(item => item.Kind == ContributionKind.Basic && item.AssigneeId == memberId && item.Rating is not null)
      .Select(item => (decimal)item.Rating!.Value).ToArray();

    return ratings.Length == 0 ? 0 : ratings.Average();
  }
}
