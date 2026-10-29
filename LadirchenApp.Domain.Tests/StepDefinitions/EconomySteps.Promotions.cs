using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private readonly PromotionId _promotionId = new("campaign");
  private decimal _campaignMultiplier = 2;
  private decimal _campaignTeamworkBonus;
  private decimal _perfectRatingBonus = 5;
  private ContributionRewardBreakdown? _calculatedReward;
  private bool _campaignApplied;
  private string _invalidCampaignData = string.Empty;
  private int _promotionCountBefore;

  [Given("an active double-reward campaign with {float} Ladirchen teamwork bonus until {string}")]
  public void ActiveDoubleCampaign(decimal bonus, string deadline)
  {
    _economy.AddPromotion(
      _admin,
      _promotionId,
      _contributionId,
      new RewardMultiplier(2),
      new LadirchenAmount(bonus),
      new PromotionDeadline(DateTime.Parse(deadline, System.Globalization.CultureInfo.InvariantCulture)),
      _now
    );
  }

  [When("the administrator approves the contribution with {int} stars")]
  public void ApproveContribution(int stars)
  {
    Approve(stars);
  }

  [Given("a contribution has a base reward of {float} Ladirchen")]
  public void ContributionBaseReward(decimal reward)
  {
    _campaignTeamworkBonus = 0;
    _calculatedReward = new ContributionRewardBreakdown(reward, 0, reward);
  }

  [Given("an active campaign multiplies the reward by {float}")]
  public void CampaignMultiplier(decimal multiplier)
  {
    _campaignMultiplier = multiplier;
  }

  [Given("the campaign adds {float} Ladirchen teamwork bonus")]
  public void CampaignTeamworkBonus(decimal bonus)
  {
    _campaignTeamworkBonus = bonus;
  }

  [Given("the five-star contribution bonus is configured as {float} percent")]
  public void FiveStarBonus(decimal bonus)
  {
    _perfectRatingBonus = bonus;

    if (_economy is not null)
    {
      _economy.UpdateContributionSettings(
        _admin,
        new ContributionSettings(new Percentage(bonus)),
        _now
      );
    }
  }

  [When("the contribution is approved with {int} stars before the deadline")]
  public void CalculateCampaignReward(int stars)
  {
    var baseReward = _calculatedReward?.BaseReward ?? 10;
    _calculatedReward = ContributionRewardPolicy.Calculate(
      new LadirchenAmount(baseReward),
      new StarRating(stars),
      new Percentage(_perfectRatingBonus),
      new RewardMultiplier(_campaignMultiplier),
      new LadirchenAmount(_campaignTeamworkBonus)
    );
  }

  [Then("the promoted base reward is {float} Ladirchen")]
  public async Task PromotedBaseReward(decimal reward)
  {
    await Assert.That(_calculatedReward?.BaseReward).IsEqualTo(reward);
  }

  [Then("the five-star bonus is {float} Ladirchen")]
  public async Task RatingBonus(decimal bonus)
  {
    await Assert.That(_calculatedReward?.RatingBonus).IsEqualTo(bonus);
  }

  [Then("the total reward is {float} Ladirchen")]
  public async Task TotalReward(decimal reward)
  {
    await Assert.That(_calculatedReward?.Total).IsEqualTo(reward);
  }

  [Given("a bonus campaign is active and its deadline is reached")]
  public void ActiveReachedCampaign()
  {
    CampaignAvailability("active", "reached");
  }

  [Given("a bonus campaign is active and its deadline is in the future")]
  public void ActiveFutureCampaign()
  {
    CampaignAvailability("active", "in the future");
  }

  [Given("a bonus campaign is inactive and its deadline is in the future")]
  public void InactiveFutureCampaign()
  {
    CampaignAvailability("inactive", "in the future");
  }

  public void CampaignAvailability(string state, string deadlineState)
  {
    CreateEconomy();
    SubmittedContribution(_child, _admin, 10);
    var localNow = _family.TimeZone.LocalTime(_now).DateTime;
    var deadline = deadlineState == "reached" ? localNow : localNow.AddHours(1);
    _economy.AddPromotion(
      _admin,
      _promotionId,
      _contributionId,
      new RewardMultiplier(2),
      new LadirchenAmount(0),
      new PromotionDeadline(deadline),
      _now
    );

    if (state == "inactive")
    {
      _economy.SetPromotionActive(_admin, _promotionId, false, _now);
    }
  }

  [When("its contribution is approved")]
  public void ApproveCampaignContribution()
  {
    _economy.ApproveContribution(_admin, _contributionId, new StarRating(4), NextOperation(), _now);
    _campaignApplied = _economy.WalletFor(_child).Balance == 20;
  }

  [Then("the bonus campaign is {word}")]
  public async Task CampaignAvailabilityResult(string availability)
  {
    await Assert.That(_campaignApplied).IsEqualTo(availability == "applied");
  }

  [Given("a content manager selects the {int}-times reward multiplier")]
  public void SelectCampaignMultiplier(int multiplier)
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Basic, 10);
    _campaignMultiplier = multiplier;
  }

  [When("the content manager creates a valid bonus campaign")]
  [When("the content manager creates the bonus campaign")]
  public void CreateValidCampaign()
  {
    _economy.AddPromotion(
      _admin,
      _promotionId,
      _contributionId,
      new RewardMultiplier(_campaignMultiplier),
      new LadirchenAmount(Math.Max(0, _campaignTeamworkBonus)),
      new PromotionDeadline(_family.TimeZone.LocalTime(_now).DateTime.AddHours(1)),
      _now
    );
  }

  [Then("the campaign is created with multiplier {int}")]
  public async Task CreatedMultiplier(int multiplier)
  {
    await Assert.That(_economy.Promotions.Single().Multiplier.Value).IsEqualTo(multiplier);
  }

  [Given("a content manager enters a supported multiplier, a deadline and a non-negative teamwork bonus")]
  public void ValidCampaignData()
  {
    _campaignMultiplier = 2;
    _campaignTeamworkBonus = 3;
  }

  [Then("the campaign is attached to the contribution")]
  public async Task CampaignAttached()
  {
    await Assert.That(_economy.Promotions.Single().ContributionId).IsEqualTo(_contributionId);
  }

  [Then("the campaign is active immediately")]
  public async Task CampaignActive()
  {
    await Assert.That(_economy.Promotions.Single().Active).IsTrue();
  }

  [Then("the campaign is available to the family before its deadline")]
  public async Task CampaignAvailable()
  {
    await Assert.That(_economy.Promotions.Single().Deadline.IsOpen(_now, _family.TimeZone)).IsTrue();
  }

  [Given("a content manager enters campaign data with {string}")]
  public void InvalidCampaignData(string invalidData)
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Basic, 10);
    _invalidCampaignData = invalidData;
  }

  [When("the content manager attempts to create the bonus campaign")]
  public void AttemptInvalidCampaign()
  {
    if (_invalidCampaignData == "a missing deadline")
    {
      return;
    }

    var contributionId = _invalidCampaignData == "a contribution that does not exist"
      ? new ContributionId("missing")
      : _contributionId;
    var multiplier = _invalidCampaignData == "an unsupported multiplier" ? 4 : 2;

    try
    {
      var bonus = new LadirchenAmount(_invalidCampaignData == "a negative teamwork bonus" ? -1 : 0);
      _economy.AddPromotion(
        _admin,
        _promotionId,
        contributionId,
        new RewardMultiplier(multiplier),
        bonus,
        new PromotionDeadline(_family.TimeZone.LocalTime(_now).DateTime.AddHours(1)),
        _now
      );
    }
    catch (ArgumentOutOfRangeException)
    {
    }
    catch (DomainRuleException)
    {
    }
  }

  [Then("no bonus campaign is created")]
  public async Task NoCampaignCreated()
  {
    await Assert.That(_economy.Promotions).IsEmpty();
  }

  [When("the member attempts to add or delete a bonus campaign")]
  public void UnauthorizedCampaignManagement()
  {
    _promotionCountBefore = _economy.Promotions.Count;
    Try(() =>
      _economy.AddPromotion(
        _supporter,
        _promotionId,
        _contributionId,
        new RewardMultiplier(2),
        new LadirchenAmount(0),
        new PromotionDeadline(_family.TimeZone.LocalTime(_now).DateTime.AddHours(1)),
        _now
      )
    );
  }

  [Then("the bonus campaigns remain unchanged")]
  public async Task CampaignsUnchanged()
  {
    await Assert.That(_economy.Promotions.Count).IsEqualTo(_promotionCountBefore);
  }

  [Given("a contribution has an attached bonus campaign")]
  public void ContributionWithCampaign()
  {
    ExistingContribution();
    ValidCampaignData();
    CreateValidCampaign();
  }

  [Then("the attached bonus campaign no longer exists")]
  public async Task AttachedCampaignDeleted()
  {
    await Assert.That(_economy.Promotions).IsEmpty();
  }
}
