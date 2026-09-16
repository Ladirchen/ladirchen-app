import { useI18n } from 'vue-i18n';

import type { Contribution, Promotion } from '@/domain/contributions/types';
import type { HouseAccessory } from '@/domain/house/entities';
import type { SavingGoal } from '@/domain/savings/types';
import type { ShopReward } from '@/domain/shop/types';
import { appendTranslationKey } from '@/locales/translation-keys';
import type { TranslationField, TranslationKey, TranslationNamespaceKey } from '@/locales/translation-keys';

type TranslatableEntity<Namespace extends TranslationNamespaceKey> = { readonly translationKey?: Namespace };

export const useLocalizedDomainContent = () => {
  const { t, te } = useI18n();

  const resolveField = <
    Namespace extends TranslationNamespaceKey,
    Field extends TranslationField<Namespace>,
  >(entity: TranslatableEntity<Namespace>, field: Field, fallback: string): string => {
    const key = entity.translationKey ? appendTranslationKey(entity.translationKey, field) : undefined;
    return key && te(key) ? t(key) : fallback;
  };

  const resolveKey = (key: TranslationKey | undefined, fallback: string, params?: Record<string, number | string>): string =>
    key && te(key) ? t(key, params ?? {}) : fallback;

  const contribution = (item: Contribution): Contribution => ({
    ...item,
    title: resolveField(item, 'title', item.title),
    description: resolveField(item, 'description', item.description),
    area: resolveKey(item.areaKey, resolveField(item, 'area', item.area)),
    dueLabel: resolveKey(item.dueLabelKey, resolveField(item, 'dueLabel', item.dueLabel)),
  });

  const goal = (item: SavingGoal): SavingGoal => ({
    ...item,
    title: resolveField(item, 'title', item.title),
  });

  const promotion = (item: Promotion): Promotion => ({
    ...item,
    title: resolveKey(item.titleKey, resolveField(item, 'title', item.title), { multiplier: item.multiplier }),
  });

  const reward = (item: ShopReward): ShopReward => ({
    ...item,
    title: resolveField(item, 'title', item.title),
    description: resolveField(item, 'description', item.description),
    conditions: resolveField(item, 'conditions', item.conditions),
  });

  const accessory = (item: HouseAccessory): HouseAccessory => ({
    ...item,
    title: resolveField(item, 'title', item.title),
    description: resolveField(item, 'description', item.description),
  });

  return { accessory, contribution, goal, promotion, reward };
};
