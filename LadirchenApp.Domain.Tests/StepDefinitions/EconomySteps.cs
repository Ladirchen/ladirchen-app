using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed partial class EconomySteps
{
  private readonly FamilyMemberId _admin = new("admin");
  private readonly FamilyMemberId _child = new("child");
  private readonly FamilyMemberId _sibling = new("sibling");
  private readonly FamilyMemberId _supporter = new("supporter");
  private readonly ContributionId _contributionId = new("task");
  private DateTimeOffset _now = DateTimeOffset.Parse("2026-10-10T10:00:00Z", CultureInfo.InvariantCulture);
  private FamilyAggregate _family = null!;
  private FamilyEconomy _economy = null!;
  private DomainRuleException? _error;
  private string? _beforeRejectedCommand;
  private int _operationSequence;

  [Given("a core economy")]
  public void CreateEconomy()
  {
    _family = new(
      new FamilyId(Guid.Parse("94bac411-e603-40bd-ae9f-19a46c9d752f")),
      _admin,
      "Alex",
      new("Europe/Zurich")
    );
    _family.AddMember(_admin, _child, "Sam", MemberRole.Child, GuardianAccess.Supporter, false, _now);
    _family.AddMember(_admin, _sibling, "Robin", MemberRole.Child, GuardianAccess.Supporter, false, _now);
    _family.AddMember(_admin, _supporter, "Pat", MemberRole.Guardian, GuardianAccess.Supporter, false, _now);
    _economy = new(_family);
  }

  [Given("a submitted child contribution worth {float} Ladirchen")]
  public void ChildContribution(decimal amount)
  {
    SubmittedContribution(_child, _admin, amount);
  }

  [Given("a submitted guardian contribution worth {float} Ladirchen")]
  public void GuardianContribution(decimal amount)
  {
    SubmittedContribution(_supporter, _supporter, amount);
  }

  private void SubmittedContribution(FamilyMemberId assignee, FamilyMemberId creator, decimal amount)
  {
    _economy.AddContribution(
      creator,
      _contributionId,
      new("Help the family", ContributionKind.Basic, new(amount), new(50), assignee),
      _now
    );
    _economy.SubmitContribution(assignee, _contributionId, _now);
  }

  [When("the guardian administrator approves it with {int} stars")]
  public void Approve(int stars)
  {
    _economy.ApproveContribution(_admin, _contributionId, new(stars), NextOperation(), _now);
  }

  [When("the guardian administrator tries to approve it again")]
  public void DuplicateApproval()
  {
    Try(() => Approve(5));
  }

  [When("the child approves the guardian contribution")]
  public void ApproveGuardian()
  {
    _economy.ApproveContribution(_child, _contributionId, new(5), NextOperation(), _now);
  }

  [When("the supporter tries to approve the child contribution")]
  public void SupporterApproval()
  {
    Try(() => _economy.ApproveContribution(_supporter, _contributionId, new(5), NextOperation(), _now));
  }

  [Given("an unassigned contribution")]
  public void UnassignedContribution()
  {
    _economy.AddContribution(_admin, _contributionId, new("Help", ContributionKind.Extra, new(10), new(0), null), _now);
  }

  [When("the child claims it")]
  public void Claim()
  {
    _economy.ClaimContribution(_child, _contributionId, _now);
  }

  [When("the guardian administrator returns it")]
  public void Return()
  {
    _economy.ReturnContribution(_admin, _contributionId, _now);
  }

  [When("the child submits it again")]
  public void Resubmit()
  {
    _economy.SubmitContribution(_child, _contributionId, _now);
  }

  [Given("a double reward promotion with {float} teamwork bonus until 17:00")]
  public void Promotion(decimal bonus)
  {
    _economy.AddPromotion(_admin, new("promo"), _contributionId, new(2), new(bonus), new(new TimeOnly(17, 0)), _now);
  }

  [Given("the local time is {string}")]
  public void LocalTime(string text)
  {
    var local = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.None);
    var zone = TimeZoneInfo.FindSystemTimeZoneById(_family.TimeZone.Value);
    _now = new DateTimeOffset(local, zone.GetUtcOffset(local));
  }

  [Then("the child wallet contains {float} Ladirchen")]
  public async Task ChildBalance(decimal amount)
  {
    await Assert.That(_economy.WalletFor(_child).Balance).IsEqualTo(amount);
  }

  [Then("the guardian wallet contains {float} Ladirchen")]
  public async Task GuardianBalance(decimal amount)
  {
    await Assert.That(_economy.WalletFor(_supporter).Balance).IsEqualTo(amount);
  }

  [Then("the reward breakdown is {float} base and {float} bonus")]
  public async Task Breakdown(decimal baseReward, decimal bonus)
  {
    var reward =
      _economy.Contributions.Single().EarnedReward
      ?? throw new InvalidOperationException("Expected an approved reward.");
    await Assert.That(reward.BaseReward).IsEqualTo(baseReward);
    await Assert.That(reward.RatingBonus).IsEqualTo(bonus);
    var approved = _economy.DomainEvents.OfType<ContributionApproved>().Single();
    await Assert.That(approved.Reward).IsEqualTo(reward);
    await Assert.That(approved.OccurredAtUtc).IsEqualTo(_now.ToUniversalTime());
  }

  [Then("the contribution status is {string}")]
  public async Task ContributionState(string status)
  {
    await Assert.That(_economy.Contributions.Single().Status.ToString()).IsEqualTo(status);
  }

  [Then("there are {int} ledger entries")]
  public async Task LedgerCount(int count)
  {
    await Assert.That(_economy.Ledger.Count).IsEqualTo(count);
  }

  [Then("the economy command fails with {string}")]
  public async Task Error(string code)
  {
    await Assert.That(_error?.Code.ToString()).IsEqualTo(code);
  }

  [Then("the rejected command left the economy unchanged")]
  public async Task Unchanged()
  {
    await Assert.That(Snapshot()).IsEqualTo(_beforeRejectedCommand);
  }

  private TransactionId NextOperation()
  {
    return new($"operation-{++_operationSequence}");
  }

  private void Try(Action action)
  {
    _beforeRejectedCommand = Snapshot();

    try
    {
      action();
    }
    catch (DomainRuleException exception)
    {
      _error = exception;
    }
  }

  private string Snapshot()
  {
    return string.Join(";", _economy.Wallets.Select(item => $"{item.Id.Value}:{item.Balance}:{item.Reserved}"))
      + string.Join(
        ";",
        _economy.Contributions.Select(item => $"{item.Id.Value}:{item.Status}:{item.AssigneeId}:{item.EarnedReward}")
      )
      + string.Join(
        ";",
        _economy
          .GoalsFor(_child)
          .Select(item =>
            $"{item.Id.Value}:{item.Saved}:{item.Target}:{item.Visibility}:{item.InterestEarned}:{item.Cheered}"
          )
      )
      + string.Join(
        ";",
        _economy.ShopRewards.Select(item =>
          $"{item.Id.Value}:{item.Price.Value}:{item.Quantity}:{item.Status}:{item.IsVisible}"
        )
      )
      + string.Join(
        ";",
        _economy.RedemptionRequests.Select(item => $"{item.Id.Value}:{item.Status}:{item.Price.Value}")
      )
      + $"|{_economy.Ledger.Count}|{_economy.DomainEvents.Count}";
  }
}
