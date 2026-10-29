import type { FamilyProfileState } from "@/application/contracts/family-profile-contract";
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
import { isAvatarAppearance } from "@/domain/avatar";
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
function isFamilyMember(value: unknown): value is FamilyMember {
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
    (value.appearance === undefined || isAvatarAppearance(value.appearance)) &&
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
  return (
    isRecord(value) &&
    isArrayOf(value.members, isFamilyMember) &&
    hasUniqueIds(value.members) &&
    typeof value.onboardingCompleted === "boolean" &&
    isArrayOf(value.pets, isFamilyPet) &&
    hasUniqueIds(value.pets) &&
    isSubscriptionTier(value.subscriptionTier) &&
    isIanaTimeZone(value.timeZone)
  );
}
