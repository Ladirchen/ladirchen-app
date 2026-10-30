import { createDomainId } from "@/domain/shared/identifiers";
import type {
  ActivateMemberAccountCommand,
  AuthenticationGateway,
  AuthenticationSession,
  InviteFamilyMemberCommand,
  MemberAccountInvitation,
  RegisterAccountCommand,
  RegistrationStatus,
} from "@/application/ports/authentication-gateway";
import {
  isFamilyId,
  isFamilyMemberId,
  isIsoDateTime,
  isNonEmptyString,
  isRecord,
} from "@/application/contracts/validation/runtime-validation";

interface AuthenticationSessionDto {
  readonly familyId: string;
  readonly familyName: string;
  readonly memberId: string;
}

interface MemberAccountInvitationDto {
  readonly activationCode: string;
  readonly email: string;
  readonly expiresAt: string;
  readonly memberId: string;
}

function isAuthenticationSessionDto(value: unknown): value is AuthenticationSessionDto {
  return (
    isRecord(value) &&
    isFamilyId(value.familyId) &&
    isNonEmptyString(value.familyName) &&
    isFamilyMemberId(value.memberId)
  );
}

function sessionFromDto(dto: AuthenticationSessionDto): AuthenticationSession {
  return {
    familyId: createDomainId.family(dto.familyId),
    familyName: dto.familyName,
    memberId: createDomainId.familyMember(dto.memberId),
  };
}

function isMemberAccountInvitationDto(value: unknown): value is MemberAccountInvitationDto {
  return (
    isRecord(value) &&
    isNonEmptyString(value.activationCode) &&
    isNonEmptyString(value.email) &&
    isIsoDateTime(value.expiresAt) &&
    isFamilyMemberId(value.memberId)
  );
}

function isRegistrationStatus(value: unknown): value is RegistrationStatus {
  return isRecord(value) && typeof value.registrationAvailable === "boolean";
}

export class HttpAuthenticationGateway implements AuthenticationGateway {
  readonly #baseUrl: string;

  constructor(baseUrl: string) {
    this.#baseUrl = baseUrl.replace(/\/+$/u, "");
  }

  async activate(command: ActivateMemberAccountCommand): Promise<AuthenticationSession> {
    const response = await fetch(`${this.#baseUrl}/api/auth/activate`, {
      body: JSON.stringify(command),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Account activation failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isAuthenticationSessionDto(value)) {
      throw new TypeError("The account activation response is invalid.");
    }

    return sessionFromDto(value);
  }

  async authenticate(identifier: string, password: string): Promise<AuthenticationSession | null> {
    const response = await fetch(`${this.#baseUrl}/api/auth/login`, {
      body: JSON.stringify({ identifier, password }),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (response.status === 401) {
      return null;
    }

    if (!response.ok) {
      throw new Error(`Authentication failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isAuthenticationSessionDto(value)) {
      throw new TypeError("The authentication response is invalid.");
    }

    return sessionFromDto(value);
  }

  async getCurrentSession(): Promise<AuthenticationSession | null> {
    const response = await fetch(`${this.#baseUrl}/api/auth/restore`, {
      cache: "no-store",
      credentials: "include",
      method: "POST",
    });

    if (response.status === 401) {
      return null;
    }

    if (!response.ok) {
      throw new Error(`Current authentication session failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isAuthenticationSessionDto(value)) {
      throw new TypeError("The current authentication session response is invalid.");
    }

    return sessionFromDto(value);
  }

  async getRegistrationStatus(): Promise<RegistrationStatus> {
    const response = await fetch(`${this.#baseUrl}/api/auth/registration-status`, {
      cache: "no-store",
      credentials: "include",
      method: "GET",
    });

    if (!response.ok) {
      throw new Error(`Registration status failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isRegistrationStatus(value)) {
      throw new TypeError("The registration status response is invalid.");
    }

    return value;
  }

  async inviteMember(command: InviteFamilyMemberCommand): Promise<MemberAccountInvitation> {
    const response = await fetch(`${this.#baseUrl}/api/auth/member-invitations`, {
      body: JSON.stringify(command),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Member invitation failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isMemberAccountInvitationDto(value)) {
      throw new TypeError("The member invitation response is invalid.");
    }

    return {
      activationCode: value.activationCode,
      email: value.email,
      expiresAt: value.expiresAt,
      memberId: createDomainId.familyMember(value.memberId),
    };
  }

  async register(command: RegisterAccountCommand): Promise<AuthenticationSession> {
    const response = await fetch(`${this.#baseUrl}/api/auth/register`, {
      body: JSON.stringify(command),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Registration failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isAuthenticationSessionDto(value)) {
      throw new TypeError("The registration response is invalid.");
    }

    return sessionFromDto(value);
  }

  async signOut(): Promise<void> {
    const response = await fetch(`${this.#baseUrl}/api/auth/logout`, {
      credentials: "include",
      method: "POST",
    });

    if (!response.ok && response.status !== 401) {
      throw new Error(`Sign-out failed with status ${response.status}.`);
    }
  }
}
