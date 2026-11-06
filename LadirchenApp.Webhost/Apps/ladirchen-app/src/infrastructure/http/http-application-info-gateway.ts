import type { ApplicationInfo, ApplicationInfoGateway } from "@/application/ports/application-info-gateway";
import { isNonEmptyString, isRecord } from "@/application/contracts/validation/runtime-validation";

export class HttpApplicationInfoGateway implements ApplicationInfoGateway {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl.replace(/\/+$/u, "");
  }

  async getInfo(): Promise<ApplicationInfo> {
    const response = await fetch(`${this.baseUrl}/about`, {
      cache: "no-store",
      credentials: "include",
      method: "GET",
    });

    if (!response.ok) {
      throw new Error(`Application information failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (
      !isRecord(value) ||
      !isNonEmptyString(value.semVer) ||
      !isNonEmptyString(value.informationalVersion) ||
      !/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/u.test(value.semVer) ||
      value.informationalVersion.split("+")[0] !== value.semVer
    ) {
      throw new TypeError("The application information response is invalid.");
    }

    return { semVer: value.semVer, informationalVersion: value.informationalVersion };
  }
}
