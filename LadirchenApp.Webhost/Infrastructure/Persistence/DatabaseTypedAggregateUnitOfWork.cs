using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class DatabaseTypedAggregateUnitOfWork : ITypedAggregateUnitOfWork
{
  private readonly LadirchenDbContext _database;
  private readonly TimeProvider _timeProvider;

  public DatabaseTypedAggregateUnitOfWork(
    LadirchenDbContext database,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _timeProvider = timeProvider;
  }

  public async Task<TypedAggregateSaveResult> SaveAsync(
    TypedAggregateSaveBatch batch,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(batch);
    ValidateRootIdentities(batch);
    var targetKinds = TargetKinds(batch);

    if (targetKinds.Count == 0)
    {
      return new TypedAggregateSaveResult(null, null, null, null);
    }

    IDbContextTransaction? ownedTransaction = null;

    try
    {
      if (_database.Database.CurrentTransaction is null)
      {
        ownedTransaction = await _database.Database.BeginTransactionAsync(
          IsolationLevel.Serializable,
          cancellationToken
        );
      }
      else if (
        _database.Database.CurrentTransaction.GetDbTransaction().IsolationLevel
          != IsolationLevel.Serializable
      )
      {
        throw new InvalidOperationException(
          "Typed aggregate revision guards require a serializable database transaction."
        );
      }

      DetachTrackedTypedEntities(batch.FamilyId.Value);
      var locked = await LockTargetRowsAsync(batch.FamilyId.Value, targetKinds, cancellationToken);
      ValidateExpectedRevisions(batch, locked);
      var now = _timeProvider.GetUtcNow();
      var result = await ApplyWritesAsync(batch, locked, now, cancellationToken);
      await _database.SaveChangesAsync(cancellationToken);

      if (ownedTransaction is not null)
      {
        await ownedTransaction.CommitAsync(cancellationToken);
      }

      return result;
    }
    catch (FamilyAggregateConflictException)
    {
      await RollbackOwnedTransactionAsync(ownedTransaction);
      throw;
    }
    catch (DbUpdateConcurrencyException exception)
    {
      await RollbackOwnedTransactionAsync(ownedTransaction);
      var expectation = FirstWriteExpectation(batch);

      throw FamilyAggregateConflictException.UnknownActualRevision(
        AggregateName(expectation.Kind),
        expectation.ExpectedRevision,
        exception
      );
    }
    catch (DbUpdateException exception) when (IsRootInsertConflict(exception))
    {
      await RollbackOwnedTransactionAsync(ownedTransaction);
      var expectation = FirstWriteExpectation(batch);

      throw FamilyAggregateConflictException.UnknownActualRevision(
        AggregateName(expectation.Kind),
        expectation.ExpectedRevision,
        exception
      );
    }
    catch (Exception exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
    )
    {
      await RollbackOwnedTransactionAsync(ownedTransaction);

      throw FamilyAggregateConflictException.ConcurrentChange(exception);
    }
    catch
    {
      await RollbackOwnedTransactionAsync(ownedTransaction);
      throw;
    }
    finally
    {
      if (ownedTransaction is not null)
      {
        await ownedTransaction.DisposeAsync();
      }
    }
  }

  private async Task<TypedAggregateSaveResult> ApplyWritesAsync(
    TypedAggregateSaveBatch batch,
    LockedTypedAggregateRows locked,
    DateTimeOffset now,
    CancellationToken cancellationToken
  )
  {
    LoadedAggregate<LadirchenApp.Domain.Family.Family>? familyResult = null;
    LoadedFamilyEconomy? economyResult = null;
    LoadedAggregate<LadirchenApp.Domain.House.FamilyHouse>? progressionResult = null;
    LoadedAggregate<LadirchenApp.Domain.House.Shop.FamilyHouseShop>? houseResult = null;
    var audit = new AggregateAudit(batch.UpdatedBy, now);

    if (batch.Family is not null)
    {
      var pending = batch.Family;
      var entity = locked.Family;

      if (entity is null)
      {
        entity = FamilyPersistenceMapper.CreateEntity(pending.Root);
        _database.Families.Add(entity);
      }
      else
      {
        await DeleteFamilyChildrenAsync(batch.FamilyId.Value, cancellationToken);
        FamilyPersistenceMapper.Apply(entity, pending.Root);
      }

      SetRevision(entity, pending.ExpectedRevision, batch.UpdatedBy.Value, now);
      familyResult = new LoadedAggregate<LadirchenApp.Domain.Family.Family>(
        pending.Root,
        entity.Revision,
        audit
      );
    }

    if (batch.FamilyEconomy is not null)
    {
      var pending = batch.FamilyEconomy;
      var entity = locked.FamilyEconomy;

      if (entity is null)
      {
        entity = FamilyEconomyPersistenceMapper.CreateEntity(pending.Root, pending.Presentation);
        _database.FamilyEconomies.Add(entity);
      }
      else
      {
        await DeleteFamilyEconomyChildrenAsync(batch.FamilyId.Value, cancellationToken);
        FamilyEconomyPersistenceMapper.Apply(entity, pending.Root, pending.Presentation);
      }

      SetRevision(entity, pending.ExpectedRevision, batch.UpdatedBy.Value, now);
      economyResult = new LoadedFamilyEconomy(
        pending.Root,
        pending.Presentation,
        entity.Revision,
        audit
      );
    }

    if (batch.FamilyProgression is not null)
    {
      var pending = batch.FamilyProgression;
      var entity = locked.FamilyProgression;

      if (entity is null)
      {
        entity = FamilyProgressionPersistenceMapper.CreateEntity(pending.Root);
        _database.FamilyProgressions.Add(entity);
      }
      else
      {
        await DeleteFamilyProgressionChildrenAsync(batch.FamilyId.Value, cancellationToken);
        FamilyProgressionPersistenceMapper.Apply(entity, pending.Root);
      }

      SetRevision(entity, pending.ExpectedRevision, batch.UpdatedBy.Value, now);
      progressionResult = new LoadedAggregate<LadirchenApp.Domain.House.FamilyHouse>(
        pending.Root,
        entity.Revision,
        audit
      );
    }

    if (batch.HouseCustomization is not null)
    {
      var pending = batch.HouseCustomization;
      var entity = locked.HouseCustomization;

      if (entity is null)
      {
        entity = HouseCustomizationPersistenceMapper.CreateEntity(pending.Root);
        _database.HouseCustomizations.Add(entity);
      }
      else
      {
        await DeleteHouseCustomizationChildrenAsync(batch.FamilyId.Value, cancellationToken);
        HouseCustomizationPersistenceMapper.Apply(entity, pending.Root);
      }

      SetRevision(entity, pending.ExpectedRevision, batch.UpdatedBy.Value, now);
      houseResult = new LoadedAggregate<LadirchenApp.Domain.House.Shop.FamilyHouseShop>(
        pending.Root,
        entity.Revision,
        audit
      );
    }

    return new TypedAggregateSaveResult(
      familyResult,
      economyResult,
      progressionResult,
      houseResult
    );
  }

  private async Task<LockedTypedAggregateRows> LockTargetRowsAsync(
    Guid familyId,
    IReadOnlySet<TypedAggregateKind> targetKinds,
    CancellationToken cancellationToken
  )
  {
    var rows = new LockedTypedAggregateRows();

    if (targetKinds.Contains(TypedAggregateKind.Family))
    {
      rows.Family = await _database.Families
        .FromSqlInterpolated($"SELECT * FROM \"Families\" WHERE \"FamilyId\" = {familyId} FOR UPDATE")
        .SingleOrDefaultAsync(cancellationToken);
    }

    if (targetKinds.Contains(TypedAggregateKind.FamilyEconomy))
    {
      rows.FamilyEconomy = await _database.FamilyEconomies
        .FromSqlInterpolated($"SELECT * FROM \"FamilyEconomies\" WHERE \"FamilyId\" = {familyId} FOR UPDATE")
        .SingleOrDefaultAsync(cancellationToken);
    }

    if (targetKinds.Contains(TypedAggregateKind.FamilyProgression))
    {
      rows.FamilyProgression = await _database.FamilyProgressions
        .FromSqlInterpolated($"SELECT * FROM \"FamilyProgressions\" WHERE \"FamilyId\" = {familyId} FOR UPDATE")
        .SingleOrDefaultAsync(cancellationToken);
    }

    if (targetKinds.Contains(TypedAggregateKind.HouseCustomization))
    {
      rows.HouseCustomization = await _database.HouseCustomizations
        .FromSqlInterpolated($"SELECT * FROM \"HouseCustomizations\" WHERE \"FamilyId\" = {familyId} FOR UPDATE")
        .SingleOrDefaultAsync(cancellationToken);
    }

    return rows;
  }

  private static void ValidateExpectedRevisions(
    TypedAggregateSaveBatch batch,
    LockedTypedAggregateRows rows
  )
  {
    if (batch.Family is not null)
    {
      RequireRevision(
        TypedAggregateKind.Family,
        batch.Family.ExpectedRevision,
        rows.Family?.Revision ?? 0
      );
    }

    if (batch.FamilyEconomy is not null)
    {
      RequireRevision(
        TypedAggregateKind.FamilyEconomy,
        batch.FamilyEconomy.ExpectedRevision,
        rows.FamilyEconomy?.Revision ?? 0
      );
    }

    if (batch.FamilyProgression is not null)
    {
      RequireRevision(
        TypedAggregateKind.FamilyProgression,
        batch.FamilyProgression.ExpectedRevision,
        rows.FamilyProgression?.Revision ?? 0
      );
    }

    if (batch.HouseCustomization is not null)
    {
      RequireRevision(
        TypedAggregateKind.HouseCustomization,
        batch.HouseCustomization.ExpectedRevision,
        rows.HouseCustomization?.Revision ?? 0
      );
    }

    foreach (var guard in batch.RevisionGuards)
    {
      RequireRevision(guard.AggregateKind, guard.ExpectedRevision, rows.Revision(guard.AggregateKind));
    }
  }

  private static void RequireRevision(
    TypedAggregateKind kind,
    long expectedRevision,
    long actualRevision
  )
  {
    if (actualRevision != expectedRevision)
    {
      throw new FamilyAggregateConflictException(
        AggregateName(kind),
        expectedRevision,
        actualRevision
      );
    }
  }

  private static void ValidateRootIdentities(TypedAggregateSaveBatch batch)
  {
    var expected = batch.FamilyId;

    if (batch.Family is not null && batch.Family.Root.Id != expected)
    {
      throw new ArgumentException("The family root belongs to another family.", nameof(batch));
    }

    if (batch.FamilyEconomy is not null && batch.FamilyEconomy.Root.Id != expected)
    {
      throw new ArgumentException("The family economy belongs to another family.", nameof(batch));
    }

    if (batch.FamilyProgression is not null && batch.FamilyProgression.Root.Id != expected)
    {
      throw new ArgumentException("The family progression belongs to another family.", nameof(batch));
    }

    if (batch.HouseCustomization is not null && batch.HouseCustomization.Root.Id != expected)
    {
      throw new ArgumentException("The house customization belongs to another family.", nameof(batch));
    }
  }

  private static HashSet<TypedAggregateKind> TargetKinds(TypedAggregateSaveBatch batch)
  {
    var kinds = batch.RevisionGuards.Select(item => item.AggregateKind).ToHashSet();

    if (batch.Family is not null)
    {
      kinds.Add(TypedAggregateKind.Family);
    }

    if (batch.FamilyEconomy is not null)
    {
      kinds.Add(TypedAggregateKind.FamilyEconomy);
    }

    if (batch.FamilyProgression is not null)
    {
      kinds.Add(TypedAggregateKind.FamilyProgression);
    }

    if (batch.HouseCustomization is not null)
    {
      kinds.Add(TypedAggregateKind.HouseCustomization);
    }

    return kinds;
  }

  private void DetachTrackedTypedEntities(Guid familyId)
  {
    foreach (var entry in _database.ChangeTracker.Entries().ToArray())
    {
      if (
        entry.Metadata.ClrType.Namespace
          != "LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities"
        || entry.Metadata.FindProperty("FamilyId") is null
        || entry.Property("FamilyId").CurrentValue is not Guid entityFamilyId
        || entityFamilyId != familyId
      )
      {
        continue;
      }

      entry.State = EntityState.Detached;
    }
  }

  private async Task DeleteFamilyChildrenAsync(Guid familyId, CancellationToken cancellationToken)
  {
    await _database.Set<FamilyMemberEntity>()
      .Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<FamilyPetEntity>()
      .Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
  }

  private async Task DeleteFamilyEconomyChildrenAsync(
    Guid familyId,
    CancellationToken cancellationToken
  )
  {
    await DeleteEconomyPresentationsAsync(familyId, cancellationToken);
    await _database.Set<EconomyLedgerEntryEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyAppliedOperationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyRedemptionRequestEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyShopRewardEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyPromotionEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyContributionPartnerEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyContributionEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyUsedContributionIdEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await DeleteSavingsChildrenAsync(familyId, cancellationToken);
    await _database.Set<EconomyGuardianGiftEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomySettledInterestPeriodEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<FamilyEconomyWalletEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
  }

  private async Task DeleteSavingsChildrenAsync(Guid familyId, CancellationToken cancellationToken)
  {
    await _database.Set<EconomySavingsVoucherConsentEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomySavingsVoucherParticipantEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomySavingsVoucherEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyGoalCancellationConsentEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomyGoalCancellationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomySavingGoalRefundEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomySavingGoalParticipantEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<EconomySavingGoalEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
  }

  private async Task DeleteEconomyPresentationsAsync(
    Guid familyId,
    CancellationToken cancellationToken
  )
  {
    await _database.Set<ContributionPresentationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<SavingGoalPresentationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<ShopRewardPresentationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<GuardianGiftPresentationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
  }

  private async Task DeleteFamilyProgressionChildrenAsync(
    Guid familyId,
    CancellationToken cancellationToken
  )
  {
    await _database.Set<ClosedFamilyWeekEvaluationEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<ClosedFamilyWeekEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<FamilyProgressionParticipantEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
  }

  private async Task DeleteHouseCustomizationChildrenAsync(
    Guid familyId,
    CancellationToken cancellationToken
  )
  {
    await _database.Set<HouseAccessoryEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<HouseThemeEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<HouseRoomDesignEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
    await _database.Set<HousePlacementEntity>().Where(item => item.FamilyId == familyId)
      .ExecuteDeleteAsync(cancellationToken);
  }

  private static void SetRevision(
    FamilyRootEntity entity,
    long expectedRevision,
    string updatedBy,
    DateTimeOffset updatedAt
  )
  {
    entity.Revision = checked(expectedRevision + 1);
    entity.UpdatedBy = updatedBy;
    entity.UpdatedAt = updatedAt;
  }

  private static void SetRevision(
    FamilyEconomyRootEntity entity,
    long expectedRevision,
    string updatedBy,
    DateTimeOffset updatedAt
  )
  {
    entity.Revision = checked(expectedRevision + 1);
    entity.UpdatedBy = updatedBy;
    entity.UpdatedAt = updatedAt;
  }

  private static void SetRevision(
    FamilyProgressionRootEntity entity,
    long expectedRevision,
    string updatedBy,
    DateTimeOffset updatedAt
  )
  {
    entity.Revision = checked(expectedRevision + 1);
    entity.UpdatedBy = updatedBy;
    entity.UpdatedAt = updatedAt;
  }

  private static void SetRevision(
    HouseCustomizationRootEntity entity,
    long expectedRevision,
    string updatedBy,
    DateTimeOffset updatedAt
  )
  {
    entity.Revision = checked(expectedRevision + 1);
    entity.UpdatedBy = updatedBy;
    entity.UpdatedAt = updatedAt;
  }

  private static (TypedAggregateKind Kind, long ExpectedRevision) FirstWriteExpectation(
    TypedAggregateSaveBatch batch
  )
  {
    if (batch.Family is not null)
    {
      return (TypedAggregateKind.Family, batch.Family.ExpectedRevision);
    }

    if (batch.FamilyEconomy is not null)
    {
      return (TypedAggregateKind.FamilyEconomy, batch.FamilyEconomy.ExpectedRevision);
    }

    if (batch.FamilyProgression is not null)
    {
      return (TypedAggregateKind.FamilyProgression, batch.FamilyProgression.ExpectedRevision);
    }

    if (batch.HouseCustomization is not null)
    {
      return (TypedAggregateKind.HouseCustomization, batch.HouseCustomization.ExpectedRevision);
    }

    throw new InvalidOperationException("A concurrency failure requires a pending aggregate write.");
  }

  private static string AggregateName(TypedAggregateKind kind)
  {
    return kind switch
    {
      TypedAggregateKind.Family => "Family",
      TypedAggregateKind.FamilyEconomy => "FamilyEconomy",
      TypedAggregateKind.FamilyProgression => "FamilyProgression",
      TypedAggregateKind.HouseCustomization => "HouseCustomization",
      _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
  }

  private static bool IsRootInsertConflict(DbUpdateException exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    Exception? current = exception;

    while (current is not null)
    {
      if (current is PostgresException postgresException)
      {
        return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
          && postgresException.ConstraintName is
            "PK_Families"
            or "PK_FamilyEconomies"
            or "PK_FamilyProgressions"
            or "PK_HouseCustomizations";
      }

      current = current.InnerException;
    }

    return false;
  }

  private static async Task RollbackOwnedTransactionAsync(IDbContextTransaction? transaction)
  {
    if (transaction is null)
    {
      return;
    }

    try
    {
      await transaction.RollbackAsync(CancellationToken.None);
    }
    catch
    {
      // Preserve the original persistence failure.
    }
  }

}
