import { FAMILY_READ_MODEL_DESCRIPTORS } from "@/application/contracts/family-read-model-descriptors";
import type { FamilyReadModelRepositories } from "@/application/ports/family-read-model-repositories";
import { HttpFamilyReadModelRepository } from "./http-family-read-model-repository";

export function createHttpFamilyReadModelRepositories(apiBaseUrl: string): FamilyReadModelRepositories {
  return {
    contributions: new HttpFamilyReadModelRepository({
      ...FAMILY_READ_MODEL_DESCRIPTORS.contributions,
      apiBaseUrl,
    }),
    familyProfile: new HttpFamilyReadModelRepository({
      ...FAMILY_READ_MODEL_DESCRIPTORS.familyProfile,
      apiBaseUrl,
    }),
    familyProgression: new HttpFamilyReadModelRepository({
      ...FAMILY_READ_MODEL_DESCRIPTORS.familyProgression,
      apiBaseUrl,
    }),
    homeCustomization: new HttpFamilyReadModelRepository({
      ...FAMILY_READ_MODEL_DESCRIPTORS.homeCustomization,
      apiBaseUrl,
    }),
    rewardShop: new HttpFamilyReadModelRepository({
      ...FAMILY_READ_MODEL_DESCRIPTORS.rewardShop,
      apiBaseUrl,
    }),
    savings: new HttpFamilyReadModelRepository({
      ...FAMILY_READ_MODEL_DESCRIPTORS.savings,
      apiBaseUrl,
    }),
  };
}
