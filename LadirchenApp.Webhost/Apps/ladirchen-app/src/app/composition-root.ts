import { FAMILY_AGGREGATE_DESCRIPTORS } from "@/application/contracts/family-aggregate-descriptors";
import type { FamilyAggregateRepositories } from "@/application/ports/family-aggregate-repositories";
import { VersionedAggregateService } from "@/application/services/versioned-aggregate-service";
import { frontendRuntimeConfig } from "@/config/runtime";
import { localFamilyContext } from "@/infrastructure/context/local-family-context";
import { createHttpFamilyAggregateRepositories } from "@/infrastructure/http/http-family-aggregate-repositories";
import { createLocalStorageFamilyAggregateRepositories } from "@/infrastructure/persistence/local-storage-family-aggregate-repositories";
import { createLocalAuthenticationGateway } from "@/features/auth/local-auth";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import { browserClientStorage } from "@/infrastructure/storage/browser-client-storage";
import { browserScheduler } from "@/infrastructure/scheduling/browser-scheduler";
import { createFamilyWorldInitialData } from "@/infrastructure/fixtures/family-world-fixtures";

const createRepositories = (): FamilyAggregateRepositories =>
  frontendRuntimeConfig.dataSource === "api"
    ? createHttpFamilyAggregateRepositories(frontendRuntimeConfig.apiBaseUrl)
    : createLocalStorageFamilyAggregateRepositories();

const repositories = createRepositories();
const initialDataFactory = { create: createFamilyWorldInitialData };

export const familyContext = localFamilyContext;
export const authenticationGateway = createLocalAuthenticationGateway(browserClientStorage);

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
  familyContext,
  familyProfileService,
  familyProgressionService,
  homeCustomizationService,
  initialDataFactory,
  rewardShopService,
  scheduler: browserScheduler,
  savingsService,
};
