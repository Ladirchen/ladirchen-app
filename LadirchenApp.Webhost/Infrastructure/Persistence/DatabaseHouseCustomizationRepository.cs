using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class DatabaseHouseCustomizationRepository : IHouseCustomizationRepository
{
  private readonly LadirchenDbContext _database;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public DatabaseHouseCustomizationRepository(
    LadirchenDbContext database,
    ITypedAggregateUnitOfWork unitOfWork
  )
  {
    _database = database;
    _unitOfWork = unitOfWork;
  }

  public async Task<LoadedAggregate<FamilyHouseShop>?> LoadAsync(
    Family family,
    int currentHouseLevel,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    var entity = await _database.HouseCustomizations
      .AsNoTracking()
      .AsSplitQuery()
      .Include(item => item.Accessories)
      .Include(item => item.Themes)
      .Include(item => item.RoomDesigns)
      .Include(item => item.Placements)
      .SingleOrDefaultAsync(item => item.FamilyId == family.Id.Value, cancellationToken);

    if (entity is null)
    {
      return null;
    }

    return new LoadedAggregate<FamilyHouseShop>(
      HouseCustomizationPersistenceMapper.ToDomain(entity, family, currentHouseLevel),
      entity.Revision,
      new AggregateAudit(new FamilyMemberId(entity.UpdatedBy), entity.UpdatedAt)
    );
  }

  public async Task<LoadedAggregate<FamilyHouseShop>> SaveAsync(
    FamilyHouseShop root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(root);
    var batch = new TypedAggregateSaveBatch(
      root.Id,
      updatedBy,
      houseCustomization: new PendingHouseCustomizationSave(root, expectedRevision)
    );
    var result = await _unitOfWork.SaveAsync(batch, cancellationToken);

    return result.HouseCustomization
      ?? throw new InvalidOperationException("The house-customization save did not return its aggregate.");
  }
}
