using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed class SimulateSavingsInterestQueryHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;

  public SimulateSavingsInterestQueryHandler(IFamilyAggregateSnapshotRepository repository)
  {
    _repository = repository;
  }

  public async Task<SavingsInterestSimulation> HandleAsync(
    SimulateSavingsInterestQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);
    var snapshots = await _repository.GetManyAsync(
      query.FamilyId,
      ["family-profile", "family-progression", "savings"],
      cancellationToken
    );
    var profile = RequireSnapshot(snapshots, "family-profile");
    var family = SavingsReadModelSnapshotMapper.RehydrateFamily(profile);
    var memberExists = family.Members.Any(member => member.Id == query.MemberId);

    if (!memberExists)
    {
      throw new ArgumentException("The family member does not exist.", nameof(query));
    }

    var progression = RequireSnapshot(snapshots, "family-progression");
    var maximumRate = SavingsReadModelSnapshotMapper.ReadMaximumInterestRate(progression);
    var participation = new PersonalParticipation(query.StreakDays, query.CompletionPercent, query.AverageRating);
    var policy = new LadiProgressionPolicy();
    var value = policy.Calculate(participation);
    var stage = policy.DetermineStage(value.Capped);
    var rate = SavingsInterestPolicy.CalculateRate(stage, maximumRate);
    var savings = RequireSnapshot(snapshots, "savings");
    var economy = SavingsReadModelSnapshotMapper.RehydrateEconomy(family, savings, null);
    var projectedCredit = economy.SavingsGoals
      .Where(goal => goal.Owner.MemberId == query.MemberId)
      .Sum(goal =>
        SavingsInterestPolicy.CalculateCredit(
          new LadirchenAmount(goal.Saved),
          new LadirchenAmount(goal.Target),
          rate
        )
      );

    return new SavingsInterestSimulation(stage.Id, stage.Position, rate.Value, projectedCredit);
  }

  private static FamilyAggregateSnapshot RequireSnapshot(
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots,
    string aggregateType
  )
  {
    return snapshots.TryGetValue(aggregateType, out var snapshot)
      ? snapshot
      : throw new InvalidOperationException($"The {aggregateType} aggregate has not been initialized.");
  }
}
