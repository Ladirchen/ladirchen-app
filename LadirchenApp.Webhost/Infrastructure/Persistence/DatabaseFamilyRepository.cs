using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class DatabaseFamilyRepository : IFamilyRepository
{
  private readonly LadirchenDbContext _database;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public DatabaseFamilyRepository(
    LadirchenDbContext database,
    ITypedAggregateUnitOfWork unitOfWork
  )
  {
    _database = database;
    _unitOfWork = unitOfWork;
  }

  public async Task<IReadOnlyList<FamilyId>> ListIdsAsync(
    CancellationToken cancellationToken
  )
  {
    return await _database.Families
      .AsNoTracking()
      .OrderBy(item => item.FamilyId)
      .Select(item => new FamilyId(item.FamilyId))
      .ToArrayAsync(cancellationToken);
  }

  public async Task<LoadedAggregate<Family>?> LoadAsync(
    FamilyId familyId,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(familyId);
    var entity = await _database.Families
      .AsNoTracking()
      .AsSplitQuery()
      .Include(item => item.Members)
      .Include(item => item.Pets)
      .SingleOrDefaultAsync(item => item.FamilyId == familyId.Value, cancellationToken);

    if (entity is null)
    {
      return null;
    }

    return new LoadedAggregate<Family>(
      FamilyPersistenceMapper.ToDomain(entity),
      entity.Revision,
      new AggregateAudit(new FamilyMemberId(entity.UpdatedBy), entity.UpdatedAt)
    );
  }

  public async Task<LoadedAggregate<Family>> SaveAsync(
    Family root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(root);
    var batch = new TypedAggregateSaveBatch(
      root.Id,
      updatedBy,
      family: new PendingFamilySave(root, expectedRevision)
    );
    var result = await _unitOfWork.SaveAsync(batch, cancellationToken);

    return result.Family
      ?? throw new InvalidOperationException("The family save did not return its aggregate.");
  }
}
