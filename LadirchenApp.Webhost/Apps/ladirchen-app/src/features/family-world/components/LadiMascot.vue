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
    :aria-label="`${stage.name}, das Sparfaultier, Bewertung ${score.toFixed(1)} von 5`"
  >
    <svg class="ladi" viewBox="0 0 150 150">
      <g v-if="isBored" class="bored-branch">
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
        <ellipse v-if="!isBored" class="ground-shadow" cx="76" cy="138" rx="42" ry="6" />

        <g v-if="!isBored && score >= 3" class="leaf-cluster leaf-left">
          <path d="M27 124C10 116 7 101 12 95c12 2 20 12 15 29Z" />
          <path d="M31 125c-3-17 7-29 16-29 6 11 2 23-16 29Z" />
        </g>
        <g v-if="!isBored && score >= 4.5" class="leaf-cluster leaf-right">
          <path d="M119 124c3-17 14-26 24-23 2 12-6 22-24 23Z" />
          <path d="M116 124c-3-14-12-21-21-18 0 10 7 17 21 18Z" />
        </g>

        <path v-if="!isBored && !isSuper" class="branch" d="M10 127c31-9 71 8 130-4" />
        <path v-if="!isBored && !isSuper" class="branch-cut" d="m111 125 14-12" />

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

        <g class="ladirchen-core">
          <circle class="coin-glow" cx="75" cy="108" r="27" />
          <circle class="coin" cx="75" cy="108" r="22" />
          <path class="coin-letter" d="M69 96v23h14" />
          <path class="coin-shine" d="M61 103c2-7 7-11 14-12" />
        </g>

        <g v-if="!isBored" class="hugging-arms">
          <path d="M38 78c-12 16-7 35 15 42" />
          <path d="M112 78c12 16 7 35-15 42" />
          <path class="claw" d="m51 115 7 1m-8 4 7 1" />
          <path class="claw" d="m99 115-7 1m8 4-7 1" />
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

import { getLadiStage } from '../domain/ladi';

const props = withDefaults(defineProps<{ score: number; size?: number; showScore?: boolean }>(), { size: 46, showScore: true });
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

<style scoped>
.ladi-wrap {
  --ladi-size: 46px;
  --fur: #a86d46;
  --fur-dark: #6e452f;
  --face: #efd3a5;
  --mask: #806047;
  width: calc(var(--ladi-size) + 23px);
  height: var(--ladi-size);
  position: relative;
  display: inline-flex;
  align-items: center;
  flex-shrink: 0;
}
.ladi {
  width: var(--ladi-size);
  height: var(--ladi-size);
  overflow: visible;
  filter: drop-shadow(0 3px 2px rgba(48, 45, 36, 0.22));
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
  fill: rgba(43, 74, 65, 0.14);
  transform-origin: center;
  animation: shadow-breathe 4.5s ease-in-out infinite;
}
.branch,
.branch-cut {
  fill: none;
  stroke: #795038;
  stroke-linecap: round;
  stroke-width: 9;
}
.branch-cut {
  stroke-width: 5;
}
.bored-branch {
  fill: none;
  stroke: #795038;
  stroke-linecap: round;
  stroke-width: 10;
}
.bored-branch .branch-twig {
  stroke-width: 5;
}
.bored-branch .branch-ring {
  stroke: #a87a55;
  stroke-width: 2.5;
}
.bored-bubble {
  animation: bored-bubble 3.2s ease-in-out infinite;
}
.bored-bubble path {
  fill: #fffaf0;
  stroke: #795038;
  stroke-width: 2;
}
.bored-bubble circle {
  fill: #795038;
}
.body {
  fill: var(--fur);
  stroke: var(--fur-dark);
  stroke-width: 3.5;
}
.belly {
  fill: #c88f61;
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
  fill: #2f2b29;
  transform-origin: center;
  animation: blink 5.4s ease-in-out infinite;
}
.eye-glint {
  fill: white;
}
.nose {
  fill: #493329;
}
.smile {
  fill: none;
  stroke: #5a392c;
  stroke-linecap: round;
  stroke-width: 2.5;
}
.bored-eyes,
.bored-mouth {
  fill: none;
  stroke: #5a392c;
  stroke-linecap: round;
  stroke-width: 3;
}
.cheek {
  fill: #e98975;
  opacity: 0.45;
}
.feet path,
.hugging-arms > path:not(.claw) {
  fill: none;
  stroke: var(--fur-dark);
  stroke-linecap: round;
  stroke-width: 10;
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
  stroke: #f5dfbd;
  stroke-linecap: round;
  stroke-width: 2.5;
}
.claw {
  fill: none;
  stroke: #f5dfbd;
  stroke-linecap: round;
  stroke-width: 2;
}
.coin-glow {
  fill: #ffeaa0;
  opacity: 0.45;
  transform-origin: 75px 108px;
  animation: coin-heartbeat 2.4s ease-in-out infinite;
}
.coin {
  fill: #ffd05b;
  stroke: #a46019;
  stroke-width: 3.5;
}
.coin-letter,
.coin-shine {
  fill: none;
  stroke: #885016;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 4;
}
.coin-shine {
  stroke: #fff0a8;
  stroke-width: 3;
}
.leaf-cluster,
.tiny-leaf {
  fill: #65bc76;
  stroke: #2e7f52;
  stroke-linejoin: round;
  stroke-width: 2.5;
  transform-origin: center;
  animation: leaf-wave 3s ease-in-out infinite;
}
.sun-scarf {
  fill: #f0b845;
  stroke: #a8641a;
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.cool-shades {
  transform-origin: 75px 55px;
  animation: shades-bop 3.6s ease-in-out infinite;
}
.shade-frame {
  fill: none;
  stroke: #252638;
  stroke-linecap: round;
  stroke-width: 4;
}
.shade-lens {
  fill: #292b43;
  stroke: #161725;
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.shade-glint {
  fill: none;
  stroke: #82e2ef;
  stroke-linecap: round;
  stroke-width: 2;
  opacity: 0.72;
}
.cool-sparkles {
  fill: #fff1a0;
  stroke: #d08925;
  stroke-linejoin: round;
  stroke-width: 1.5;
  animation: cool-sparkle 2s ease-in-out infinite;
}
.super-speed-lines {
  fill: none;
  stroke: #72d5cf;
  stroke-linecap: round;
  stroke-width: 4;
  opacity: 0.65;
  animation: super-speed 1.4s ease-in-out infinite;
}
.super-cape {
  fill: #e65462;
  stroke: #933645;
  stroke-linejoin: round;
  stroke-width: 3;
  transform-origin: 75px 57px;
  animation: cape-flutter 1.8s ease-in-out infinite;
}
.super-suit > path:first-child {
  fill: #3d78d8;
  stroke: #24509a;
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.super-belt {
  fill: none;
  stroke: #ffd05b;
  stroke-width: 5;
}
.super-badge {
  fill: #ffd05b;
  stroke: #a46019;
  stroke-linejoin: round;
  stroke-width: 2;
}
.super-letter {
  fill: none;
  stroke: #9d3543;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.super-mask {
  fill: #315fae;
  stroke: #1d3f79;
  stroke-linejoin: round;
  stroke-width: 2;
}
.super-mask-glint {
  fill: none;
  stroke: #91def2;
  stroke-linecap: round;
  stroke-width: 2;
}
.coin-orbit {
  fill: #fff1a0;
  stroke: #d08925;
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
  stroke: #70d6bb;
  stroke-linecap: round;
  stroke-width: 4;
  opacity: 0.72;
  animation: aurora-drift 3.4s ease-in-out infinite;
}
.floating-homes {
  fill: #fff2ae;
  stroke: #97601e;
  stroke-linejoin: round;
  stroke-width: 2;
  animation: homes-float 2.8s ease-in-out infinite;
}
.garden {
  --fur: #a9774f;
  --fur-dark: #5c4932;
  --face: #e9d7a8;
  --mask: #74634a;
}
.sun {
  --fur: #b97748;
  --fur-dark: #71462e;
  --face: #f2d6a2;
  --mask: #7c5941;
}
.aurora {
  --fur: #829e83;
  --fur-dark: #405f54;
  --face: #e6dfb5;
  --mask: #5a7061;
}
.aurora .ladi {
  filter: drop-shadow(0 0 7px rgba(90, 207, 180, 0.68))
    drop-shadow(0 3px 2px rgba(48, 45, 36, 0.2));
}
.super {
  --fur: #a86d46;
  --fur-dark: #61402e;
  --face: #f0d5aa;
  --mask: #75553f;
}
.super .ladi {
  filter: drop-shadow(0 0 8px rgba(71, 151, 224, 0.56))
    drop-shadow(0 5px 3px rgba(48, 45, 36, 0.2));
}
.ladi-score {
  min-width: 30px;
  margin-left: -8px;
  padding: 3px 6px 3px 9px;
  display: inline-flex;
  align-items: center;
  gap: 3px;
  color: #594016;
  border: 1px solid rgba(166, 96, 24, 0.16);
  border-radius: 0 10px 10px 0;
  background: #fff3c7;
  box-shadow: 0 2px 5px rgba(87, 59, 17, 0.1);
  font-size: 9px;
  font-weight: 950;
  white-space: nowrap;
}
.ladi-score i {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: #f2b84e;
  box-shadow: 0 0 0 2px rgba(242, 184, 78, 0.2);
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
@media (prefers-reduced-motion: reduce) {
  .ladi-wrap * {
    animation: none !important;
  }
}
</style>
