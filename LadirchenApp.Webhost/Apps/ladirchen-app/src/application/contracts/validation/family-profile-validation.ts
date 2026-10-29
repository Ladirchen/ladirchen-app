import type { AvatarProfileCatalog, FamilyProfileState } from "@/application/contracts/family-profile-contract";
import {
  exhaustiveValues,
  hasUniqueIds,
  isArrayOf,
  isDomainId,
  isFamilyMemberId,
  isHexColor,
  isKnownString,
  isNonEmptyString,
  isNonNegativeInteger,
  isOptionalBoolean,
  isOptionalString,
  isRecord,
} from "./runtime-validation";
import { isAvatarAppearanceStructure } from "@/domain/avatar";
import type { AvatarAppearance } from "@/domain/avatar";
import type { FamilyPermissions } from "@/domain/family/permissions";
import type {
  FamilyMember,
  FamilyPet,
  FamilyPetKindId,
  GuardianAccessLevel,
  SubscriptionTier,
  ViewerRole,
} from "@/domain/family/types";
import { isIanaTimeZone } from "@/domain/family/time-zone";
const viewerRoles = exhaustiveValues<ViewerRole>({ child: true, guardian: true });
const subscriptionTiers = exhaustiveValues<SubscriptionTier>({ free: true, pro: true });
const guardianAccessLevels = exhaustiveValues<GuardianAccessLevel>({ admin: true, supporter: true });
const familyPetKinds = exhaustiveValues<FamilyPetKindId>({
  bird: true,
  cat: true,
  dog: true,
  other: true,
  rabbit: true,
});
export function isViewerRole(value: unknown): value is ViewerRole {
  return isKnownString(value, viewerRoles);
}
export function isSubscriptionTier(value: unknown): value is SubscriptionTier {
  return isKnownString(value, subscriptionTiers);
}
export function isGuardianAccessLevel(value: unknown): value is GuardianAccessLevel {
  return isKnownString(value, guardianAccessLevels);
}
export function isFamilyPetKindId(value: unknown): value is FamilyPetKindId {
  return isKnownString(value, familyPetKinds);
}
function isFamilyPermissions(value: unknown): value is FamilyPermissions {
  return (
    isRecord(value) &&
    typeof value.canInviteMembers === "boolean" &&
    typeof value.canManageContent === "boolean" &&
    typeof value.canManageFamily === "boolean" &&
    typeof value.canManageGoals === "boolean" &&
    typeof value.canSupportChildGoals === "boolean" &&
    typeof value.canViewFamilyGoals === "boolean" &&
    typeof value.canViewGuardianGoals === "boolean"
  );
}
function isAvatarProfileCatalog(value: unknown): value is AvatarProfileCatalog {
  return (
    isRecord(value) &&
    isArrayOf(value.accessoryIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.ages, (item): item is string => typeof item === "string") &&
    isArrayOf(value.faceIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.faceShapeIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.funAccessoryIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.hairColorIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.hairIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.outfitColorIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.outfitIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.seasonalAccessoryIds, (item): item is string => typeof item === "string") &&
    isArrayOf(value.skinToneIds, (item): item is string => typeof item === "string")
  );
}
function appearanceBelongsToCatalog(appearance: AvatarAppearance, catalog: AvatarProfileCatalog): boolean {
  return (
    catalog.ages.includes(appearance.age) &&
    catalog.skinToneIds.includes(appearance.skinToneId) &&
    catalog.faceShapeIds.includes(appearance.faceShape) &&
    catalog.faceIds.includes(appearance.face) &&
    catalog.hairIds.includes(appearance.hair) &&
    catalog.hairColorIds.includes(appearance.hairColorId) &&
    catalog.outfitIds.includes(appearance.outfit) &&
    catalog.outfitColorIds.includes(appearance.outfitColorId) &&
    catalog.accessoryIds.includes(appearance.accessoryId) &&
    catalog.funAccessoryIds.includes(appearance.funAccessoryId) &&
    catalog.seasonalAccessoryIds.includes(appearance.seasonalAccessoryId)
  );
}
function isFamilyMember(
  value: unknown,
  avatarCatalog: Readonly<Record<ViewerRole, AvatarProfileCatalog>>,
): value is FamilyMember {
  return (
    isRecord(value) &&
    isFamilyMemberId(value.id) &&
    isNonEmptyString(value.name) &&
    isOptionalString(value.nickname) &&
    isNonEmptyString(value.avatar) &&
    isHexColor(value.color) &&
    isViewerRole(value.role) &&
    (value.guardianAccess === undefined || isGuardianAccessLevel(value.guardianAccess)) &&
    isOptionalBoolean(value.participatesInWeeklyGoal) &&
    isNonNegativeInteger(value.weeklyStreak) &&
    isOptionalString(value.email) &&
    isOptionalBoolean(value.invitationPending) &&
    (value.appearance === undefined ||
      (isAvatarAppearanceStructure(value.appearance) &&
        appearanceBelongsToCatalog(value.appearance, avatarCatalog[value.role]))) &&
    (value.permissions === undefined || isFamilyPermissions(value.permissions))
  );
}
function isFamilyPet(value: unknown): value is FamilyPet {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isNonEmptyString(value.name) &&
    isFamilyPetKindId(value.kind) &&
    isNonEmptyString(value.avatar) &&
    isHexColor(value.color)
  );
}
export function isFamilyProfileState(value: unknown): value is FamilyProfileState {
  if (
    !isRecord(value) ||
    !isRecord(value.avatarCatalog) ||
    !isAvatarProfileCatalog(value.avatarCatalog.child) ||
    !isAvatarProfileCatalog(value.avatarCatalog.guardian)
  ) {
    return false;
  }

  const avatarCatalog = {
    child: value.avatarCatalog.child,
    guardian: value.avatarCatalog.guardian,
  } satisfies Readonly<Record<ViewerRole, AvatarProfileCatalog>>;

  return (
    isArrayOf(value.members, (member): member is FamilyMember => isFamilyMember(member, avatarCatalog)) &&
    hasUniqueIds(value.members) &&
    typeof value.onboardingCompleted === "boolean" &&
    isArrayOf(value.pets, isFamilyPet) &&
    hasUniqueIds(value.pets) &&
    isSubscriptionTier(value.subscriptionTier) &&
    isIanaTimeZone(value.timeZone)
  );
}
