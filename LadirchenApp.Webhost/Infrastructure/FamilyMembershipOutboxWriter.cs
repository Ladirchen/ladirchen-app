using System;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class FamilyMembershipOutboxWriter
{
  private readonly LadirchenDbContext _database;
  private readonly TimeProvider _timeProvider;

  public FamilyMembershipOutboxWriter(
    LadirchenDbContext database,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _timeProvider = timeProvider;
  }

  public void AddChanges(TypedAggregateSaveResult result, FamilyId familyId)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(familyId);
    var family = result.Family
      ?? throw new InvalidOperationException("The family membership change was not persisted.");
    var economy = result.FamilyEconomy
      ?? throw new InvalidOperationException("The family economy membership change was not persisted.");
    var progression = result.FamilyProgression
      ?? throw new InvalidOperationException("The family progression membership change was not persisted.");
    var house = result.HouseCustomization
      ?? throw new InvalidOperationException("The house membership change was not persisted.");
    Add(familyId, "family-profile", family.Revision, family.Audit);
    Add(familyId, "contributions", economy.Revision, economy.Audit);
    Add(familyId, "savings", economy.Revision, economy.Audit);
    Add(familyId, "reward-shop", economy.Revision, economy.Audit);
    Add(familyId, "family-progression", progression.Revision, progression.Audit);
    Add(familyId, "home-customization", house.Revision, house.Audit);
  }

  private void Add(
    FamilyId familyId,
    string aggregateType,
    long revision,
    AggregateAudit audit
  )
  {
    _database.FamilyAggregateOutboxMessages.Add(
      new FamilyAggregateOutboxMessageEntity
      {
        AggregateType = aggregateType,
        FamilyId = familyId.Value,
        MessageId = Guid.NewGuid(),
        OccurredAt = _timeProvider.GetUtcNow(),
        Revision = revision,
        UpdatedAt = audit.UpdatedAt,
        UpdatedBy = audit.UpdatedBy.Value,
      }
    );
  }
}