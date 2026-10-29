import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface AuthenticationSession {
  readonly familyId: FamilyId;
  readonly familyName: string;
  readonly memberId: FamilyMemberId;
}

export interface RegisterAccountCommand extends AuthenticationSession {
  readonly guardianName: string;
  readonly password: string;
  readonly username: string;
}

export interface AuthenticationGateway {
  authenticate: (username: string, password: string) => Promise<AuthenticationSession | null>;
  signOut: () => Promise<void>;
  register: (command: RegisterAccountCommand) => Promise<void>;
}
