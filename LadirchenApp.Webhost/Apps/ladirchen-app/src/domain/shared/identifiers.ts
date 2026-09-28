declare const domainIdBrand: unique symbol;

type DomainId<Kind extends string> = string & { readonly [domainIdBrand]: Kind };

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

export const isDomainIdValue = (value: unknown): value is string =>
  typeof value === "string" && domainIdPattern.test(value);
export const isUuidValue = (value: unknown): value is string =>
  typeof value === "string" && uuidPattern.test(value);

const domainId = <Kind extends string>(value: string): DomainId<Kind> => {
  if (!isDomainIdValue(value)) {throw new TypeError(`Invalid ${value ? "format" : "empty value"} for a domain ID.`);}
  return value as DomainId<Kind>;
};

export const createDomainId = {
  contribution: (value: string): ContributionId => domainId<"contribution">(value),
  family: (value: string): FamilyId => {
    if (!isUuidValue(value)) {throw new TypeError("A family ID must be a UUID.");}
    return domainId<"family">(value);
  },
  familyMember: (value: string): FamilyMemberId => domainId<"family-member">(value),
  familyPet: (value: string): FamilyPetId => domainId<"family-pet">(value),
  guardianGift: (value: string): GuardianGiftId => domainId<"guardian-gift">(value),
  houseLayoutPlacement: (value: string): HouseLayoutPlacementId => domainId<"house-layout-placement">(value),
  promotion: (value: string): PromotionId => domainId<"promotion">(value),
  savingGoal: (value: string): SavingGoalId => domainId<"saving-goal">(value),
  shopReward: (value: string): ShopRewardId => domainId<"shop-reward">(value),
} as const;
