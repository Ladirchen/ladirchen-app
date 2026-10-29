using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class ProgressionFamilyCommandHandler : TypedFamilyCommandAreaHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;

  public ProgressionFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "progression.")
  {
    _economyRepository = economyRepository
      ?? throw new ArgumentNullException(nameof(economyRepository));
    _progressionRepository = progressionRepository
      ?? throw new ArgumentNullException(nameof(progressionRepository));
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    LoadedAggregate<Family> loadedFamily,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentNullException.ThrowIfNull(loadedFamily);
    var family = loadedFamily.Root;
    _ = family.ActiveMember(command.ActorId);
    var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken)
      ?? throw new FamilyCommandException(
        "The family-economy aggregate has not been initialized."
      );
    var loadedProgression = await _progressionRepository.LoadAsync(family, cancellationToken)
      ?? throw new FamilyCommandException(
        "The family-progression aggregate has not been initialized."
      );
    var house = loadedProgression.Root;
    var currentPeriod = IsoWeekPeriod.For(
      family.TimeZone.LocalDate(TimeProvider.GetUtcNow())
    );
    var evaluation = FamilyProgressionEvaluator.Evaluate(
      family,
      loadedEconomy.Root,
      house,
      currentPeriod
    );
    IReadOnlyList<string> changedReadModels;

    switch (command.CommandType)
    {
      case "progression.refresh":
        changedReadModels =
        [
          "family-progression",
          "savings",
          "home-customization",
        ];
        break;
      case "progression.set-maximum-savings-rate":
        family.RequireAdministrator(command.ActorId);
        house.SetMaximumSavingsInterestRate(
          new Percentage(Round(RequiredDecimal(RequiredObject(command.Payload), "percent")))
        );
        changedReadModels =
        [
          "family-progression",
          "savings",
        ];
        break;
      case "progression.complete-week":
        family.RequireAdministrator(command.ActorId);
        Require(
          house.CloseSuccessfulWeek(
            currentPeriod,
            evaluation.FamilyEnergy,
            evaluation.Members.Select(item => item.LadiEvaluation),
            TimeProvider.GetUtcNow()
          ),
          "Family energy is too low to complete the week."
        );
        changedReadModels =
        [
          "family-profile",
          "family-progression",
          "savings",
          "home-customization",
        ];
        break;
      case "progression.fail-week":
        family.RequireAdministrator(command.ActorId);
        house.CloseFailedWeek(
          currentPeriod,
          evaluation.FamilyEnergy,
          evaluation.Members.Select(item => item.LadiEvaluation),
          TimeProvider.GetUtcNow()
        );
        changedReadModels =
        [
          "family-profile",
          "family-progression",
          "savings",
          "home-customization",
        ];
        break;
      default:
        throw new FamilyCommandException("The progression command is not supported.");
    }

    var result = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        family.Id,
        command.ActorId,
        familyProgression: new PendingFamilyProgressionSave(
          house,
          loadedProgression.Revision
        ),
        revisionGuards:
        [
          new TypedAggregateRevisionGuard(
            TypedAggregateKind.Family,
            loadedFamily.Revision
          ),
          new TypedAggregateRevisionGuard(
            TypedAggregateKind.FamilyEconomy,
            loadedEconomy.Revision
          ),
        ]
      ),
      cancellationToken
    );
    var saved = result.FamilyProgression
      ?? throw new InvalidOperationException("The family progression was not saved.");

    return new FamilyCommandResult(
      changedReadModels
        .Select(readModel => Change(family.Id, readModel, saved.Revision, saved.Audit))
        .ToArray()
    );
  }
}
