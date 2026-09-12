import type { ContributionsService, FamilyProfileService, FamilyProgressionService, HomeCustomizationService, RewardShopService, SavingsService } from '@/application/services/family-aggregate-services';
import type { ClientStorage } from './client-storage';
import type { FamilyContext } from './family-context';
import type { Scheduler } from './scheduler';
import type { FamilyWorldInitialDataFactory } from './family-world-initial-data';

export interface FamilyWorldDependencies {
  readonly clientStorage: ClientStorage;
  readonly contributionsService: ContributionsService;
  readonly familyContext: FamilyContext;
  readonly initialDataFactory: FamilyWorldInitialDataFactory;
  readonly familyProfileService: FamilyProfileService;
  readonly familyProgressionService: FamilyProgressionService;
  readonly homeCustomizationService: HomeCustomizationService;
  readonly rewardShopService: RewardShopService;
  readonly scheduler: Scheduler;
  readonly savingsService: SavingsService;
}
