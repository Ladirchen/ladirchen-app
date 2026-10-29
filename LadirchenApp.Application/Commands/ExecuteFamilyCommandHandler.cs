using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;

namespace LadirchenApp.Application.Commands;

public sealed class ExecuteFamilyCommandHandler
  : ICommandHandler<FamilyCommand, FamilyCommandResult>
{
  private readonly IReadOnlyList<ITypedFamilyCommandAreaHandler> _typedAreaHandlers;
  private readonly IFamilyCommandExecutionCoordinator _executionCoordinator;
  private readonly IFamilyRepository _familyRepository;

  public ExecuteFamilyCommandHandler(
    IFamilyRepository familyRepository,
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    IHouseCustomizationRepository houseRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    IFamilyCommandExecutionCoordinator executionCoordinator,
    TimeProvider timeProvider
  )
  {
    _familyRepository = familyRepository
      ?? throw new ArgumentNullException(nameof(familyRepository));
    _executionCoordinator = executionCoordinator
      ?? throw new ArgumentNullException(nameof(executionCoordinator));
    _typedAreaHandlers =
    [
      new ProfileFamilyCommandHandler(
        economyRepository,
        progressionRepository,
        houseRepository,
        unitOfWork,
        timeProvider
      ),
      new HomeFamilyCommandHandler(
        economyRepository,
        progressionRepository,
        houseRepository,
        unitOfWork,
        timeProvider
      ),
      new ProgressionFamilyCommandHandler(
        economyRepository,
        progressionRepository,
        unitOfWork,
        timeProvider
      ),
      new SavingsFamilyCommandHandler(
        economyRepository,
        progressionRepository,
        unitOfWork,
        timeProvider
      ),
      new RewardShopFamilyCommandHandler(economyRepository, unitOfWork, timeProvider),
      new ContributionsFamilyCommandHandler(economyRepository, unitOfWork, timeProvider),
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
    var loadedFamily = await _familyRepository.LoadAsync(
      command.FamilyId,
      cancellationToken
    ) ?? throw new FamilyCommandException(
      "The family aggregate has not been initialized."
    );
    loadedFamily.Root.RequireActorAccess(
      command.AuthenticatedMemberId ?? command.ActorId,
      command.ActorId
    );
    var handler = _typedAreaHandlers.FirstOrDefault(candidate => candidate.CanHandle(command))
      ?? throw new FamilyCommandException("The family command is not supported.");

    return await handler.HandleAsync(command, loadedFamily, cancellationToken);
  }
}
