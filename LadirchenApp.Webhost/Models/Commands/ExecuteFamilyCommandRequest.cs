using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "commandType")]
[JsonDerivedType(typeof(ContributionsAcknowledgeRewardCommandRequest), "contributions.acknowledge-reward")]
[JsonDerivedType(typeof(ContributionsAddCommandRequest), "contributions.add")]
[JsonDerivedType(typeof(ContributionsAddPromotionCommandRequest), "contributions.add-promotion")]
[JsonDerivedType(typeof(ContributionsApproveCommandRequest), "contributions.approve")]
[JsonDerivedType(typeof(ContributionsAssignCommandRequest), "contributions.assign")]
[JsonDerivedType(typeof(ContributionsClaimCommandRequest), "contributions.claim")]
[JsonDerivedType(typeof(ContributionsDeleteCommandRequest), "contributions.delete")]
[JsonDerivedType(typeof(ContributionsDeletePromotionCommandRequest), "contributions.delete-promotion")]
[JsonDerivedType(typeof(ContributionsReturnCommandRequest), "contributions.return")]
[JsonDerivedType(typeof(ContributionsSetPartnersCommandRequest), "contributions.set-partners")]
[JsonDerivedType(typeof(ContributionsSetPerfectRatingBonusCommandRequest), "contributions.set-perfect-rating-bonus")]
[JsonDerivedType(typeof(ContributionsSubmitCommandRequest), "contributions.submit")]
[JsonDerivedType(typeof(HomeMoveEntityCommandRequest), "home.move-entity")]
[JsonDerivedType(typeof(HomePlaceAccessoryCommandRequest), "home.place-accessory")]
[JsonDerivedType(typeof(HomePurchaseAccessoryCommandRequest), "home.purchase-accessory")]
[JsonDerivedType(typeof(HomePurchaseFurnitureSetCommandRequest), "home.purchase-furniture-set")]
[JsonDerivedType(typeof(HomePurchaseRoomDesignCommandRequest), "home.purchase-room-design")]
[JsonDerivedType(typeof(HomePurchaseThemeCommandRequest), "home.purchase-theme")]
[JsonDerivedType(typeof(HomeResetEntityCommandRequest), "home.reset-entity")]
[JsonDerivedType(typeof(HomeSelectRoomDesignCommandRequest), "home.select-room-design")]
[JsonDerivedType(typeof(HomeSelectThemeCommandRequest), "home.select-theme")]
[JsonDerivedType(typeof(HomeStoreAccessoryCommandRequest), "home.store-accessory")]
[JsonDerivedType(typeof(HomeToggleAccessoryCommandRequest), "home.toggle-accessory")]
[JsonDerivedType(typeof(ProfileCompleteSetupCommandRequest), "profile.complete-setup")]
[JsonDerivedType(typeof(ProfileSaveOwnAppearanceCommandRequest), "profile.save-own-appearance")]
[JsonDerivedType(typeof(ProfileSetGuardianAccessCommandRequest), "profile.set-guardian-access")]
[JsonDerivedType(typeof(ProfileSetOwnNicknameCommandRequest), "profile.set-own-nickname")]
[JsonDerivedType(typeof(ProfileSetWeeklyParticipationCommandRequest), "profile.set-weekly-participation")]
[JsonDerivedType(typeof(ProgressionCompleteWeekCommandRequest), "progression.complete-week")]
[JsonDerivedType(typeof(ProgressionFailWeekCommandRequest), "progression.fail-week")]
[JsonDerivedType(typeof(ProgressionRefreshCommandRequest), "progression.refresh")]
[JsonDerivedType(typeof(ProgressionSetMaximumSavingsRateCommandRequest), "progression.set-maximum-savings-rate")]
[JsonDerivedType(typeof(RewardShopAddCommandRequest), "reward-shop.add")]
[JsonDerivedType(typeof(RewardShopCancelCommandRequest), "reward-shop.cancel")]
[JsonDerivedType(typeof(RewardShopDecideCommandRequest), "reward-shop.decide")]
[JsonDerivedType(typeof(RewardShopDeleteCommandRequest), "reward-shop.delete")]
[JsonDerivedType(typeof(RewardShopRequestCommandRequest), "reward-shop.request")]
[JsonDerivedType(typeof(RewardShopSetVisibilityCommandRequest), "reward-shop.set-visibility")]
[JsonDerivedType(typeof(SavingsAddGoalCommandRequest), "savings.add-goal")]
[JsonDerivedType(typeof(SavingsCancelGoalCommandRequest), "savings.cancel-goal")]
[JsonDerivedType(typeof(SavingsDecideGoalCancellationCommandRequest), "savings.decide-goal-cancellation")]
[JsonDerivedType(typeof(SavingsConsentVoucherCommandRequest), "savings.consent-voucher")]
[JsonDerivedType(typeof(SavingsConsumeGuardianGiftCommandRequest), "savings.consume-guardian-gift")]
[JsonDerivedType(typeof(SavingsDepositCommandRequest), "savings.deposit")]
[JsonDerivedType(typeof(SavingsGiftToGoalCommandRequest), "savings.gift-to-goal")]
[JsonDerivedType(typeof(SavingsGiftToMemberCommandRequest), "savings.gift-to-member")]
[JsonDerivedType(typeof(SavingsRedeemVoucherCommandRequest), "savings.redeem-voucher")]
[JsonDerivedType(typeof(SavingsRequestGoalCancellationCommandRequest), "savings.request-goal-cancellation")]
[JsonDerivedType(typeof(SavingsSetCurrencyCommandRequest), "savings.set-currency")]
[JsonDerivedType(typeof(SavingsSetExchangeRateCommandRequest), "savings.set-exchange-rate")]
[JsonDerivedType(typeof(SavingsSettleWeeklyInterestCommandRequest), "savings.settle-weekly-interest")]
[JsonDerivedType(typeof(SavingsSpecialGiftCommandRequest), "savings.special-gift")]
[JsonDerivedType(typeof(SavingsSupportGoalCommandRequest), "savings.support-goal")]
[JsonDerivedType(typeof(SavingsToggleCheerCommandRequest), "savings.toggle-cheer")]
[JsonDerivedType(typeof(SavingsUpdateGoalCommandRequest), "savings.update-goal")]
[JsonDerivedType(typeof(SavingsWithdrawCommandRequest), "savings.withdraw")]
public abstract record ExecuteFamilyCommandRequest(
  [property: JsonRequired] Guid CommandId,
  [property: JsonRequired] string ActorId
)
{
  private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

  internal abstract string GetCommandName();

  internal abstract JsonElement CreatePayloadElement();

  protected static JsonElement SerializePayload<TPayload>(TPayload payload)
    where TPayload : class
  {
    ArgumentNullException.ThrowIfNull(payload);

    return JsonSerializer.SerializeToElement(payload, _serializerOptions);
  }
}