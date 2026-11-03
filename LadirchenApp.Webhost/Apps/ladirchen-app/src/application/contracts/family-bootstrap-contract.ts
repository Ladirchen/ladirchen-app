import type { ContributionsReadModel } from "@/application/contracts/contributions-contract";
import type { FamilyProfileReadModel } from "@/application/contracts/family-profile-contract";
import type { FamilyProgressionReadModel } from "@/application/contracts/family-progression-contract";
import type { HomeCustomizationReadModel } from "@/application/contracts/home-customization-contract";
import type { RewardShopReadModel } from "@/application/contracts/reward-shop-contract";
import type { SavingsReadModel } from "@/application/contracts/savings-contract";
import { FAMILY_READ_MODEL_DESCRIPTORS } from "@/application/contracts/family-read-model-descriptors";
import { isFamilyReadModel } from "@/application/contracts/family-read-model-validation";
import { isRecord } from "@/application/contracts/validation/runtime-validation";

export interface FamilyBootstrapReadModel {
  readonly contributions: ContributionsReadModel;
  readonly familyProfile: FamilyProfileReadModel;
  readonly familyProgression: FamilyProgressionReadModel | null;
  readonly homeCustomization: HomeCustomizationReadModel | null;
  readonly rewardShop: RewardShopReadModel;
  readonly savings: SavingsReadModel;
}

export function isFamilyBootstrapReadModel(value: unknown): value is FamilyBootstrapReadModel {
  return (
    isRecord(value) &&
    isFamilyReadModel(
      value.contributions,
      FAMILY_READ_MODEL_DESCRIPTORS.contributions.readModelType,
      FAMILY_READ_MODEL_DESCRIPTORS.contributions.isState,
    ) &&
    isFamilyReadModel(
      value.familyProfile,
      FAMILY_READ_MODEL_DESCRIPTORS.familyProfile.readModelType,
      FAMILY_READ_MODEL_DESCRIPTORS.familyProfile.isState,
    ) &&
    (value.familyProgression === null ||
      isFamilyReadModel(
        value.familyProgression,
        FAMILY_READ_MODEL_DESCRIPTORS.familyProgression.readModelType,
        FAMILY_READ_MODEL_DESCRIPTORS.familyProgression.isState,
      )) &&
    (value.homeCustomization === null ||
      isFamilyReadModel(
        value.homeCustomization,
        FAMILY_READ_MODEL_DESCRIPTORS.homeCustomization.readModelType,
        FAMILY_READ_MODEL_DESCRIPTORS.homeCustomization.isState,
      )) &&
    isFamilyReadModel(
      value.rewardShop,
      FAMILY_READ_MODEL_DESCRIPTORS.rewardShop.readModelType,
      FAMILY_READ_MODEL_DESCRIPTORS.rewardShop.isState,
    ) &&
    isFamilyReadModel(
      value.savings,
      FAMILY_READ_MODEL_DESCRIPTORS.savings.readModelType,
      FAMILY_READ_MODEL_DESCRIPTORS.savings.isState,
    )
  );
}
