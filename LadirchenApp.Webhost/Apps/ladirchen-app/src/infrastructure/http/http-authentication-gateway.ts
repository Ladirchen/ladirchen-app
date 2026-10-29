import { createDomainId } from "@/domain/shared/identifiers";
import type {
  AuthenticationGateway,
  AuthenticationSession,
  RegisterAccountCommand,
} from "@/application/ports/authentication-gateway";
import { isRecord } from "@/application/contracts/validation/runtime-validation";

interface AuthenticationSessionDto {
  readonly familyId: string;
  readonly familyName: string;
  readonly memberId: string;
}

function isAuthenticationSessionDto(value: unknown): value is AuthenticationSessionDto {
  return (
    isRecord(value) &&
    typeof value.familyId === "string" &&
    typeof value.familyName === "string" &&
    typeof value.memberId === "string"
  );
}

function sessionFromDto(dto: AuthenticationSessionDto): AuthenticationSession {
  return {
    familyId: createDomainId.family(dto.familyId),
    familyName: dto.familyName,
    memberId: createDomainId.familyMember(dto.memberId),
  };
}

export class HttpAuthenticationGateway implements AuthenticationGateway {
  readonly #baseUrl: string;

  constructor(baseUrl: string) {
    this.#baseUrl = baseUrl.replace(/\/+$/u, "");
  }

  async authenticate(username: string, password: string): Promise<AuthenticationSession | null> {
    const response = await fetch(`${this.#baseUrl}/api/auth/login`, {
      body: JSON.stringify({ password, username }),
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

  async register(command: RegisterAccountCommand): Promise<void> {
    const response = await fetch(`${this.#baseUrl}/api/auth/register`, {
      body: JSON.stringify(command),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Registration failed with status ${response.status}.`);
    }
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
