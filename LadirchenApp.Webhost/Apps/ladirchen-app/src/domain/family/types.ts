import type { AvatarAppearance } from "@/domain/avatar";
import type { HexColor } from "@/domain/house";
import type { FamilyMemberId, FamilyPetId } from "@/domain/shared/identifiers";

export type { IanaTimeZone } from "./time-zone";

export type ViewerRole = "child" | "guardian";
export type SubscriptionTier = "free" | "pro";
export type GuardianAccessLevel = "admin" | "supporter";
export type FamilyPetKindId = "cat" | "dog" | "rabbit" | "bird" | "other";

export interface FamilyMember {
  id: FamilyMemberId;
  name: string;
  nickname?: string;
  avatar: string;
  color: HexColor;
  role: ViewerRole;
  guardianAccess?: GuardianAccessLevel;
  participatesInWeeklyGoal?: boolean;
  weeklyStreak: number;
  email?: string;
  invitationPending?: boolean;
  appearance?: AvatarAppearance;
}

export interface FamilyPet {
  id: FamilyPetId;
  name: string;
  kind: FamilyPetKindId;
  avatar: string;
  color: HexColor;
}
