import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import type { GuardianAccessLevel, ViewerRole } from "@/domain/family/types";

export interface AuthenticationSession {
  readonly familyId: FamilyId;
  readonly familyName: string;
  readonly memberId: FamilyMemberId;
}

export interface RegisterAccountCommand extends AuthenticationSession {
  readonly email: string;
  readonly guardianName: string;
  readonly password: string;
  readonly username: string;
}

export interface ActivateMemberAccountCommand {
  readonly activationCode: string;
  readonly email: string;
  readonly password: string;
  readonly username: string;
}

export interface InviteFamilyMemberCommand {
  readonly email: string;
  readonly guardianAccess?: GuardianAccessLevel;
  readonly name: string;
  readonly role: ViewerRole;
}

export interface MemberAccountInvitation {
  readonly activationCode: string;
  readonly email: string;
  readonly expiresAt: string;
  readonly memberId: FamilyMemberId;
}

export interface RegistrationStatus {
  readonly registrationAvailable: boolean;
}

export interface AuthenticationGateway {
  activate: (command: ActivateMemberAccountCommand) => Promise<AuthenticationSession>;
  authenticate: (identifier: string, password: string) => Promise<AuthenticationSession | null>;
  getCurrentSession: () => Promise<AuthenticationSession | null>;
  getRegistrationStatus: () => Promise<RegistrationStatus>;
  inviteMember: (command: InviteFamilyMemberCommand) => Promise<MemberAccountInvitation>;
  signOut: () => Promise<void>;
  register: (command: RegisterAccountCommand) => Promise<AuthenticationSession>;
}
