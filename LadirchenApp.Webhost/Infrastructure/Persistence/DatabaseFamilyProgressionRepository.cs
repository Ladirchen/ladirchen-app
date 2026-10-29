using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class DatabaseFamilyProgressionRepository : IFamilyProgressionRepository
{
  private readonly LadirchenDbContext _database;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public DatabaseFamilyProgressionRepository(
    LadirchenDbContext database,
    ITypedAggregateUnitOfWork unitOfWork
  )
  {
    _database = database;
    _unitOfWork = unitOfWork;
  }

  public async Task<LoadedAggregate<FamilyHouse>?> LoadAsync(
    Family family,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    var entity = await _database.FamilyProgressions
      .AsNoTracking()
      .AsSplitQuery()
      .Include(item => item.Participants)
      .Include(item => item.ClosedWeeks).ThenInclude(item => item.MemberEvaluations)
      .SingleOrDefaultAsync(item => item.FamilyId == family.Id.Value, cancellationToken);

    if (entity is null)
    {
      return null;
    }

    return new LoadedAggregate<FamilyHouse>(
      FamilyProgressionPersistenceMapper.ToDomain(entity, family),
      entity.Revision,
      new AggregateAudit(new FamilyMemberId(entity.UpdatedBy), entity.UpdatedAt)
    );
  }

  public async Task<LoadedAggregate<FamilyHouse>> SaveAsync(
    FamilyHouse root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(root);
    var batch = new TypedAggregateSaveBatch(
      root.Id,
      updatedBy,
      familyProgression: new PendingFamilyProgressionSave(root, expectedRevision)
    );
    var result = await _unitOfWork.SaveAsync(batch, cancellationToken);

    return result.FamilyProgression
      ?? throw new InvalidOperationException("The family-progression save did not return its aggregate.");
  }
}
