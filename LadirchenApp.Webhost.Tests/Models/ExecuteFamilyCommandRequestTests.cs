using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Models.Commands;

namespace LadirchenApp.Webhost.Tests.Models;

public sealed class ExecuteFamilyCommandRequestTests
{
  private const string CommandId = "d9dd1e2c-2ad6-4d95-b52d-3b1a6ff112b1";

  private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
  {
    AllowOutOfOrderMetadataProperties = true,
  };

  [Test]
  public async Task EverySupportedCommandDeserializesToItsTypedRequest()
  {
    foreach (var command in SupportedCommands())
    {
      var json = $$"""
        {
          "commandId": "{{CommandId}}",
          "actorId": "guardian",
          "commandType": "{{command.Key}}",
          "payload": {{command.Value.Payload}}
        }
        """;

      var request = JsonSerializer.Deserialize<ExecuteFamilyCommandRequest>(json, _serializerOptions);

      await Assert.That(request).IsNotNull();
      await Assert.That(request!.GetType()).IsEqualTo(command.Value.RequestType);

      var roundTrip = JsonSerializer.SerializeToElement(request, _serializerOptions);
      await Assert.That(roundTrip.GetProperty("commandType").GetString()).IsEqualTo(command.Key);
      await Assert.That(roundTrip.GetProperty("payload").ValueKind).IsEqualTo(JsonValueKind.Object);
    }
  }

  [Test]
  public async Task UnknownCommandTypeIsRejectedDuringDeserialization()
  {
    const string json = $$"""
      {
        "commandId": "{{CommandId}}",
        "actorId": "guardian",
        "commandType": "unknown.command",
        "payload": {}
      }
      """;

    await Assert.That(() => JsonSerializer.Deserialize<ExecuteFamilyCommandRequest>(json, _serializerOptions))
      .Throws<JsonException>();
  }

  [Test]
  public async Task MissingRequiredPayloadPropertyIsRejectedDuringDeserialization()
  {
    const string json = $$"""
      {
        "commandId": "{{CommandId}}",
        "actorId": "guardian",
        "commandType": "contributions.submit",
        "payload": {}
      }
      """;

    await Assert.That(() => JsonSerializer.Deserialize<ExecuteFamilyCommandRequest>(json, _serializerOptions))
      .Throws<JsonException>();
  }

  [Test]
  public async Task MissingPayloadIsRejectedDuringDeserialization()
  {
    const string json = $$"""
      {
        "commandId": "{{CommandId}}",
        "actorId": "guardian",
        "commandType": "progression.refresh"
      }
      """;

    await Assert.That(() => JsonSerializer.Deserialize<ExecuteFamilyCommandRequest>(json, _serializerOptions))
      .Throws<JsonException>();
  }

  [Test]
  public async Task InvalidPayloadPropertyTypeIsRejectedDuringDeserialization()
  {
    const string json = $$"""
      {
        "commandId": "{{CommandId}}",
        "actorId": "guardian",
        "commandType": "contributions.approve",
        "payload": { "id": "dishes", "stars": "five" }
      }
      """;

    await Assert.That(() => JsonSerializer.Deserialize<ExecuteFamilyCommandRequest>(json, _serializerOptions))
      .Throws<JsonException>();
  }

  private static IReadOnlyDictionary<string, (Type RequestType, string Payload)> SupportedCommands()
  {
    return new Dictionary<string, (Type, string)>(StringComparer.Ordinal)
    {
      ["contributions.acknowledge-reward"] = (typeof(ContributionsAcknowledgeRewardCommandRequest), Id()),
      ["contributions.add"] = (typeof(ContributionsAddCommandRequest), """{"id":"dishes","title":"Dishes","description":"","icon":"dish","reward":10,"energy":20,"kind":"basic"}"""),
      ["contributions.add-promotion"] = (typeof(ContributionsAddPromotionCommandRequest), """{"id":"promo","contributionId":"dishes","multiplier":2,"deadline":"18:00","teamworkBonus":1}"""),
      ["contributions.approve"] = (typeof(ContributionsApproveCommandRequest), """{"id":"dishes","stars":5}"""),
      ["contributions.assign"] = (typeof(ContributionsAssignCommandRequest), """{"id":"dishes","memberId":"child"}"""),
      ["contributions.claim"] = (typeof(ContributionsClaimCommandRequest), Id()),
      ["contributions.delete"] = (typeof(ContributionsDeleteCommandRequest), Id()),
      ["contributions.delete-promotion"] = (typeof(ContributionsDeletePromotionCommandRequest), Id()),
      ["contributions.return"] = (typeof(ContributionsReturnCommandRequest), Id()),
      ["contributions.set-partners"] = (typeof(ContributionsSetPartnersCommandRequest), """{"id":"dishes","partnerIds":["child"]}"""),
      ["contributions.set-perfect-rating-bonus"] = (typeof(ContributionsSetPerfectRatingBonusCommandRequest), Percentage()),
      ["contributions.submit"] = (typeof(ContributionsSubmitCommandRequest), Id()),
      ["home.move-entity"] = (typeof(HomeMoveEntityCommandRequest), """{"placementId":"chair","zoneId":"inside","x":10,"y":20}"""),
      ["home.place-accessory"] = (typeof(HomePlaceAccessoryCommandRequest), """{"id":"plant","zoneId":"inside"}"""),
      ["home.purchase-accessory"] = (typeof(HomePurchaseAccessoryCommandRequest), Id()),
      ["home.purchase-furniture-set"] = (typeof(HomePurchaseFurnitureSetCommandRequest), Id()),
      ["home.purchase-room-design"] = (typeof(HomePurchaseRoomDesignCommandRequest), Id()),
      ["home.purchase-theme"] = (typeof(HomePurchaseThemeCommandRequest), Id()),
      ["home.reset-entity"] = (typeof(HomeResetEntityCommandRequest), """{"placementId":"chair"}"""),
      ["home.select-room-design"] = (typeof(HomeSelectRoomDesignCommandRequest), Id()),
      ["home.select-theme"] = (typeof(HomeSelectThemeCommandRequest), Id()),
      ["home.store-accessory"] = (typeof(HomeStoreAccessoryCommandRequest), Id()),
      ["home.toggle-accessory"] = (typeof(HomeToggleAccessoryCommandRequest), Id()),
      ["profile.complete-setup"] = (typeof(ProfileCompleteSetupCommandRequest), """{"members":[{"id":"child","name":"Anna","avatar":"A","color":"#fff","role":"child"}],"pets":[{"id":"pet","name":"Milo","kind":"cat","avatar":"M","color":"#000"}]}"""),
      ["profile.save-own-appearance"] = (typeof(ProfileSaveOwnAppearanceCommandRequest), """{"appearance":{"age":"adult","skinToneId":"skin-medium","faceShape":"soft","face":"happy","hair":"short","hairColorId":"hair-brown","outfit":"shirt","outfitColorId":"outfit-blue","accessoryId":"none","funAccessoryId":"none","seasonalAccessoryId":"none"}}"""),
      ["profile.set-guardian-access"] = (typeof(ProfileSetGuardianAccessCommandRequest), """{"memberId":"guardian-2","guardianAccess":"admin"}"""),
      ["profile.set-own-nickname"] = (typeof(ProfileSetOwnNicknameCommandRequest), """{"nickname":"Mum"}"""),
      ["profile.set-weekly-participation"] = (typeof(ProfileSetWeeklyParticipationCommandRequest), """{"memberId":"child","participates":true}"""),
      ["progression.complete-week"] = (typeof(ProgressionCompleteWeekCommandRequest), "{}"),
      ["progression.fail-week"] = (typeof(ProgressionFailWeekCommandRequest), "{}"),
      ["progression.refresh"] = (typeof(ProgressionRefreshCommandRequest), "{}"),
      ["progression.set-maximum-savings-rate"] = (typeof(ProgressionSetMaximumSavingsRateCommandRequest), Percentage()),
      ["reward-shop.add"] = (typeof(RewardShopAddCommandRequest), """{"id":"reward","title":"Cinema","description":"Film","icon":"ticket","price":20,"category":"activity","quantity":1,"conditions":"Weekend"}"""),
      ["reward-shop.cancel"] = (typeof(RewardShopCancelCommandRequest), Id()),
      ["reward-shop.decide"] = (typeof(RewardShopDecideCommandRequest), """{"id":"reward","approved":true}"""),
      ["reward-shop.delete"] = (typeof(RewardShopDeleteCommandRequest), Id()),
      ["reward-shop.request"] = (typeof(RewardShopRequestCommandRequest), Id()),
      ["reward-shop.set-visibility"] = (typeof(RewardShopSetVisibilityCommandRequest), """{"id":"reward","isVisible":true}"""),
      ["savings.add-goal"] = (typeof(SavingsAddGoalCommandRequest), """{"id":"goal","ownerId":"child","title":"Bike","icon":"bike","target":100,"visibility":"private"}"""),
      ["savings.cancel-goal"] = (typeof(SavingsCancelGoalCommandRequest), Id("goal")),
      ["savings.decide-goal-cancellation"] = (typeof(SavingsDecideGoalCancellationCommandRequest), """{"id":"goal","approved":true}"""),
      ["savings.consent-voucher"] = (typeof(SavingsConsentVoucherCommandRequest), Id("goal")),
      ["savings.consume-guardian-gift"] = (typeof(SavingsConsumeGuardianGiftCommandRequest), """{"giftId":"gift"}"""),
      ["savings.deposit"] = (typeof(SavingsDepositCommandRequest), GoalAmount()),
      ["savings.gift-to-goal"] = (typeof(SavingsGiftToGoalCommandRequest), GoalGift()),
      ["savings.gift-to-member"] = (typeof(SavingsGiftToMemberCommandRequest), """{"recipientId":"child","amount":10,"giftId":"gift"}"""),
      ["savings.redeem-voucher"] = (typeof(SavingsRedeemVoucherCommandRequest), Id("goal")),
      ["savings.request-goal-cancellation"] = (typeof(SavingsRequestGoalCancellationCommandRequest), Id("goal")),
      ["savings.set-currency"] = (typeof(SavingsSetCurrencyCommandRequest), """{"currency":"CHF"}"""),
      ["savings.set-exchange-rate"] = (typeof(SavingsSetExchangeRateCommandRequest), """{"rate":4}"""),
      ["savings.settle-weekly-interest"] = (typeof(SavingsSettleWeeklyInterestCommandRequest), """{"year":2026,"week":42}"""),
      ["savings.special-gift"] = (typeof(SavingsSpecialGiftCommandRequest), """{"childId":"child","amount":10,"giftId":"gift","reason":"Birthday"}"""),
      ["savings.support-goal"] = (typeof(SavingsSupportGoalCommandRequest), GoalGift()),
      ["savings.toggle-cheer"] = (typeof(SavingsToggleCheerCommandRequest), Id("goal")),
      ["savings.update-goal"] = (typeof(SavingsUpdateGoalCommandRequest), """{"id":"goal","title":"Bike","icon":"bike","target":120,"visibility":"private"}"""),
      ["savings.withdraw"] = (typeof(SavingsWithdrawCommandRequest), GoalAmount()),
    };
  }

  private static string GoalAmount()
  {
    return """{"id":"goal","amount":10}""";
  }

  private static string GoalGift()
  {
    return """{"id":"goal","amount":10,"giftId":"gift"}""";
  }

  private static string Id(string id = "dishes")
  {
    return $$"""{"id":"{{id}}"}""";
  }

  private static string Percentage()
  {
    return """{"percent":8}""";
  }
}