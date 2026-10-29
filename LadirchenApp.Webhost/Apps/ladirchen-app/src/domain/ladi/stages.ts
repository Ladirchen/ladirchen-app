import type { TranslationKey } from "@/locales/translation-keys";
/** Typed progression stages for the Ladi mascot. */
export type LadiTier = "spark" | "garden" | "sun" | "aurora" | "super";
export type LadiStageId = "idle-ladi" | "starter-ladi" | "garden-ladi" | "sun-ladi" | "aurora-ladi" | "super-ladi";
export type LadiSpriteId = LadiStageId | "perched-ladi" | "smart-ladi";
export interface LadiStage {
  readonly id: LadiStageId;
  readonly tier: LadiTier;
  readonly nameKey: TranslationKey;
  readonly descriptionKey: TranslationKey;
}
export const LADI_STAGES: readonly LadiStage[] = [
  {
    id: "super-ladi",
    tier: "super",
    nameKey: "ladi.stages.super.name",
    descriptionKey: "ladi.stages.super.description",
  },
  {
    id: "aurora-ladi",
    tier: "aurora",
    nameKey: "ladi.stages.aurora.name",
    descriptionKey: "ladi.stages.aurora.description",
  },
  {
    id: "sun-ladi",
    tier: "sun",
    nameKey: "ladi.stages.sun.name",
    descriptionKey: "ladi.stages.sun.description",
  },
  {
    id: "garden-ladi",
    tier: "garden",
    nameKey: "ladi.stages.garden.name",
    descriptionKey: "ladi.stages.garden.description",
  },
  {
    id: "starter-ladi",
    tier: "spark",
    nameKey: "ladi.stages.starter.name",
    descriptionKey: "ladi.stages.starter.description",
  },
  {
    id: "idle-ladi",
    tier: "spark",
    nameKey: "ladi.stages.idle.name",
    descriptionKey: "ladi.stages.idle.description",
  },
];
export function ladiStageById(stageId: LadiStageId): LadiStage {
  return LADI_STAGES.find((stage) => stage.id === stageId) ?? LADI_STAGES[LADI_STAGES.length - 1]!;
}
