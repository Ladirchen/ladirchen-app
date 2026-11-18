import type { Component } from "vue";
import type {
  AvatarAccessoryId,
  AvatarFaceExtensionId,
  AvatarFaceShapeExtensionId,
  AvatarFunAccessoryExtensionId,
  AvatarHairExtensionId,
  AvatarOutfitExtensionId,
  AvatarSeasonalAccessoryExtensionId,
} from "@/domain/avatar";
import BlueOcarinaVisual from "./BlueOcarinaVisual.vue";
import CapVisual from "./CapVisual.vue";
import CatEarsVisual from "./CatEarsVisual.vue";
import CrownVisual from "./CrownVisual.vue";
import FantasyEarsVisual from "./FantasyEarsVisual.vue";
import FlowerCrownVisual from "./FlowerCrownVisual.vue";
import ForestAdventurerVisual from "./ForestAdventurerVisual.vue";
import ForestBraidVisual from "./ForestBraidVisual.vue";
import GemstoneEarringsVisual from "./GemstoneEarringsVisual.vue";
import GlitterStageJacketVisual from "./GlitterStageJacketVisual.vue";
import GlassesVisual from "./GlassesVisual.vue";
import HeadphonesVisual from "./HeadphonesVisual.vue";
import HeadsetMicrophoneVisual from "./HeadsetMicrophoneVisual.vue";
import PopstarSpaceBunsVisual from "./PopstarSpaceBunsVisual.vue";
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
  "gemstone-earrings": { component: GemstoneEarringsVisual, requiresHeadClearance: false },
  "headset-microphone": { component: HeadsetMicrophoneVisual, requiresHeadClearance: false },
  "fantasy-ears": { component: FantasyEarsVisual, requiresHeadClearance: false },
  "blue-ocarina": { component: BlueOcarinaVisual, requiresHeadClearance: false },
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
    "popstar-space-buns": PopstarSpaceBunsVisual,
    "forest-braid": ForestBraidVisual,
    // design-generator:avatar-hair-visual
  },
  outfit: {
    "glitter-stage-jacket": GlitterStageJacketVisual,
    "forest-adventurer": ForestAdventurerVisual,
    // design-generator:avatar-outfit-visual
  },
  "seasonal-accessory": {
    // design-generator:avatar-seasonal-accessory-visual
  },
};
export function avatarExtensionVisualFor(
  category: keyof AvatarExtensionVisualRegistry,
  id: string,
): Component | undefined {
  return (avatarExtensionVisuals[category] as Readonly<Record<string, Component>>)[id];
}
