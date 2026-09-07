<template>
  <aside class="global-ladi-guide" :class="[`mood-${mood}`, { hidden: isHidden, 'gift-celebration': giftCelebration }]" aria-label="Ladi-Begleiter">
    <button v-if="isHidden" class="guide-branch" type="button" aria-label="Ladi wieder hervorholen" @click="revealGuide">
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
          <button v-if="!moodPromptPending" aria-label="Sprechblase schließen" class="guide-close" type="button" @click="closeSpeech">×</button>
          <strong>{{ speechHeading }}</strong>
          <span>{{ speech }}</span>
          <div v-if="speechProgress || speechActionLabel" class="speech-actions">
            <span v-if="speechProgress" class="speech-progress">{{ speechProgress }}</span>
            <button v-if="speechActionLabel" class="speech-next" type="button" @click="triggerSpeechAction">
              {{ speechActionLabel }}
              <span aria-hidden="true">→</span>
            </button>
          </div>
          <div v-if="choosingMood" class="mood-picker" aria-label="Stimmung auswählen">
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

      <button class="guide-hide" type="button" aria-label="Ladi am Bildschirmrand verstecken" @click="hideGuide">›</button>
      <span v-if="giftCelebration" class="guide-high-five" aria-hidden="true">✋</span>
      <span v-if="giftCelebration" class="guide-flying-gift" aria-hidden="true">🎁</span>
      <button class="guide-ladi" :class="randomMotion" type="button" :aria-expanded="Boolean(speech)" aria-label="Seitenerklärung von Ladi öffnen" @click="speakCurrentPageIntro">
        <LadiMascot :score="ladiScore" :show-coin="false" :show-scene-base="false" :show-score="false" :smart="store.piggyBankOpen || isSmart" :size="80" />
      </button>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

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

const store = useFamilyWorldStore();
const route = useRoute();
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
const pageIntroHeading = ref('Ladi sagt');
const pageIntroMessage = ref('Ich bin da, wenn du einen Tipp brauchst.');
let speechTimer: number | undefined;
let motionTimer: number | undefined;
let initialMoodTimer: number | undefined;
let celebrationTimer: number | undefined;

const moodOptions: ReadonlyArray<{ id: GuideMood; icon: string; label: string }> = [
  { id: 'gentle', icon: '😌', label: 'Ruhig' },
  { id: 'calm', icon: '🙂', label: 'Okay' },
  { id: 'happy', icon: '😄', label: 'Fröhlich' },
];
const pageMessages: Record<string, { heading: string; message: string }> = {
  '/': { heading: 'Eure Familienwelt', message: 'Hier lebt eure Familienwelt. Tippe auf Haus, Garten oder eine Anzeige, wenn du mehr wissen möchtest.' },
  '/beitraege': { heading: 'Beiträge', message: 'Grundbeiträge versorgen eure Familienwelt. Freiwillige Zusatzbeiträge bringen dir zusätzliche Ladirchen.' },
  '/wuensche': { heading: 'Clever sparen', message: 'Hier sammelst du Ladirchen für deine Wünsche oder hilfst bei sichtbaren Zielen deiner Familie mit. Unter Ladis Fleiß-Bonus zeige ich dir, wie dein Einsatz zusätzliche Ladirchen für deine Ziele bringt.' },
  '/shop': { heading: 'Shop', message: 'Im Familien-Shop findest du echte Belohnungen, im Hauskatalog virtuelle Dekorationen.' },
  '/familie': { heading: 'Meine Familie', message: 'Hier siehst du Beiträge, Tagesserien und sichtbare Ziele deiner ganzen Familie auf einen Blick.' },
  '/ich': { heading: 'Ich', message: 'Das ist dein Bereich. Hier kannst du dein Profil, deine Einstellungen und deinen Ladi-Fortschritt ansehen.' },
};

const ladiScore = computed(() => mood.value === 'gentle' ? 2.8 : mood.value === 'happy' ? 4.6 : 3.7);
const speechHeading = computed(() => customHeading.value || (mood.value === 'gentle' ? 'Ganz in Ruhe' : 'Ladi sagt'));
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
    showSpeech('Hier siehst du, welche Ladirchen frei sind, welche schon für Wünsche sparen und wie du sie verschieben oder verschenken kannst.', 'Dein Guthaben', true);
    return;
  }
  const fallback = pageMessages[route.path] ?? { heading: 'Ladi sagt', message: 'Ich bin da, wenn du einen Tipp brauchst.' };
  if (route.path === '/wuensche') {
    showSpeech(
      pageIntroMessage.value || fallback.message,
      pageIntroHeading.value || fallback.heading,
      true,
      { progress: '1 / 6', actionLabel: 'Weiter', actionEvent: 'savings-interest:start' },
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
  customHeading.value = nextMood === 'happy' ? 'Juhu!' : nextMood === 'gentle' ? 'Ganz in Ruhe' : 'Alles klar';
  speech.value = nextMood === 'happy'
    ? 'Deine gute Laune steckt mich an! Wir schaffen das zusammen.'
    : nextMood === 'gentle'
      ? 'Heute machen wir alles Schritt für Schritt. Du musst dich nicht beeilen.'
      : 'Ich bleibe an deiner Seite und helfe dir bei den nächsten Schritten.';
  clearSpeechTimer();
  speechTimer = window.setTimeout(speakCurrentPageIntro, 1700);
};
const handleGuideEvent = (event: Event) => {
  const detail = (event as CustomEvent<LadiGuideEventDetail>).detail;
  if (!detail?.message) return;
  if (detail.pageIntro) {
    pageIntroHeading.value = detail.heading || 'Ladi sagt';
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
  customHeading.value = 'Da bin ich!';
  speech.value = mood.value === 'gentle' ? 'Ganz langsam – ich bin wieder bei dir.' : 'Hallo! Ich helfe dir gern weiter.';
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
  const intro = pageMessages[route.path] ?? { heading: 'Ladi sagt', message: 'Ich bin da, wenn du einen Tipp brauchst.' };
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
    showSpeech('Hier siehst du, welche Ladirchen frei sind, welche bereits sparen und wohin du sie schicken kannst.', 'Dein Guthaben', true);
  }
});
onMounted(() => {
  loadMood();
  isHidden.value = localStorage.getItem(hiddenStorageKey.value) === 'true';
  const intro = pageMessages[route.path] ?? { heading: 'Ladi sagt', message: 'Ich bin da, wenn du einen Tipp brauchst.' };
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
      customHeading.value = 'Wie geht es dir?';
      speech.value = 'Wähle einmal aus, wie du dich heute fühlst. Ich passe mich dann an deine Stimmung an.';
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

<style scoped>
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
    rgba(255, 255, 255, 0.86),
    rgba(255, 255, 255, 0.28) 60%,
    transparent 61%
  );
  filter: drop-shadow(0 7px 6px rgba(50, 71, 61, 0.16));
  transition: transform 0.18s ease;
}
.guide-ladi:hover,
.guide-ladi:focus-visible {
  outline: 0;
  transform: translateY(-5px) rotate(-3deg);
}
.guide-ladi:focus-visible {
  box-shadow: 0 0 0 4px rgba(64, 166, 128, 0.22);
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
  color: #53746a;
  border: 2px solid #fff;
  border-radius: 12px 0 0 12px;
  background: #e8f7f0;
  box-shadow: 0 4px 0 rgba(49, 139, 105, 0.15);
  font-size: 23px;
  font-weight: 900;
  line-height: 1;
}
.guide-branch {
  width: 58px;
  height: 68px;
  padding: 0;
  @apply position-relative d-grid place-center pointer-events-auto cursor-pointer overflow-hidden;
  color: #286f59;
  border: 2px solid rgba(64, 164, 126, 0.2);
  border-right: 0;
  border-radius: 24px 0 0 24px;
  background:
    radial-gradient(
      circle at 18% 20%,
      rgba(255, 220, 103, 0.4),
      transparent 29%
    ),
    linear-gradient(145deg, #f9fffc, #dcf5e9);
  box-shadow:
    0 5px 0 rgba(49, 139, 105, 0.14),
    0 10px 20px rgba(49, 93, 76, 0.14);
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
      rgba(255, 220, 103, 0.55),
      transparent 30%
    ),
    linear-gradient(145deg, #fff, #cef0df);
}
.guide-branch svg {
  width: 58px;
  height: 68px;
  overflow: visible;
}
.branch-vine {
  fill: none;
  stroke: #3d8d69;
  stroke-linecap: round;
  stroke-width: 5;
}
.branch-leaf {
  fill: #62bd83;
  stroke: #fff;
  stroke-width: 2;
  transform-box: fill-box;
  transform-origin: center;
}
.branch-leaf--top {
  transform: rotate(-31deg);
}
.branch-leaf--side {
  fill: #8bce8e;
  transform: rotate(24deg);
}
.branch-charm {
  transform-box: fill-box;
  transform-origin: 28px 27px;
  animation: branch-charm-swing 2.7s ease-in-out infinite;
}
.branch-charm path {
  fill: none;
  stroke: #377f62;
  stroke-linecap: round;
  stroke-width: 2.5;
}
.branch-charm circle {
  fill: #ffd257;
  stroke: #fff5bd;
  stroke-width: 3;
  filter: drop-shadow(0 3px 1px rgba(159, 103, 20, 0.25));
}
.branch-charm text {
  fill: #82540b;
  font-size: 14px;
  font-weight: 950;
  text-anchor: middle;
}
.branch-spark {
  fill: #f0b43b;
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
  color: #405d55;
  border: 2px solid rgba(61, 157, 121, 0.24);
  border-radius: 22px 22px 6px 22px;
  background: linear-gradient(145deg, #fff, #eefaf5);
  box-shadow:
    0 6px 0 rgba(49, 139, 105, 0.13),
    0 14px 28px rgba(52, 91, 77, 0.15);
  font-size: 13px;
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
  border-top: 2px solid rgba(61, 157, 121, 0.22);
  border-right: 2px solid rgba(61, 157, 121, 0.22);
  background: #f1faf6;
}
.guide-speech strong,
.guide-speech > span {
  @apply d-block;
}
.guide-speech strong {
  padding-right: 24px;
  margin-bottom: 5px;
  color: #237257;
  font-size: 15px;
  line-height: 1.25;
}
.guide-close {
  width: 26px;
  height: 26px;
  @apply position-absolute d-grid place-center cursor-pointer;
  top: 7px;
  right: 8px;
  color: #59726a;
  border: 0;
  border-radius: 10px;
  background: rgba(83, 145, 122, 0.09);
  font-size: 18px;
  line-height: 1;
}
.speech-actions {
  margin-top: 12px;
  padding-top: 10px;
  @apply d-flex align-center justify-space-between;
  gap: 10px;
  border-top: 1px solid rgba(48, 142, 105, 0.14);
}
.speech-progress {
  min-width: 42px;
  padding: 6px 9px;
  @apply text-center;
  color: #806019;
  border-radius: 999px;
  background: #fff0b9;
  font-size: 11px;
  font-weight: 950;
}
.speech-next {
  min-height: 35px;
  padding: 7px 13px;
  @apply d-flex align-center justify-center cursor-pointer;
  gap: 8px;
  color: #fff;
  border: 0;
  border-radius: 12px;
  background: linear-gradient(145deg, #45c397, #279a73);
  box-shadow:
    0 3px 0 #1e7558,
    0 7px 13px rgba(35, 125, 93, 0.16);
  font-size: 12px;
  font-weight: 900;
}
.speech-next:hover,
.speech-next:focus-visible {
  transform: translateY(-2px);
  outline: 0;
  box-shadow:
    0 5px 0 #1e7558,
    0 10px 16px rgba(35, 125, 93, 0.18);
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
  color: #64766f;
  border: 1px solid rgba(68, 145, 116, 0.15);
  border-radius: 11px;
  background: rgba(255, 255, 255, 0.75);
  font-size: 8px;
  font-weight: 850;
}
.mood-picker button > span {
  font-size: 16px;
}
.mood-picker button.active {
  color: #237257;
  border-color: rgba(48, 159, 116, 0.35);
  background: #e6f8ef;
  box-shadow: 0 3px 0 rgba(48, 142, 105, 0.12);
}
.mood-happy .guide-speech {
  background: linear-gradient(145deg, #fffdf0, #fff1bf);
}
.mood-happy .guide-speech::after {
  background: #fff6d2;
}
.mood-gentle .guide-speech {
  background: linear-gradient(145deg, #f7fbff, #eef5ff);
}
.mood-gentle .guide-speech::after {
  background: #f0f7ff;
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
  font-size: 32px;
  transform-origin: bottom right;
  animation: high-five-pop 1.25s ease-out 2;
}
.guide-flying-gift {
  top: 28px;
  left: 20px;
  font-size: 28px;
  filter: drop-shadow(0 5px 5px rgba(91, 70, 35, 0.2));
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
@media (max-width: 532px) {
  .global-ladi-guide {
    right: 2px;
    bottom: 80px;
  }
  .global-ladi-guide.hidden {
    right: -1px;
  }
}
@media (max-width: 380px) {
  .guide-speech {
    width: 240px;
    right: 60px;
  }
}
@media (prefers-reduced-motion: reduce) {
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
