using System;
using System.Linq;
using System.Text.Json;
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
    var profile = await RequireSnapshotAsync(query, "family-profile", cancellationToken);
    var memberExists = profile.State.GetProperty("members").EnumerateArray().Any(member =>
      member.GetProperty("id").GetString() == query.MemberId.Value
    );

    if (!memberExists)
    {
      throw new ArgumentException("The family member does not exist.", nameof(query));
    }

    var progression = await RequireSnapshotAsync(query, "family-progression", cancellationToken);
    var maximumRate = progression.State.GetProperty("maximumSavingsInterestRate").GetDecimal();
    var participation = new PersonalParticipation(query.StreakDays, query.CompletionPercent, query.AverageRating);
    var policy = new LadiProgressionPolicy();
    var value = policy.Calculate(participation);
    var stage = policy.DetermineStage(value.Capped);
    var rate = SavingsInterestPolicy.CalculateRate(stage, new Percentage(maximumRate));
    var savings = await RequireSnapshotAsync(query, "savings", cancellationToken);
    var projectedCredit = savings.State
      .GetProperty("goals")
      .EnumerateArray()
      .Where(goal => goal.GetProperty("ownerId").GetString() == query.MemberId.Value)
      .Sum(goal =>
        SavingsInterestPolicy.CalculateCredit(
          new LadirchenAmount(goal.GetProperty("saved").GetDecimal()),
          new LadirchenAmount(goal.GetProperty("target").GetDecimal()),
          rate
        )
      );

    return new SavingsInterestSimulation(stage.Id, stage.Position, rate.Value, projectedCredit);
  }

  private async Task<FamilyAggregateSnapshot> RequireSnapshotAsync(
    SimulateSavingsInterestQuery query,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    return await _repository.GetAsync(query.FamilyId, aggregateType, cancellationToken)
      ?? throw new InvalidOperationException($"The {aggregateType} aggregate has not been initialized.");
  }
}
