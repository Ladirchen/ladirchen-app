using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class ProgressionAndContributionCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("1222c10d-1ed8-4e88-b7c2-45640626408b"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly FamilyMemberId _supporterId = new("supporter");

  [Test]
  public async Task RefreshWithAnUncompletedAssignedBasicContributionUsesIdleLadiInterest()
  {
    var repository = CreateRepository(
      [
        Contribution(
          "daily-basic",
          "basic",
          _childId,
          Array.Empty<string>()
        ),
      ]
    );
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "progression.refresh", new
    {
    });

    var progression = repository.Required(_familyId, "family-progression").State;
    var childProgression = progression
      .GetProperty("memberProgressions")
      .GetProperty(_childId.Value);
    await Assert.That(childProgression.GetProperty("completionPercent").GetDecimal())
      .IsEqualTo(0);
    await Assert.That(childProgression.GetProperty("participationValue").GetDecimal())
      .IsEqualTo(1);
    await Assert.That(childProgression.GetProperty("ladiStageId").GetString())
      .IsEqualTo("idle-ladi");
    await Assert.That(childProgression.GetProperty("ladiStagePosition").GetInt32())
      .IsEqualTo(1);
    await Assert.That(childProgression.GetProperty("savingsInterestRate").GetDecimal())
      .IsEqualTo(1.33m);
    await Assert.That(progression.GetProperty("maximumSavingsInterestRate").GetDecimal())
      .IsEqualTo(8);
    await Assert.That(repository.Required(_familyId, "family-profile").Revision).IsEqualTo(1);
    await Assert.That(repository.Required(_familyId, "contributions").Revision).IsEqualTo(1);
    await Assert.That(repository.LastRevisionGuards.Select(item => item.AggregateType))
      .IsEquivalentTo(["contributions", "family-profile"]);
  }

  [Test]
  public async Task SetPartnersPersistsAnActiveGuardianAndChildTeamThroughTheDomainModel()
  {
    var repository = CreateRepository(
      [
        Contribution(
          "family-project",
          "extra",
          _guardianId,
          Array.Empty<string>()
        ),
      ]
    );
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "contributions.set-partners", new
    {
      id = "family-project",
      partnerIds = new[] { _childId.Value },
    });

    var contribution = repository.Required(_familyId, "contributions")
      .State
      .GetProperty("contributions")
      .EnumerateArray()
      .Single();
    var partnerIds = contribution.GetProperty("invitedChildIds")
      .EnumerateArray()
      .Select(item => item.GetString() ?? throw new InvalidOperationException("A partner ID is required."))
      .ToArray();
    await Assert.That(contribution.GetProperty("assigneeId").GetString())
      .IsEqualTo(_guardianId.Value);
    await Assert.That(partnerIds).IsEquivalentTo([_childId.Value]);
    await Assert.That(contribution.GetProperty("description").GetString())
      .IsEqualTo("Stored presentation");
    await Assert.That(repository.Required(_familyId, "family-progression").Revision)
      .IsEqualTo(2);
    await Assert.That(repository.LastRevisionGuards.Select(item => item.AggregateType))
      .IsEquivalentTo(["family-profile"]);
  }

  [Test]
  public async Task SupporterCannotActAsAChildInsideTheCommandUnitOfWork()
  {
    var repository = CreateRepository([]);
    var handler = CreateHandler(repository);

    await Assert.That(async () =>
      await ExecuteAsync(
        handler,
        _childId,
        "progression.refresh",
        new { },
        _supporterId
      )
    ).Throws<DomainRuleException>();
    await Assert.That(repository.Required(_familyId, "family-profile").Revision).IsEqualTo(1);
    await Assert.That(repository.Required(_familyId, "family-progression").Revision).IsEqualTo(1);
  }

  [Test]
  public async Task AClosedFamilyWeekCannotBeClosedAgain()
  {
    var repository = CreateRepository(
      [
        new
        {
          id = "approved-basic",
          title = "Approved contribution",
          description = "Stored presentation",
          icon = "approved-icon",
          kind = "basic",
          status = "approved",
          reward = 10,
          energy = 100,
          assigneeId = _childId.Value,
          invitedChildIds = Array.Empty<string>(),
          stars = 5,
          earnedReward = 10,
          earnedRatingBonus = 1,
          earnedPromotionMultiplier = 1,
          approvedAt = "2026-10-12T08:00:00Z",
        },
      ]
    );
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "progression.complete-week", new { });
    var revisionAfterFirstClosure = repository.Required(_familyId, "family-progression").Revision;
    await Assert.That(async () =>
      await ExecuteAsync(handler, _guardianId, "progression.complete-week", new { })
    ).Throws<DomainRuleException>();

    var progression = repository.Required(_familyId, "family-progression");
    await Assert.That(progression.Revision).IsEqualTo(revisionAfterFirstClosure);
    await Assert.That(progression.State.GetProperty("closedWeekOutcomes").EnumerateObject().Count())
      .IsEqualTo(1);
    await Assert.That(progression.State.GetProperty("weeklyEvaluations").EnumerateObject().Count())
      .IsEqualTo(1);
  }

  [Test]
  public async Task ConfiguredPerfectRatingBonusIsRestoredAndUsedByContributionApproval()
  {
    var repository = CreateRepository(
      [
        Contribution(
          "daily-basic",
          "basic",
          _childId,
          Array.Empty<string>()
        ),
      ]
    );
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "contributions.set-perfect-rating-bonus", new
    {
      percent = 25,
    });
    await ExecuteAsync(handler, _childId, "contributions.submit", new
    {
      id = "daily-basic",
    });
    await ExecuteAsync(handler, _guardianId, "contributions.approve", new
    {
      id = "daily-basic",
      stars = 5,
    });

    var contributions = repository.Required(_familyId, "contributions").State;
    var savings = repository.Required(_familyId, "savings").State;
    await Assert.That(contributions.GetProperty("perfectRatingBonusPercent").GetDecimal())
      .IsEqualTo(25);
    await Assert.That(savings.GetProperty("balances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(13);
  }

  [Test]
  public async Task DeletedContributionIdentityRemainsUsedAfterTheNextReload()
  {
    var repository = CreateRepository(
      [
        Contribution(
          "daily-basic",
          "basic",
          _childId,
          Array.Empty<string>()
        ),
      ]
    );
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "contributions.delete", new
    {
      id = "daily-basic",
    });

    var contributionsAfterDelete = repository.Required(_familyId, "contributions");
    var usedIds = contributionsAfterDelete.State
      .GetProperty("usedContributionIds")
      .EnumerateArray()
      .Select(item => item.GetString())
      .ToArray();
    await Assert.That(contributionsAfterDelete.SchemaVersion)
      .IsEqualTo(FamilyAggregateSchema.GetVersion("contributions"));
    await Assert.That(usedIds).Contains("daily-basic");

    await Assert.That(async () =>
      await ExecuteAsync(handler, _guardianId, "contributions.add", new
      {
        id = "daily-basic",
        title = "Reused contribution",
        description = "A reused identity must remain forbidden.",
        icon = "reused-icon",
        kind = "basic",
        reward = 10,
        energy = 25,
        assigneeId = _childId.Value,
      })
    ).Throws<DomainRuleException>();

    var contributionsAfterRetry = repository.Required(_familyId, "contributions");
    await Assert.That(contributionsAfterRetry.Revision).IsEqualTo(contributionsAfterDelete.Revision);
    await Assert.That(contributionsAfterRetry.State.GetProperty("contributions").GetArrayLength())
      .IsEqualTo(0);
  }

  [Test]
  public async Task ContributionApprovalRestoresAppliedOperationsFromSavings()
  {
    var replayedCommandId = Guid.Parse("cc34726e-dda7-4427-9c1f-2461b0cdb7bf");
    var repository = CreateRepository(
      [
        Contribution(
          "daily-basic",
          "basic",
          _childId,
          Array.Empty<string>()
        ),
      ]
    );
    repository.Seed(Snapshot("savings", 3, new
    {
      balances = new
      {
        guardian = 0,
        child = 0,
      },
      appliedOperationIds = new[] { replayedCommandId.ToString("N") },
      ledger = Array.Empty<object>(),
    }));
    var handler = CreateHandler(repository);
    await ExecuteAsync(handler, _childId, "contributions.submit", new
    {
      id = "daily-basic",
    });

    await Assert.That(async () =>
      await ExecuteAsync(handler, _guardianId, "contributions.approve", new
      {
        id = "daily-basic",
        stars = 5,
      }, commandId: replayedCommandId)
    ).Throws<DomainRuleException>();

    var contribution = repository.Required(_familyId, "contributions")
      .State
      .GetProperty("contributions")
      .EnumerateArray()
      .Single();
    await Assert.That(contribution.GetProperty("status").GetString()).IsEqualTo("pending");
    await Assert.That(
      repository.Required(_familyId, "savings")
        .State
        .GetProperty("balances")
        .GetProperty(_childId.Value)
        .GetDecimal()
    ).IsEqualTo(0);
  }

  private static ExecuteFamilyCommandHandler CreateHandler(
    TestFamilyAggregateSnapshotRepository repository
  )
  {
    return new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero))
    );
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository(
    object[] contributions
  )
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(Snapshot("family-profile", 2, new
    {
      timeZone = "Europe/Zurich",
      pets = Array.Empty<object>(),
      members = new object[]
      {
        new
        {
          id = _guardianId.Value,
          name = "Guardian",
          role = "guardian",
          guardianAccess = "admin",
          invitationPending = false,
          participatesInWeeklyGoal = false,
          weeklyStreak = 0,
        },
        new
        {
          id = _childId.Value,
          name = "Child",
          role = "child",
          invitationPending = false,
          participatesInWeeklyGoal = true,
          weeklyStreak = 0,
        },
        new
        {
          id = _supporterId.Value,
          name = "Supporter",
          role = "guardian",
          guardianAccess = "supporter",
          invitationPending = false,
          participatesInWeeklyGoal = false,
          weeklyStreak = 0,
        },
      },
    }));
    repository.Seed(Snapshot("contributions", 1, new
    {
      contributions,
      perfectRatingBonusPercent = 5,
      promotions = Array.Empty<object>(),
    }));
    repository.Seed(Snapshot("family-progression", 4, new
    {
      completedWeeklyStreak = 0,
      closedWeekOutcomes = new
      {
      },
      currentWeekTarget = 7,
      familyEnergy = 0,
      houseLevel = 0,
      houseVisualLevel = 0,
      maximumSavingsInterestRate = 8,
      memberProgressions = new
      {
      },
      weeklyEvaluations = new
      {
      },
    }));
    repository.Seed(Snapshot("savings", 2, new
    {
      balances = new
      {
        guardian = 0,
        child = 0,
      },
    }));

    return repository;
  }

  private static object Contribution(
    string id,
    string kind,
    FamilyMemberId assigneeId,
    string[] partnerIds
  )
  {
    return new
    {
      id,
      title = "Family contribution",
      description = "Stored presentation",
      icon = "team-icon",
      kind,
      status = "available",
      reward = 10,
      energy = 100,
      assigneeId = assigneeId.Value,
      invitedChildIds = partnerIds,
    };
  }

  private static FamilyAggregateSnapshot Snapshot(
    string aggregateType,
    int schemaVersion,
    object state
  )
  {
    return new FamilyAggregateSnapshot(
      _familyId,
      aggregateType,
      schemaVersion,
      1,
      JsonSerializer.SerializeToElement(state),
      _guardianId,
      DateTimeOffset.UnixEpoch
    );
  }

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload,
    FamilyMemberId? authenticatedMemberId = null,
    Guid? commandId = null
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        commandId ?? Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload),
        authenticatedMemberId
      ),
      CancellationToken.None
    );
  }
}
