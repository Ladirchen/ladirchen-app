using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed class GetWeeklyInterestSettlementPlanQueryHandler
{
  private static readonly string[] RequiredAggregateTypes =
    ["family-profile", "family-progression", "reward-shop", "savings"];
  private readonly IFamilyAggregateSnapshotRepository _repository;

  public GetWeeklyInterestSettlementPlanQueryHandler(
    IFamilyAggregateSnapshotRepository repository
  )
  {
    _repository = repository;
  }

  public async Task<WeeklyInterestSettlementPlan?> HandleAsync(
    FamilyId familyId,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(familyId);
    var snapshots = await _repository.GetManyAsync(
      familyId,
      RequiredAggregateTypes,
      cancellationToken
    );

    if (RequiredAggregateTypes.Any(type => !snapshots.ContainsKey(type)))
    {
      return null;
    }

    var profile = ParseObject(snapshots["family-profile"], "family profile");
    var progression = ParseObject(snapshots["family-progression"], "family progression");
    var savings = ParseObject(snapshots["savings"], "savings");
    var rewardShop = ParseObject(snapshots["reward-shop"], "reward shop");
    var family = FamilyProfileSnapshotMapper.Rehydrate(familyId, profile);
    var administrator = family.Members.FirstOrDefault(member => member.IsAdministrator);

    if (administrator is null)
    {
      return null;
    }

    var economy = FamilyEconomySnapshotMapper.RehydrateSavings(family, savings, rewardShop);
    var house = new FamilyHouse();
    house.Restore(
      FamilyProgressionSnapshotMapper.ReadHouseLevel(progression),
      FamilyProgressionSnapshotMapper.ReadCompletedWeeklyStreak(progression)
    );
    FamilyProgressionSnapshotMapper.RestoreClosedWeeks(progression, house);
    var periods = house.ClosedWeeks
      .Select(week => week.Period)
      .Where(period => !economy.SettledInterestPeriods.Contains(period))
      .OrderBy(period => period.Start)
      .ToArray();

    return new WeeklyInterestSettlementPlan(administrator.Id, periods);
  }

  private static JsonObject ParseObject(FamilyAggregateSnapshot snapshot, string aggregateName)
  {
    return JsonNode.Parse(snapshot.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException($"The {aggregateName} state must be an object.");
  }
}
