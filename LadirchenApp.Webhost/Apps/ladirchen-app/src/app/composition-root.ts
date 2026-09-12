import { ContributionsService, FamilyProfileService, FamilyProgressionService, HomeCustomizationService, RewardShopService, SavingsService } from '@/application/services/family-aggregate-services';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';
import { frontendRuntimeConfig } from '@/config/runtime';
import { localFamilyContext } from '@/infrastructure/context/local-family-context';
import { createHttpFamilyAggregateRepositories } from '@/infrastructure/http/http-family-aggregate-repositories';
import { createLocalStorageFamilyAggregateRepositories } from '@/infrastructure/persistence/local-storage-family-aggregate-repositories';

const createRepositories = (): FamilyAggregateRepositories =>
  frontendRuntimeConfig.dataSource === 'api'
    ? createHttpFamilyAggregateRepositories(frontendRuntimeConfig.apiBaseUrl)
    : createLocalStorageFamilyAggregateRepositories();

const repositories = createRepositories();

export const familyContext = localFamilyContext;

export const homeCustomizationService = new HomeCustomizationService(repositories.homeCustomization);
export const familyProfileService = new FamilyProfileService(repositories.familyProfile);
export const contributionsService = new ContributionsService(repositories.contributions);
export const savingsService = new SavingsService(repositories.savings);
export const rewardShopService = new RewardShopService(repositories.rewardShop);
export const familyProgressionService = new FamilyProgressionService(repositories.familyProgression);
