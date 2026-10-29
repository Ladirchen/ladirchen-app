import type { FamilyReadModelRepositories } from "@/application/ports/family-read-model-repositories";
import { frontendRuntimeConfig } from "@/config/runtime";
import { localFamilyContext } from "@/infrastructure/context/local-family-context";
import { createHttpFamilyReadModelRepositories } from "@/infrastructure/http/http-family-read-model-repositories";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import { browserClientStorage } from "@/infrastructure/storage/browser-client-storage";
import { browserScheduler } from "@/infrastructure/scheduling/browser-scheduler";
import { createEmptyFamilyWorldInitialData } from "@/infrastructure/empty-family-world-initial-data";
import { createBundledLocaleMessagesSource } from "@/infrastructure/i18n/bundled-locale-messages-source";
import { SignalRFamilyAggregateUpdatesGateway } from "@/infrastructure/realtime/signalr-family-aggregate-updates";
import { HttpFamilyCommandGateway } from "@/infrastructure/http/http-family-command-gateway";
import { HttpAuthenticationGateway } from "@/infrastructure/http/http-authentication-gateway";
import { HttpSavingsInterestSimulationGateway } from "@/infrastructure/http/http-savings-interest-simulation-gateway";
const repositories: FamilyReadModelRepositories = createHttpFamilyReadModelRepositories(
  frontendRuntimeConfig.apiBaseUrl,
);
const familyAggregateUpdates = new SignalRFamilyAggregateUpdatesGateway(frontendRuntimeConfig.apiBaseUrl);
const familyCommands = new HttpFamilyCommandGateway(frontendRuntimeConfig.apiBaseUrl);
const initialDataFactory = { create: createEmptyFamilyWorldInitialData };
export const familyContext = localFamilyContext;
export const authenticationGateway = new HttpAuthenticationGateway(frontendRuntimeConfig.apiBaseUrl);
export const savingsInterestSimulationGateway = new HttpSavingsInterestSimulationGateway(
  frontendRuntimeConfig.apiBaseUrl,
);
export const localeMessagesSource = createBundledLocaleMessagesSource();
export const homeCustomizationService = repositories.homeCustomization;
export const familyProfileService = repositories.familyProfile;
export const contributionsService = repositories.contributions;
export const savingsService = repositories.savings;
export const rewardShopService = repositories.rewardShop;
export const familyProgressionService = repositories.familyProgression;
export const familyWorldDependencies: FamilyWorldDependencies = {
  authentication: authenticationGateway,
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
