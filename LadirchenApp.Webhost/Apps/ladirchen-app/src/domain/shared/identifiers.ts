declare const domainIdBrand: unique symbol;
type DomainId<Kind extends string> = string & {
  readonly [domainIdBrand]: Kind;
};
export type FamilyMemberId = DomainId<"family-member">;
export type FamilyId = DomainId<"family">;
export type FamilyPetId = DomainId<"family-pet">;
export type ContributionId = DomainId<"contribution">;
export type SavingGoalId = DomainId<"saving-goal">;
export type PromotionId = DomainId<"promotion">;
export type ShopRewardId = DomainId<"shop-reward">;
export type GuardianGiftId = DomainId<"guardian-gift">;
export type HouseLayoutPlacementId = DomainId<"house-layout-placement">;
const domainIdPattern = /^\w[\w.:-]{0,127}$/u;
const uuidPattern = /^[\da-f]{8}-(?:[\da-f]{4}-){3}[\da-f]{12}$/iu;
export function isDomainIdValue(value: unknown): value is string {

  return typeof value === "string" && domainIdPattern.test(value);
}
export function isUuidValue(value: unknown): value is string {

  return typeof value === "string" && uuidPattern.test(value);
}
function domainId<Kind extends string>(value: string): DomainId<Kind> {

  if (!isDomainIdValue(value)) {
    throw new TypeError(`Invalid ${value ? "format" : "empty value"} for a domain ID.`);
  }

  return value as DomainId<Kind>;
}
export const createDomainId = {
  contribution(this: void, value: string): ContributionId {

    return domainId<"contribution">(value);
  },
  family(this: void, value: string): FamilyId {

    if (!isUuidValue(value)) {
      throw new TypeError("A family ID must be a UUID.");
    }

    return domainId<"family">(value);
  },
  familyMember(this: void, value: string): FamilyMemberId {

    return domainId<"family-member">(value);
  },
  familyPet(this: void, value: string): FamilyPetId {

    return domainId<"family-pet">(value);
  },
  guardianGift(this: void, value: string): GuardianGiftId {

    return domainId<"guardian-gift">(value);
  },
  houseLayoutPlacement(this: void, value: string): HouseLayoutPlacementId {

    return domainId<"house-layout-placement">(value);
  },
  promotion(this: void, value: string): PromotionId {

    return domainId<"promotion">(value);
  },
  savingGoal(this: void, value: string): SavingGoalId {

    return domainId<"saving-goal">(value);
  },
  shopReward(this: void, value: string): ShopRewardId {

    return domainId<"shop-reward">(value);
  },
} as const;
