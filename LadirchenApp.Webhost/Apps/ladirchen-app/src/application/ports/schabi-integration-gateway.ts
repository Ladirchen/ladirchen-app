import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface SchabiIntegration {
  readonly memberId: FamilyMemberId;
  readonly username: string;
  readonly configured: boolean;
  readonly enabled: boolean;
  readonly syncDayOfWeek: number;
  readonly syncTimeLocal: string;
  readonly lastSuccessfulSyncAt?: string;
  readonly lastError?: string;
}

export interface SaveSchabiIntegrationCommand {
  readonly calendarUrl: string;
  readonly enabled: boolean;
  readonly syncDayOfWeek: number;
  readonly syncTimeLocal: string;
  readonly username: string;
}

export interface SchabiIntegrationGateway {
  list: (familyId: FamilyId) => Promise<SchabiIntegration[]>;
  remove: (familyId: FamilyId, memberId: FamilyMemberId) => Promise<void>;
  save: (
    familyId: FamilyId,
    memberId: FamilyMemberId,
    command: SaveSchabiIntegrationCommand,
  ) => Promise<SchabiIntegration>;
  synchronize: (familyId: FamilyId, memberId: FamilyMemberId) => Promise<SchabiIntegration>;
}
