import type { TranslationKey } from "@/locales/translation-keys";

/** Typed progression stages for the Ladi mascot. */

export type LadiTier = "spark" | "garden" | "sun" | "aurora" | "super";
export type LadiStageId = "idle-ladi" | "starter-ladi" | "garden-ladi" | "sun-ladi" | "aurora-ladi" | "super-ladi";
export type LadiSpriteId = LadiStageId | "perched-ladi" | "smart-ladi";

export interface LadiStage {
  readonly id: LadiStageId;
  readonly tier: LadiTier;
  readonly minimumScore: number;
  readonly nameKey: TranslationKey;
  readonly descriptionKey: TranslationKey;
}

export const LADI_STAGES: readonly LadiStage[] = [
  {
    id: "super-ladi",
    tier: "super",
    minimumScore: 4.8,
    nameKey: "ladi.stages.super.name",
    descriptionKey: "ladi.stages.super.description",
  },
  {
    id: "aurora-ladi",
    tier: "aurora",
    minimumScore: 4.3,
    nameKey: "ladi.stages.aurora.name",
    descriptionKey: "ladi.stages.aurora.description",
  },
  {
    id: "sun-ladi",
    tier: "sun",
    minimumScore: 4,
    nameKey: "ladi.stages.sun.name",
    descriptionKey: "ladi.stages.sun.description",
  },
  {
    id: "garden-ladi",
    tier: "garden",
    minimumScore: 3,
    nameKey: "ladi.stages.garden.name",
    descriptionKey: "ladi.stages.garden.description",
  },
  {
    id: "starter-ladi",
    tier: "spark",
    minimumScore: 2.5,
    nameKey: "ladi.stages.starter.name",
    descriptionKey: "ladi.stages.starter.description",
  },
  {
    id: "idle-ladi",
    tier: "spark",
    minimumScore: Number.NEGATIVE_INFINITY,
    nameKey: "ladi.stages.idle.name",
    descriptionKey: "ladi.stages.idle.description",
  },
];

export const getLadiStage = (score: number): LadiStage =>
  LADI_STAGES.find(stage => score >= stage.minimumScore) ?? LADI_STAGES[LADI_STAGES.length - 1]!;
