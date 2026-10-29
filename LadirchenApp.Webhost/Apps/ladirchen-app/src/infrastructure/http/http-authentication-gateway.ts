import { createDomainId } from "@/domain/shared/identifiers";
import type {
  AuthenticationGateway,
  AuthenticationSession,
  RegisterAccountCommand,
} from "@/application/ports/authentication-gateway";

interface AuthenticationSessionDto {
  readonly familyId: string;
  readonly familyName: string;
  readonly memberId: string;
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
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (response.status === 401) {
      return null;
    }

    if (!response.ok) {
      throw new Error(`Authentication failed with status ${response.status}.`);
    }

    return sessionFromDto((await response.json()) as AuthenticationSessionDto);
  }

  async register(command: RegisterAccountCommand): Promise<void> {
    const response = await fetch(`${this.#baseUrl}/api/auth/register`, {
      body: JSON.stringify(command),
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Registration failed with status ${response.status}.`);
    }
  }

  async usernameExists(username: string): Promise<boolean> {
    const search = new URLSearchParams({ username });
    const response = await fetch(`${this.#baseUrl}/api/auth/username-exists?${search.toString()}`);

    if (!response.ok) {
      throw new Error(`Username lookup failed with status ${response.status}.`);
    }

    return (await response.json()) as boolean;
  }
}
