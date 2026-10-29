using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Models.ReadModels;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class RewardShopQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("f1cd8ee7-52bd-488a-a9c0-9172da753f87")
  );
  private static readonly FamilyMemberId _administratorId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task QueryProjectsCapabilitiesFromTypedFamilyEconomyState()
  {
    var response = await QueryAsync(
      _administratorId,
      new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero)
    );
    var affordable = Reward(response, "affordable");
    var requested = Reward(response, "requested");
    var scheduled = Reward(response, "scheduled");

    await Assert.That(response.State.RedemptionOpen).IsTrue();
    await Assert.That(affordable.Published).IsTrue();
    await Assert.That(affordable.PublicationStatus).IsEqualTo("visible");
    await Assert.That(affordable.RequestableByMemberIds).IsEmpty();
    await Assert.That(requested.CancellableByMemberIds).IsEmpty();
    await Assert.That(requested.ApprovableByMemberIds)
      .IsEquivalentTo([_administratorId.Value]);
    await Assert.That(scheduled.Published).IsFalse();
    await Assert.That(scheduled.PublicationStatus).IsEqualTo("scheduled");
    await Assert.That(scheduled.NextStatusChangeAt)
      .IsEqualTo("2026-10-10T22:00:00.0000000+00:00");
  }

  [Test]
  public async Task ClosedShopRemovesRequestAndApprovalCapabilitiesButKeepsCancellation()
  {
    var response = await QueryAsync(
      _administratorId,
      new DateTimeOffset(2026, 10, 10, 16, 0, 0, TimeSpan.Zero)
    );

    await Assert.That(response.State.RedemptionOpen).IsFalse();
    await Assert.That(Reward(response, "affordable").RequestableByMemberIds).IsEmpty();
    await Assert.That(Reward(response, "requested").ApprovableByMemberIds).IsEmpty();
    await Assert.That(Reward(response, "requested").CancellableByMemberIds).IsEmpty();
  }

  [Test]
  public async Task OrdinaryMemberCannotRetrieveUnpublishedRewards()
  {
    var response = await QueryAsync(
      _childId,
      new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero)
    );

    await Assert.That(response.State.Rewards.Any(item => item.Id == "scheduled")).IsFalse();
    await Assert.That(Reward(response, "affordable").RequestableByMemberIds).IsEmpty();
    await Assert.That(Reward(response, "requested").CancellableByMemberIds)
      .IsEquivalentTo([_childId.Value]);
  }

  private static async Task<RewardShopReadModelResponse> QueryAsync(
    FamilyMemberId actorId,
    DateTimeOffset now
  )
  {
    var store = CreateStore(now);
    var handler = new GetFamilyAggregateQueryHandler(
      store,
      store,
      store,
      store,
      new FixedTimeProvider(now)
    );
    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "reward-shop", actorId),
      CancellationToken.None
    ) as RewardShopReadModelProjection
      ?? throw new InvalidOperationException("The reward shop result is required.");

    return RewardShopReadModelResponse.FromProjection(result);
  }

  private static TestTypedAggregateStore CreateStore(DateTimeOffset now)
  {
    var supporterId = new FamilyMemberId("supporter");
    var siblingId = new FamilyMemberId("sibling");
    var invitedGuardianId = new FamilyMemberId("invitedGuardian");
    var family = Family.Reconstitute(new FamilyState(
      _familyId,
      new IanaTimeZone("Europe/Zurich"),
      FamilyCurrency.CHF,
      new ExchangeRate(1),
      _childId,
      false,
      [
        Member(_administratorId, "Guardian", MemberRole.Guardian, GuardianAccess.Admin),
        Member(supporterId, "Supporter", MemberRole.Guardian, GuardianAccess.Supporter),
        Member(_childId, "Child", MemberRole.Child, GuardianAccess.Supporter),
        Member(siblingId, "Sibling", MemberRole.Child, GuardianAccess.Supporter),
        new FamilyMemberState(
          invitedGuardianId,
          "Invited guardian",
          MemberRole.Guardian,
          GuardianAccess.Admin,
          true,
          null,
          null,
          false
        ),
      ],
      []
    ));
    var affordableId = new ShopRewardId("affordable");
    var requestedId = new ShopRewardId("requested");
    var scheduledId = new ShopRewardId("scheduled");
    var economy = FamilyEconomy.Reconstitute(family, new FamilyEconomyState(
      _familyId,
      ContributionSettings.Default,
      [
        new MemberWalletState(_administratorId, 0, 0),
        new MemberWalletState(supporterId, 0, 0),
        new MemberWalletState(_childId, 100, 80),
        new MemberWalletState(siblingId, 50, 0),
      ],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      [
        Reward(affordableId, "Affordable reward", ShopRewardCategory.Activity, 30, 2),
        Reward(requestedId, "Requested reward", ShopRewardCategory.Gift, 80, 1,
          status: ShopRewardStatus.Requested),
        Reward(
          scheduledId,
          "Scheduled reward",
          ShopRewardCategory.Time,
          10,
          1,
          from: new DateOnly(2026, 10, 11)
        ),
      ],
      [
        new RedemptionRequestState(
          new RedemptionRequestId("pending-request"),
          requestedId,
          _childId,
          new LadirchenAmount(80),
          now.AddDays(-1),
          RedemptionStatus.Pending,
          null
        ),
      ]
    ));
    var presentation = new FamilyEconomyPresentationState(
      [],
      [],
      [
        new ShopRewardPresentationState(affordableId, "", "", ""),
        new ShopRewardPresentationState(requestedId, "", "", ""),
        new ShopRewardPresentationState(scheduledId, "", "", ""),
      ],
      []
    );
    var store = new TestTypedAggregateStore(new FixedTimeProvider(now));
    store.Seed(family, economy, presentation);

    return store;
  }

  private static ShopRewardState Reward(
    ShopRewardId id,
    string title,
    ShopRewardCategory category,
    decimal price,
    int quantity,
    DateOnly? from = null,
    ShopRewardStatus status = ShopRewardStatus.Available
  )
  {
    return new ShopRewardState(
      id,
      title,
      category,
      new LadirchenAmount(price),
      quantity,
      true,
      new AvailabilityWindow(from, null),
      status,
      false
    );
  }

  private static FamilyMemberState Member(
    FamilyMemberId id,
    string name,
    MemberRole role,
    GuardianAccess access
  )
  {
    return new FamilyMemberState(
      id,
      name,
      role,
      access,
      false,
      null,
      null,
      role == MemberRole.Child
    );
  }

  private static ShopRewardReadModel Reward(
    RewardShopReadModelResponse response,
    string rewardId
  )
  {
    return response.State.Rewards.Single(item => item.Id == rewardId);
  }
}
