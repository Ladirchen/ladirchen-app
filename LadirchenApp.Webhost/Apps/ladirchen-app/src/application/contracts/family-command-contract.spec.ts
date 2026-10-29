import { describe, expectTypeOf, it } from "vite-plus/test";

import type {
  ContributionsAcknowledgeRewardCommandRequest,
  ContributionsAddCommandRequest,
  ContributionsAddPromotionCommandRequest,
  ContributionsApproveCommandRequest,
  ContributionsAssignCommandRequest,
  ContributionsClaimCommandRequest,
  ContributionsDeleteCommandRequest,
  ContributionsDeletePromotionCommandRequest,
  ContributionsReturnCommandRequest,
  ContributionsSetPartnersCommandRequest,
  ContributionsSetPerfectRatingBonusCommandRequest,
  ContributionsSubmitCommandRequest,
  HomeMoveEntityCommandRequest,
  HomePlaceAccessoryCommandRequest,
  HomePurchaseAccessoryCommandRequest,
  HomePurchaseFurnitureSetCommandRequest,
  HomePurchaseRoomDesignCommandRequest,
  HomePurchaseThemeCommandRequest,
  HomeResetEntityCommandRequest,
  HomeSelectRoomDesignCommandRequest,
  HomeSelectThemeCommandRequest,
  HomeStoreAccessoryCommandRequest,
  HomeToggleAccessoryCommandRequest,
  ProfileCompleteSetupCommandRequest,
  ProfileInviteGuardianCommandRequest,
  ProfileSaveOwnAppearanceCommandRequest,
  ProfileSetGuardianAccessCommandRequest,
  ProfileSetOwnNicknameCommandRequest,
  ProfileSetWeeklyParticipationCommandRequest,
  ProgressionCompleteWeekCommandRequest,
  ProgressionFailWeekCommandRequest,
  ProgressionRefreshCommandRequest,
  ProgressionSetMaximumSavingsRateCommandRequest,
  RewardShopAddCommandRequest,
  RewardShopCancelCommandRequest,
  RewardShopDecideCommandRequest,
  RewardShopDeleteCommandRequest,
  RewardShopRequestCommandRequest,
  RewardShopSetVisibilityCommandRequest,
  SavingsAddGoalCommandRequest,
  SavingsCancelGoalCommandRequest,
  SavingsConsentVoucherCommandRequest,
  SavingsConsumeGuardianGiftCommandRequest,
  SavingsDecideGoalCancellationCommandRequest,
  SavingsDepositCommandRequest,
  SavingsGiftToGoalCommandRequest,
  SavingsGiftToMemberCommandRequest,
  SavingsRedeemVoucherCommandRequest,
  SavingsRequestGoalCancellationCommandRequest,
  SavingsSetCurrencyCommandRequest,
  SavingsSetExchangeRateCommandRequest,
  SavingsSettleWeeklyInterestCommandRequest,
  SavingsSpecialGiftCommandRequest,
  SavingsSupportGoalCommandRequest,
  SavingsToggleCheerCommandRequest,
  SavingsUpdateGoalCommandRequest,
  SavingsWithdrawCommandRequest,
} from "@api/LadirchenApp.Webhost";
import type { FamilyCommandPayload, FamilyCommandPayloadMap, FamilyCommandType } from "./family-command-contract";
import type { ContributionId, SavingGoalId } from "@/domain/shared/identifiers";

interface GeneratedFamilyCommandRequestMap {
  readonly "contributions.acknowledge-reward": ContributionsAcknowledgeRewardCommandRequest;
  readonly "contributions.add": ContributionsAddCommandRequest;
  readonly "contributions.add-promotion": ContributionsAddPromotionCommandRequest;
  readonly "contributions.approve": ContributionsApproveCommandRequest;
  readonly "contributions.assign": ContributionsAssignCommandRequest;
  readonly "contributions.claim": ContributionsClaimCommandRequest;
  readonly "contributions.delete": ContributionsDeleteCommandRequest;
  readonly "contributions.delete-promotion": ContributionsDeletePromotionCommandRequest;
  readonly "contributions.return": ContributionsReturnCommandRequest;
  readonly "contributions.set-partners": ContributionsSetPartnersCommandRequest;
  readonly "contributions.set-perfect-rating-bonus": ContributionsSetPerfectRatingBonusCommandRequest;
  readonly "contributions.submit": ContributionsSubmitCommandRequest;
  readonly "home.move-entity": HomeMoveEntityCommandRequest;
  readonly "home.place-accessory": HomePlaceAccessoryCommandRequest;
  readonly "home.purchase-accessory": HomePurchaseAccessoryCommandRequest;
  readonly "home.purchase-furniture-set": HomePurchaseFurnitureSetCommandRequest;
  readonly "home.purchase-room-design": HomePurchaseRoomDesignCommandRequest;
  readonly "home.purchase-theme": HomePurchaseThemeCommandRequest;
  readonly "home.reset-entity": HomeResetEntityCommandRequest;
  readonly "home.select-room-design": HomeSelectRoomDesignCommandRequest;
  readonly "home.select-theme": HomeSelectThemeCommandRequest;
  readonly "home.store-accessory": HomeStoreAccessoryCommandRequest;
  readonly "home.toggle-accessory": HomeToggleAccessoryCommandRequest;
  readonly "profile.complete-setup": ProfileCompleteSetupCommandRequest;
  readonly "profile.invite-guardian": ProfileInviteGuardianCommandRequest;
  readonly "profile.save-own-appearance": ProfileSaveOwnAppearanceCommandRequest;
  readonly "profile.set-guardian-access": ProfileSetGuardianAccessCommandRequest;
  readonly "profile.set-own-nickname": ProfileSetOwnNicknameCommandRequest;
  readonly "profile.set-weekly-participation": ProfileSetWeeklyParticipationCommandRequest;
  readonly "progression.complete-week": ProgressionCompleteWeekCommandRequest;
  readonly "progression.fail-week": ProgressionFailWeekCommandRequest;
  readonly "progression.refresh": ProgressionRefreshCommandRequest;
  readonly "progression.set-maximum-savings-rate": ProgressionSetMaximumSavingsRateCommandRequest;
  readonly "reward-shop.add": RewardShopAddCommandRequest;
  readonly "reward-shop.cancel": RewardShopCancelCommandRequest;
  readonly "reward-shop.decide": RewardShopDecideCommandRequest;
  readonly "reward-shop.delete": RewardShopDeleteCommandRequest;
  readonly "reward-shop.request": RewardShopRequestCommandRequest;
  readonly "reward-shop.set-visibility": RewardShopSetVisibilityCommandRequest;
  readonly "savings.add-goal": SavingsAddGoalCommandRequest;
  readonly "savings.cancel-goal": SavingsCancelGoalCommandRequest;
  readonly "savings.consent-voucher": SavingsConsentVoucherCommandRequest;
  readonly "savings.consume-guardian-gift": SavingsConsumeGuardianGiftCommandRequest;
  readonly "savings.decide-goal-cancellation": SavingsDecideGoalCancellationCommandRequest;
  readonly "savings.deposit": SavingsDepositCommandRequest;
  readonly "savings.gift-to-goal": SavingsGiftToGoalCommandRequest;
  readonly "savings.gift-to-member": SavingsGiftToMemberCommandRequest;
  readonly "savings.redeem-voucher": SavingsRedeemVoucherCommandRequest;
  readonly "savings.request-goal-cancellation": SavingsRequestGoalCancellationCommandRequest;
  readonly "savings.set-currency": SavingsSetCurrencyCommandRequest;
  readonly "savings.set-exchange-rate": SavingsSetExchangeRateCommandRequest;
  readonly "savings.settle-weekly-interest": SavingsSettleWeeklyInterestCommandRequest;
  readonly "savings.special-gift": SavingsSpecialGiftCommandRequest;
  readonly "savings.support-goal": SavingsSupportGoalCommandRequest;
  readonly "savings.toggle-cheer": SavingsToggleCheerCommandRequest;
  readonly "savings.update-goal": SavingsUpdateGoalCommandRequest;
  readonly "savings.withdraw": SavingsWithdrawCommandRequest;
}

type GeneratedFamilyCommandPayloadMap = {
  readonly [
    TCommandType in keyof GeneratedFamilyCommandRequestMap
  ]: GeneratedFamilyCommandRequestMap[TCommandType]["payload"];
};

type DeepReadonly<TValue> =
  TValue extends ReadonlyArray<infer TItem>
    ? ReadonlyArray<DeepReadonly<TItem>>
    : TValue extends object
      ? { readonly [TKey in keyof TValue]: DeepReadonly<TValue[TKey]> }
      : TValue;

type HasSameKeys<TLeft, TRight> =
  Exclude<keyof TLeft, keyof TRight> extends never
    ? Exclude<keyof TRight, keyof TLeft> extends never
      ? true
      : false
    : false;

type HaveExactNestedKeys<TApplication, TGenerated> =
  NonNullable<TGenerated> extends ReadonlyArray<infer TGeneratedItem>
    ? NonNullable<TApplication> extends ReadonlyArray<infer TApplicationItem>
      ? HaveExactNestedKeys<TApplicationItem, TGeneratedItem>
      : false
    : NonNullable<TGenerated> extends object
      ? keyof NonNullable<TGenerated> extends never
        ? true
        : NonNullable<TApplication> extends object
          ? HasSameKeys<NonNullable<TApplication>, NonNullable<TGenerated>> extends true
            ? false extends {
                readonly [TKey in keyof NonNullable<TGenerated>]: TKey extends keyof NonNullable<TApplication>
                  ? HaveExactNestedKeys<NonNullable<TApplication>[TKey], NonNullable<TGenerated>[TKey]>
                  : false;
              }[keyof NonNullable<TGenerated>]
              ? false
              : true
            : false
          : false
      : true;

type IncompatiblePayloadCommandTypes = {
  readonly [TCommandType in FamilyCommandType]: FamilyCommandPayloadMap[TCommandType] extends DeepReadonly<
    GeneratedFamilyCommandPayloadMap[TCommandType]
  >
    ? HaveExactNestedKeys<
        FamilyCommandPayloadMap[TCommandType],
        GeneratedFamilyCommandPayloadMap[TCommandType]
      > extends true
      ? never
      : TCommandType
    : TCommandType;
}[FamilyCommandType];

function acceptFamilyCommand<TCommandType extends FamilyCommandType>(
  commandType: TCommandType,
  payload: FamilyCommandPayload<NoInfer<TCommandType>>,
): void {
  void commandType;
  void payload;
}

function verifyInvalidCommandsAreRejected(contributionId: ContributionId, savingGoalId: SavingGoalId): void {
  // @ts-expect-error A contribution ID cannot identify a savings goal.
  acceptFamilyCommand("savings.deposit", { amount: 10, id: contributionId });
  // @ts-expect-error Deposit commands do not accept approval fields.
  acceptFamilyCommand("savings.deposit", { amount: 10, id: savingGoalId, stars: 5 });
  // @ts-expect-error Unknown command types cannot cross the command boundary.
  acceptFamilyCommand("savings.deposit-now", { amount: 10, id: savingGoalId });
}

describe("family command contract", () => {
  it("covers every generated polymorphic request with a compatible payload", () => {
    expectTypeOf<Exclude<FamilyCommandType, keyof GeneratedFamilyCommandRequestMap>>().toEqualTypeOf<never>();
    expectTypeOf<Exclude<keyof GeneratedFamilyCommandRequestMap, FamilyCommandType>>().toEqualTypeOf<never>();
    expectTypeOf<IncompatiblePayloadCommandTypes>().toEqualTypeOf<never>();
  });

  it("keeps command type and payload coupled", () => {
    expectTypeOf(verifyInvalidCommandsAreRejected).toBeFunction();
  });
});
