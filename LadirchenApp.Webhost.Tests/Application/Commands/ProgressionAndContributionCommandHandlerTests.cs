using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class ProgressionAndContributionCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("1222c10d-1ed8-4e88-b7c2-45640626408b"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly FamilyMemberId _supporterId = new("supporter");
  private static readonly DateTimeOffset _now = new(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task RefreshKeepsBusinessStateAndGuardsItsDecisionInputs()
  {
    var store = CreateStore(Contribution("daily-basic", ContributionKind.Basic, _childId));
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "progression.refresh", new
    {
    });

    var progression = store.RequiredProgression();
    await Assert.That(progression.MaximumSavingsInterestRate.Value).IsEqualTo(8);
    await Assert.That(progression.StoredLevel).IsEqualTo(0);
    await Assert.That(progression.CompletedWeeklyStreak).IsEqualTo(0);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyProgression)).IsEqualTo(2);
    await Assert.That(store.Revision(TypedAggregateKind.Family)).IsEqualTo(1);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy)).IsEqualTo(1);
    await Assert.That(store.LastRevisionGuards.Select(item => item.AggregateKind))
      .IsEquivalentTo([TypedAggregateKind.Family, TypedAggregateKind.FamilyEconomy]);
  }

  [Test]
  public async Task SetPartnersPersistsAnActiveGuardianAndChildTeamThroughTheDomainModel()
  {
    var store = CreateStore(Contribution("family-project", ContributionKind.Extra, _guardianId));
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.set-partners", new
    {
      id = "family-project",
      partnerIds = new[] { _childId.Value },
    });

    var contribution = store.RequiredEconomy().Contributions.Single();
    var presentation = store.EconomyPresentation.Contributions.Single();
    await Assert.That(contribution.AssigneeId).IsEqualTo(_guardianId);
    await Assert.That(contribution.InvitedPartners).IsEquivalentTo([_childId]);
    await Assert.That(presentation.Description).IsEqualTo("Stored presentation");
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy)).IsEqualTo(2);
    await Assert.That(store.LastRevisionGuards.Select(item => item.AggregateKind))
      .IsEquivalentTo([TypedAggregateKind.Family]);
  }

  [Test]
  public async Task AvailableContributionsCanBeEditedAndCopiedWithoutAnAssignment()
  {
    var store = CreateStore(Contribution("family-project", ContributionKind.Extra, _childId));
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.update", new
    {
      id = "family-project",
      title = "Updated family project",
      description = "Updated presentation",
      icon = "updated-icon",
      category = "Hausaufgabe",
      kind = "basic",
      reward = 25,
      energy = 30,
      assigneeId = _supporterId.Value,
    });
    await ExecuteAsync(handler, _guardianId, "contributions.copy", new
    {
      id = "family-project-copy",
      sourceId = "family-project",
    });

    var economy = store.RequiredEconomy();
    var updated = economy.Contributions.Single(item => item.Id.Value == "family-project");
    var copy = economy.Contributions.Single(item => item.Id.Value == "family-project-copy");
    var copiedPresentation = store.EconomyPresentation.Contributions
      .Single(item => item.ContributionId == copy.Id);
    await Assert.That(updated.Title).IsEqualTo("Updated family project");
    await Assert.That(updated.AssigneeId).IsEqualTo(_supporterId);
    await Assert.That(copy.Title).IsEqualTo(updated.Title);
    await Assert.That(copy.AssigneeId).IsNull();
    await Assert.That(copiedPresentation.Description).IsEqualTo("Updated presentation");
    await Assert.That(copiedPresentation.Icon).IsEqualTo("updated-icon");
    await Assert.That(copiedPresentation.Category).IsEqualTo("Hausaufgabe");
  }

  [Test]
  public async Task OnlyAdministratorCanCreateAContributionHiddenFromChildren()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();
    var payload = new
    {
      id = "hidden-task",
      title = "Hidden task",
      description = "Hidden until it is ready",
      icon = "hidden-icon",
      category = "School",
      kind = "school",
      reward = 5,
      energy = 10,
      assigneeId = _childId.Value,
      isVisibleToChildren = false,
    };

    await Assert.That(async () =>
      await ExecuteAsync(handler, _supporterId, "contributions.add", payload)
    ).Throws<DomainRuleException>();
    await ExecuteAsync(handler, _guardianId, "contributions.add", payload);

    await Assert.That(store.EconomyPresentation.Contributions.Single().IsVisibleToChildren)
      .IsFalse();
  }

  [Test]
  public async Task SupporterCannotActAsAChildInsideTheCommandUnitOfWork()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();

    await Assert.That(async () =>
      await ExecuteAsync(
        handler,
        _childId,
        "progression.refresh",
        new
        {
        },
        _supporterId
      )
    ).Throws<DomainRuleException>();
    await Assert.That(store.Revision(TypedAggregateKind.Family)).IsEqualTo(1);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyProgression)).IsEqualTo(1);
  }

  [Test]
  public async Task AClosedFamilyWeekCannotBeClosedAgain()
  {
    var store = CreateStore(ApprovedContribution());
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "progression.complete-week", new
    {
    });
    var revisionAfterFirstClosure = store.Revision(TypedAggregateKind.FamilyProgression);
    await Assert.That(async () =>
      await ExecuteAsync(handler, _guardianId, "progression.complete-week", new
      {
      })
    ).Throws<DomainRuleException>();

    var progression = store.RequiredProgression();
    await Assert.That(store.Revision(TypedAggregateKind.FamilyProgression))
      .IsEqualTo(revisionAfterFirstClosure);
    await Assert.That(progression.ClosedWeeks.Count).IsEqualTo(1);
  }

  [Test]
  public async Task ConfiguredPerfectRatingBonusIsRestoredAndUsedByContributionApproval()
  {
    var store = CreateStore(Contribution("daily-basic", ContributionKind.Basic, _childId));
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.set-perfect-rating-bonus", new
    {
      percent = 25,
    });
    await ExecuteAsync(handler, _childId, "contributions.submit", new
    {
      id = "daily-basic",
    });
    var result = await ExecuteAsync(handler, _guardianId, "contributions.approve", new
    {
      id = "daily-basic",
      stars = 5,
    });

    var economy = store.RequiredEconomy();
    await Assert.That(economy.ContributionSettings.PerfectRatingBonus.Value).IsEqualTo(25);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(13);
    await Assert.That(result.UserNotifications).HasSingleItem();
    var notification = result.UserNotifications.Single();
    await Assert.That(notification.RecipientId).IsEqualTo(_childId);
    await Assert.That(notification.BodyDe).Contains("13 Ladirchen");
  }

  [Test]
  public async Task ApprovingAWeeklyContributionCreatesTheNextScheduledOccurrence()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.add", new
    {
      id = "weekly-homework",
      title = "Weekly homework",
      description = "Prepare the school bag.",
      icon = "school",
      category = "Hausaufgabe",
      kind = "school",
      reward = 10,
      energy = 0,
      assigneeId = _childId.Value,
      dueDate = "2026-10-12",
      recurrence = "weekly",
    });
    await ExecuteAsync(handler, _childId, "contributions.submit", new
    {
      id = "weekly-homework",
    });
    await ExecuteAsync(handler, _guardianId, "contributions.approve", new
    {
      id = "weekly-homework",
      stars = 4,
    });

    var next = store.RequiredEconomy().Contributions.Single(item => item.Status == ContributionStatus.Available);
    var nextPresentation = store.EconomyPresentation.Contributions
      .Single(item => item.ContributionId == next.Id);
    await Assert.That(next.AssigneeId).IsEqualTo(_childId);
    await Assert.That(nextPresentation.DueDate).IsEqualTo(new DateOnly(2026, 10, 19));
    await Assert.That(nextPresentation.Recurrence).IsEqualTo("weekly");
  }

  [Test]
  public async Task AssigningAContributionCreatesARecipientNotification()
  {
    var store = CreateStore(Contribution("daily-basic", ContributionKind.Basic, _childId));
    var handler = store.CreateCommandHandler();

    var result = await ExecuteAsync(handler, _guardianId, "contributions.assign", new
    {
      id = "daily-basic",
      memberId = _supporterId.Value,
    });

    await Assert.That(result.UserNotifications).HasSingleItem();
    var notification = result.UserNotifications.Single();
    await Assert.That(notification.RecipientId).IsEqualTo(_supporterId);
    await Assert.That(notification.BodyEn).Contains("Family contribution");
  }

  [Test]
  public async Task DeletedContributionIdentityAndPresentationStayDeletedAfterReload()
  {
    var store = CreateStore(Contribution("daily-basic", ContributionKind.Basic, _childId));
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.delete", new
    {
      id = "daily-basic",
    });

    var economyAfterDelete = store.RequiredEconomy();
    await Assert.That(economyAfterDelete.UsedContributionIds)
      .Contains(new ContributionId("daily-basic"));
    await Assert.That(economyAfterDelete.Contributions).IsEmpty();
    await Assert.That(store.EconomyPresentation.Contributions).IsEmpty();
    var revisionAfterDelete = store.Revision(TypedAggregateKind.FamilyEconomy);

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

    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy))
      .IsEqualTo(revisionAfterDelete);
    await Assert.That(store.RequiredEconomy().Contributions).IsEmpty();
  }

  [Test]
  public async Task ContributionApprovalRestoresAppliedOperationsFromTheEconomy()
  {
    var replayedCommandId = Guid.Parse("cc34726e-dda7-4427-9c1f-2461b0cdb7bf");
    var store = CreateStore(
      [Contribution("daily-basic", ContributionKind.Basic, _childId)],
      [new TransactionId(replayedCommandId.ToString("N"))]
    );
    var handler = store.CreateCommandHandler();
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

    var economy = store.RequiredEconomy();
    await Assert.That(economy.Contributions.Single().Status)
      .IsEqualTo(ContributionStatus.Pending);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(0);
  }

  [Test]
  public async Task FocusPreparationsTrackProgressAndPayTheFiveStarBonusOnce()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.add-focus", new
    {
      id = "english-test",
      title = "English test",
      description = "Prepare Unit 2",
      icon = "🎯",
      period = "date",
      startDate = "2026-10-12",
      endDate = "2026-10-18",
      assigneeId = _childId.Value,
      completionBonus = 5,
    });
    await ExecuteAsync(handler, _guardianId, "contributions.add-focus-preparations", new
    {
      focusId = "english-test",
      ids = new[] { "english-test-1", "english-test-2" },
      dates = new[] { "2026-10-12", "2026-10-14" },
      minutes = 30,
      reward = 2,
    });

    foreach (var id in new[] { "english-test-1", "english-test-2" })
    {
      await ExecuteAsync(handler, _childId, "contributions.submit", new
      {
        id,
      });
    }

    await ExecuteAsync(handler, _guardianId, "contributions.approve", new
    {
      id = "english-test-1",
      stars = 4,
    });
    var result = await ExecuteAsync(handler, _guardianId, "contributions.approve", new
    {
      id = "english-test-2",
      stars = 5,
    });

    var economy = store.RequiredEconomy();
    var focusPresentation = store.EconomyPresentation.Contributions
      .Single(item => item.ContributionId.Value == "english-test");
    await Assert.That(focusPresentation.IsFocus).IsTrue();
    await Assert.That(focusPresentation.FocusBonusPaid).IsTrue();
    await Assert.That(focusPresentation.FocusCompleted).IsTrue();
    await Assert.That(economy.Ledger.Count(item => item.Reason.Contains("5-Sterne-Abschlussbonus")))
      .IsEqualTo(1);
    var completionNotification = result.UserNotifications
      .Single(item => item.Tag == "focus-completed-english-test");
    await Assert.That(completionNotification.RecipientId).IsEqualTo(_childId);
    await Assert.That(completionNotification.TitleDe).IsEqualTo("Fokusziel erreicht");
  }

  [Test]
  public async Task CompletingFocusPreparationsClosesTheGoalAndNotifiesTheChildWithoutABonus()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "contributions.add-focus", new
    {
      id = "reading-goal",
      title = "Read a chapter",
      description = "Read every day",
      icon = "📖",
      period = "weekly",
      startDate = "2026-10-12",
      endDate = "2026-10-18",
      assigneeId = _childId.Value,
      completionBonus = 0,
    });
    await ExecuteAsync(handler, _guardianId, "contributions.add-focus-preparations", new
    {
      focusId = "reading-goal",
      ids = new[] { "reading-goal-1" },
      dates = new[] { "2026-10-12" },
      minutes = 20,
      reward = 1,
    });
    await ExecuteAsync(handler, _childId, "contributions.submit", new
    {
      id = "reading-goal-1",
    });
    var result = await ExecuteAsync(handler, _guardianId, "contributions.approve", new
    {
      id = "reading-goal-1",
      stars = 4,
    });

    var focusPresentation = store.EconomyPresentation.Contributions
      .Single(item => item.ContributionId.Value == "reading-goal");
    await Assert.That(focusPresentation.FocusCompleted).IsTrue();
    await Assert.That(focusPresentation.FocusBonusPaid).IsFalse();
    await Assert.That(store.RequiredEconomy().Ledger.Any(
      item => item.Reason.Contains("5-Sterne-Abschlussbonus")
    )).IsFalse();
    var completionNotification = result.UserNotifications
      .Single(item => item.Tag == "focus-completed-reading-goal");
    await Assert.That(completionNotification.RecipientId).IsEqualTo(_childId);
    await Assert.That(completionNotification.BodyDe).Contains("Stark gemacht");
  }

  private static TestTypedAggregateStore CreateStore(params ContributionSeed[] contributions)
  {
    return CreateStore(contributions, []);
  }

  private static TestTypedAggregateStore CreateStore(
    ContributionSeed[] contributions,
    TransactionId[] appliedOperations
  )
  {
    var family = new Family(_familyId, _guardianId, "Guardian", new IanaTimeZone("Europe/Zurich"));
    family.AddMember(
      _guardianId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    family.AddMember(
      _guardianId,
      _supporterId,
      "Supporter",
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      false,
      _now
    );
    var economy = new FamilyEconomy(family);

    foreach (var contribution in contributions)
    {
      economy.RestoreContribution(
        contribution.Id,
        new ContributionDefinition(
          "Family contribution",
          contribution.Kind,
          new LadirchenAmount(10),
          new Percentage(100),
          contribution.AssigneeId
        ),
        contribution.Status,
        contribution.Rating,
        contribution.EarnedReward,
        contribution.ApprovedAt,
        contribution.PartnerIds
      );
    }

    foreach (var operationId in appliedOperations)
    {
      economy.RestoreAppliedOperation(operationId);
    }

    var presentation = new FamilyEconomyPresentationState(
      contributions.Select(item => new ContributionPresentationState(
        item.Id,
        "Stored presentation",
        "team-icon",
        false
      )),
      [],
      [],
      []
    );
    var store = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    store.Seed(
      family,
      economy,
      presentation,
      FamilyHouse.CreateInitial(family)
    );

    return store;
  }

  private static ContributionSeed Contribution(
    string id,
    ContributionKind kind,
    FamilyMemberId assigneeId
  )
  {
    return new ContributionSeed(
      new ContributionId(id),
      kind,
      assigneeId,
      [],
      ContributionStatus.Available,
      null,
      null,
      null
    );
  }

  private static ContributionSeed ApprovedContribution()
  {
    return new ContributionSeed(
      new ContributionId("approved-basic"),
      ContributionKind.Basic,
      _childId,
      [],
      ContributionStatus.Approved,
      new StarRating(5),
      new ContributionRewardBreakdown(10, 1, 1),
      new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero)
    );
  }

  private static async Task<FamilyCommandResult> ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload,
    FamilyMemberId? authenticatedMemberId = null,
    Guid? commandId = null
  )
  {
    return await handler.HandleAsync(
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

  private sealed record ContributionSeed(
    ContributionId Id,
    ContributionKind Kind,
    FamilyMemberId AssigneeId,
    FamilyMemberId[] PartnerIds,
    ContributionStatus Status,
    StarRating? Rating,
    ContributionRewardBreakdown? EarnedReward,
    DateTimeOffset? ApprovedAt
  );
}
