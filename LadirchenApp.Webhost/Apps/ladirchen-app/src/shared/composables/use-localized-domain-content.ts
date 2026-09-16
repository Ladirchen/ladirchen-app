import { useI18n } from 'vue-i18n';

import type { Contribution, HouseAccessory, Promotion, SavingGoal, ShopReward } from '@/domain/types';

type TranslatableEntity = { readonly translationKey?: string };

export const useLocalizedDomainContent = () => {
  const { t, te } = useI18n();

  const resolveField = (entity: TranslatableEntity, field: string, fallback: string): string => {
    const key = entity.translationKey ? `${entity.translationKey}.${field}` : undefined;
    return key && te(key) ? t(key) : fallback;
  };

  const resolveKey = (key: string | undefined, fallback: string, params?: Record<string, number | string>): string =>
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
