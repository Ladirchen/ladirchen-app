export const FAMILY_READ_MODEL_TYPES = [
  "contributions",
  "family-profile",
  "family-progression",
  "home-customization",
  "reward-shop",
  "savings",
] as const;

export type FamilyReadModelType = (typeof FAMILY_READ_MODEL_TYPES)[number];

const familyReadModelTypeSet = new Set<string>(FAMILY_READ_MODEL_TYPES);

export function isFamilyReadModelType(value: unknown): value is FamilyReadModelType {
  return typeof value === "string" && familyReadModelTypeSet.has(value);
}

export interface FamilyReadModel<TReadModelType extends FamilyReadModelType, TState> {
  readonly readModelType: TReadModelType;
  readonly revision: number;
  readonly state: TState;
}
