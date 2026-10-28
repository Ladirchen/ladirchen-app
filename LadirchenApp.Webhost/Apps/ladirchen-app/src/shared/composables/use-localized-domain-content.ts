import { useI18n } from "vue-i18n";

import type { Contribution, Promotion } from "@/domain/contributions/types";
import type { HouseAccessory } from "@/domain/house/entities";
import type { SavingGoal } from "@/domain/savings/types";
import type { ShopReward } from "@/domain/shop/types";
import { appendTranslationKey } from "@/locales/translation-keys";
import type { TranslationField, TranslationKey, TranslationNamespaceKey } from "@/locales/translation-keys";

type TranslatableEntity<Namespace extends TranslationNamespaceKey> = { readonly translationKey?: Namespace };

export function useLocalizedDomainContent() {
  const i18n = useI18n();

  function resolveField<Namespace extends TranslationNamespaceKey, Field extends TranslationField<Namespace>>(
    entity: TranslatableEntity<Namespace>,
    field: Field,
    fallback: string,
  ): string {
    const key = entity.translationKey ? appendTranslationKey(entity.translationKey, field) : undefined;

    return key && i18n.te(key) ? i18n.t(key) : fallback;
  }

  function resolveKey(
    key: TranslationKey | undefined,
    fallback: string,
    params?: Record<string, number | string>,
  ): string {
    return key && i18n.te(key) ? i18n.t(key, params ?? {}) : fallback;
  }

  function contribution(item: Contribution): Contribution {
    return {
      ...item,
      title: resolveField(item, "title", item.title),
      description: resolveField(item, "description", item.description),
      area: resolveKey(item.areaKey, resolveField(item, "area", item.area)),
      dueLabel: resolveKey(item.dueLabelKey, resolveField(item, "dueLabel", item.dueLabel)),
    };
  }

  function goal(item: SavingGoal): SavingGoal {
    return { ...item, title: resolveField(item, "title", item.title) };
  }

  function promotion(item: Promotion): Promotion {
    return {
      ...item,
      title: resolveKey(item.titleKey, resolveField(item, "title", item.title), { multiplier: item.multiplier }),
    };
  }

  function reward(item: ShopReward): ShopReward {
    return {
      ...item,
      title: resolveField(item, "title", item.title),
      description: resolveField(item, "description", item.description),
      conditions: resolveField(item, "conditions", item.conditions),
    };
  }

  function accessory(item: HouseAccessory): HouseAccessory {
    return {
      ...item,
      title: resolveField(item, "title", item.title),
      description: resolveField(item, "description", item.description),
    };
  }

  return { accessory, contribution, goal, promotion, reward };
}
