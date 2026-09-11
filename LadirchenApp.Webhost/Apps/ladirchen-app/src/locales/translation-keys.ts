import type de from './de';

type StringKey<T> = Extract<keyof T, string>;
type StringShape<T> = { readonly [Key in keyof T]: T[Key] extends string ? string : StringShape<T[Key]> };
type LeafPaths<T> = {
  [Key in StringKey<T>]: T[Key] extends string
    ? Key
    : T[Key] extends Readonly<Record<string, unknown>>
      ? `${Key}.${LeafPaths<T[Key]>}`
      : never;
}[StringKey<T>];

export type LocaleMessageSchema = StringShape<typeof de>;
export type TranslationKey = LeafPaths<typeof de>;

export type ContributionTranslationNamespaceKey = `seed.contributions.${StringKey<typeof de.seed.contributions>}`;
export type PromotionTranslationNamespaceKey = `seed.promotions.${StringKey<typeof de.seed.promotions>}`;
export type SavingGoalTranslationNamespaceKey = `seed.goals.${StringKey<typeof de.seed.goals>}`;
export type ShopRewardTranslationNamespaceKey = `seed.rewards.${StringKey<typeof de.seed.rewards>}`;
export type HouseAccessoryTranslationNamespaceKey = `catalog.accessories.${StringKey<typeof de.catalog.accessories>}`;
export type TranslationNamespaceKey =
  | ContributionTranslationNamespaceKey
  | HouseAccessoryTranslationNamespaceKey
  | PromotionTranslationNamespaceKey
  | SavingGoalTranslationNamespaceKey
  | ShopRewardTranslationNamespaceKey;
export type TranslationField<Namespace extends TranslationNamespaceKey> =
  Namespace extends ContributionTranslationNamespaceKey ? 'area' | 'description' | 'dueLabel' | 'title'
    : Namespace extends HouseAccessoryTranslationNamespaceKey ? 'description' | 'title'
      : Namespace extends ShopRewardTranslationNamespaceKey ? 'conditions' | 'description' | 'title'
        : 'title';
export const appendTranslationKey = <
  Namespace extends TranslationNamespaceKey,
  Field extends TranslationField<Namespace>,
>(namespace: Namespace, field: Field): TranslationKey =>
  `${namespace}.${field}` as TranslationKey;
