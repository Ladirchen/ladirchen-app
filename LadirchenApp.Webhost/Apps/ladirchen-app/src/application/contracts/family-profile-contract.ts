import type { FamilyReadModel } from "./family-read-model-contract";
import type { FamilyMember, FamilyPet, IanaTimeZone, SubscriptionTier, ViewerRole } from "@/domain/family/types";

export const FAMILY_PROFILE_READ_MODEL_TYPE = "family-profile";

export interface AvatarProfileCatalog {
  readonly accessoryIds: ReadonlyArray<string>;
  readonly ages: ReadonlyArray<string>;
  readonly faceIds: ReadonlyArray<string>;
  readonly faceShapeIds: ReadonlyArray<string>;
  readonly funAccessoryIds: ReadonlyArray<string>;
  readonly hairColorIds: ReadonlyArray<string>;
  readonly hairIds: ReadonlyArray<string>;
  readonly outfitColorIds: ReadonlyArray<string>;
  readonly outfitIds: ReadonlyArray<string>;
  readonly seasonalAccessoryIds: ReadonlyArray<string>;
  readonly skinToneIds: ReadonlyArray<string>;
}

export interface FamilyProfileState {
  readonly avatarCatalog: Readonly<Record<ViewerRole, AvatarProfileCatalog>>;
  readonly members: ReadonlyArray<FamilyMember>;
  readonly onboardingCompleted: boolean;
  readonly pets: ReadonlyArray<FamilyPet>;
  readonly subscriptionTier: SubscriptionTier;
  readonly timeZone: IanaTimeZone;
}

export type FamilyProfileReadModel = FamilyReadModel<typeof FAMILY_PROFILE_READ_MODEL_TYPE, FamilyProfileState>;
