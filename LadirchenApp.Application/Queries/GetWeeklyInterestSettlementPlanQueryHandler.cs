using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed class GetWeeklyInterestSettlementPlanQueryHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyRepository _familyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;

  public GetWeeklyInterestSettlementPlanQueryHandler(
    IFamilyRepository familyRepository,
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository
  )
  {
    _familyRepository = familyRepository;
    _economyRepository = economyRepository;
    _progressionRepository = progressionRepository;
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

    var periods = progression.Root.ClosedWeeks
      .Select(week => week.Period)
      .Where(period => !economy.Root.SettledInterestPeriods.Contains(period))
      .OrderBy(period => period.Start)
      .ToArray();

    return new WeeklyInterestSettlementPlan(administrator.Id, periods);
  }
}
