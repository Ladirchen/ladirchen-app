import type { ContributionsSnapshot } from "@/application/contracts/contributions-contract";
import type { FamilyProfileSnapshot } from "@/application/contracts/family-profile-contract";
import type { FamilyProgressionSnapshot } from "@/application/contracts/family-progression-contract";
import type { HomeCustomizationSnapshot } from "@/application/contracts/home-customization-contract";
import type { RewardShopSnapshot } from "@/application/contracts/reward-shop-contract";
import type { SavingsSnapshot } from "@/application/contracts/savings-contract";
import type { FamilyId } from "@/domain/shared/identifiers";
import type { ClientStorage } from "./client-storage";
import type { FamilyContext } from "./family-context";
import type { Scheduler } from "./scheduler";
import type { FamilyWorldInitialDataFactory } from "./family-world-initial-data";
import type { FamilyAggregateUpdatesGateway } from "./family-aggregate-updates";
import type { FamilyCommandGateway } from "./family-command-gateway";
import type { AuthenticationGateway } from "./authentication-gateway";

interface AggregateServicePort<TSnapshot> {
  load: (familyId: FamilyId) => Promise<TSnapshot | null>;
}

export interface FamilyWorldDependencies {
  readonly authentication: AuthenticationGateway;
  readonly clientStorage: ClientStorage;
  readonly contributionsService: AggregateServicePort<ContributionsSnapshot>;
  readonly familyAggregateUpdates: FamilyAggregateUpdatesGateway;
  readonly familyCommands: FamilyCommandGateway;
  readonly familyContext: FamilyContext;
  readonly initialDataFactory: FamilyWorldInitialDataFactory;
  readonly familyProfileService: AggregateServicePort<FamilyProfileSnapshot>;
  readonly familyProgressionService: AggregateServicePort<FamilyProgressionSnapshot>;
  readonly homeCustomizationService: AggregateServicePort<HomeCustomizationSnapshot>;
  readonly rewardShopService: AggregateServicePort<RewardShopSnapshot>;
  readonly scheduler: Scheduler;
  readonly savingsService: AggregateServicePort<SavingsSnapshot>;
}
