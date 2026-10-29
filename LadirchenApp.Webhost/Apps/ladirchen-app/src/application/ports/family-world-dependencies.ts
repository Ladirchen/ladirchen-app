import type { ContributionsReadModel } from "@/application/contracts/contributions-contract";
import type { FamilyProfileReadModel } from "@/application/contracts/family-profile-contract";
import type { FamilyProgressionReadModel } from "@/application/contracts/family-progression-contract";
import type { HomeCustomizationReadModel } from "@/application/contracts/home-customization-contract";
import type { RewardShopReadModel } from "@/application/contracts/reward-shop-contract";
import type { SavingsReadModel } from "@/application/contracts/savings-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import type { ClientStorage } from "./client-storage";
import type { FamilyContext } from "./family-context";
import type { Scheduler } from "./scheduler";
import type { FamilyWorldInitialDataFactory } from "./family-world-initial-data";
import type { FamilyAggregateUpdatesGateway } from "./family-aggregate-updates";
import type { FamilyCommandGateway } from "./family-command-gateway";
import type { AuthenticationGateway } from "./authentication-gateway";

interface ReadModelServicePort<TReadModel> {
  load: (familyId: FamilyId, viewerMemberId?: FamilyMemberId) => Promise<TReadModel | null>;
}

export interface FamilyWorldDependencies {
  readonly authentication: AuthenticationGateway;
  readonly clientStorage: ClientStorage;
  readonly contributionsService: ReadModelServicePort<ContributionsReadModel>;
  readonly familyAggregateUpdates: FamilyAggregateUpdatesGateway;
  readonly familyCommands: FamilyCommandGateway;
  readonly familyContext: FamilyContext;
  readonly initialDataFactory: FamilyWorldInitialDataFactory;
  readonly familyProfileService: ReadModelServicePort<FamilyProfileReadModel>;
  readonly familyProgressionService: ReadModelServicePort<FamilyProgressionReadModel>;
  readonly homeCustomizationService: ReadModelServicePort<HomeCustomizationReadModel>;
  readonly rewardShopService: ReadModelServicePort<RewardShopReadModel>;
  readonly scheduler: Scheduler;
  readonly savingsService: ReadModelServicePort<SavingsReadModel>;
}
