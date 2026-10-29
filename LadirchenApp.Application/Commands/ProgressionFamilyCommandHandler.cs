using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class ProgressionFamilyCommandHandler : FamilyCommandAreaHandler
{
  public ProgressionFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    TimeProvider timeProvider
  )
    : base(repository, timeProvider, "progression.")
  {
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);
    _ = family.ActiveMember(command.ActorId);
    var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
    var contributionState = RequireObject(contributions.State);
    var economy = FamilyEconomySnapshotMapper.RehydrateContributions(family, contributionState);
    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var storedProgression = RequireObject(progression.State);
    var house = FamilyProgressionApplicationService.RestoreHouse(
      family,
      profileState,
      storedProgression
    );
    var zone = family.TimeZone;
    var currentPeriod = IsoWeekPeriod.For(zone.LocalDate(_timeProvider.GetUtcNow()));
    var maximumRate = FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(storedProgression);
    var evaluation = FamilyProgressionApplicationService.Evaluate(
      family,
      economy,
      profileState,
      storedProgression,
      currentPeriod,
      maximumRate
    );
    var progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
      storedProgression,
      evaluation,
      house
    );
    var saves = new List<PendingFamilyAggregateSave>();
    var profileIsWritten = false;

    switch (command.CommandType)
    {
      case "progression.refresh":
        break;
      case "progression.set-maximum-savings-rate":
        family.RequireAdministrator(command.ActorId);
        var requestedMaximum = Round(RequiredDecimal(payload, "percent"));
        maximumRate = new SavingsInterestPolicy(new Percentage(requestedMaximum)).MaximumRate;
        evaluation = FamilyProgressionApplicationService.Evaluate(
          family,
          economy,
          profileState,
          storedProgression,
          currentPeriod,
          maximumRate
        );
        progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
          storedProgression,
          evaluation,
          house
        );
        break;
      case "progression.complete-week":
        family.RequireAdministrator(command.ActorId);
        Require(
          house.CloseSuccessfulWeek(
            currentPeriod,
            evaluation.FamilyEnergy,
            evaluation.Members.Select(item => item.LadiEvaluation),
            _timeProvider.GetUtcNow()
          ),
          "Family energy is too low to complete the week."
        );
        FamilyProgressionSnapshotMapper.WriteParticipantStreaks(profileState, family, house);
        progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
          progressionState,
          evaluation,
          house
        );
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        profileIsWritten = true;
        break;
      case "progression.fail-week":
        family.RequireAdministrator(command.ActorId);
        house.CloseFailedWeek(
          currentPeriod,
          evaluation.FamilyEnergy,
          evaluation.Members.Select(item => item.LadiEvaluation),
          _timeProvider.GetUtcNow()
        );
        FamilyProgressionSnapshotMapper.WriteParticipantStreaks(profileState, family, house);
        progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
          progressionState,
          evaluation,
          house
        );
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        profileIsWritten = true;
        break;
      default:
        throw new FamilyCommandException("The progression command is not supported.");
    }

    saves.Add(
      FamilyProgressionApplicationService.CreateSave(
        progression,
        progressionState,
        command.ActorId
      )
    );
    var revisionGuards = new List<FamilyAggregateRevisionGuard>
    {
      FamilyAggregateRevisionGuard.FromSnapshot(contributions),
    };

    if (!profileIsWritten)
    {
      revisionGuards.Add(FamilyAggregateRevisionGuard.FromSnapshot(profile));
    }

    var changed = await _repository.SaveBatchAsync(saves, revisionGuards, cancellationToken);

    return new FamilyCommandResult(changed);
  }

}
