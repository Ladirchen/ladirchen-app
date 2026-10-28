using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private readonly FamilyRewardCatalogPolicy _rewardCatalogPolicy = new();
  private bool _requestAccepted;
  private int _catalogCountBefore;
  private string _invalidRewardData = string.Empty;
  private ShopRewardCategory _preparedCategory;
  private FamilyMemberId _shopViewer = null!;
  private bool _shopAccess;
  private int _displayedRewardCount;
  private decimal _requestSnapshotBalance;
  private decimal _requestSnapshotReserved;

  [When("the member attempts to add, hide or delete a shop reward")]
  public void UnauthorizedCatalogChange()
  {
    _catalogCountBefore = _economy.ShopRewards.Count;
    Try(() =>
      _economy.AddShopReward(
        _supporter,
        _rewardId,
        new ShopRewardDefinition("Reward", ShopRewardCategory.Gift, new LadirchenAmount(10), 1, true, new AvailabilityWindow()),
        _now
      )
    );
  }

  [Then("the reward catalog remains unchanged")]
  public async Task RewardCatalogUnchanged()
  {
    await Assert.That(_economy.ShopRewards.Count).IsEqualTo(_catalogCountBefore);
  }

  [Given("a reward has visibility {string} and availability {string}")]
  public void RewardAvailability(string visibility, string availability)
  {
    CreateEconomy();
    FundChild(100);
    var localDate = _family.TimeZone.LocalDate(_now);
    var window = availability switch
    {
      "before start date" => new AvailabilityWindow(localDate.AddDays(1), localDate.AddDays(2)),
      "after end date" => new AvailabilityWindow(localDate.AddDays(-2), localDate.AddDays(-1)),
      "on start date" => new AvailabilityWindow(localDate, localDate.AddDays(1)),
      "on end date" => new AvailabilityWindow(localDate.AddDays(-1), localDate),
      _ => new AvailabilityWindow(),
    };
    _economy.AddShopReward(
      _admin,
      _rewardId,
      new ShopRewardDefinition("Reward", ShopRewardCategory.Gift, new LadirchenAmount(10), 1, visibility == "visible", window),
      _now
    );
  }

  [When("the child attempts to request the reward")]
  public void ChildAttemptsRewardRequest()
  {
    _requestAccepted = AttemptRequest(_child);
  }

  [Then("the request is {word}")]
  public async Task RewardRequestResult(string result)
  {
    await Assert.That(_requestAccepted).IsEqualTo(result == "accepted");
  }

  [Given("an available reward has stock {int} and costs {float} Ladirchen")]
  public void AvailableReward(int stock, decimal price)
  {
    CreateEconomy();
    _economy.AddShopReward(
      _admin,
      _rewardId,
      new ShopRewardDefinition("Reward", ShopRewardCategory.Gift, new LadirchenAmount(price), stock, true, new AvailabilityWindow()),
      _now
    );
  }

  [Given("the child has {float} available Ladirchen")]
  public void ChildAvailableBalance(decimal amount)
  {
    if (amount > 0)
    {
      FundChild(amount);
    }
  }

  [Given("a child has a pending reward request")]
  [Given("a child has a pending request for that reward")]
  [Given("a child has a pending reward request with reserved funds")]
  public void PendingRewardRequest()
  {
    CreateEconomy();
    FundChild(100);
    CreateReward(80, 1);
    RequestReward();
    var wallet = _economy.WalletFor(_child);
    _requestSnapshotBalance = wallet.Balance;
    _requestSnapshotReserved = wallet.Reserved;
  }

  [Given("the family local time is {int}:{int}")]
  public void FamilyLocalTime(int hour, int minute)
  {
    LocalTime($"2026-10-10T{hour:00}:{minute:00}:00");
  }

  [When("an administrator attempts to approve the request")]
  public void AdministratorAttemptsApproval()
  {
    Try(ApproveReward);
  }

  [Then("the request and wallet remain unchanged")]
  public async Task RequestAndWalletUnchanged()
  {
    var wallet = _economy.WalletFor(_child);
    await Assert.That(wallet.Balance).IsEqualTo(_requestSnapshotBalance);
    await Assert.That(wallet.Reserved).IsEqualTo(_requestSnapshotReserved);
    await Assert.That(_economy.RedemptionRequests.Single().Status).IsEqualTo(RedemptionStatus.Pending);
  }

  [Given("a published reward is available and affordable")]
  public void AffordablePublishedReward()
  {
    CreateEconomy();
    CreateReward(10, 1);
  }

  [When("a guardian attempts to request the reward")]
  public void GuardianRequestsReward()
  {
    _requestAccepted = AttemptRequest(_supporter);
  }

  [Then("no reward request is created")]
  public async Task NoRewardRequest()
  {
    await Assert.That(_economy.RedemptionRequests).IsEmpty();
  }

  [Then("no funds are reserved")]
  public async Task NoFundsReserved()
  {
    await Assert.That(_economy.WalletFor(_supporter).Reserved).IsEqualTo(0);
  }

  [Given("a published reward has stock {int}")]
  public void PublishedRewardStock(int stock)
  {
    CreateEconomy();
    FundChild(100);
    CreateReward(10, stock);
  }

  [When("a content manager approves the request before the local cutoff")]
  public void ApproveBeforeCutoff()
  {
    ApproveReward();
  }

  [Then("the reward cannot be requested again")]
  public async Task RewardCannotBeRequestedAgain()
  {
    await Assert.That(AttemptRequest(_sibling)).IsFalse();
  }

  [Given("the signed-in family member is a {string}")]
  public void SignedInShopRole(string role)
  {
    CreateEconomy();
    _shopViewer = role switch
    {
      "child" => _child,
      "guardian admin" => _admin,
      _ => _supporter,
    };
  }

  [When("the family member opens the family reward shop")]
  public void OpenFamilyRewardShop()
  {
    _shopAccess = _rewardCatalogPolicy.CanOpen(_family.ActiveMember(_shopViewer));
  }

  [Then("access to the family reward shop is {word}")]
  public async Task FamilyRewardShopAccess(string access)
  {
    await Assert.That(_shopAccess).IsEqualTo(access == "allowed");
  }

  [Given("the catalog contains visible, hidden, scheduled and expired rewards")]
  public void MixedRewardCatalog()
  {
    CreateEconomy();
    var local = _family.TimeZone.LocalDate(_now);
    AddCatalogReward("visible", true, new AvailabilityWindow());
    AddCatalogReward("hidden", false, new AvailabilityWindow());
    AddCatalogReward("scheduled", true, new AvailabilityWindow(local.AddDays(1), null));
    AddCatalogReward("expired", true, new AvailabilityWindow(null, local.AddDays(-1)));
  }

  [When("a child views the family reward shop")]
  public void ChildViewsRewardShop()
  {
    _displayedRewardCount = _economy.ShopRewards.Count(item => item.IsPublished(_now, _family.TimeZone));
  }

  [Then("only currently published rewards are displayed")]
  public async Task OnlyPublishedRewardsDisplayed()
  {
    await Assert.That(_displayedRewardCount).IsEqualTo(1);
  }

  [When("a content manager views the family reward shop")]
  public void ManagerViewsRewardShop()
  {
    _displayedRewardCount = _economy.ShopRewards.Count;
  }

  [Then("all catalog rewards are displayed with their publication status")]
  public async Task AllRewardsDisplayed()
  {
    await Assert.That(_displayedRewardCount).IsEqualTo(4);
  }

  [When("a content manager hides the reward")]
  public void HideReward()
  {
    _economy.SetShopRewardVisibility(_admin, _rewardId, false, _now);
  }

  [Then("the pending request remains unchanged")]
  public async Task PendingRequestUnchanged()
  {
    await Assert.That(_economy.RedemptionRequests.Single().Status).IsEqualTo(RedemptionStatus.Pending);
  }

  [Then("the funds remain reserved until approval, rejection, cancellation or deletion")]
  public async Task FundsRemainReserved()
  {
    await Assert.That(_economy.WalletFor(_child).Reserved).IsEqualTo(80);
  }

  [Given("a content manager enters reward data with {string}")]
  public void InvalidRewardData(string invalidData)
  {
    CreateEconomy();
    _invalidRewardData = invalidData;
  }

  [When("the content manager attempts to add the reward")]
  public void AttemptAddInvalidReward()
  {
    var icon = _invalidRewardData == "a blank icon" ? "" : "gift";
    var title = _invalidRewardData == "a blank title" ? "" : "Reward";
    var description = _invalidRewardData == "a blank description" ? "" : "Description";
    var conditions = _invalidRewardData == "blank conditions" ? "" : "Conditions";
    var price = _invalidRewardData == "a price below 1 Ladirchen" ? 0 : 10;
    var quantity = _invalidRewardData == "a quantity below 1" ? 0 : 1;
    var scheduled = _invalidRewardData == "a scheduled publication without a start date";
    var limited = _invalidRewardData == "a limited publication without an end date";
    DateOnly? start = scheduled ? null : new DateOnly(2026, 10, 10);
    DateOnly? end = limited ? null : new DateOnly(2026, 10, 12);

    if (_invalidRewardData == "a start date after the end date")
    {
      start = new DateOnly(2026, 10, 13);
    }

    var valid = _rewardCatalogPolicy.IsValidNewReward(
      icon,
      title,
      description,
      conditions,
      price,
      quantity,
      start,
      end,
      scheduled,
      limited
    );

    if (valid)
    {
      _economy.AddShopReward(
        _admin,
        _rewardId,
        new ShopRewardDefinition(title, ShopRewardCategory.Gift, new LadirchenAmount(price), quantity, true, new AvailabilityWindow(start, end)),
        _now
      );
    }
  }

  [Then("the reward is not added to the catalog")]
  public async Task RewardNotAdded()
  {
    await Assert.That(_economy.ShopRewards).IsEmpty();
  }

  [Given("a content manager creates a valid reward in category {string}")]
  public void ValidRewardCategory(string category)
  {
    CreateEconomy();
    _preparedCategory = Enum.Parse<ShopRewardCategory>(category, true);
  }

  [When("the reward is added to the catalog")]
  public void AddCategorizedReward()
  {
    _economy.AddShopReward(
      _admin,
      _rewardId,
      new ShopRewardDefinition("Reward", _preparedCategory, new LadirchenAmount(10), 1, true, new AvailabilityWindow()),
      _now
    );
  }

  [Then("its category is {string}")]
  public async Task RewardCategory(string category)
  {
    await Assert.That(_economy.ShopRewards.Single().Category.ToString().ToLowerInvariant()).IsEqualTo(category);
  }

  private bool AttemptRequest(FamilyMemberId actor)
  {
    try
    {
      _economy.RequestShopReward(actor, _rewardId, _requestId, NextOperation(), _now);

      return true;
    }
    catch (DomainRuleException)
    {
      return false;
    }
  }

  private void AddCatalogReward(string id, bool visible, AvailabilityWindow window)
  {
    _economy.AddShopReward(
      _admin,
      new ShopRewardId(id),
      new ShopRewardDefinition(id, ShopRewardCategory.Custom, new LadirchenAmount(10), 1, visible, window),
      _now
    );
  }
}
