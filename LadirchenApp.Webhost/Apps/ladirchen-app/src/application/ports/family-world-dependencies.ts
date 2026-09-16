import type { ContributionsState, ContributionsSnapshot } from '@/application/contracts/contributions-contract';
import type { FamilyProfileState, FamilyProfileSnapshot } from '@/application/contracts/family-profile-contract';
import type { FamilyProgressionState, FamilyProgressionSnapshot } from '@/application/contracts/family-progression-contract';
import type { HomeCustomizationState, HomeCustomizationSnapshot } from '@/application/contracts/home-customization-contract';
import type { RewardShopState, RewardShopSnapshot } from '@/application/contracts/reward-shop-contract';
import type { SavingsState, SavingsSnapshot } from '@/application/contracts/savings-contract';
import type { AggregateDraft } from '@/application/contracts/versioned-aggregate-contract';
import type { FamilyId } from '@/domain/shared/identifiers';
import type { ClientStorage } from './client-storage';
import type { FamilyContext } from './family-context';
import type { Scheduler } from './scheduler';
import type { FamilyWorldInitialDataFactory } from './family-world-initial-data';

interface AggregateServicePort<TState, TSnapshot> {
  load: (familyId: FamilyId) => Promise<TSnapshot | null>;
  scheduleSave: (draft: AggregateDraft<TState>, onError: (error: unknown) => void) => void;
}

export interface FamilyWorldDependencies {
  readonly clientStorage: ClientStorage;
  readonly contributionsService: AggregateServicePort<ContributionsState, ContributionsSnapshot>;
  readonly familyContext: FamilyContext;
  readonly initialDataFactory: FamilyWorldInitialDataFactory;
  readonly familyProfileService: AggregateServicePort<FamilyProfileState, FamilyProfileSnapshot>;
  readonly familyProgressionService: AggregateServicePort<FamilyProgressionState, FamilyProgressionSnapshot>;
  readonly homeCustomizationService: AggregateServicePort<HomeCustomizationState, HomeCustomizationSnapshot>;
  readonly rewardShopService: AggregateServicePort<RewardShopState, RewardShopSnapshot>;
  readonly scheduler: Scheduler;
  readonly savingsService: AggregateServicePort<SavingsState, SavingsSnapshot>;
}
