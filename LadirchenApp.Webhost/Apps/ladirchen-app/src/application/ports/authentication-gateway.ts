import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface AuthenticationSession {
  readonly familyId: FamilyId;
  readonly familyName: string;
  readonly memberId: FamilyMemberId;
}

export interface RegisterAccountCommand extends AuthenticationSession {
  readonly password: string;
  readonly username: string;
}

export interface AuthenticationGateway {
  authenticate: (username: string, password: string) => Promise<AuthenticationSession | null>;
  register: (command: RegisterAccountCommand) => Promise<void>;
  usernameExists: (username: string) => Promise<boolean>;
}
