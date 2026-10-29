using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed class SimulateSavingsInterestQueryHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyRepository _familyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;

  public SimulateSavingsInterestQueryHandler(
    IFamilyRepository familyRepository,
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository
  )
  {
    _familyRepository = familyRepository;
    _economyRepository = economyRepository;
    _progressionRepository = progressionRepository;
  }

  public async Task<SavingsInterestSimulation> HandleAsync(
    SimulateSavingsInterestQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);
    var family = await _familyRepository.LoadAsync(query.FamilyId, cancellationToken)
      ?? throw new InvalidOperationException("The family aggregate has not been initialized.");
    _ = family.Root.ActiveMember(query.MemberId);
    var progression = await _progressionRepository.LoadAsync(family.Root, cancellationToken)
      ?? throw new InvalidOperationException("The family progression has not been initialized.");
    var economy = await _economyRepository.LoadAsync(family.Root, cancellationToken)
      ?? throw new InvalidOperationException("The family economy has not been initialized.");
    var participation = new PersonalParticipation(query.StreakDays, query.CompletionPercent, query.AverageRating);
    var policy = new LadiProgressionPolicy();
    var value = policy.Calculate(participation);
    var stage = policy.DetermineStage(value.Capped);
    var rate = SavingsInterestPolicy.CalculateRate(
      stage,
      progression.Root.MaximumSavingsInterestRate
    );
    var projectedCredit = economy.Root.SavingsGoals
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
}
