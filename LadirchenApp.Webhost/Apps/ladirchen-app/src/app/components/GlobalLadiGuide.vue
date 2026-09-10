<template>
  <aside class="global-ladi-guide" :class="[`mood-${mood}`, { hidden: isHidden, 'gift-celebration': giftCelebration }]" :aria-label="t('guide.aria')">
    <button v-if="isHidden" class="guide-branch" type="button" :aria-label="t('guide.reveal')" @click="revealGuide">
      <svg aria-hidden="true" viewBox="0 0 58 68">
        <path class="branch-vine" d="M64 11C48 10 43 17 36 23c-6 6-12 8-20 8" />
        <ellipse class="branch-leaf branch-leaf--top" cx="36" cy="20" rx="10" ry="6" />
        <ellipse class="branch-leaf branch-leaf--side" cx="18" cy="31" rx="10" ry="6" />
        <g class="branch-charm">
          <path d="M28 27v10" />
          <circle cx="28" cy="47" r="11" />
          <text x="28" y="52">L</text>
        </g>
        <path class="branch-spark" d="m10 10 1.8 4 4.2 1.8-4.2 1.8-1.8 4-1.8-4L4 15.8 8.2 14Z" />
      </svg>
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
        <LadiMascot :score="ladiScore" :show-coin="false" :show-scene-base="false" :show-score="false" :smart="store.piggyBankOpen || isSmart" :size="80" />
      </button>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

import LadiMascot from '@/shared/components/LadiMascot.vue';
import { useFamilyWorldStore } from '@/stores/family-world';

type GuideMood = 'gentle' | 'calm' | 'happy';

interface LadiGuideEventDetail {
  heading?: string;
  message: string;
  pageIntro?: boolean;
  smart?: boolean;
  progress?: string;
  actionLabel?: string;
  actionEvent?: string;
  celebration?: 'gift';
}

const isOptionalString = (value: unknown): boolean => value === undefined || typeof value === 'string';
const isOptionalBoolean = (value: unknown): boolean => value === undefined || typeof value === 'boolean';
const isLadiGuideEventDetail = (value: unknown): value is LadiGuideEventDetail => {
  if (typeof value !== 'object' || value === null || !('message' in value)) {return false;}
  return typeof value.message === 'string' && value.message.trim().length > 0 &&
    isOptionalString('heading' in value ? value.heading : undefined) &&
    isOptionalBoolean('pageIntro' in value ? value.pageIntro : undefined) &&
    isOptionalBoolean('smart' in value ? value.smart : undefined) &&
    isOptionalString('progress' in value ? value.progress : undefined) &&
    isOptionalString('actionLabel' in value ? value.actionLabel : undefined) &&
    isOptionalString('actionEvent' in value ? value.actionEvent : undefined) &&
    (!('celebration' in value) || value.celebration === undefined || value.celebration === 'gift');
};

const store = useFamilyWorldStore();
const route = useRoute();
const { t } = useI18n();
const speech = ref('');
const customHeading = ref('');
const choosingMood = ref(false);
const mood = ref<GuideMood>('calm');
const randomMotion = ref('');
const isHidden = ref(false);
const isSmart = ref(false);
const speechProgress = ref('');
const speechActionLabel = ref('');
const speechActionEvent = ref('');
const giftCelebration = ref(false);
const moodPromptPending = ref(true);
const pageIntroHeading = ref(t('guide.defaultHeading'));
const pageIntroMessage = ref(t('guide.defaultMessage'));
let speechTimer: number | undefined;
let motionTimer: number | undefined;
let initialMoodTimer: number | undefined;
let celebrationTimer: number | undefined;

const moodOptions = computed<ReadonlyArray<{ id: GuideMood; icon: string; label: string }>>(() => [
  { id: 'gentle', icon: '😌', label: t('guide.mood.gentle') },
  { id: 'calm', icon: '🙂', label: t('guide.mood.calm') },
  { id: 'happy', icon: '😄', label: t('guide.mood.happy') },
]);
const pageMessages = computed<Record<string, { heading: string; message: string }>>(() => ({
  '/': { heading: t('guide.pages.world.heading'), message: t('guide.pages.world.message') },
  '/beitraege': { heading: t('guide.pages.contributions.heading'), message: t('guide.pages.contributions.message') },
  '/wuensche': { heading: t('guide.pages.wishes.heading'), message: t('guide.pages.wishes.message') },
  '/shop': { heading: t('guide.pages.shop.heading'), message: t('guide.pages.shop.message') },
  '/familie': { heading: t('guide.pages.family.heading'), message: t('guide.pages.family.message') },
  '/ich': { heading: t('guide.pages.profile.heading'), message: t('guide.pages.profile.message') },
}));
const fallbackPageMessage = () => ({ heading: t('guide.defaultHeading'), message: t('guide.defaultMessage') });

const ladiScore = computed(() => mood.value === 'gentle' ? 2.8 : mood.value === 'happy' ? 4.6 : 3.7);
const speechHeading = computed(() => customHeading.value || (mood.value === 'gentle' ? t('guide.gentleHeading') : t('guide.defaultHeading')));
const moodStorageKey = computed(() => `ladirchen:guide-mood:${store.activeChildId}`);
const hiddenStorageKey = computed(() => `ladirchen:guide-hidden:${store.activeChildId}`);

const clearSpeechTimer = () => {
  if (speechTimer !== undefined) window.clearTimeout(speechTimer);
  speechTimer = undefined;
};
const closeSpeech = () => {
  speech.value = '';
  isSmart.value = false;
  speechProgress.value = '';
  speechActionLabel.value = '';
  speechActionEvent.value = '';
};
const showSpeech = (message: string, heading = '', smart = false, detail?: Pick<LadiGuideEventDetail, 'progress' | 'actionLabel' | 'actionEvent' | 'celebration'>) => {
  clearSpeechTimer();
  choosingMood.value = false;
  isSmart.value = smart;
  speechProgress.value = detail?.progress || '';
  speechActionLabel.value = detail?.actionLabel || '';
  speechActionEvent.value = detail?.actionEvent || '';
  customHeading.value = heading;
  speech.value = message;
  if (detail?.celebration === 'gift') {
    giftCelebration.value = true;
    randomMotion.value = 'does-highfive';
    if (celebrationTimer !== undefined) window.clearTimeout(celebrationTimer);
    celebrationTimer = window.setTimeout(() => {
      giftCelebration.value = false;
      randomMotion.value = '';
    }, 2400);
  }
  if (!smart) speechTimer = window.setTimeout(closeSpeech, 11000);
};
const triggerSpeechAction = () => {
  if (!speechActionEvent.value) return;
  window.dispatchEvent(new CustomEvent(speechActionEvent.value));
};
const speakCurrentPageIntro = () => {
  if (choosingMood.value) return;
  if (store.piggyBankOpen) {
    showSpeech(t('guide.balance.message'), t('guide.balance.heading'), true);
    return;
  }
  const fallback = pageMessages.value[route.path] ?? fallbackPageMessage();
  if (route.path === '/wuensche') {
    showSpeech(
      pageIntroMessage.value || fallback.message,
      pageIntroHeading.value || fallback.heading,
      true,
      { progress: '1 / 6', actionLabel: t('common.next'), actionEvent: 'savings-interest:start' },
    );
    return;
  }
  showSpeech(pageIntroMessage.value || fallback.message, pageIntroHeading.value || fallback.heading);
};
const selectMood = (nextMood: GuideMood) => {
  mood.value = nextMood;
  localStorage.setItem(moodStorageKey.value, nextMood);
  choosingMood.value = false;
  moodPromptPending.value = false;
  customHeading.value = nextMood === 'happy' ? t('guide.mood.happyHeading') : nextMood === 'gentle' ? t('guide.gentleHeading') : t('guide.mood.calmHeading');
  speech.value = nextMood === 'happy'
    ? t('guide.mood.happyMessage')
    : nextMood === 'gentle'
      ? t('guide.mood.gentleMessage')
      : t('guide.mood.calmMessage');
  clearSpeechTimer();
  speechTimer = window.setTimeout(speakCurrentPageIntro, 1700);
};
const handleGuideEvent = (event: Event) => {
  if (!(event instanceof CustomEvent) || !isLadiGuideEventDetail(event.detail)) {return;}
  const detail = event.detail;
  if (detail.pageIntro) {
    pageIntroHeading.value = detail.heading || t('guide.defaultHeading');
    pageIntroMessage.value = detail.message;
    if (choosingMood.value) return;
  }
  if (moodPromptPending.value) return;
  showSpeech(detail.message, detail.heading, detail.smart, detail);
};
const handleGuidedClick = (event: MouseEvent) => {
  if (moodPromptPending.value) return;
  if (event.target instanceof Element && event.target.closest('[data-ladi-ignore]')) return;
  const target = event.target instanceof Element ? event.target.closest<HTMLElement>('[data-ladi-tip]') : null;
  const message = target?.dataset.ladiTip?.trim();
  if (message) showSpeech(message, target?.dataset.ladiHeading);
};
const loadMood = () => {
  const saved = localStorage.getItem(moodStorageKey.value);
  mood.value = saved === 'gentle' || saved === 'happy' || saved === 'calm' ? saved : 'calm';
};
const hideGuide = () => {
  closeSpeech();
  choosingMood.value = false;
  isHidden.value = true;
  localStorage.setItem(hiddenStorageKey.value, 'true');
};
const revealGuide = () => {
  isHidden.value = false;
  localStorage.removeItem(hiddenStorageKey.value);
  randomMotion.value = 'does-emerge';
  customHeading.value = t('guide.welcomeBack.heading');
  speech.value = mood.value === 'gentle' ? t('guide.welcomeBack.gentle') : t('guide.welcomeBack.message');
  if (motionTimer !== undefined) window.clearTimeout(motionTimer);
  motionTimer = window.setTimeout(() => {
    randomMotion.value = '';
    scheduleRandomMotion();
  }, 1250);
};
const scheduleRandomMotion = () => {
  motionTimer = window.setTimeout(() => {
    const motions = ['does-wave', 'does-hop', 'does-peek'] as const;
    randomMotion.value = motions[Math.floor(Math.random() * motions.length)] ?? 'does-wave';
    motionTimer = window.setTimeout(() => {
      randomMotion.value = '';
      scheduleRandomMotion();
    }, 950);
  }, 3200 + Math.round(Math.random() * 4200));
};

watch(() => store.activeChildId, () => {
  loadMood();
  isHidden.value = localStorage.getItem(hiddenStorageKey.value) === 'true';
});
watch(() => route.path, () => {
  closeSpeech();
  if (!moodPromptPending.value) choosingMood.value = false;
  const intro = pageMessages.value[route.path] ?? fallbackPageMessage();
  pageIntroHeading.value = intro.heading;
  pageIntroMessage.value = intro.message;
});
watch(() => store.piggyBankOpen, (isOpen) => {
  if (isOpen && moodPromptPending.value) {
    if (initialMoodTimer !== undefined) window.clearTimeout(initialMoodTimer);
    initialMoodTimer = undefined;
    moodPromptPending.value = false;
  }
  closeSpeech();
  choosingMood.value = false;
  if (isOpen) {
    showSpeech(t('guide.balance.openMessage'), t('guide.balance.heading'), true);
  }
});
onMounted(() => {
  loadMood();
  isHidden.value = localStorage.getItem(hiddenStorageKey.value) === 'true';
  const intro = pageMessages.value[route.path] ?? fallbackPageMessage();
  pageIntroHeading.value = intro.heading;
  pageIntroMessage.value = intro.message;
  window.addEventListener('ladi-guide:say', handleGuideEvent);
  document.addEventListener('click', handleGuidedClick, true);
  scheduleRandomMotion();
  if (!isHidden.value) {
    choosingMood.value = true;
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    initialMoodTimer = window.setTimeout(() => {
      clearSpeechTimer();
      customHeading.value = t('guide.mood.promptHeading');
      speech.value = t('guide.mood.promptMessage');
      initialMoodTimer = undefined;
    }, reduceMotion ? 650 : 2650);
  } else {
    moodPromptPending.value = false;
  }
});
onUnmounted(() => {
  clearSpeechTimer();
  if (motionTimer !== undefined) window.clearTimeout(motionTimer);
  if (initialMoodTimer !== undefined) window.clearTimeout(initialMoodTimer);
  if (celebrationTimer !== undefined) window.clearTimeout(celebrationTimer);
  window.removeEventListener('ladi-guide:say', handleGuideEvent);
  document.removeEventListener('click', handleGuidedClick, true);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.global-ladi-guide {
  @apply position-fixed pointer-events-none;
  right: max(4px, calc((100vw - 500px) / 2 + 4px));
  bottom: 86px;
  z-index: 3000;
}
.global-ladi-guide.hidden {
  right: max(-1px, calc((100vw - 500px) / 2 - 1px));
}
.guide-ladi {
  width: 94px;
  height: 88px;
  padding: 0;
  @apply position-relative d-grid place-center pointer-events-auto cursor-pointer;
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
  @apply position-absolute d-grid place-center pointer-events-auto cursor-pointer;
  right: -1px;
  top: 25px;
  z-index: 5;
  color: var(--lad-palette-teal-600);
  border: 2px solid var(--lad-palette-white);
  border-radius: 12px 0 0 12px;
  background: var(--lad-palette-background);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent);
  font-size: 1.4375rem;
  font-weight: var(--lad-font-weight-heavy);
  line-height: 1;
}
.guide-branch {
  width: 58px;
  height: 68px;
  padding: 0;
  @apply position-relative d-grid place-center pointer-events-auto cursor-pointer overflow-hidden;
  color: var(--lad-palette-teal-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 20%, transparent);
  border-right: 0;
  border-radius: 24px 0 0 24px;
  background:
    radial-gradient(
      circle at 18% 20%,
      color-mix(in srgb, var(--lad-palette-yellow) 40%, transparent),
      transparent 29%
    ),
    linear-gradient(
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
  background:
    radial-gradient(
      circle at 18% 20%,
      color-mix(in srgb, var(--lad-palette-yellow) 60%, transparent),
      transparent 30%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-white),
      var(--lad-palette-teal-150)
    );
}
.guide-branch svg {
  width: 58px;
  height: 68px;
  overflow: visible;
}
.branch-vine {
  fill: none;
  stroke: var(--lad-palette-teal-600);
  stroke-linecap: round;
  stroke-width: 5;
}
.branch-leaf {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-white);
  stroke-width: 2;
  transform-box: fill-box;
  transform-origin: center;
}
.branch-leaf--top {
  transform: rotate(-31deg);
}
.branch-leaf--side {
  fill: var(--lad-palette-green-250);
  transform: rotate(24deg);
}
.branch-charm {
  transform-box: fill-box;
  transform-origin: 28px 27px;
  animation: branch-charm-swing 2.7s ease-in-out infinite;
}
.branch-charm path {
  fill: none;
  stroke: var(--lad-palette-teal-600);
  stroke-linecap: round;
  stroke-width: 2.5;
}
.branch-charm circle {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-150);
  stroke-width: 3;
  filter: drop-shadow(
    0 3px 1px color-mix(in srgb, var(--lad-palette-amber-650) 25%, transparent)
  );
}
.branch-charm text {
  fill: var(--lad-palette-amber-700);
  font-size: 0.875rem;
  font-weight: var(--lad-font-weight-black);
  text-anchor: middle;
}
.branch-spark {
  fill: var(--lad-palette-amber-450);
  transform-box: fill-box;
  transform-origin: center;
  animation: branch-spark-pop 2.2s ease-in-out infinite;
}
.guide-speech {
  width: 280px;
  max-width: calc(100vw - 30px);
  padding: 15px 16px;
  @apply position-absolute pointer-events-auto;
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
  font-size: 0.8125rem;
  line-height: 1.52;
}
.guide-speech::after {
  content: "";
  width: 12px;
  height: 12px;
  @apply position-absolute;
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
  @apply d-block;
}
.guide-speech strong {
  padding-right: 24px;
  margin-bottom: 5px;
  color: var(--lad-palette-teal-700);
  font-size: 0.9375rem;
  line-height: 1.25;
}
.guide-close {
  width: 26px;
  height: 26px;
  @apply position-absolute d-grid place-center cursor-pointer;
  top: 7px;
  right: 8px;
  color: var(--lad-palette-teal-600);
  border: 0;
  border-radius: 10px;
  background: color-mix(in srgb, var(--lad-palette-teal-550) 8%, transparent);
  font-size: 1.125rem;
  line-height: 1;
}
.speech-actions {
  margin-top: 12px;
  padding-top: 10px;
  @apply d-flex align-center justify-space-between;
  gap: 10px;
  border-top: 1px solid
    color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent);
}
.speech-progress {
  min-width: 42px;
  padding: 6px 9px;
  @apply text-center;
  color: var(--lad-palette-amber-700);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-amber-150);
  font-size: 0.6875rem;
  font-weight: var(--lad-font-weight-black);
}
.speech-next {
  min-height: 35px;
  padding: 7px 13px;
  @apply d-flex align-center justify-center cursor-pointer;
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
  @apply d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 5px;
}
.mood-picker button {
  min-width: 0;
  padding: 5px 2px;
  @apply d-flex flex-column align-center cursor-pointer;
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
  @apply position-absolute pointer-events-none;
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
@keyframes branch-charm-swing {
  0%,
  100% {
    transform: rotate(-7deg);
  }
  50% {
    transform: rotate(8deg);
  }
}
@keyframes branch-spark-pop {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.12) rotate(24deg);
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
  .branch-charm,
  .branch-spark,
  .guide-speech-enter-active,
  .guide-speech-leave-active {
    animation: none;
    transition: none;
  }
}
</style>
