import { FAMILY_AGGREGATE_DESCRIPTORS } from "@/application/contracts/family-aggregate-descriptors";
import type { FamilyAggregateRepositories } from "@/application/ports/family-aggregate-repositories";
import { VersionedAggregateService } from "@/application/services/versioned-aggregate-service";
import { frontendRuntimeConfig } from "@/config/runtime";
import { localFamilyContext } from "@/infrastructure/context/local-family-context";
import { createHttpFamilyAggregateRepositories } from "@/infrastructure/http/http-family-aggregate-repositories";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import { browserClientStorage } from "@/infrastructure/storage/browser-client-storage";
import { browserScheduler } from "@/infrastructure/scheduling/browser-scheduler";
import { createFamilyWorldInitialData } from "@/infrastructure/fixtures/family-world-fixtures";
import { createBundledLocaleMessagesSource } from "@/infrastructure/i18n/bundled-locale-messages-source";
import { SignalRFamilyAggregateUpdatesGateway } from "@/infrastructure/realtime/signalr-family-aggregate-updates";
import { HttpFamilyCommandGateway } from "@/infrastructure/http/http-family-command-gateway";
import { HttpAuthenticationGateway } from "@/infrastructure/http/http-authentication-gateway";
import { HttpSavingsInterestSimulationGateway } from "@/infrastructure/http/http-savings-interest-simulation-gateway";
const repositories: FamilyAggregateRepositories = createHttpFamilyAggregateRepositories(
  frontendRuntimeConfig.apiBaseUrl,
);
const familyAggregateUpdates = new SignalRFamilyAggregateUpdatesGateway(frontendRuntimeConfig.apiBaseUrl);
const familyCommands = new HttpFamilyCommandGateway(frontendRuntimeConfig.apiBaseUrl);
const initialDataFactory = { create: createFamilyWorldInitialData };
export const familyContext = localFamilyContext;
export const authenticationGateway = new HttpAuthenticationGateway(frontendRuntimeConfig.apiBaseUrl);
export const savingsInterestSimulationGateway = new HttpSavingsInterestSimulationGateway(
  frontendRuntimeConfig.apiBaseUrl,
);
export const localeMessagesSource = createBundledLocaleMessagesSource();
export const homeCustomizationService = new VersionedAggregateService(
  FAMILY_AGGREGATE_DESCRIPTORS.homeCustomization.aggregateType,
  repositories.homeCustomization,
);
export const familyProfileService = new VersionedAggregateService(
  FAMILY_AGGREGATE_DESCRIPTORS.familyProfile.aggregateType,
  repositories.familyProfile,
);
export const contributionsService = new VersionedAggregateService(
  FAMILY_AGGREGATE_DESCRIPTORS.contributions.aggregateType,
  repositories.contributions,
);
export const savingsService = new VersionedAggregateService(
  FAMILY_AGGREGATE_DESCRIPTORS.savings.aggregateType,
  repositories.savings,
);
export const rewardShopService = new VersionedAggregateService(
  FAMILY_AGGREGATE_DESCRIPTORS.rewardShop.aggregateType,
  repositories.rewardShop,
);
export const familyProgressionService = new VersionedAggregateService(
  FAMILY_AGGREGATE_DESCRIPTORS.familyProgression.aggregateType,
  repositories.familyProgression,
);
export const familyWorldDependencies: FamilyWorldDependencies = {
  clientStorage: browserClientStorage,
  contributionsService,
  familyAggregateUpdates,
  familyCommands,
  familyContext,
  familyProfileService,
  familyProgressionService,
  homeCustomizationService,
  initialDataFactory,
  rewardShopService,
  scheduler: browserScheduler,
  savingsService,
};
