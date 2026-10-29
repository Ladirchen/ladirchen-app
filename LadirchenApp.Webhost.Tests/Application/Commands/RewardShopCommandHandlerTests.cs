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

public sealed class RewardShopCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("a01156a8-dd90-4bf4-beb5-527ff0b29040"));
  private static readonly FamilyMemberId _administratorId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = new(2026, 10, 10, 15, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task CommandsUseDomainReservationsAndPreservePresentationFields()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();
    var firstRewardId = new ShopRewardId("movie-night");
    var firstRequestCommandId = Guid.Parse("fb5bddc7-1efe-4daa-b73f-8cb3e57a7868");

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.add",
      new
      {
        id = firstRewardId.Value,
        title = "Movie night",
        description = "Choose a family movie",
        icon = "🎬",
        price = 80,
        category = "activity",
        quantity = 2,
        conditions = "Homework is complete",
        isVisible = true,
      }
    );
    await ExecuteAsync(
      handler,
      _childId,
      "reward-shop.request",
      new { id = firstRewardId.Value },
      firstRequestCommandId
    );

    var economy = store.RequiredEconomy();
    var requestedReward = economy.ShopRewards.Single(item => item.Id == firstRewardId);
    var request = economy.RedemptionRequests.Single();
    await Assert.That(requestedReward.Status).IsEqualTo(ShopRewardStatus.Requested);
    await Assert.That(request.RequesterId).IsEqualTo(_childId);
    await Assert.That(request.Id.Value).IsEqualTo(firstRequestCommandId.ToString("N"));
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(100);
    await Assert.That(economy.WalletFor(_childId).Reserved).IsEqualTo(80);

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.add",
      new
      {
        id = "small-gift",
        title = "Small gift",
        description = "Choose a small gift",
        icon = "🎁",
        price = 30,
        category = "gift",
        quantity = 1,
        conditions = "Available once",
        isVisible = true,
      }
    );
    var revisionBeforeRejectedRequest = store.Revision(TypedAggregateKind.FamilyEconomy);

    await Assert.That(async () =>
      await ExecuteAsync(
        handler,
        _childId,
        "reward-shop.request",
        new { id = "small-gift" }
      )
    ).Throws<DomainRuleException>();
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy))
      .IsEqualTo(revisionBeforeRejectedRequest);

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.decide",
      new
      {
        id = firstRewardId.Value,
        approved = true,
      }
    );

    economy = store.RequiredEconomy();
    var approvedReward = economy.ShopRewards.Single(item => item.Id == firstRewardId);
    request = economy.RedemptionRequests.Single(item => item.Id == request.Id);
    var presentation = store.EconomyPresentation.ShopRewards
      .Single(item => item.RewardId == firstRewardId);
    await Assert.That(approvedReward.Status).IsEqualTo(ShopRewardStatus.Available);
    await Assert.That(approvedReward.Quantity).IsEqualTo(1);
    await Assert.That(request.Status).IsEqualTo(RedemptionStatus.Approved);
    await Assert.That(presentation.Description).IsEqualTo("Choose a family movie");
    await Assert.That(presentation.Icon).IsEqualTo("🎬");
    await Assert.That(presentation.Conditions).IsEqualTo("Homework is complete");
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(20);
    await Assert.That(economy.WalletFor(_childId).Reserved).IsEqualTo(0);
  }

  [Test]
  public async Task VisibilityCancellationRejectionAndDeletionUseTheDomainModel()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();
    var rewardId = new ShopRewardId("family-activity");

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.add",
      new
      {
        id = rewardId.Value,
        title = "Family activity",
        description = "Choose an activity",
        icon = "🎲",
        price = 40,
        category = "activity",
        quantity = 2,
        conditions = "Everyone agrees",
        isVisible = true,
      }
    );
    await ExecuteAsync(handler, _administratorId, "reward-shop.set-visibility", new
    {
      id = rewardId.Value,
      isVisible = false,
    });
    await Assert.That(Reward(store, rewardId).IsVisible).IsFalse();
    await ExecuteAsync(handler, _administratorId, "reward-shop.set-visibility", new
    {
      id = rewardId.Value,
      isVisible = true,
    });

    await ExecuteAsync(handler, _childId, "reward-shop.request", new
    {
      id = rewardId.Value,
    });
    await ExecuteAsync(handler, _childId, "reward-shop.cancel", new
    {
      id = rewardId.Value,
    });
    await Assert.That(Reward(store, rewardId).Status).IsEqualTo(ShopRewardStatus.Available);
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Available).IsEqualTo(100);

    await ExecuteAsync(handler, _childId, "reward-shop.request", new
    {
      id = rewardId.Value,
    });
    await ExecuteAsync(handler, _administratorId, "reward-shop.decide", new
    {
      id = rewardId.Value,
      approved = false,
    });
    await Assert.That(Reward(store, rewardId).Status).IsEqualTo(ShopRewardStatus.Available);
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Available).IsEqualTo(100);

    await ExecuteAsync(handler, _childId, "reward-shop.request", new
    {
      id = rewardId.Value,
    });
    await ExecuteAsync(handler, _administratorId, "reward-shop.delete", new
    {
      id = rewardId.Value,
    });
    var economy = store.RequiredEconomy();
    await Assert.That(economy.ShopRewards.Any(item => item.Id == rewardId)).IsFalse();
    await Assert.That(store.EconomyPresentation.ShopRewards.Any(item => item.RewardId == rewardId))
      .IsFalse();
    await Assert.That(economy.WalletFor(_childId).Available).IsEqualTo(100);
    await Assert.That(economy.WalletFor(_childId).Reserved).IsEqualTo(0);
  }

  private static TestTypedAggregateStore CreateStore()
  {
    var family = new Family(
      _familyId,
      _administratorId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _administratorId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    var economy = new FamilyEconomy(family);
    economy.RestoreWallet(_childId, 100, 0);
    var store = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    store.Seed(
      family,
      economy,
      FamilyEconomyPresentationState.Empty,
      FamilyHouse.CreateInitial(family)
    );

    return store;
  }

  private static ShopReward Reward(TestTypedAggregateStore store, ShopRewardId rewardId)
  {
    return store.RequiredEconomy().ShopRewards.Single(item => item.Id == rewardId);
  }

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload,
    Guid? commandId = null
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        commandId ?? Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload)
      ),
      CancellationToken.None
    );
  }
}
