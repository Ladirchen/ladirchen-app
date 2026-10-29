import type { CONTRIBUTIONS_READ_MODEL_TYPE, ContributionsState } from "@/application/contracts/contributions-contract";
import type {
  FAMILY_PROFILE_READ_MODEL_TYPE,
  FamilyProfileState,
} from "@/application/contracts/family-profile-contract";
import type {
  FAMILY_PROGRESSION_READ_MODEL_TYPE,
  FamilyProgressionState,
} from "@/application/contracts/family-progression-contract";
import type {
  HOME_CUSTOMIZATION_READ_MODEL_TYPE,
  HomeCustomizationState,
} from "@/application/contracts/home-customization-contract";
import type { REWARD_SHOP_READ_MODEL_TYPE, RewardShopState } from "@/application/contracts/reward-shop-contract";
import type { SAVINGS_READ_MODEL_TYPE, SavingsState } from "@/application/contracts/savings-contract";
import type { FamilyReadModelRepository } from "./family-read-model-repository";

export type ContributionsRepository = FamilyReadModelRepository<
  typeof CONTRIBUTIONS_READ_MODEL_TYPE,
  ContributionsState
>;

export type FamilyProfileRepository = FamilyReadModelRepository<
  typeof FAMILY_PROFILE_READ_MODEL_TYPE,
  FamilyProfileState
>;

export type FamilyProgressionRepository = FamilyReadModelRepository<
  typeof FAMILY_PROGRESSION_READ_MODEL_TYPE,
  FamilyProgressionState
>;

export type HomeCustomizationRepository = FamilyReadModelRepository<
  typeof HOME_CUSTOMIZATION_READ_MODEL_TYPE,
  HomeCustomizationState
>;

export type RewardShopRepository = FamilyReadModelRepository<typeof REWARD_SHOP_READ_MODEL_TYPE, RewardShopState>;

export type SavingsRepository = FamilyReadModelRepository<typeof SAVINGS_READ_MODEL_TYPE, SavingsState>;

export interface FamilyReadModelRepositories {
  readonly contributions: ContributionsRepository;
  readonly familyProfile: FamilyProfileRepository;
  readonly familyProgression: FamilyProgressionRepository;
  readonly homeCustomization: HomeCustomizationRepository;
  readonly rewardShop: RewardShopRepository;
  readonly savings: SavingsRepository;
}
