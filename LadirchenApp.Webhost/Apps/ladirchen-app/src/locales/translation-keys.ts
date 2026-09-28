import type de from "./de.json";

export type TranslationKey = Extract<keyof typeof de, string>;
export type LocaleMessageSchema = { readonly [Key in TranslationKey]: string };

type NamespaceKeys<Prefix extends string> = TranslationKey extends infer Key
  ? Key extends `${Prefix}.${infer Id}.${string}`
    ? `${Prefix}.${Id}`
    : never
  : never;

export type ContributionTranslationNamespaceKey = NamespaceKeys<"seed.contributions">;
export type PromotionTranslationNamespaceKey = NamespaceKeys<"seed.promotions">;
export type SavingGoalTranslationNamespaceKey = NamespaceKeys<"seed.goals">;
export type ShopRewardTranslationNamespaceKey = NamespaceKeys<"seed.rewards">;
export type HouseAccessoryTranslationNamespaceKey = NamespaceKeys<"catalog.accessories">;
export type TranslationNamespaceKey =
  | ContributionTranslationNamespaceKey
  | HouseAccessoryTranslationNamespaceKey
  | PromotionTranslationNamespaceKey
  | SavingGoalTranslationNamespaceKey
  | ShopRewardTranslationNamespaceKey;
export type TranslationField<Namespace extends TranslationNamespaceKey> =
  Namespace extends ContributionTranslationNamespaceKey
    ? "area" | "description" | "dueLabel" | "title"
    : Namespace extends HouseAccessoryTranslationNamespaceKey
      ? "description" | "title"
      : Namespace extends ShopRewardTranslationNamespaceKey
        ? "conditions" | "description" | "title"
        : "title";
export function appendTranslationKey<
  Namespace extends TranslationNamespaceKey,
  Field extends TranslationField<Namespace>,
>(namespace: Namespace, field: Field): TranslationKey {
  return `${namespace}.${field}` as TranslationKey;
}
