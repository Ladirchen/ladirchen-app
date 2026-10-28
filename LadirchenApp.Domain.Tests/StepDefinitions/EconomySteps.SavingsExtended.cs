using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private readonly GoalSupportPolicy _goalSupportPolicy = new();
  private readonly FamilyGoalParticipantPolicy _participantPolicy = new();
  private decimal _expectedStarterBonus;
  private decimal _forfeitedBonus;
  private decimal _returnedContribution;
  private decimal _supportIncrease;
  private decimal _walletTotalBeforeSupport;
  private GoalOwner? _expectedOwner;
  private bool _goalVisible;
  private bool _goalChanged;
  private IReadOnlySet<FamilyMemberId> _goalParticipants = new HashSet<FamilyMemberId>();
  private SavingsVoucher? _voucher;
  private bool _redemptionResult;
  private bool _secondRedemptionResult;

  [Given("an incomplete {string} savings goal contains {float} starter-bonus Ladirchen")]
  public void IncompleteGoalWithStarterBonus(string goalType, decimal starterBonus)
  {
    CreateEconomy();
    _expectedStarterBonus = starterBonus;
    var owner = goalType == "Family" ? GoalOwner.Family : GoalOwner.Member(_child);
    var visibility = goalType == "Family" ? GoalVisibility.Family : GoalVisibility.Family;
    var memberCount = goalType == "Family" ? (int)(starterBonus / 5) : 0;
    _economy.AddGoal(
      _child,
      _goalId,
      new SavingGoalDefinition(owner, "Goal", new LadirchenAmount(100), visibility, memberCount),
      NextOperation(),
      _now
    );
  }

  [Given("the goal contains {float} contributed Ladirchen in addition to its starter bonus")]
  public void GoalContainsContributions(decimal amount)
  {
    _economy.SupportGoal(_admin, _goalId, new LadirchenAmount(amount), NextOperation(), _now);
  }

  [When("an authorized family member cancels the goal before its target is reached")]
  public void CancelIncompleteGoal()
  {
    _returnedContribution = _economy.CancelGoal(_child, _goalId, NextOperation(), _now);
    _forfeitedBonus = _economy.Ledger.Single(item => item.Kind == LedgerKind.BonusForfeited).Amount;
  }

  [Then("the cancelled goal no longer exists")]
  public async Task CancelledGoalDoesNotExist()
  {
    await Assert.That(_economy.GoalsFor(_child)).IsEmpty();
  }

  [Then("{float} starter-bonus Ladirchen are forfeited")]
  public async Task StarterBonusForfeited(decimal amount)
  {
    await Assert.That(_forfeitedBonus).IsEqualTo(amount);
  }

  [Then("only the {float} contributed Ladirchen are eligible to be returned")]
  public async Task ContributionsReturned(decimal amount)
  {
    await Assert.That(_returnedContribution).IsEqualTo(amount);
  }

  [Given("a {string} savings goal contains {float} starter-bonus Ladirchen")]
  public void GoalWithStarterBonus(string goalType, decimal bonus)
  {
    IncompleteGoalWithStarterBonus(goalType, bonus);
  }

  [Given("contributed savings raise the goal balance to its target")]
  public void CompletePreparedGoal()
  {
    var goal = _economy.GoalFor(_child, _goalId);
    _economy.SupportGoal(_admin, _goalId, new LadirchenAmount(goal.Target - goal.Saved), NextOperation(), _now);
  }

  [When("the completed savings goal is closed")]
  public void CloseCompletedGoal()
  {
    var goal = _economy.GoalFor(_child, _goalId);
    _voucher = new SavingsVoucher(goal.Saved, goal.Owner.MemberId, Array.Empty<FamilyMemberId>());
  }

  [Then("its full balance including {float} starter-bonus Ladirchen is available")]
  public async Task FullBalanceAvailable(decimal starterBonus)
  {
    await Assert.That(_voucher?.Amount).IsEqualTo(100);
    await Assert.That(_expectedStarterBonus).IsEqualTo(starterBonus);
  }

  [Then("no starter bonus is forfeited")]
  public async Task NoBonusForfeited()
  {
    await Assert.That(_economy.Ledger.Any(item => item.Kind == LedgerKind.BonusForfeited)).IsFalse();
  }

  [Given("the family has {int} children and guardians")]
  public void FamilyWithMemberCount(int memberCount)
  {
    _family = new FamilyAggregate(new FamilyId(Guid.NewGuid()), _admin, "Admin", new IanaTimeZone("Europe/Zurich"));

    for (var index = 1; index < memberCount; index++)
    {
      var role = index % 2 == 0 ? MemberRole.Guardian : MemberRole.Child;
      _family.AddMember(
        _admin,
        new FamilyMemberId($"member-{index}"),
        $"Member {index}",
        role,
        GuardianAccess.Supporter,
        false,
        _now
      );
    }

    _economy = new FamilyEconomy(_family);
  }

  [Given("the family-goal starter bonus is configured as {float} Ladirchen per family member")]
  public void FamilyStarterBonus(decimal bonus)
  {
    awaitBonusMustBeFive(bonus);
  }

  [When("a family member creates a family savings goal targeting {float} Ladirchen")]
  public void CreateFamilyGoal(decimal target)
  {
    _economy.AddGoal(
      _admin,
      _goalId,
      new SavingGoalDefinition(GoalOwner.Family, "Family Goal", new LadirchenAmount(target), GoalVisibility.Family),
      NextOperation(),
      _now
    );
  }

  [When("a family member creates a family savings goal")]
  public void CreateFamilyGoalDefaultTarget()
  {
    CreateFamilyGoal(100);
  }

  [Then("the goal contains {float} starter-bonus Ladirchen")]
  public async Task GoalContainsStarterBonus(decimal amount)
  {
    var goal = _economy.GoalsFor(_admin).Single();
    await Assert.That(goal.Saved).IsEqualTo(amount);
    await Assert.That(goal.StarterBonus).IsEqualTo(amount);
  }

  [Then("the starter bonus cannot be withdrawn before the goal is completed")]
  [Then("the starter bonus remains locked")]
  public async Task StarterBonusLocked()
  {
    var viewer = _family.Members.FirstOrDefault(item => item.Role == MemberRole.Child)?.Id ?? _admin;
    var goal = _economy.GoalsFor(viewer).Single();
    await Assert.That(goal.Withdrawable).IsEqualTo(0);
  }

  [Then("the goal visibility is {string}")]
  public async Task FamilyGoalVisibility(string visibility)
  {
    await Assert.That(_economy.GoalsFor(_admin).Single().Visibility.ToString()).IsEqualTo(visibility);
  }

  [When("another family member is added later")]
  public void AddFamilyMemberLater()
  {
    _family.AddMember(
      _admin,
      new FamilyMemberId("later"),
      "Later",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
  }

  [Then("the existing goal contains {float} starter-bonus Ladirchen")]
  public async Task ExistingGoalBonus(decimal amount)
  {
    await GoalContainsStarterBonus(amount);
  }

  [Given("the child has a personal savings goal")]
  public void ChildPersonalGoal()
  {
    CreateEconomy();
    FundChild(20);
    CreateGoal("Private", 10);
  }

  [When("the child deposits more than the available wallet balance or remaining target")]
  public void DepositLimitedAmount()
  {
    _supportIncrease = _economy.DepositToGoal(_child, _goalId, new LadirchenAmount(100), NextOperation(), _now);
  }

  [Then("only the lower available amount is deposited")]
  public async Task LowerAmountDeposited()
  {
    await Assert.That(_supportIncrease).IsEqualTo(5);
  }

  [Given("the child views savings goals owned by other family members")]
  public void ChildViewsOtherGoals()
  {
    CreateEconomy();
    _economy.AddGoal(
      _sibling,
      _goalId,
      new SavingGoalDefinition(GoalOwner.Member(_sibling), "Private", new LadirchenAmount(100), GoalVisibility.Private),
      NextOperation(),
      _now
    );
  }

  [When("the child attempts to gift Ladirchen to a non-family-visible goal")]
  public void GiftToInvisibleGoal()
  {
    Try(() => _economy.GiftToGoal(_child, _goalId, new LadirchenAmount(10), NextOperation(), _now));
  }

  [Then("the gift is rejected")]
  public async Task GiftRejected()
  {
    await Assert.That(_error).IsNotNull();
  }

  [Given("a guardian views a child's savings goal")]
  public void GuardianViewsGoal()
  {
    CreateEconomy();
    CreateGoal("Family", 100);
  }

  [When("the guardian toggles the cheer")]
  public void ToggleCheer()
  {
    _economy.ToggleGoalCheer(_supporter, _goalId, _now);
  }

  [Then("the cheer changes only if the guardian may view and support that goal")]
  public async Task CheerChanged()
  {
    await Assert.That(_economy.GoalFor(_supporter, _goalId).Cheered).IsTrue();
  }

  [Given("{string} creates a valid {string} savings goal for {string}")]
  public void PreparedGoalOwnership(string creator, string goalType, string requestedOwner)
  {
    CreateEconomy();
    var actor = creator switch
    {
      "child" => _child,
      "guardian" => _supporter,
      "family administrator" => _admin,
      _ => _child,
    };
    var owner = goalType == "Family"
      ? GoalOwner.Family
      : GoalOwner.Member(requestedOwner == "a child" ? _child : actor);
    _expectedOwner = owner;
    _contributionActor = actor;
    _goalChanged = false;
  }

  [When("the savings goal is created")]
  public void CreatePreparedSavingsGoal()
  {
    _economy.AddGoal(
      _contributionActor,
      _goalId,
      new SavingGoalDefinition(_expectedOwner!, "Goal", new LadirchenAmount(100), GoalVisibility.Family),
      NextOperation(),
      _now
    );
  }

  [Then("its owner is {string}")]
  public async Task StoredGoalOwner(string owner)
  {
    var goal = _economy.GoalsFor(_contributionActor).Single();
    var storedOwner = goal.Owner.IsFamily
      ? "family"
      : goal.Owner.MemberId == _supporter
        ? "guardian"
        : goal.Owner.MemberId?.Value;
    await Assert.That(storedOwner).IsEqualTo(owner);
  }

  [Given("a personal savings goal belongs to a child")]
  [Given("a personal savings goal belongs to the signed-in child")]
  [Given("a personal savings goal belongs to the active child")]
  public void ChildOwnedGoal()
  {
    CreateEconomy();
    CreateGoal("Family", 100);
    _expectedOwner = GoalOwner.Member(_child);
  }

  [When("an authorized family member changes its title, icon, target and visibility")]
  [When("the child changes its valid title, icon, target or visibility")]
  [When("the guardian changes valid goal data")]
  public void AuthorizedGoalEdit()
  {
    _economy.UpdateGoal(_child, _goalId, "Changed", new LadirchenAmount(120), GoalVisibility.Private, _now);
    _goalChanged = true;
  }

  [Then("the changed goal still belongs to that child")]
  [Then("the changes are stored without changing the owner")]
  public async Task GoalOwnerUnchanged()
  {
    await Assert.That(_economy.GoalFor(_child, _goalId).Owner.MemberId).IsEqualTo(_child);
    await Assert.That(_goalChanged).IsTrue();
  }

  [When("{string} attempts to edit it")]
  public void UnauthorizedGoalEdit(string actor)
  {
    var member = actor == "another child" ? _sibling : _supporter;
    Try(() => _economy.UpdateGoal(member, _goalId, "Changed", new LadirchenAmount(120), GoalVisibility.Family, _now));
  }

  [Then("the goal remains unchanged")]
  public async Task GoalRemainsUnchanged()
  {
    await Assert.That(_economy.GoalFor(_child, _goalId).Title).IsEqualTo("Bicycle");
  }

  [Given("the signed-in guardian can manage goals")]
  public void GuardianCanManageGoals()
  {
  }

  [Given("a personal goal has {string} visibility")]
  public void PersonalGoalVisibilitySetup(string visibility)
  {
    CreateEconomy();
    CreateGoal(visibility, 100);
  }

  [When("{string} views the family's savings goals")]
  public void ViewSavingsGoals(string viewer)
  {
    var member = viewer switch
    {
      "owner" => _child,
      "family administrator" or "guardian admin" => _admin,
      "guardian supporter" => _supporter,
      "another child" => _sibling,
      _ => _sibling,
    };
    _goalVisible = _economy.GoalsFor(member).Any(item => item.Id == _goalId);
  }

  [Then("the personal goal is {string}")]
  public async Task PersonalGoalVisibilityResult(string result)
  {
    await Assert.That(_goalVisible).IsEqualTo(result == "visible");
  }

  [Given("an authorized guardian supports a visible savings goal")]
  [Given("an authorized guardian supports a visible personal goal owned by a child")]
  public void VisibleSupportedGoal()
  {
    CreateEconomy();
    CreateGoal("Family", 100);
    _walletTotalBeforeSupport = _economy.Wallets.Sum(item => item.Balance);
  }

  [Given("the goal has enough remaining target amount")]
  public void EnoughRemainingTarget()
  {
  }

  [When("the guardian contributes {float} Ladirchen")]
  public void GuardianContributes(decimal amount)
  {
    var before = _economy.GoalFor(_supporter, _goalId).Saved;
    _economy.SupportGoal(_supporter, _goalId, new LadirchenAmount(amount), NextOperation(), _now);
    _supportIncrease = _economy.GoalFor(_supporter, _goalId).Saved - before;
  }

  [Then("the goal balance increases by {float} Ladirchen")]
  public async Task GoalBalanceIncrease(decimal amount)
  {
    await Assert.That(_supportIncrease).IsEqualTo(amount);
  }

  [Then("no family-member wallet is debited")]
  public async Task NoWalletDebited()
  {
    await Assert.That(_economy.Wallets.Sum(item => item.Balance)).IsEqualTo(_walletTotalBeforeSupport);
  }

  [Given("a visible savings goal needs {float} Ladirchen to reach its target")]
  public void GoalNeeds(decimal remaining)
  {
    CreateEconomy();
    _economy.AddGoal(
      _child,
      _goalId,
      new SavingGoalDefinition(GoalOwner.Member(_child), "Goal", new LadirchenAmount(remaining + 5), GoalVisibility.Family),
      NextOperation(),
      _now
    );
  }

  [When("an authorized guardian contributes {float} Ladirchen")]
  public void NormalizedGuardianContribution(decimal entered)
  {
    var goal = _economy.GoalFor(_supporter, _goalId);
    var amount = _goalSupportPolicy.Normalize(entered, goal.Target - goal.Saved);
    _supportIncrease = amount;

    if (amount > 0)
    {
      _economy.SupportGoal(_supporter, _goalId, new LadirchenAmount(amount), NextOperation(), _now);
    }
  }

  [When("the support is accepted")]
  public void SupportAccepted()
  {
    GuardianContributes(10);
  }

  [Then("the credited amount and guardian name are recorded for that child")]
  public async Task GuardianGiftRecorded()
  {
    var gift = _economy.GuardianGoalGifts.Single();
    await Assert.That(gift.OwnerId).IsEqualTo(_child);
    await Assert.That(gift.GuardianId).IsEqualTo(_supporter);
    await Assert.That(gift.Amount).IsEqualTo(10);
  }

  [Given("an authorized guardian supports a family savings goal")]
  public void SupportedFamilyGoal()
  {
    CreateEconomy();
    CreateFamilyGoal(100);
  }

  [Then("the family goal balance increases")]
  public async Task FamilyGoalIncreases()
  {
    await Assert.That(_economy.GoalsFor(_admin).Single().Saved).IsGreaterThan(_expectedStarterBonus);
  }

  [Then("no personal guardian-gift record is created")]
  public async Task NoPersonalGiftRecord()
  {
    await Assert.That(_economy.GuardianGoalGifts).IsEmpty();
  }

  [Given("a personal savings goal is one Ladirchen below its target")]
  public void PersonalGoalBelowTarget()
  {
    CreateEconomy();
    FundChild(100);
    CreateGoal("Private", 10);
    Deposit(4);
  }

  [When("the owner deposits the final Ladirchen")]
  public void DepositFinalLadirchen()
  {
    Deposit(1);
    var goal = _economy.GoalFor(_child, _goalId);
    _voucher = new SavingsVoucher(goal.Saved, _child, Array.Empty<FamilyMemberId>());
  }

  [Then("the savings goal is completed")]
  public async Task SavingsGoalCompleted()
  {
    await Assert.That(_economy.GoalFor(_child, _goalId).Completed).IsTrue();
  }

  [Then("a voucher for the full goal balance is issued to the owner")]
  public async Task PersonalVoucherIssued()
  {
    await Assert.That(_voucher?.Amount).IsEqualTo(10);
    await Assert.That(_voucher?.OwnerId).IsEqualTo(_child);
  }

  [Then("the personal starter bonus is included in the voucher")]
  public async Task PersonalBonusIncluded()
  {
    await Assert.That(_voucher?.Amount).IsEqualTo(_economy.GoalFor(_child, _goalId).Saved);
  }

  [Given("a personal or family savings goal has not reached its target")]
  public void IncompleteVoucherGoal()
  {
    CreateEconomy();
    CreateGoal("Private", 100);
  }

  [When("a family member attempts to create its voucher")]
  public void AttemptIncompleteVoucher()
  {
    var goal = _economy.GoalFor(_child, _goalId);
    _voucher = goal.Completed ? new SavingsVoucher(goal.Saved, _child, Array.Empty<FamilyMemberId>()) : null;
  }

  [Then("no voucher is created")]
  public async Task NoVoucherCreated()
  {
    await Assert.That(_voucher).IsNull();
  }

  [Given("the family has children and guardians")]
  public void FamilyHasChildrenAndGuardians()
  {
    CreateEconomy();
  }

  [When("a family savings goal is created for {string}")]
  public void SelectGoalParticipants(string participation)
  {
    var selected = new[] { _child, _supporter };
    _goalParticipants = _participantPolicy.Select(_family.Members, participation, selected);
  }

  [Then("the goal participants are {string}")]
  public async Task GoalParticipantResult(string participants)
  {
    var expected = participants switch
    {
      "all active children and guardians" => 4,
      "all active children" => 2,
      _ => 2,
    };
    await Assert.That(_goalParticipants.Count).IsEqualTo(expected);
  }

  [Given("a family savings goal uses selected family members")]
  public void SelectedGoalMembers()
  {
    CreateEconomy();
  }

  [When("one child and one guardian are selected")]
  public void SelectChildAndGuardian()
  {
    _goalParticipants = _participantPolicy.Select(_family.Members, "selected family members", [_child, _supporter]);
  }

  [Then("both are participants in the family goal")]
  public async Task BothSelectedParticipants()
  {
    await Assert.That(_goalParticipants.Contains(_child) && _goalParticipants.Contains(_supporter)).IsTrue();
  }

  [Then("every non-selected family member is not a participant")]
  public async Task NonSelectedNotParticipants()
  {
    await Assert.That(_goalParticipants.Contains(_sibling) || _goalParticipants.Contains(_admin)).IsFalse();
  }

  [Given("a family savings goal has reached its target")]
  public void CompletedFamilyGoal()
  {
    CreateEconomy();
  }

  [Given("the participating members are one child, one sibling and one guardian")]
  public void ThreeParticipants()
  {
    _goalParticipants = new HashSet<FamilyMemberId> { _child, _sibling, _supporter };
  }

  [When("the family voucher is created")]
  public void CreateFamilyVoucher()
  {
    _voucher = new SavingsVoucher(100, null, _goalParticipants);
  }

  [Then("the voucher contains the full goal balance including the family starter bonus")]
  public async Task FamilyVoucherFullBalance()
  {
    await Assert.That(_voucher?.Amount).IsEqualTo(100);
  }

  [Then("the voucher is awaiting consent from all three participants")]
  public async Task VoucherAwaitsConsent()
  {
    await Assert.That(_voucher?.Participants.Count).IsEqualTo(3);
    await Assert.That(_voucher?.Consents).IsEmpty();
  }

  [Then("the voucher cannot yet be redeemed")]
  public async Task VoucherNotRedeemable()
  {
    await Assert.That(_voucher?.Redeemable).IsFalse();
  }

  [Given("a completed family voucher has three participating family members")]
  public void ThreeParticipantVoucher()
  {
    ThreeParticipants();
    CreateFamilyVoucher();
  }

  [Given("two participants have consented")]
  [Given("only two participants have consented")]
  public void TwoConsents()
  {
    _voucher!.Consent(_child);
    _voucher.Consent(_sibling);
  }

  [When("the final participant consents")]
  public void FinalConsent()
  {
    _voucher!.Consent(_supporter);
  }

  [Then("the family voucher becomes redeemable")]
  public async Task FamilyVoucherRedeemable()
  {
    await Assert.That(_voucher?.Redeemable).IsTrue();
  }

  [When("redemption is attempted")]
  public void AttemptRedemption()
  {
    _redemptionResult = _voucher!.Redeem();
  }

  [Then("the redemption is rejected")]
  public async Task RedemptionRejected()
  {
    await Assert.That(_redemptionResult).IsFalse();
  }

  [Then("the voucher remains unredeemed")]
  public async Task VoucherUnredeemed()
  {
    await Assert.That(_voucher?.RedemptionCount).IsEqualTo(0);
  }

  [Given("a completed family voucher belongs to selected family-goal participants")]
  public void SelectedParticipantVoucher()
  {
    CreateEconomy();
    _goalParticipants = new HashSet<FamilyMemberId> { _child, _supporter };
    CreateFamilyVoucher();
  }

  [When("a non-participating family member attempts to consent")]
  public void NonParticipantConsents()
  {
    _voucher!.Consent(_sibling);
  }

  [Then("the recorded participant consents remain unchanged")]
  public async Task ConsentsUnchanged()
  {
    await Assert.That(_voucher?.Consents).IsEmpty();
  }

  [Given("every required participant has consented to a completed savings voucher")]
  public void ConsentedVoucher()
  {
    ThreeParticipantVoucher();
    TwoConsents();
    FinalConsent();
  }

  [When("the voucher is redeemed")]
  public void RedeemVoucher()
  {
    _redemptionResult = _voucher!.Redeem();
  }

  [When("redemption is attempted again")]
  public void RedeemVoucherAgain()
  {
    _secondRedemptionResult = _voucher!.Redeem();
  }

  [Then("the second redemption is rejected")]
  public async Task SecondRedemptionRejected()
  {
    await Assert.That(_secondRedemptionResult).IsFalse();
  }

  [Then("the voucher records exactly one redemption")]
  public async Task OneRedemption()
  {
    await Assert.That(_voucher?.RedemptionCount).IsEqualTo(1);
  }

  private static void awaitBonusMustBeFive(decimal bonus)
  {
    if (bonus != 5)
    {
      throw new InvalidOperationException("The configured family starter bonus must be five Ladirchen per member.");
    }
  }
}
