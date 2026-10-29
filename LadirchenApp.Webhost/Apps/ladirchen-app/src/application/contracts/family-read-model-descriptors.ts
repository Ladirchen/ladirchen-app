import { CONTRIBUTIONS_READ_MODEL_TYPE } from "./contributions-contract";
import type { ContributionsState } from "./contributions-contract";
import { FAMILY_PROFILE_READ_MODEL_TYPE } from "./family-profile-contract";
import type { FamilyProfileState } from "./family-profile-contract";
import { FAMILY_PROGRESSION_READ_MODEL_TYPE } from "./family-progression-contract";
import type { FamilyProgressionState } from "./family-progression-contract";
import { HOME_CUSTOMIZATION_READ_MODEL_TYPE } from "./home-customization-contract";
import type { HomeCustomizationState } from "./home-customization-contract";
import { REWARD_SHOP_READ_MODEL_TYPE } from "./reward-shop-contract";
import type { RewardShopState } from "./reward-shop-contract";
import { SAVINGS_READ_MODEL_TYPE } from "./savings-contract";
import type { SavingsState } from "./savings-contract";
import {
  isContributionsState,
  isFamilyProfileState,
  isFamilyProgressionState,
  isHomeCustomizationState,
  isRewardShopState,
  isSavingsState,
} from "./family-read-model-validation";
import type { StateGuard } from "./family-read-model-validation";
import type { FamilyReadModelType } from "./family-read-model-contract";

export interface FamilyReadModelDescriptor<TReadModelType extends FamilyReadModelType, TState> {
  readonly isState: StateGuard<TState>;
  readonly readModelType: TReadModelType;
}

export const FAMILY_READ_MODEL_DESCRIPTORS = {
  contributions: {
    readModelType: CONTRIBUTIONS_READ_MODEL_TYPE,
    isState: isContributionsState,
  } satisfies FamilyReadModelDescriptor<typeof CONTRIBUTIONS_READ_MODEL_TYPE, ContributionsState>,
  familyProfile: {
    readModelType: FAMILY_PROFILE_READ_MODEL_TYPE,
    isState: isFamilyProfileState,
  } satisfies FamilyReadModelDescriptor<typeof FAMILY_PROFILE_READ_MODEL_TYPE, FamilyProfileState>,
  familyProgression: {
    readModelType: FAMILY_PROGRESSION_READ_MODEL_TYPE,
    isState: isFamilyProgressionState,
  } satisfies FamilyReadModelDescriptor<typeof FAMILY_PROGRESSION_READ_MODEL_TYPE, FamilyProgressionState>,
  homeCustomization: {
    readModelType: HOME_CUSTOMIZATION_READ_MODEL_TYPE,
    isState: isHomeCustomizationState,
  } satisfies FamilyReadModelDescriptor<typeof HOME_CUSTOMIZATION_READ_MODEL_TYPE, HomeCustomizationState>,
  rewardShop: {
    readModelType: REWARD_SHOP_READ_MODEL_TYPE,
    isState: isRewardShopState,
  } satisfies FamilyReadModelDescriptor<typeof REWARD_SHOP_READ_MODEL_TYPE, RewardShopState>,
  savings: {
    readModelType: SAVINGS_READ_MODEL_TYPE,
    isState: isSavingsState,
  } satisfies FamilyReadModelDescriptor<typeof SAVINGS_READ_MODEL_TYPE, SavingsState>,
} as const;
