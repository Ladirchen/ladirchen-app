<template>
  <aside class="global-ladi-guide" :class="[`mood-${mood}`, { hidden: isHidden, 'gift-celebration': giftCelebration }]" :aria-label="t('guide.aria')">
    <button v-if="isHidden" class="guide-branch" type="button" :aria-label="t('guide.reveal')" @click="revealGuide">
      <img :src="WORLD_DECORATION_SPRITE_URLS['guide-branch']" alt="">
    </button>
    <template v-else>
      <Transition name="guide-speech">
        <div v-if="speech" class="guide-speech" role="status">
          <button v-if="!moodPromptPending" :aria-label="t('guide.close')" class="guide-close" type="button" @click="closeSpeech">×</button>
          <strong>{{ speechHeading }}</strong>
          <span>{{ speech }}</span>
          <div v-if="speechProgress || speechActionLabel" class="speech-actions">
            <span v-if="speechProgress" class="speech-progress">{{ speechProgress }}</span>
            <button v-if="speechActionLabel" class="speech-next" type="button" @click="triggerSpeechAction">
              {{ speechActionLabel }}
              <span aria-hidden="true">→</span>
            </button>
          </div>
          <div v-if="choosingMood" class="mood-picker" :aria-label="t('guide.mood.choose')">
            <button
              v-for="option in moodOptions"
              :key="option.id"
              :aria-pressed="mood === option.id"
              :class="{ active: mood === option.id }"
              type="button"
              @click="selectMood(option.id)"
            ><span aria-hidden="true">{{ option.icon }}</span>{{ option.label }}</button>
          </div>
        </div>
      </Transition>

      <button class="guide-hide" type="button" :aria-label="t('guide.hide')" @click="hideGuide">›</button>
      <span v-if="giftCelebration" class="guide-high-five" aria-hidden="true">✋</span>
      <span v-if="giftCelebration" class="guide-flying-gift" aria-hidden="true">🎁</span>
      <button class="guide-ladi" :class="randomMotion" type="button" :aria-expanded="Boolean(speech)" :aria-label="t('guide.openExplanation')" @click="speakCurrentPageIntro">
        <LadiMascot :score="ladiScore" :show-score="false" :smart="store.piggyBankOpen || isSmart" :size="80" />
      </button>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import LadiMascot from "@/shared/components/LadiMascot.vue";
import { WORLD_DECORATION_SPRITE_URLS } from "@/shared/assets/world-sprite-assets";
import { useGlobalLadiGuide } from "@/app/composables/use-global-ladi-guide";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const {
  choosingMood, closeSpeech, giftCelebration, hideGuide, isHidden, isSmart, ladiScore, mood, moodOptions,
  moodPromptPending, randomMotion, revealGuide, selectMood, speakCurrentPageIntro, speech, speechActionLabel,
  speechHeading, speechProgress, store, triggerSpeechAction,
} = useGlobalLadiGuide();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.global-ladi-guide {
  --uno: position-fixed pointer-events-none;
  right: calc(var(--lad-shell-inline-offset, 0px) + 4px);
  bottom: 86px;
  z-index: 3000;
}
.global-ladi-guide.hidden {
  right: calc(var(--lad-shell-inline-offset, 0px) - 1px);
}
.guide-ladi {
  width: 94px;
  height: 88px;
  padding: 0;
  --uno: position-relative d-grid place-center pointer-events-auto cursor-pointer;
  border: 0;
  border-radius: 28px;
  background: radial-gradient(
    circle,
    color-mix(in srgb, var(--lad-palette-white) 85%, transparent),
    color-mix(in srgb, var(--lad-palette-white) 30%, transparent) 60%,
    transparent 61%
  );
  filter: drop-shadow(
    0 7px 6px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 15%, transparent)
  );
  transition: transform 0.18s ease;
}
.guide-ladi:hover,
.guide-ladi:focus-visible {
  outline: 0;
  transform: translateY(-5px) rotate(-3deg);
}
.guide-ladi:focus-visible {
  box-shadow: 0 0 0 4px
    color-mix(in srgb, var(--lad-palette-teal-550) 20%, transparent);
}
.guide-ladi :deep(.ladi-wrap) {
  grid-area: 1 / 1;
}
.guide-hide {
  width: 25px;
  height: 34px;
  --uno: position-absolute d-grid place-center pointer-events-auto cursor-pointer;
  right: -1px;
  top: 25px;
  z-index: 5;
  color: var(--lad-palette-teal-600);
  border: 2px solid var(--lad-palette-white);
  border-radius: 12px 0 0 12px;
  background: var(--lad-palette-background);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent);
  font-size: rem(23);
  font-weight: var(--lad-font-weight-heavy);
  line-height: 1;
}
.guide-branch {
  width: 58px;
  height: 68px;
  padding: 0;
  --uno: position-relative d-grid place-center pointer-events-auto cursor-pointer overflow-hidden;
  color: var(--lad-palette-teal-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 20%, transparent);
  border-right: 0;
  border-radius: 24px 0 0 24px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent),
    0 10px 20px
      color-mix(in srgb, var(--lad-palette-muted-700) 15%, transparent);
  transition:
    width 0.18s ease,
    background 0.18s ease;
}
.guide-branch:hover,
.guide-branch:focus-visible {
  width: 63px;
  outline: 0;
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-teal-150)
  );
}
.guide-branch img {
  width: 72px;
  height: 72px;
  display: block;
  object-fit: contain;
  transform: translateX(7px);
  filter: drop-shadow(
    0 3px 2px color-mix(in srgb, var(--lad-palette-orange-750) 18%, transparent)
  );
}
.guide-speech {
  width: 280px;
  max-width: calc(100vw - 30px);
  padding: 15px 16px;
  --uno: position-absolute pointer-events-auto;
  right: 70px;
  bottom: 64px;
  color: var(--lad-palette-muted-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 25%, transparent);
  border-radius: 22px 22px 6px 22px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent),
    0 14px 28px
      color-mix(in srgb, var(--lad-palette-muted-700) 15%, transparent);
  font-size: rem(13);
  line-height: 1.52;
}
.guide-speech::after {
  content: "";
  width: 12px;
  height: 12px;
  --uno: position-absolute;
  right: -7px;
  bottom: 12px;
  transform: rotate(45deg);
  border-top: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 20%, transparent);
  border-right: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 20%, transparent);
  background: var(--lad-palette-background);
}
.guide-speech strong,
.guide-speech > span {
  --uno: d-block;
}
.guide-speech strong {
  padding-right: 24px;
  margin-bottom: 5px;
  color: var(--lad-palette-teal-700);
  font-size: rem(15);
  line-height: 1.25;
}
.guide-close {
  width: 26px;
  height: 26px;
  --uno: position-absolute d-grid place-center cursor-pointer;
  top: 7px;
  right: 8px;
  color: var(--lad-palette-teal-600);
  border: 0;
  border-radius: 10px;
  background: color-mix(in srgb, var(--lad-palette-teal-550) 8%, transparent);
  font-size: rem(18);
  line-height: 1;
}
.speech-actions {
  margin-top: 12px;
  padding-top: 10px;
  --uno: d-flex align-center justify-space-between;
  gap: 10px;
  border-top: 1px solid
    color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent);
}
.speech-progress {
  min-width: 42px;
  padding: 6px 9px;
  --uno: text-center;
  color: var(--lad-palette-amber-700);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-amber-150);
  font-size: rem(11);
  font-weight: var(--lad-font-weight-black);
}
.speech-next {
  min-height: 35px;
  padding: 7px 13px;
  --uno: d-flex align-center justify-center cursor-pointer;
  gap: 8px;
  color: var(--lad-palette-white);
  border: 0;
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-mint),
    var(--lad-palette-mint-strong)
  );
  box-shadow:
    0 3px 0 var(--lad-palette-teal-700),
    0 7px 13px color-mix(in srgb, var(--lad-palette-teal-700) 15%, transparent);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-heavy);
}
.speech-next:hover,
.speech-next:focus-visible {
  transform: translateY(-2px);
  outline: 0;
  box-shadow:
    0 5px 0 var(--lad-palette-teal-700),
    0 10px 16px color-mix(in srgb, var(--lad-palette-teal-700) 18%, transparent);
}
.mood-picker {
  margin-top: 9px;
  --uno: d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 5px;
}
.mood-picker button {
  min-width: 0;
  padding: 5px 2px;
  --uno: d-flex flex-column align-center cursor-pointer;
  gap: 2px;
  color: var(--lad-palette-muted);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 15%, transparent);
  border-radius: 11px;
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-strong);
}
.mood-picker button > span {
  font-size: 1rem;
}
.mood-picker button.active {
  color: var(--lad-palette-teal-700);
  border-color: color-mix(
    in srgb,
    var(--lad-palette-teal-550) 35%,
    transparent
  );
  background: var(--lad-palette-background);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent);
}
.mood-happy .guide-speech {
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-150)
  );
}
.mood-happy .guide-speech::after {
  background: var(--lad-palette-amber-100);
}
.mood-gentle .guide-speech {
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
}
.mood-gentle .guide-speech::after {
  background: var(--lad-palette-background);
}
.guide-speech-enter-active,
.guide-speech-leave-active {
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}
.guide-speech-enter-from,
.guide-speech-leave-to {
  opacity: 0;
  transform: translate(8px, 7px) scale(0.94);
}
.guide-ladi.does-wave {
  animation: guide-wave 0.9s ease-in-out;
}
.guide-ladi.does-hop {
  animation: guide-hop 0.9s cubic-bezier(0.2, 0.8, 0.3, 1);
}
.guide-ladi.does-peek {
  animation: guide-peek 0.95s ease-in-out;
}
.guide-ladi.does-emerge {
  animation: guide-emerge 1.2s cubic-bezier(0.18, 0.89, 0.32, 1.28);
}
.guide-ladi.does-highfive {
  animation: guide-highfive 1.05s cubic-bezier(0.2, 0.8, 0.3, 1) 2;
}
.guide-high-five,
.guide-flying-gift {
  --uno: position-absolute pointer-events-none;
  z-index: 8;
}
.guide-high-five {
  top: -5px;
  left: 4px;
  font-size: 2rem;
  transform-origin: bottom right;
  animation: high-five-pop 1.25s ease-out 2;
}
.guide-flying-gift {
  top: 28px;
  left: 20px;
  font-size: 1.75rem;
  filter: drop-shadow(
    0 5px 5px color-mix(in srgb, var(--lad-palette-orange-750) 20%, transparent)
  );
  animation: gift-flight 2.3s cubic-bezier(0.18, 0.78, 0.22, 1) both;
}
@keyframes guide-wave {
  0%,
  100% {
    transform: rotate(0);
  }
  28% {
    transform: translateX(-8px) rotate(-13deg);
  }
  56% {
    transform: translateX(3px) rotate(11deg);
  }
  78% {
    transform: rotate(-6deg);
  }
}
@keyframes guide-hop {
  0%,
  100% {
    transform: translateY(0) scale(1);
  }
  38% {
    transform: translateY(-21px) scale(1.08, 0.92);
  }
  67% {
    transform: translateY(4px) scale(0.93, 1.09);
  }
}
@keyframes guide-peek {
  0%,
  100% {
    transform: translateX(0) rotate(0);
  }
  42% {
    transform: translateX(-24px) rotate(-9deg);
  }
  72% {
    transform: translateX(-10px) rotate(5deg);
  }
}
@keyframes guide-emerge {
  0% {
    opacity: 0;
    transform: translateX(95px) rotate(18deg) scale(0.5);
  }
  58% {
    opacity: 1;
    transform: translateX(-15px) rotate(-10deg) scale(1.08);
  }
  78% {
    transform: translateX(5px) rotate(7deg) scale(0.98);
  }
  100% {
    transform: none;
  }
}
@keyframes guide-highfive {
  0%,
  100% {
    transform: rotate(0) scale(1);
  }
  38% {
    transform: translate(-8px, -10px) rotate(-10deg) scale(1.08);
  }
  68% {
    transform: translate(2px, 2px) rotate(7deg) scale(0.97);
  }
}
@keyframes high-five-pop {
  0%,
  100% {
    opacity: 0;
    transform: translate(12px, 18px) rotate(15deg) scale(0.35);
  }
  28%,
  68% {
    opacity: 1;
    transform: translate(-3px, -5px) rotate(-12deg) scale(1.1);
  }
}
@keyframes gift-flight {
  0% {
    opacity: 0;
    transform: translate(36px, 35px) rotate(18deg) scale(0.35);
  }
  18% {
    opacity: 1;
  }
  72% {
    opacity: 1;
    transform: translate(-150px, -72px) rotate(-10deg) scale(1.05);
  }
  100% {
    opacity: 0;
    transform: translate(-210px, -95px) rotate(-20deg) scale(0.7);
  }
}
@include respond-down(guide) {
  .global-ladi-guide {
    right: 2px;
    bottom: 80px;
  }
  .global-ladi-guide.hidden {
    right: -1px;
  }
}
@include respond-down(compact) {
  .guide-speech {
    width: 240px;
    right: 60px;
  }
}
@include reduced-motion {
  .guide-ladi,
  .guide-ladi.does-wave,
  .guide-ladi.does-hop,
  .guide-ladi.does-peek,
  .guide-ladi.does-emerge,
  .guide-ladi.does-highfive,
  .guide-high-five,
  .guide-flying-gift,
  .guide-speech-enter-active,
  .guide-speech-leave-active {
    animation: none;
    transition: none;
  }
}
</style>
