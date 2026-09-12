<template>
  <motion.span
    class="ladi-wrap"
    :class="stage.tier"
    :style="{ '--ladi-size': `${size}px` }"
    :data-motion-state="motionState"
    :initial="false"
    :animate="characterMotion"
    :transition="characterTransition"
    :while-hover="reducedMotion ? undefined : { scale: characterMotion.scale * 1.035, y: characterMotion.y - 2 }"
    :while-press="reducedMotion ? undefined : { scale: characterMotion.scale * .96 }"
    role="img"
    :aria-label="t('ladi.mascotAria', { name: smart ? t('ladi.smartName') : t(stage.nameKey), score: score.toFixed(1) })"
  >
    <svg class="ladi" viewBox="0 0 150 150">
      <g v-if="isBored && showSceneBase" class="bored-branch">
        <path d="M5 24c41 8 91-8 141 2" />
        <path class="branch-twig" d="m112 24 14-13" />
        <path class="branch-ring" d="M18 23q5 5 10 1" />
      </g>

      <g v-if="isBored" class="bored-bubble" aria-hidden="true">
        <path d="M112 54c0-8 7-14 16-14s16 6 16 14-7 14-16 14h-5l-7 7 2-9c-4-3-6-7-6-12Z" />
        <circle cx="122" cy="54" r="2.2" />
        <circle cx="129" cy="54" r="2.2" />
        <circle cx="136" cy="54" r="2.2" />
      </g>

      <g v-if="score >= 4.5" class="aurora-ribbons">
        <path d="M17 43C2 28 13 12 34 19" />
        <path d="M118 25c22 4 25 20 10 30" />
        <path d="M121 106c18 6 17 22-2 26" />
      </g>

      <g v-if="isSuper" class="super-speed-lines" aria-hidden="true">
        <path d="M16 52h24M8 67h24M116 39h22M121 119h20" />
      </g>

      <g v-if="score >= 4" class="coin-orbit">
        <ellipse cx="76" cy="84" rx="64" ry="28" />
        <circle cx="15" cy="83" r="5" />
        <circle v-if="score >= 4.5" cx="132" cy="70" r="4" />
      </g>

      <g class="ladi-sloth" :class="{ 'is-bored': isBored, 'is-super': isSuper }">
        <ellipse v-if="!isBored && showSceneBase" class="ground-shadow" cx="76" cy="138" rx="42" ry="6" />

        <g v-if="!isBored && showSceneBase && score >= 3" class="leaf-cluster leaf-left">
          <path d="M27 124C10 116 7 101 12 95c12 2 20 12 15 29Z" />
          <path d="M31 125c-3-17 7-29 16-29 6 11 2 23-16 29Z" />
        </g>
        <g v-if="!isBored && showSceneBase && score >= 4.5" class="leaf-cluster leaf-right">
          <path d="M119 124c3-17 14-26 24-23 2 12-6 22-24 23Z" />
          <path d="M116 124c-3-14-12-21-21-18 0 10 7 17 21 18Z" />
        </g>

        <path v-if="!isBored && !isSuper && showSceneBase" class="branch" d="M10 127c31-9 71 8 130-4" />
        <path v-if="!isBored && !isSuper && showSceneBase" class="branch-cut" d="m111 125 14-12" />

        <g v-if="!isBored && !isSuper" class="feet">
          <path d="M51 117c-9 5-12 14-6 18 6 3 13-3 16-12" />
          <path d="M99 117c9 5 12 14 6 18-6 3-13-3-16-12" />
        </g>

        <g v-if="isSuper" class="super-cape" aria-hidden="true">
          <path d="M43 53C18 69 19 111 5 130c22 2 39-5 52-19Z" />
          <path d="M107 53c25 16 24 58 38 77-22 2-39-5-52-19Z" />
        </g>

        <path class="body" d="M75 34c29 0 46 24 43 57-3 31-19 43-43 43S35 121 32 91c-3-33 14-57 43-57Z" />
        <path class="belly" d="M75 72c21 0 32 17 30 39-2 18-13 25-30 25s-29-8-30-25c-2-22 9-39 30-39Z" />
        <g v-if="isSuper" class="super-suit" aria-hidden="true">
          <path d="M45 88c17-9 43-9 60 0l-4 39c-14 10-39 10-52 0Z" />
          <path class="super-belt" d="M47 113c19 6 38 6 56 0" />
          <path class="super-badge" d="m75 82 12 7-3 14-9 7-9-7-3-14Z" />
          <path class="super-letter" d="M72 89v13h8" />
        </g>

        <circle class="ear" cx="38" cy="57" r="12" />
        <circle class="ear" cx="112" cy="57" r="12" />
        <ellipse class="face" cx="75" cy="58" rx="37" ry="32" />
        <path class="eye-mask eye-mask-left" d="M43 53c8-14 21-17 29-7-5 17-17 24-29 7Z" />
        <path class="eye-mask eye-mask-right" d="M107 53c-8-14-21-17-29-7 5 17 17 24 29 7Z" />
        <template v-if="!isBored">
          <ellipse class="eye" cx="59" cy="54" rx="4" ry="5" />
          <ellipse class="eye" cx="91" cy="54" rx="4" ry="5" />
          <circle class="eye-glint" cx="60" cy="52" r="1.3" />
          <circle class="eye-glint" cx="92" cy="52" r="1.3" />
        </template>
        <g v-else class="bored-eyes">
          <path d="M51 55q8 5 16 0" />
          <path d="M83 55q8 5 16 0" />
        </g>
        <path class="nose" d="M69 65q6-5 12 0l-6 6Z" />
        <path v-if="!isBored" class="smile" d="M75 71q-7 8-13 2m13-2q7 8 13 2" />
        <path v-else class="bored-mouth" d="M67 75q8-3 16 0" />
        <circle v-if="!isBored" class="cheek" cx="48" cy="69" r="4" />
        <circle v-if="!isBored" class="cheek" cx="102" cy="69" r="4" />

        <g v-if="isCool" class="cool-shades">
          <path class="shade-frame" d="M42 48q15-6 30 1m6 0q15-7 30-1M70 50q5-3 10 0" />
          <path class="shade-lens" d="M44 49q13-5 27 1c0 12-6 17-14 16-8-1-12-7-13-17Z" />
          <path class="shade-lens" d="M79 50q14-6 27-1c-1 10-5 16-13 17-8 1-14-4-14-16Z" />
          <path class="shade-glint" d="m51 51 8 10m2-11 6 8m24-7 7 9" />
        </g>

        <g v-if="smart" class="smart-glasses" aria-hidden="true">
          <circle cx="58" cy="56" r="13" />
          <circle cx="92" cy="56" r="13" />
          <path d="M71 55q4-3 8 0M45 52l-8-3m68 3 8-3" />
          <path class="smart-glasses-glint" d="m51 49 7 7m27-7 7 7" />
        </g>

        <g v-if="isSuper" class="super-mask">
          <path d="M43 50c8-8 18-9 29-3l-3 14c-9 6-19 3-26-6Zm64 0c-8-8-18-9-29-3l3 14c9 6 19 3 26-6Z" />
          <path class="super-mask-glint" d="m50 50 9 7m30-7 9 7" />
        </g>

        <g v-if="isCool" class="cool-sparkles" aria-hidden="true">
          <path d="m117 42 3 6 6 3-6 3-3 6-3-6-6-3 6-3Z" />
          <path d="m31 75 2 4 4 2-4 2-2 4-2-4-4-2 4-2Z" />
        </g>

        <g v-if="score >= 4" class="sun-scarf">
          <path d="M42 82c18 8 46 8 65 0l-4 12c-19 7-39 7-57 0Z" />
          <path d="m95 91 15 18-13 4-9-20Z" />
        </g>

        <g v-if="showCoin" class="ladirchen-core">
          <circle class="coin-glow" cx="75" cy="108" r="27" />
          <circle class="coin" cx="75" cy="108" r="22" />
          <path class="coin-letter" d="M69 96v23h14" />
          <path class="coin-shine" d="M61 103c2-7 7-11 14-12" />
        </g>

        <g v-if="!isBored && showCoin" class="hugging-arms">
          <path d="M38 78c-12 16-7 35 15 42" />
          <path d="M112 78c12 16 7 35-15 42" />
          <path class="claw" d="m51 115 7 1m-8 4 7 1" />
          <path class="claw" d="m99 115-7 1m8 4-7 1" />
        </g>

        <g v-if="!isBored && !showCoin" class="free-arms">
          <path class="resting-arm" d="M39 80c-10 18-5 31 9 39" />
          <g class="waving-arm">
            <path d="M110 81c16-13 22-28 15-43" />
            <path class="free-claw" d="m121 42 2-9m3 10 6-7" />
          </g>
        </g>

        <g v-else class="hanging-pose">
          <path class="hanging-arm" d="M48 76C35 59 39 38 48 23" />
          <path class="hanging-arm" d="M102 76c13-17 9-38 0-53" />
          <path class="hanging-claw" d="m43 27 6-8m1 10 4-9m53 7-6-8m-1 10-4-9" />
          <path class="dangling-foot" d="M54 124c-5 10-3 17 3 19m39-19c5 10 3 17-3 19" />
        </g>

        <g v-if="score < 3" class="tiny-leaf">
          <path d="M119 120c-1-10 5-17 12-17 3 8-1 15-12 17Z" />
        </g>
        <g v-if="score >= 4.5" class="floating-homes">
          <path d="m18 63 7-7 7 7v9H18Z" />
          <path d="m119 92 6-6 6 6v8h-12Z" />
        </g>
      </g>
    </svg>

    <span v-if="showScore" class="ladi-score"><i aria-hidden="true" />{{ score.toFixed(1) }}</span>
  </motion.span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { motion, useReducedMotion } from 'motion-v';
import { useI18n } from 'vue-i18n';

import { getLadiStage } from '@/domain/ladi';

const { t } = useI18n();
const props = withDefaults(defineProps<{ score: number; showCoin?: boolean; showSceneBase?: boolean; size?: number; showScore?: boolean; smart?: boolean }>(), { showCoin: true, showSceneBase: true, size: 46, showScore: true, smart: false });
const stage = computed(() => getLadiStage(props.score));
const isBored = computed(() => props.score < 2.5);
const isCool = computed(() => props.score >= 4.3 && props.score < 4.8);
const isSuper = computed(() => props.score >= 4.8);
const reducedMotion = useReducedMotion();
const motionState = computed(() => isSuper.value ? 'super' : isCool.value ? 'cool' : isBored.value ? 'bored' : 'happy');
const characterMotion = computed(() => {
  if (reducedMotion.value) return { rotate: 0, y: 0, scale: 1 };
  if (isSuper.value) return { rotate: 0, y: -4, scale: 1.06 };
  if (isCool.value) return { rotate: 1.5, y: -1, scale: 1.025 };
  if (isBored.value) return { rotate: -2, y: 3, scale: .96 };
  return { rotate: 0, y: 0, scale: 1 };
});
const characterTransition = { type: 'spring', stiffness: 260, damping: 22, mass: .8 } as const;
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.ladi-wrap {
  --ladi-size: 46px;
  --fur: var(--lad-palette-orange-500);
  --fur-dark: var(--lad-palette-orange-650);
  --face: var(--lad-palette-amber-200);
  --mask: var(--lad-palette-orange-600);
  width: calc(var(--ladi-size) + 23px);
  height: var(--ladi-size);
  @apply position-relative d-inline-flex align-center flex-shrink-0;
}
.ladi {
  width: var(--ladi-size);
  height: var(--ladi-size);
  @apply overflow-visible;
  filter: drop-shadow(
    0 3px 2px color-mix(in srgb, var(--lad-palette-text) 20%, transparent)
  );
}
.ladi-sloth {
  transform-origin: 75px 126px;
  animation: sloth-sway 4.5s ease-in-out infinite;
}
.ladi-sloth.is-bored {
  transform-origin: 75px 24px;
  animation: bored-dangle 5.2s ease-in-out infinite;
}
.ladi-sloth.is-super {
  transform-origin: 75px 80px;
  animation: super-hover 2.7s ease-in-out infinite;
}
.ground-shadow {
  fill: color-mix(in srgb, var(--lad-palette-text) 15%, transparent);
  transform-origin: center;
  animation: shadow-breathe 4.5s ease-in-out infinite;
}
.branch,
.branch-cut {
  fill: none;
  stroke: var(--lad-palette-orange-650);
  stroke-linecap: round;
  stroke-width: 9;
}
.branch-cut {
  stroke-width: 5;
}
.bored-branch {
  fill: none;
  stroke: var(--lad-palette-orange-650);
  stroke-linecap: round;
  stroke-width: 10;
}
.bored-branch .branch-twig {
  stroke-width: 5;
}
.bored-branch .branch-ring {
  stroke: var(--lad-palette-orange-500);
  stroke-width: 2.5;
}
.bored-bubble {
  animation: bored-bubble 3.2s ease-in-out infinite;
}
.bored-bubble path {
  fill: var(--lad-palette-surface);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 2;
}
.bored-bubble circle {
  fill: var(--lad-palette-orange-650);
}
.body {
  fill: var(--fur);
  stroke: var(--fur-dark);
  stroke-width: 3.5;
}
.belly {
  fill: var(--lad-palette-orange-400-2);
  opacity: 0.72;
}
.ear {
  fill: var(--fur);
  stroke: var(--fur-dark);
  stroke-width: 3;
}
.face {
  fill: var(--face);
  stroke: var(--fur-dark);
  stroke-width: 3;
}
.eye-mask {
  fill: var(--mask);
}
.eye {
  fill: var(--lad-palette-text);
  transform-origin: center;
  animation: blink 5.4s ease-in-out infinite;
}
.eye-glint {
  fill: white;
}
.nose {
  fill: var(--lad-palette-muted-750);
}
.smile {
  fill: none;
  stroke: var(--lad-palette-orange-750);
  stroke-linecap: round;
  stroke-width: 2.5;
}
.bored-eyes,
.bored-mouth {
  fill: none;
  stroke: var(--lad-palette-orange-750);
  stroke-linecap: round;
  stroke-width: 3;
}
.cheek {
  fill: var(--lad-palette-red-300);
  opacity: 0.45;
}
.feet path,
.hugging-arms > path:not(.claw) {
  fill: none;
  stroke: var(--fur-dark);
  stroke-linecap: round;
  stroke-width: 10;
}
.free-arms path {
  fill: none;
  stroke: var(--fur-dark);
  stroke-linecap: round;
  stroke-width: 10;
}
.free-arms .free-claw {
  stroke: var(--lad-palette-amber-150);
  stroke-width: 2.5;
}
.waving-arm {
  transform-origin: 111px 82px;
  animation: free-arm-wave 2.8s ease-in-out infinite;
}
.hanging-arm {
  fill: none;
  stroke: var(--fur-dark);
  stroke-linecap: round;
  stroke-width: 10;
}
.hanging-claw,
.dangling-foot {
  fill: none;
  stroke: var(--lad-palette-amber-150);
  stroke-linecap: round;
  stroke-width: 2.5;
}
.claw {
  fill: none;
  stroke: var(--lad-palette-amber-150);
  stroke-linecap: round;
  stroke-width: 2;
}
.coin-glow {
  fill: var(--lad-palette-amber-150);
  opacity: 0.45;
  transform-origin: 75px 108px;
  animation: coin-heartbeat 2.4s ease-in-out infinite;
}
.coin {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 3.5;
}
.coin-letter,
.coin-shine {
  fill: none;
  stroke: var(--lad-palette-amber-700);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 4;
}
.coin-shine {
  stroke: var(--lad-palette-amber-150);
  stroke-width: 3;
}
.leaf-cluster,
.tiny-leaf {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-teal-700);
  stroke-linejoin: round;
  stroke-width: 2.5;
  transform-origin: center;
  animation: leaf-wave 3s ease-in-out infinite;
}
.sun-scarf {
  fill: var(--lad-palette-amber-450);
  stroke: var(--lad-palette-amber-600);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.cool-shades {
  transform-origin: 75px 55px;
  animation: shades-bop 3.6s ease-in-out infinite;
}
.shade-frame {
  fill: none;
  stroke: var(--lad-palette-text);
  stroke-linecap: round;
  stroke-width: 4;
}
.shade-lens {
  fill: var(--lad-palette-text);
  stroke: var(--lad-palette-violet-850);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.shade-glint {
  fill: none;
  stroke: var(--lad-palette-blue-250);
  stroke-linecap: round;
  stroke-width: 2;
  opacity: 0.72;
}
.smart-glasses {
  fill: color-mix(in srgb, var(--lad-palette-blue-150) 25%, transparent);
  stroke: var(--lad-palette-muted-750-2);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3.5;
}
.smart-glasses-glint {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  stroke-width: 2;
}
.cool-sparkles {
  fill: var(--lad-palette-amber-150);
  stroke: var(--lad-palette-amber-550);
  stroke-linejoin: round;
  stroke-width: 1.5;
  animation: cool-sparkle 2s ease-in-out infinite;
}
.super-speed-lines {
  fill: none;
  stroke: var(--lad-palette-blue-250);
  stroke-linecap: round;
  stroke-width: 4;
  opacity: 0.65;
  animation: super-speed 1.4s ease-in-out infinite;
}
.super-cape {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
  stroke-linejoin: round;
  stroke-width: 3;
  transform-origin: 75px 57px;
  animation: cape-flutter 1.8s ease-in-out infinite;
}
.super-suit > path:first-child {
  fill: var(--lad-palette-blue-strong);
  stroke: var(--lad-palette-indigo-650);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.super-belt {
  fill: none;
  stroke: var(--lad-palette-yellow);
  stroke-width: 5;
}
.super-badge {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 2;
}
.super-letter {
  fill: none;
  stroke: var(--lad-palette-red-600);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.super-mask {
  fill: var(--lad-palette-blue-strong);
  stroke: var(--lad-palette-indigo-750);
  stroke-linejoin: round;
  stroke-width: 2;
}
.super-mask-glint {
  fill: none;
  stroke: var(--lad-palette-blue-250);
  stroke-linecap: round;
  stroke-width: 2;
}
.coin-orbit {
  fill: var(--lad-palette-amber-150);
  stroke: var(--lad-palette-amber-550);
  stroke-dasharray: 5 5;
  stroke-width: 2.5;
  transform-origin: 76px 84px;
  animation: orbit-rock 5.5s ease-in-out infinite;
}
.coin-orbit ellipse {
  fill: none;
}
.aurora-ribbons {
  fill: none;
  stroke: var(--lad-palette-teal-400);
  stroke-linecap: round;
  stroke-width: 4;
  opacity: 0.72;
  animation: aurora-drift 3.4s ease-in-out infinite;
}
.floating-homes {
  fill: var(--lad-palette-amber-150);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 2;
  animation: homes-float 2.8s ease-in-out infinite;
}
.garden {
  --fur: var(--lad-palette-orange-500);
  --fur-dark: var(--lad-palette-orange-750);
  --face: var(--lad-palette-amber-200);
  --mask: var(--lad-palette-orange-600);
}
.sun {
  --fur: var(--lad-palette-orange-500);
  --fur-dark: var(--lad-palette-orange-650);
  --face: var(--lad-palette-amber-200);
  --mask: var(--lad-palette-orange-600);
}
.aurora {
  --fur: var(--lad-palette-muted);
  --fur-dark: var(--lad-palette-muted-700);
  --face: var(--lad-palette-amber-200);
  --mask: var(--lad-palette-muted-600-2);
}
.aurora .ladi {
  filter: drop-shadow(
      0 0 7px color-mix(in srgb, var(--lad-palette-teal-400) 70%, transparent)
    )
    drop-shadow(
      0 3px 2px color-mix(in srgb, var(--lad-palette-text) 20%, transparent)
    );
}
.super {
  --fur: var(--lad-palette-orange-500);
  --fur-dark: var(--lad-palette-orange-750);
  --face: var(--lad-palette-amber-200);
  --mask: var(--lad-palette-orange-650);
}
.super .ladi {
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
  @apply d-inline-flex align-center;
  gap: 3px;
  color: var(--lad-palette-orange-750);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-amber-650) 15%, transparent);
  border-radius: 0 10px 10px 0;
  background: var(--lad-palette-amber-100);
  box-shadow: 0 2px 5px
    color-mix(in srgb, var(--lad-palette-orange-750) 10%, transparent);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-black);
  @apply text-no-wrap;
}
.ladi-score i {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--lad-palette-yellow);
  box-shadow: 0 0 0 2px
    color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent);
}
@keyframes sloth-sway {
  0%,
  100% {
    transform: translateY(2px) rotate(-1.5deg);
  }
  50% {
    transform: translateY(-3px) rotate(1.5deg);
  }
}
@keyframes bored-dangle {
  0%,
  100% {
    transform: rotate(-2deg) translateY(1px);
  }
  50% {
    transform: rotate(2deg) translateY(4px);
  }
}
@keyframes super-hover {
  0%,
  100% {
    transform: translateY(2px) rotate(-1deg);
  }
  50% {
    transform: translateY(-6px) rotate(1deg);
  }
}
@keyframes bored-bubble {
  0%,
  100% {
    opacity: 0.42;
    transform: translateY(2px) scale(0.96);
  }
  50% {
    opacity: 0.9;
    transform: translateY(-2px) scale(1);
  }
}
@keyframes shadow-breathe {
  0%,
  100% {
    transform: scaleX(1);
    opacity: 0.14;
  }
  50% {
    transform: scaleX(0.82);
    opacity: 0.08;
  }
}
@keyframes blink {
  0%,
  45%,
  49%,
  100% {
    transform: scaleY(1);
  }
  47%,
  48% {
    transform: scaleY(0.12);
  }
}
@keyframes coin-heartbeat {
  0%,
  100% {
    transform: scale(0.88);
    opacity: 0.32;
  }
  50% {
    transform: scale(1.08);
    opacity: 0.7;
  }
}
@keyframes leaf-wave {
  0%,
  100% {
    transform: rotate(-2deg);
  }
  50% {
    transform: rotate(3deg);
  }
}
@keyframes orbit-rock {
  0%,
  100% {
    transform: rotate(-4deg);
  }
  50% {
    transform: rotate(5deg);
  }
}
@keyframes aurora-drift {
  0%,
  100% {
    opacity: 0.35;
    transform: translateY(2px);
  }
  50% {
    opacity: 0.85;
    transform: translateY(-3px);
  }
}
@keyframes homes-float {
  0%,
  100% {
    transform: translateY(1px);
  }
  50% {
    transform: translateY(-4px);
  }
}
@keyframes shades-bop {
  0%,
  100% {
    transform: rotate(-1deg);
  }
  50% {
    transform: rotate(2deg) translateY(-1px);
  }
}
@keyframes cool-sparkle {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.82) rotate(-5deg);
  }
  50% {
    opacity: 1;
    transform: scale(1.08) rotate(5deg);
  }
}
@keyframes cape-flutter {
  0%,
  100% {
    transform: skewY(-2deg) scaleX(0.96);
  }
  50% {
    transform: skewY(3deg) scaleX(1.05);
  }
}
@keyframes super-speed {
  0%,
  100% {
    opacity: 0.25;
    transform: translateX(-3px);
  }
  50% {
    opacity: 0.85;
    transform: translateX(4px);
  }
}
@keyframes free-arm-wave {
  0%,
  58%,
  100% {
    transform: rotate(0);
  }
  68% {
    transform: rotate(-18deg);
  }
  77% {
    transform: rotate(13deg);
  }
  86% {
    transform: rotate(-9deg);
  }
}
@include reduced-motion {
  .ladi-wrap * {
    animation: none !important;
  }
}
</style>
