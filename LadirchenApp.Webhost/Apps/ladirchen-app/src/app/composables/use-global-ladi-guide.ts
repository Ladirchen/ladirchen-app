import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

import { useFamilyWorldStore } from '@/stores/family-world';
import { browserClientStorage } from '@/infrastructure/storage/browser-client-storage';
import { ladiGuideController } from '@/shared/services/ladi-guide-controller';
import type { LadiGuideActionId, LadiGuideMessage } from '@/shared/services/ladi-guide-controller';

type GuideMood = 'gentle' | 'calm' | 'happy';

const GUIDE_TIMING = Object.freeze({
  celebrationDuration: 2400,
  defaultSpeechDuration: 11_000,
  emergeDuration: 1250,
  moodSelectionDelay: 1700,
  motionDuration: 950,
  randomMotionBaseDelay: 3200,
  randomMotionDelayVariance: 4200,
  reducedMotionPromptDelay: 650,
  standardPromptDelay: 2650,
});

export const useGlobalLadiGuide = () => {
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
const speechActionId = ref<LadiGuideActionId>();
const giftCelebration = ref(false);
const moodPromptPending = ref(true);
const pageIntroHeading = ref(t('guide.defaultHeading'));
const pageIntroMessage = ref(t('guide.defaultMessage'));
let speechTimer: number | undefined;
let motionTimer: number | undefined;
let initialMoodTimer: number | undefined;
let celebrationTimer: number | undefined;
let unsubscribeGuide: (() => void) | undefined;

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
  if (speechTimer !== undefined) {window.clearTimeout(speechTimer);}
  speechTimer = undefined;
};
const closeSpeech = () => {
  speech.value = '';
  isSmart.value = false;
  speechProgress.value = '';
  speechActionLabel.value = '';
  speechActionId.value = undefined;
};
const showSpeech = (message: string, heading = '', smart = false, detail?: Pick<LadiGuideMessage, 'progress' | 'actionLabel' | 'actionId' | 'celebration'>) => {
  clearSpeechTimer();
  choosingMood.value = false;
  isSmart.value = smart;
  speechProgress.value = detail?.progress || '';
  speechActionLabel.value = detail?.actionLabel || '';
  speechActionId.value = detail?.actionId;
  customHeading.value = heading;
  speech.value = message;
  if (detail?.celebration === 'gift') {
    giftCelebration.value = true;
    randomMotion.value = 'does-highfive';
    if (celebrationTimer !== undefined) {window.clearTimeout(celebrationTimer);}
    celebrationTimer = window.setTimeout(() => {
      giftCelebration.value = false;
      randomMotion.value = '';
    }, GUIDE_TIMING.celebrationDuration);
  }
  if (!smart) {speechTimer = window.setTimeout(closeSpeech, GUIDE_TIMING.defaultSpeechDuration);}
};
const triggerSpeechAction = () => {
  if (!speechActionId.value) {return;}
  ladiGuideController.trigger(speechActionId.value);
};
const speakCurrentPageIntro = () => {
  if (choosingMood.value) {return;}
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
      { progress: '1 / 6', actionLabel: t('common.next'), actionId: 'savings-interest:start' },
    );
    return;
  }
  showSpeech(pageIntroMessage.value || fallback.message, pageIntroHeading.value || fallback.heading);
};
const selectMood = (nextMood: GuideMood) => {
  mood.value = nextMood;
  browserClientStorage.setItem(moodStorageKey.value, nextMood);
  choosingMood.value = false;
  moodPromptPending.value = false;
  customHeading.value = nextMood === 'happy' ? t('guide.mood.happyHeading') : nextMood === 'gentle' ? t('guide.gentleHeading') : t('guide.mood.calmHeading');
  speech.value = nextMood === 'happy'
    ? t('guide.mood.happyMessage')
    : nextMood === 'gentle'
      ? t('guide.mood.gentleMessage')
      : t('guide.mood.calmMessage');
  clearSpeechTimer();
  speechTimer = window.setTimeout(speakCurrentPageIntro, GUIDE_TIMING.moodSelectionDelay);
};
const handleGuideMessage = (detail: LadiGuideMessage) => {
  if (detail.pageIntro) {
    pageIntroHeading.value = detail.heading || t('guide.defaultHeading');
    pageIntroMessage.value = detail.message;
    if (choosingMood.value) {return;}
  }
  if (moodPromptPending.value) {return;}
  showSpeech(detail.message, detail.heading, detail.smart, detail);
};
const handleGuidedClick = (event: MouseEvent) => {
  if (moodPromptPending.value) {return;}
  if (event.target instanceof Element && event.target.closest('[data-ladi-ignore]')) {return;}
  const target = event.target instanceof Element ? event.target.closest<HTMLElement>('[data-ladi-tip]') : null;
  const message = target?.dataset.ladiTip?.trim();
  if (message) {showSpeech(message, target?.dataset.ladiHeading);}
};
const loadMood = () => {
  const saved = browserClientStorage.getItem(moodStorageKey.value);
  mood.value = saved === 'gentle' || saved === 'happy' || saved === 'calm' ? saved : 'calm';
};
const hideGuide = () => {
  closeSpeech();
  choosingMood.value = false;
  isHidden.value = true;
  browserClientStorage.setItem(hiddenStorageKey.value, 'true');
};
const revealGuide = () => {
  isHidden.value = false;
  browserClientStorage.removeItem(hiddenStorageKey.value);
  randomMotion.value = 'does-emerge';
  customHeading.value = t('guide.welcomeBack.heading');
  speech.value = mood.value === 'gentle' ? t('guide.welcomeBack.gentle') : t('guide.welcomeBack.message');
  if (motionTimer !== undefined) {window.clearTimeout(motionTimer);}
  motionTimer = window.setTimeout(() => {
    randomMotion.value = '';
    scheduleRandomMotion();
  }, GUIDE_TIMING.emergeDuration);
};
const scheduleRandomMotion = () => {
  motionTimer = window.setTimeout(() => {
    const motions = ['does-wave', 'does-hop', 'does-peek'] as const;
    randomMotion.value = motions[Math.floor(Math.random() * motions.length)] ?? 'does-wave';
    motionTimer = window.setTimeout(() => {
      randomMotion.value = '';
      scheduleRandomMotion();
    }, GUIDE_TIMING.motionDuration);
  }, GUIDE_TIMING.randomMotionBaseDelay + Math.round(Math.random() * GUIDE_TIMING.randomMotionDelayVariance));
};

watch(() => store.activeChildId, () => {
  loadMood();
  isHidden.value = browserClientStorage.getItem(hiddenStorageKey.value) === 'true';
});
watch(() => route.path, () => {
  closeSpeech();
  if (!moodPromptPending.value) {choosingMood.value = false;}
  const intro = pageMessages.value[route.path] ?? fallbackPageMessage();
  pageIntroHeading.value = intro.heading;
  pageIntroMessage.value = intro.message;
});
watch(() => store.piggyBankOpen, (isOpen) => {
  if (isOpen && moodPromptPending.value) {
    if (initialMoodTimer !== undefined) {window.clearTimeout(initialMoodTimer);}
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
  isHidden.value = browserClientStorage.getItem(hiddenStorageKey.value) === 'true';
  const intro = pageMessages.value[route.path] ?? fallbackPageMessage();
  pageIntroHeading.value = intro.heading;
  pageIntroMessage.value = intro.message;
  unsubscribeGuide = ladiGuideController.subscribe(handleGuideMessage);
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
    }, reduceMotion ? GUIDE_TIMING.reducedMotionPromptDelay : GUIDE_TIMING.standardPromptDelay);
  } else {
    moodPromptPending.value = false;
  }
});
onUnmounted(() => {
  clearSpeechTimer();
  if (motionTimer !== undefined) {window.clearTimeout(motionTimer);}
  if (initialMoodTimer !== undefined) {window.clearTimeout(initialMoodTimer);}
  if (celebrationTimer !== undefined) {window.clearTimeout(celebrationTimer);}
  unsubscribeGuide?.();
  document.removeEventListener('click', handleGuidedClick, true);
});
  return {
    choosingMood,
    closeSpeech,
    giftCelebration,
    hideGuide,
    isHidden,
    isSmart,
    ladiScore,
    mood,
    moodOptions,
    moodPromptPending,
    randomMotion,
    revealGuide,
    selectMood,
    speakCurrentPageIntro,
    speech,
    speechActionLabel,
    speechHeading,
    speechProgress,
    store,
    triggerSpeechAction,
  };
};
