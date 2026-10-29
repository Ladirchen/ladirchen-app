using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class DatabaseFamilyEconomyRepository : IFamilyEconomyRepository
{
  private readonly LadirchenDbContext _database;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public DatabaseFamilyEconomyRepository(
    LadirchenDbContext database,
    ITypedAggregateUnitOfWork unitOfWork
  )
  {
    _database = database;
    _unitOfWork = unitOfWork;
  }

  public async Task<LoadedFamilyEconomy?> LoadAsync(
    Family family,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    var entity = await EconomyQuery()
      .SingleOrDefaultAsync(item => item.FamilyId == family.Id.Value, cancellationToken);

    if (entity is null)
    {
      return null;
    }

    return new LoadedFamilyEconomy(
      FamilyEconomyPersistenceMapper.ToDomain(entity, family),
      FamilyEconomyPersistenceMapper.ToPresentation(entity),
      entity.Revision,
      new AggregateAudit(new FamilyMemberId(entity.UpdatedBy), entity.UpdatedAt)
    );
  }

  public async Task<LoadedFamilyEconomy> SaveAsync(
    FamilyEconomy root,
    FamilyEconomyPresentationState presentation,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(root);
    var batch = new TypedAggregateSaveBatch(
      root.Id,
      updatedBy,
      familyEconomy: new PendingFamilyEconomySave(root, presentation, expectedRevision)
    );
    var result = await _unitOfWork.SaveAsync(batch, cancellationToken);

    return result.FamilyEconomy
      ?? throw new InvalidOperationException("The family-economy save did not return its aggregate.");
  }

  private IQueryable<FamilyEconomyRootEntity> EconomyQuery()
  {
    return _database.FamilyEconomies
      .AsNoTracking()
      .AsSplitQuery()
      .Include(item => item.Wallets)
      .Include(item => item.Contributions).ThenInclude(item => item.InvitedPartners)
      .Include(item => item.UsedContributionIds)
      .Include(item => item.Promotions)
      .Include(item => item.SavingGoals).ThenInclude(item => item.Participants)
      .Include(item => item.SavingGoals).ThenInclude(item => item.RefundableContributions)
      .Include(item => item.SavingGoals).ThenInclude(item => item.CancellationRequest!)
        .ThenInclude(item => item.Consents)
      .Include(item => item.SavingsVouchers).ThenInclude(item => item.Participants)
      .Include(item => item.SavingsVouchers).ThenInclude(item => item.Consents)
      .Include(item => item.GuardianGifts)
      .Include(item => item.SettledInterestPeriods)
      .Include(item => item.ShopRewards)
      .Include(item => item.RedemptionRequests)
      .Include(item => item.AppliedOperations)
      .Include(item => item.LedgerEntries)
      .Include(item => item.ContributionPresentations)
      .Include(item => item.SavingGoalPresentations)
      .Include(item => item.ShopRewardPresentations)
      .Include(item => item.GuardianGiftPresentations);
  }
}
