<template>
  <motion.span
    class="ladi-wrap"
    :class="[stage.tier, { 'has-score': showScore }]"
    :style="{ '--ladi-size': `${size}px` }"
    :data-motion-state="motionState"
    :initial="false"
    :animate="characterMotion"
    :transition="characterTransition"
    :while-hover="hoverMotion"
    :while-press="pressMotion"
    role="img"
    :aria-label="ariaLabel"
  >
    <img class="ladi-sprite" :src="spriteUrl" alt="">
    <span v-if="showScore" class="ladi-score"><i aria-hidden="true" />{{ score.toFixed(1) }}</span>
  </motion.span>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { motion, useReducedMotion } from "motion-v";
import { useI18n } from "vue-i18n";

import { getLadiStage } from "@/domain/ladi";
import type { LadiSpriteId } from "@/domain/ladi";
import { LADI_SPRITE_ASSET_URLS } from "@/shared/assets/ladi-sprite-assets";

const { t } = useI18n();
const props = withDefaults(defineProps<{
  score: number;
  perched?: boolean;
  showScore?: boolean;
  size?: number;
  smart?: boolean;
}>(), {
  perched: false,
  showScore: true,
  size: 46,
  smart: false,
});
const stage = computed(() => getLadiStage(props.score));
const spriteId = computed<LadiSpriteId>(() => {
  if (props.perched) return "perched-ladi";
  if (props.smart) return "smart-ladi";
  return stage.value.id;
});
const spriteUrl = computed(() => LADI_SPRITE_ASSET_URLS[spriteId.value]);
const isBored = computed(() => stage.value.id === "idle-ladi");
const isCool = computed(() => stage.value.tier === "aurora");
const isSuper = computed(() => stage.value.tier === "super");
const reducedMotion = useReducedMotion();
const motionState = computed(() => {
  if (isSuper.value) return "super";
  if (isCool.value) return "cool";
  if (isBored.value) return "bored";
  return "happy";
});
const characterMotion = computed(() => {
  if (reducedMotion.value) return { rotate: 0, y: 0, scale: 1 };
  if (isSuper.value) return { rotate: 0, y: -4, scale: 1.06 };
  if (isCool.value) return { rotate: 1.5, y: -1, scale: 1.025 };
  if (isBored.value) return { rotate: -2, y: 3, scale: .96 };
  return { rotate: 0, y: 0, scale: 1 };
});
const hoverMotion = computed(() => reducedMotion.value
  ? undefined
  : { scale: characterMotion.value.scale * 1.035, y: characterMotion.value.y - 2 });
const pressMotion = computed(() => reducedMotion.value
  ? undefined
  : { scale: characterMotion.value.scale * .96 });
const ariaLabel = computed(() => t("ladi.mascotAria", {
  name: props.smart ? t("ladi.smartName") : t(stage.value.nameKey),
  score: props.score.toFixed(1),
}));
const characterTransition = { type: "spring", stiffness: 260, damping: 22, mass: .8 } as const;
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.ladi-wrap {
  --ladi-size: 46px;
  width: var(--ladi-size);
  height: var(--ladi-size);
  --uno: position-relative d-inline-flex align-center flex-shrink-0;
}
.ladi-wrap.has-score {
  width: calc(var(--ladi-size) + 23px);
}
.ladi-sprite {
  width: var(--ladi-size);
  height: var(--ladi-size);
  --uno: d-block;
  object-fit: contain;
  filter: drop-shadow(
    0 3px 2px color-mix(in srgb, var(--lad-palette-text) 20%, transparent)
  );
  pointer-events: none;
}
.aurora .ladi-sprite {
  filter: drop-shadow(
      0 0 7px color-mix(in srgb, var(--lad-palette-teal-400) 70%, transparent)
    )
    drop-shadow(
      0 3px 2px color-mix(in srgb, var(--lad-palette-text) 20%, transparent)
    );
}
.super .ladi-sprite {
  filter: drop-shadow(
      0 0 8px color-mix(in srgb, var(--lad-palette-blue) 60%, transparent)
    )
    drop-shadow(
      0 5px 3px color-mix(in srgb, var(--lad-palette-text) 20%, transparent)
    );
}
.ladi-score {
  min-width: 30px;
  margin-left: -8px;
  padding: 3px 6px 3px 9px;
  --uno: d-inline-flex align-center text-no-wrap;
  gap: 3px;
  color: var(--lad-palette-orange-750);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-amber-650) 15%, transparent);
  border-radius: 0 10px 10px 0;
  background: var(--lad-palette-amber-100);
  box-shadow: 0 2px 5px
    color-mix(in srgb, var(--lad-palette-orange-750) 10%, transparent);
  font-size: rem(9);
  font-weight: var(--lad-font-weight-black);
}
.ladi-score i {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--lad-palette-yellow);
  box-shadow: 0 0 0 2px
    color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent);
}
</style>
