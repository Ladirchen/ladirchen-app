import type { AvatarPartDefinition } from "@/domain/avatar";
import type { TranslationKey } from "@/locales/translation-keys";

export type AvatarCatalogCategory = "skin-tone" | "face" | "face-shape" | "hair" | "hair-color" | "outfit" | "outfit-color" | "accessory" | "fun-accessory" | "seasonal-accessory";
export type AvatarCatalogItemId = Extract<TranslationKey, `avatar.${AvatarCatalogCategory}.${string}`>;

export interface AvatarOption<T extends string> {
  readonly id: AvatarCatalogItemId;
  readonly value: T;
}

export interface AvatarColorOption<T extends string> extends AvatarOption<T> {
  readonly color: string;
}

export const defineAvatarOptions = <Definitions extends readonly AvatarPartDefinition[]>(
  category: AvatarCatalogCategory,
  definitions: Definitions,
): Array<AvatarOption<Definitions[number]["id"]>> => definitions.map(definition => ({
    id: `avatar.${category}.${definition.id}` as AvatarCatalogItemId,
    value: definition.id,
  }));

export const defineAvatarColorOptions = <Definitions extends readonly AvatarPartDefinition[]>(
  category: AvatarCatalogCategory,
  definitions: Definitions,
  colors: Record<Definitions[number]["id"], string>,
): Array<AvatarColorOption<Definitions[number]["id"]>> => definitions.map((definition) => {
  const value = definition.id as Definitions[number]["id"];
  return {
    id: `avatar.${category}.${value}` as AvatarCatalogItemId,
    value,
    color: colors[value],
  };
});
