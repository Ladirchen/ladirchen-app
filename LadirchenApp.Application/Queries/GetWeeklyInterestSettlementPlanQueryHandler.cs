using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed class GetWeeklyInterestSettlementPlanQueryHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyRepository _familyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;
  private readonly TimeProvider _timeProvider;

  public GetWeeklyInterestSettlementPlanQueryHandler(
    IFamilyRepository familyRepository,
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    TimeProvider timeProvider
  )
  {
    _familyRepository = familyRepository;
    _economyRepository = economyRepository;
    _progressionRepository = progressionRepository;
    _timeProvider = timeProvider;
  }

  public async Task<WeeklyInterestSettlementPlan?> HandleAsync(
    FamilyId familyId,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(familyId);
    var family = await _familyRepository.LoadAsync(familyId, cancellationToken);

    if (family is null)
    {
      return null;
    }

    var administrator = family.Root.Members.FirstOrDefault(member => member.IsAdministrator);

    if (administrator is null)
    {
      return null;
    }

    var progression = await _progressionRepository.LoadAsync(family.Root, cancellationToken);
    var economy = await _economyRepository.LoadAsync(family.Root, cancellationToken);

    if (progression is null || economy is null)
    {
      return null;
    }

    var localTime = family.Root.TimeZone.LocalTime(_timeProvider.GetUtcNow());
    var currentPeriod = IsoWeekPeriod.For(DateOnly.FromDateTime(localTime.DateTime));
    var periods = progression.Root.ClosedWeeks
      .Select(week => week.Period)
      .Where(period => period.Start < currentPeriod.Start)
      .Where(period => !economy.Root.SettledInterestPeriods.Contains(period))
      .OrderBy(period => period.Start)
      .ToArray();
    var previousPeriod = IsoWeekPeriod.For(currentPeriod.Start.AddDays(-7));
    IsoWeekPeriod? periodToClose = progression.Root.IsWeekClosed(previousPeriod)
      ? null
      : previousPeriod;

    if (
      periodToClose is null
      && !progression.Root.IsWeekClosed(currentPeriod)
      && FamilyWeekClosingPolicy.CanClose(
        currentPeriod,
        currentPeriod,
        localTime,
        progression.Root.AverageEnergy(currentPeriod)
      )
    )
    {
      periodToClose = currentPeriod;
    }

    return new WeeklyInterestSettlementPlan(administrator.Id, periodToClose, periods);
  }
}
