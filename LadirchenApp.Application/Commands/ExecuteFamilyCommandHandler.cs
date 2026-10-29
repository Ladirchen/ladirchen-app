using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Application.Commands;

public sealed class ExecuteFamilyCommandHandler
  : ICommandHandler<FamilyCommand, FamilyCommandResult>
{
  private readonly IReadOnlyList<IFamilyCommandAreaHandler> _areaHandlers;
  private readonly IFamilyCommandExecutionCoordinator _executionCoordinator;
  private readonly IFamilyAggregateSnapshotRepository _repository;

  public ExecuteFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    IFamilyCommandExecutionCoordinator executionCoordinator,
    TimeProvider timeProvider
  )
  {
    _repository = repository;
    _executionCoordinator = executionCoordinator;
    var progression = new FamilyProgressionApplicationService(repository, timeProvider);
    _areaHandlers =
    [
      new ProfileFamilyCommandHandler(repository, timeProvider, progression),
      new HomeFamilyCommandHandler(repository, timeProvider),
      new SavingsFamilyCommandHandler(repository, timeProvider),
      new RewardShopFamilyCommandHandler(repository, timeProvider),
      new ProgressionFamilyCommandHandler(repository, timeProvider),
      new ContributionsFamilyCommandHandler(repository, timeProvider, progression),
    ];
  }

  public async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);

    if (command.CommandId == Guid.Empty)
    {
      throw new FamilyCommandException("A command ID is required.");
    }

    return await _executionCoordinator.ExecuteOnceAsync(
      command,
      token => ExecuteAsync(command, token),
      cancellationToken
    );
  }

  private async Task<FamilyCommandResult> ExecuteAsync(
    FamilyCommand command,
    CancellationToken cancellationToken
  )
  {
    var profile = await _repository.GetAsync(
      command.FamilyId,
      "family-profile",
      cancellationToken
    ) ?? throw new FamilyCommandException(
      "The family-profile aggregate has not been initialized."
    );
    var profileState = FamilyCommandAreaHandler.RequireObject(profile.State);
    var payload = FamilyCommandAreaHandler.RequireObject(command.Payload);
    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);
    family.RequireActorAccess(
      command.AuthenticatedMemberId ?? command.ActorId,
      command.ActorId
    );
    var handler = _areaHandlers.FirstOrDefault(candidate => candidate.CanHandle(command))
      ?? throw new FamilyCommandException("The family command is not supported.");

    return await handler.HandleAsync(
      command,
      profile,
      profileState,
      payload,
      cancellationToken
    );
  }
}
