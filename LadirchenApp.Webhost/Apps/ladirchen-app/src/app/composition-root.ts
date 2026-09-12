import { ContributionsService, FamilyProfileService, FamilyProgressionService, HomeCustomizationService, RewardShopService, SavingsService } from '@/application/services/family-aggregate-services';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';
import { frontendRuntimeConfig } from '@/config/runtime';
import { localFamilyContext } from '@/infrastructure/context/local-family-context';
import { createHttpFamilyAggregateRepositories } from '@/infrastructure/http/http-family-aggregate-repositories';
import { createLocalStorageFamilyAggregateRepositories } from '@/infrastructure/persistence/local-storage-family-aggregate-repositories';
import { createLocalAuthenticationGateway } from '@/features/auth/local-auth';
import type { FamilyWorldDependencies } from '@/application/ports/family-world-dependencies';
import { browserClientStorage } from '@/infrastructure/storage/browser-client-storage';
import { browserScheduler } from '@/infrastructure/scheduling/browser-scheduler';
import { createFamilyWorldInitialData } from '@/infrastructure/fixtures/family-world-fixtures';
import { configureFamilyWorldInitialData } from '@/stores/family-world-state';

const createRepositories = (): FamilyAggregateRepositories =>
  frontendRuntimeConfig.dataSource === 'api'
    ? createHttpFamilyAggregateRepositories(frontendRuntimeConfig.apiBaseUrl)
    : createLocalStorageFamilyAggregateRepositories();

const repositories = createRepositories();
const initialDataFactory = { create: createFamilyWorldInitialData };

configureFamilyWorldInitialData(initialDataFactory);

export const familyContext = localFamilyContext;
export const authenticationGateway = createLocalAuthenticationGateway(browserClientStorage);

export const homeCustomizationService = new HomeCustomizationService(repositories.homeCustomization);
export const familyProfileService = new FamilyProfileService(repositories.familyProfile);
export const contributionsService = new ContributionsService(repositories.contributions);
export const savingsService = new SavingsService(repositories.savings);
export const rewardShopService = new RewardShopService(repositories.rewardShop);
export const familyProgressionService = new FamilyProgressionService(repositories.familyProgression);

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
