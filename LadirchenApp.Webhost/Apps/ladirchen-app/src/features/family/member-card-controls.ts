import type { FamilyMember } from "@/domain/family/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export interface FamilyMemberCardControls {
  readonly canEditAccess: boolean;
  readonly showParticipation: boolean;
}

export function familyMemberCardControls(
  member: FamilyMember,
  signedInMemberId: FamilyMemberId,
  canManageFamily: boolean,
): FamilyMemberCardControls {
  const isGuardian = member.role === "guardian";
  const isPending = member.invitationPending === true;
  const isSignedIn = member.id === signedInMemberId;

  return {
    canEditAccess: !isPending && canManageFamily && isGuardian && !isSignedIn,
    showParticipation: !isPending && isGuardian && (canManageFamily || isSignedIn),
  };
}
