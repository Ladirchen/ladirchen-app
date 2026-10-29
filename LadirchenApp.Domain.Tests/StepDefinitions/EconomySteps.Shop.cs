using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private readonly ShopRewardId _rewardId = new("reward");
  private readonly RedemptionRequestId _requestId = new("request");

  [Given("a published reward costs {float} Ladirchen with stock {int}")]
  public void CreateReward(decimal price, int stock)
  {
    _economy.AddShopReward(
      _admin,
      _rewardId,
      new("Movie night", ShopRewardCategory.Activity, new(price), stock, true, new()),
      _now
    );
  }

  [When("the child requests the reward")]
  [Given("the child requests the reward")]
  public void RequestReward()
  {
    _economy.RequestShopReward(_child, _rewardId, _requestId, NextOperation(), _now);
  }

  [When("the guardian administrator approves the reward")]
  public void ApproveReward()
  {
    _economy.ApproveShopReward(_admin, _requestId, NextOperation(), _now);
  }

  [When("the guardian administrator tries to approve the reward again")]
  public void DuplicateRewardApproval()
  {
    Try(ApproveReward);
  }

  [When("the request is {string}")]
  public void DecideRequest(string decision)
  {

    switch (decision)
    {
      case "cancelled":
        _economy.CancelShopRewardRequest(_child, _requestId, NextOperation(), _now);
        break;
      case "rejected":
        _economy.RejectShopReward(_admin, _requestId, NextOperation(), _now);
        break;
      case "deleted":
        _economy.DeleteShopReward(_admin, _rewardId, NextOperation(), _now);
        break;
      default:
        throw new ArgumentException("Unknown decision.", nameof(decision));
    }

  }

  [When("the child tries to deposit {float} Ladirchen")]
  public void TryDeposit(decimal amount)
  {
    Try(() => Deposit(amount));
  }

  [When("the child tries to gift {float} Ladirchen")]
  public void TryGift(decimal amount)
  {
    Try(() => GiftSibling(amount));
  }

  [When("the child tries to request the reward")]
  public void TryRequest()
  {
    Try(RequestReward);
  }

  [When("the sibling tries to cancel the request")]
  public void CancelOtherRequest()
  {
    Try(() => _economy.CancelShopRewardRequest(_sibling, _requestId, NextOperation(), _now));
  }

  [When("the guardian administrator tries to change the price")]
  public void ChangePendingPrice()
  {
    Try(() => _economy.UpdateShopReward(_admin, _rewardId, "Movie night", new(90), 1, new(), _now));
  }

  [Then("the wallet has {float} reserved and {float} available Ladirchen")]
  public async Task Reservation(decimal reserved, decimal available)
  {
    var wallet = _economy.WalletFor(_child);
    await Assert.That(wallet.Reserved).IsEqualTo(reserved);
    await Assert.That(wallet.Available).IsEqualTo(available);
  }

  [Then("the reward has stock {int} and status {string}")]
  public async Task Stock(int stock, string status)
  {
    var reward = _economy.ShopRewards.SingleOrDefault();

    if (reward is null)
    {
      await Assert.That(_economy.RedemptionRequests.Single().Status).IsEqualTo(RedemptionStatus.Cancelled);

      return;
    }

    await Assert.That(reward.Quantity).IsEqualTo(stock);
    await Assert.That(reward.Status.ToString()).IsEqualTo(status);
  }
}
