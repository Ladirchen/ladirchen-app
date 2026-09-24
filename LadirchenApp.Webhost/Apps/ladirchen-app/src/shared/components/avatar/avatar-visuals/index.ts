import type { Component } from "vue";

import type { AvatarAccessoryId, AvatarFaceExtensionId, AvatarFaceShapeExtensionId, AvatarFunAccessoryExtensionId, AvatarHairExtensionId, AvatarOutfitExtensionId, AvatarSeasonalAccessoryExtensionId } from "@/domain/avatar";
import CapVisual from "./CapVisual.vue";
import CatEarsVisual from "./CatEarsVisual.vue";
import CrownVisual from "./CrownVisual.vue";
import FlowerCrownVisual from "./FlowerCrownVisual.vue";
import GlassesVisual from "./GlassesVisual.vue";
import HeadphonesVisual from "./HeadphonesVisual.vue";
import PropellerCapVisual from "./PropellerCapVisual.vue";
import StarGlassesVisual from "./StarGlassesVisual.vue";
// design-generator:avatar-visual-import

export interface AvatarAccessoryVisualDefinition {
  readonly component: Component;
  readonly requiresHeadClearance: boolean;
}

export const avatarAccessoryVisuals = {
  glasses: { component: GlassesVisual, requiresHeadClearance: false },
  headphones: { component: HeadphonesVisual, requiresHeadClearance: true },
  "cat-ears": { component: CatEarsVisual, requiresHeadClearance: true },
  cap: { component: CapVisual, requiresHeadClearance: true },
  crown: { component: CrownVisual, requiresHeadClearance: true },
  "star-glasses": { component: StarGlassesVisual, requiresHeadClearance: false },
  "flower-crown": { component: FlowerCrownVisual, requiresHeadClearance: true },
  "propeller-cap": { component: PropellerCapVisual, requiresHeadClearance: true },
  // design-generator:avatar-accessory-visual
} satisfies Record<Exclude<AvatarAccessoryId, "none">, AvatarAccessoryVisualDefinition>;

export interface AvatarExtensionVisualRegistry {
  readonly face: Record<AvatarFaceExtensionId, Component>;
  readonly "face-shape": Record<AvatarFaceShapeExtensionId, Component>;
  readonly "fun-accessory": Record<AvatarFunAccessoryExtensionId, Component>;
  readonly hair: Record<AvatarHairExtensionId, Component>;
  readonly outfit: Record<AvatarOutfitExtensionId, Component>;
  readonly "seasonal-accessory": Record<AvatarSeasonalAccessoryExtensionId, Component>;
}

export const avatarExtensionVisuals: AvatarExtensionVisualRegistry = {
  face: {
    // design-generator:avatar-face-visual
  },
  "face-shape": {
    // design-generator:avatar-face-shape-visual
  },
  "fun-accessory": {
    // design-generator:avatar-fun-accessory-visual
  },
  hair: {
    // design-generator:avatar-hair-visual
  },
  outfit: {
    // design-generator:avatar-outfit-visual
  },
  "seasonal-accessory": {
    // design-generator:avatar-seasonal-accessory-visual
  },
};

export const avatarExtensionVisualFor = (category: keyof AvatarExtensionVisualRegistry, id: string): Component | undefined =>
  (avatarExtensionVisuals[category] as Readonly<Record<string, Component>>)[id];
