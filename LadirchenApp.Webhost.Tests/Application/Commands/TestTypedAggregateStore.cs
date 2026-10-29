using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

internal sealed class TestTypedAggregateStore :
  IFamilyRepository,
  IFamilyEconomyRepository,
  IFamilyProgressionRepository,
  IHouseCustomizationRepository,
  ITypedAggregateUnitOfWork
{
  private readonly Lock _lock = new();
  private readonly TimeProvider _timeProvider;
  private FamilyState? _familyState;
  private FamilyEconomyState? _economyState;
  private FamilyEconomyPresentationState _economyPresentation = FamilyEconomyPresentationState.Empty;
  private FamilyHouseState? _progressionState;
  private FamilyHouseShopState? _houseState;
  private AggregateAudit? _familyAudit;
  private AggregateAudit? _economyAudit;
  private AggregateAudit? _progressionAudit;
  private AggregateAudit? _houseAudit;
  private long _familyRevision;
  private long _economyRevision;
  private long _progressionRevision;
  private long _houseRevision;

  public TestTypedAggregateStore(TimeProvider timeProvider)
  {
    _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
  }

  public IReadOnlyList<TypedAggregateRevisionGuard> LastRevisionGuards { get; private set; } = [];

  public FamilyEconomyPresentationState EconomyPresentation
  {
    get
    {
      lock (_lock)
      {
        return _economyPresentation;
      }
    }
  }

  public void Seed(
    Family family,
    FamilyEconomy? economy = null,
    FamilyEconomyPresentationState? economyPresentation = null,
    FamilyHouse? progression = null,
    FamilyHouseShop? house = null,
    long revision = 1,
    FamilyMemberId? updatedBy = null
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    var actor = updatedBy ?? family.Members.First().Id;
    var audit = new AggregateAudit(actor, DateTimeOffset.UnixEpoch);

    lock (_lock)
    {
      _familyState = family.CaptureState();
      _familyRevision = revision;
      _familyAudit = audit;
      _economyState = economy?.CaptureState();
      _economyPresentation = economyPresentation ?? FamilyEconomyPresentationState.Empty;
      _economyRevision = economy is null ? 0 : revision;
      _economyAudit = economy is null ? null : audit;
      _progressionState = progression?.CaptureState();
      _progressionRevision = progression is null ? 0 : revision;
      _progressionAudit = progression is null ? null : audit;
      _houseState = house?.CaptureState();
      _houseRevision = house is null ? 0 : revision;
      _houseAudit = house is null ? null : audit;
      LastRevisionGuards = [];
    }
  }

  public ExecuteFamilyCommandHandler CreateCommandHandler()
  {
    return new ExecuteFamilyCommandHandler(
      this,
      this,
      this,
      this,
      this,
      new ImmediateFamilyCommandExecutionCoordinator(),
      _timeProvider
    );
  }

  public Family RequiredFamily()
  {
    lock (_lock)
    {
      return Family.Reconstitute(
        _familyState ?? throw new InvalidOperationException("The family has not been seeded.")
      );
    }
  }

  public FamilyEconomy RequiredEconomy()
  {
    lock (_lock)
    {
      var family = RequiredFamily();

      return FamilyEconomy.Reconstitute(
        family,
        _economyState ?? throw new InvalidOperationException("The family economy has not been seeded.")
      );
    }
  }

  public FamilyHouse RequiredProgression()
  {
    lock (_lock)
    {
      var family = RequiredFamily();

      return FamilyHouse.Reconstitute(
        family,
        _progressionState ?? throw new InvalidOperationException("The family progression has not been seeded.")
      );
    }
  }

  public FamilyHouseShop RequiredHouse()
  {
    lock (_lock)
    {
      var family = RequiredFamily();
      var progression = RequiredProgression();

      return FamilyHouseShop.Reconstitute(
        family,
        progression.StoredLevel,
        _houseState ?? throw new InvalidOperationException("The house customization has not been seeded.")
      );
    }
  }

  public long Revision(TypedAggregateKind kind)
  {
    lock (_lock)
    {
      return RevisionFor(kind);
    }
  }

  public Task<IReadOnlyList<FamilyId>> ListIdsAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
      IReadOnlyList<FamilyId> familyIds = _familyState is null
        ? []
        : [_familyState.Id];

      return Task.FromResult(familyIds);
    }
  }

  public Task<LoadedAggregate<Family>?> LoadAsync(
    FamilyId familyId,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
      if (_familyState is null || _familyState.Id != familyId)
      {
        return Task.FromResult<LoadedAggregate<Family>?>(null);
      }

      return Task.FromResult<LoadedAggregate<Family>?>(
        new LoadedAggregate<Family>(
          Family.Reconstitute(_familyState),
          _familyRevision,
          RequiredAudit(_familyAudit, "family")
        )
      );
    }
  }

  public async Task<LoadedAggregate<Family>> SaveAsync(
    Family root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    var result = await SaveAsync(
      new TypedAggregateSaveBatch(
        root.Id,
        updatedBy,
        family: new PendingFamilySave(root, expectedRevision)
      ),
      cancellationToken
    );

    return result.Family ?? throw new InvalidOperationException("The family was not saved.");
  }

  Task<LoadedFamilyEconomy?> IFamilyEconomyRepository.LoadAsync(
    Family family,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
      if (_economyState is null || _economyState.Id != family.Id)
      {
        return Task.FromResult<LoadedFamilyEconomy?>(null);
      }

      return Task.FromResult<LoadedFamilyEconomy?>(
        new LoadedFamilyEconomy(
          FamilyEconomy.Reconstitute(family, _economyState),
          _economyPresentation,
          _economyRevision,
          RequiredAudit(_economyAudit, "family economy")
        )
      );
    }
  }

  public async Task<LoadedFamilyEconomy> SaveAsync(
    FamilyEconomy root,
    FamilyEconomyPresentationState presentation,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    var result = await SaveAsync(
      new TypedAggregateSaveBatch(
        root.Id,
        updatedBy,
        familyEconomy: new PendingFamilyEconomySave(root, presentation, expectedRevision)
      ),
      cancellationToken
    );

    return result.FamilyEconomy
      ?? throw new InvalidOperationException("The family economy was not saved.");
  }

  Task<LoadedAggregate<FamilyHouse>?> IFamilyProgressionRepository.LoadAsync(
    Family family,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
      if (_progressionState is null || _progressionState.Id != family.Id)
      {
        return Task.FromResult<LoadedAggregate<FamilyHouse>?>(null);
      }

      return Task.FromResult<LoadedAggregate<FamilyHouse>?>(
        new LoadedAggregate<FamilyHouse>(
          FamilyHouse.Reconstitute(family, _progressionState),
          _progressionRevision,
          RequiredAudit(_progressionAudit, "family progression")
        )
      );
    }
  }

  public async Task<LoadedAggregate<FamilyHouse>> SaveAsync(
    FamilyHouse root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    var result = await SaveAsync(
      new TypedAggregateSaveBatch(
        root.Id,
        updatedBy,
        familyProgression: new PendingFamilyProgressionSave(root, expectedRevision)
      ),
      cancellationToken
    );

    return result.FamilyProgression
      ?? throw new InvalidOperationException("The family progression was not saved.");
  }

  public Task<LoadedAggregate<FamilyHouseShop>?> LoadAsync(
    Family family,
    int currentHouseLevel,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
      if (_houseState is null || _houseState.Id != family.Id)
      {
        return Task.FromResult<LoadedAggregate<FamilyHouseShop>?>(null);
      }

      return Task.FromResult<LoadedAggregate<FamilyHouseShop>?>(
        new LoadedAggregate<FamilyHouseShop>(
          FamilyHouseShop.Reconstitute(family, currentHouseLevel, _houseState),
          _houseRevision,
          RequiredAudit(_houseAudit, "house customization")
        )
      );
    }
  }

  public async Task<LoadedAggregate<FamilyHouseShop>> SaveAsync(
    FamilyHouseShop root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    var result = await SaveAsync(
      new TypedAggregateSaveBatch(
        root.Id,
        updatedBy,
        houseCustomization: new PendingHouseCustomizationSave(root, expectedRevision)
      ),
      cancellationToken
    );

    return result.HouseCustomization
      ?? throw new InvalidOperationException("The house customization was not saved.");
  }

  public Task<TypedAggregateSaveResult> SaveAsync(
    TypedAggregateSaveBatch batch,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(batch);
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
      ValidateExpectations(batch);
      var audit = new AggregateAudit(batch.UpdatedBy, _timeProvider.GetUtcNow());
      LoadedAggregate<Family>? familyResult = null;
      LoadedFamilyEconomy? economyResult = null;
      LoadedAggregate<FamilyHouse>? progressionResult = null;
      LoadedAggregate<FamilyHouseShop>? houseResult = null;

      if (batch.Family is not null)
      {
        _familyState = batch.Family.Root.CaptureState();
        _familyRevision++;
        _familyAudit = audit;
        familyResult = new LoadedAggregate<Family>(
          batch.Family.Root,
          _familyRevision,
          audit
        );
      }

      if (batch.FamilyEconomy is not null)
      {
        _economyState = batch.FamilyEconomy.Root.CaptureState();
        _economyPresentation = batch.FamilyEconomy.Presentation;
        _economyRevision++;
        _economyAudit = audit;
        economyResult = new LoadedFamilyEconomy(
          batch.FamilyEconomy.Root,
          batch.FamilyEconomy.Presentation,
          _economyRevision,
          audit
        );
      }

      if (batch.FamilyProgression is not null)
      {
        _progressionState = batch.FamilyProgression.Root.CaptureState();
        _progressionRevision++;
        _progressionAudit = audit;
        progressionResult = new LoadedAggregate<FamilyHouse>(
          batch.FamilyProgression.Root,
          _progressionRevision,
          audit
        );
      }

      if (batch.HouseCustomization is not null)
      {
        _houseState = batch.HouseCustomization.Root.CaptureState();
        _houseRevision++;
        _houseAudit = audit;
        houseResult = new LoadedAggregate<FamilyHouseShop>(
          batch.HouseCustomization.Root,
          _houseRevision,
          audit
        );
      }

      LastRevisionGuards = batch.RevisionGuards.ToArray();

      return Task.FromResult(
        new TypedAggregateSaveResult(
          familyResult,
          economyResult,
          progressionResult,
          houseResult
        )
      );
    }
  }

  private void ValidateExpectations(TypedAggregateSaveBatch batch)
  {
    ValidateExpectedRevision(
      TypedAggregateKind.Family,
      batch.Family?.ExpectedRevision
    );
    ValidateExpectedRevision(
      TypedAggregateKind.FamilyEconomy,
      batch.FamilyEconomy?.ExpectedRevision
    );
    ValidateExpectedRevision(
      TypedAggregateKind.FamilyProgression,
      batch.FamilyProgression?.ExpectedRevision
    );
    ValidateExpectedRevision(
      TypedAggregateKind.HouseCustomization,
      batch.HouseCustomization?.ExpectedRevision
    );

    foreach (var guard in batch.RevisionGuards)
    {
      ValidateExpectedRevision(guard.AggregateKind, guard.ExpectedRevision);
    }
  }

  private void ValidateExpectedRevision(TypedAggregateKind kind, long? expectedRevision)
  {
    if (expectedRevision is null)
    {
      return;
    }

    var actualRevision = RevisionFor(kind);

    if (actualRevision != expectedRevision.Value)
    {
      throw new FamilyAggregateConflictException(
        kind.ToString(),
        expectedRevision.Value,
        actualRevision
      );
    }
  }

  private long RevisionFor(TypedAggregateKind kind)
  {
    return kind switch
    {
      TypedAggregateKind.Family => _familyRevision,
      TypedAggregateKind.FamilyEconomy => _economyRevision,
      TypedAggregateKind.FamilyProgression => _progressionRevision,
      TypedAggregateKind.HouseCustomization => _houseRevision,
      _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown aggregate kind."),
    };
  }

  private static AggregateAudit RequiredAudit(AggregateAudit? audit, string name)
  {
    return audit ?? throw new InvalidOperationException($"The {name} audit has not been seeded.");
  }
}
