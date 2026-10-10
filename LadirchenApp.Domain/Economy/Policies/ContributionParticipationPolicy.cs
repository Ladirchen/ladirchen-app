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
    var basic = contributions.Where(item => item.Kind == ContributionKind.Basic && item.AssigneeId == memberId).ToArray();
    var total = basic.Sum(item => item.Energy.Value);

    return new(total == 0 ? 60 : Rules.Round(basic.Where(item => item.Status == ContributionStatus.Approved)
      .Sum(item => item.Energy.Value) / total * 100));
  }

  public static int ApprovedDays(IEnumerable<Contribution> contributions, FamilyMemberId memberId,
    IsoWeekPeriod period, IanaTimeZone zone)
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(memberId);
    ArgumentNullException.ThrowIfNull(period);
    ArgumentNullException.ThrowIfNull(zone);

    return contributions.Where(item => item.AssigneeId == memberId && item.Status == ContributionStatus.Approved && item.ApprovedAt is not null)
      .Select(item => zone.LocalDate(item.ApprovedAt!.Value)).Where(period.Contains).Distinct().Count();
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