import type { Contribution, Promotion } from "@/domain/contributions/types";
import type { FamilyMember, FamilyPet } from "@/domain/family/types";
import type { HouseAccessory, HouseLayoutPlacement } from "@/domain/house/entities";
import type { SavingGoal } from "@/domain/savings/types";
import type { FamilyMemberId, SavingGoalId } from "@/domain/shared/identifiers";
import type { ShopReward } from "@/domain/shop/types";

export interface FamilyWorldInitialData {
  readonly activeChildId: FamilyMemberId;
  readonly activeGoalId: SavingGoalId;
  readonly accessories: HouseAccessory[];
  readonly balances: Record<FamilyMemberId, number>;
  readonly contributions: Contribution[];
  readonly goals: SavingGoal[];
  readonly houseLayout: HouseLayoutPlacement[];
  readonly members: FamilyMember[];
  readonly pets: FamilyPet[];
  readonly promotions: Promotion[];
  readonly shopRewards: ShopReward[];
  readonly signedInMemberId: FamilyMemberId;
}

export interface FamilyWorldInitialDataFactory {
  create: () => FamilyWorldInitialData;
}
