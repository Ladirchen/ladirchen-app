import type { FamilyProfileState } from '../family-profile-contract';
import type { StateGuard } from './runtime-validation';
import { exhaustiveValues, hasUniqueIds, isArrayOf, isDomainId, isFamilyMemberId, isHexColor, isKnownString, isNonEmptyString, isNonNegativeInteger, isOptionalBoolean, isOptionalString, isRecord } from './runtime-validation';
import { isAvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyPet, FamilyPetKindId, GuardianAccessLevel, SubscriptionTier, ViewerRole } from '@/domain/types';
import { isIanaTimeZone } from '@/domain/time-zone';

const viewerRoles = exhaustiveValues<ViewerRole>({ child: true, guardian: true });
const subscriptionTiers = exhaustiveValues<SubscriptionTier>({ free: true, pro: true });
const guardianAccessLevels = exhaustiveValues<GuardianAccessLevel>({ admin: true, supporter: true });
const familyPetKinds = exhaustiveValues<FamilyPetKindId>({ bird: true, cat: true, dog: true, other: true, rabbit: true });

export const isViewerRole = (value: unknown): value is ViewerRole => isKnownString(value, viewerRoles);
export const isSubscriptionTier = (value: unknown): value is SubscriptionTier => isKnownString(value, subscriptionTiers);
export const isGuardianAccessLevel = (value: unknown): value is GuardianAccessLevel => isKnownString(value, guardianAccessLevels);
export const isFamilyPetKindId = (value: unknown): value is FamilyPetKindId => isKnownString(value, familyPetKinds);

const isFamilyMember = (value: unknown): value is FamilyMember =>
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
  (value.appearance === undefined || isAvatarAppearance(value.appearance));

const isFamilyPet = (value: unknown): value is FamilyPet =>
  isRecord(value) &&
  isDomainId(value.id) &&
  isNonEmptyString(value.name) &&
  isFamilyPetKindId(value.kind) &&
  isNonEmptyString(value.avatar) &&
  isHexColor(value.color);

export const isFamilyProfileState: StateGuard<FamilyProfileState> = (value): value is FamilyProfileState =>
  isRecord(value) &&
  isArrayOf(value.members, isFamilyMember) && hasUniqueIds(value.members) &&
  typeof value.onboardingCompleted === 'boolean' &&
  isArrayOf(value.pets, isFamilyPet) && hasUniqueIds(value.pets) &&
  isSubscriptionTier(value.subscriptionTier) &&
  isIanaTimeZone(value.timeZone);
