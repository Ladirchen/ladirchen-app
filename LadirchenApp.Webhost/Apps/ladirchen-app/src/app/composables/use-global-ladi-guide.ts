import { useStorage } from "@vueuse/core";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";

import { useFamilyWorldStore } from "@/stores/family-world";
import { browserClientStorage } from "@/infrastructure/storage/browser-client-storage";
import type { TranslationKey } from "@/locales/translation-keys";
import { ladiGuideController } from "@/shared/services/ladi-guide-controller";
import type { LadiGuideActionId, LadiGuideMessage } from "@/shared/services/ladi-guide-controller";
import {
  LADI_GUIDE_CELEBRATION_DURATION_MS,
  LADI_GUIDE_DEFAULT_SPEECH_DURATION_MS,
  LADI_GUIDE_EMERGE_DURATION_MS,
  LADI_GUIDE_MOOD_SELECTION_DELAY_MS,
  LADI_GUIDE_MOTION_DURATION_MS,
  LADI_GUIDE_RANDOM_MOTION_BASE_DELAY_MS,
  LADI_GUIDE_RANDOM_MOTION_DELAY_VARIANCE_MS,
  LADI_GUIDE_REDUCED_MOTION_PROMPT_DELAY_MS,
  LADI_GUIDE_STANDARD_PROMPT_DELAY_MS,
} from "@/shared/runtime-timing";

type GuideMood = "gentle" | "calm" | "happy";

export const useGlobalLadiGuide = () => {
  const store = useFamilyWorldStore();
  const route = useRoute();
  const { t } = useI18n();
  const moodStorageKey = computed(() => `ladirchen:guide-mood:${store.activeChildId}`);
  const hiddenStorageKey = computed(() => `ladirchen:guide-hidden:${store.activeChildId}`);
  const speech = ref("");
  const customHeading = ref("");
  const choosingMood = ref(false);
  const mood = useStorage<GuideMood>(moodStorageKey, "calm", browserClientStorage, {
    serializer: {
      read(value) {
        return value === "gentle" || value === "happy" || value === "calm" ? value : "calm";
      },
      write(value) {
        return value;
      },
    },
    writeDefaults: false,
  });
  const hiddenPreference = useStorage<boolean | null>(hiddenStorageKey, null, browserClientStorage, {
    serializer: {
      read(value) {
        return value === "true" ? true : null;
      },
      write(value) {
        return value ? "true" : "false";
      },
    },
    writeDefaults: false,
  });
  const randomMotion = ref("");
  const isHidden = computed({
    get: () => hiddenPreference.value === true,
    set: (value) => {
      hiddenPreference.value = value ? true : null;
    },
  });
  const isSmart = ref(false);
  const speechProgress = ref("");
  const speechActionLabel = ref("");
  const speechActionId = ref<LadiGuideActionId>();
  const giftCelebration = ref(false);
  const moodPromptPending = ref(true);
  const pageIntroHeading = ref(t("guide.defaultHeading"));
  const pageIntroMessage = ref(t("guide.defaultMessage"));
  let speechTimer: number | undefined;
  let motionTimer: number | undefined;
  let initialMoodTimer: number | undefined;
  let celebrationTimer: number | undefined;
  let unsubscribeGuide: (() => void) | undefined;

  const moodOptions = computed<ReadonlyArray<{ id: GuideMood; icon: string; label: string }>>(() => [
    { id: "gentle", icon: "😌", label: t("guide.mood.gentle") },
    { id: "calm", icon: "🙂", label: t("guide.mood.calm") },
    { id: "happy", icon: "😄", label: t("guide.mood.happy") },
  ]);
  const pageMessages = computed<Record<string, { heading: string; message: string }>>(() => ({
    "/": { heading: t("guide.pages.world.heading"), message: t("guide.pages.world.message") },
    "/contributions": {
      heading: t("guide.pages.contributions.heading"),
      message: t("guide.pages.contributions.message"),
    },
    "/wishes": { heading: t("guide.pages.wishes.heading"), message: t("guide.pages.wishes.message") },
    "/shop": { heading: t("guide.pages.shop.heading"), message: t("guide.pages.shop.message") },
    "/family": { heading: t("guide.pages.family.heading"), message: t("guide.pages.family.message") },
    "/profile": { heading: t("guide.pages.profile.heading"), message: t("guide.pages.profile.message") },
  }));
  const fallbackPageMessage = () => ({ heading: t("guide.defaultHeading"), message: t("guide.defaultMessage") });

  const ladiScores: Record<GuideMood, number> = { calm: 3.7, gentle: 2.8, happy: 4.6 };
  const ladiScore = computed(() => ladiScores[mood.value]);
  const speechHeading = computed(
    () => customHeading.value || (mood.value === "gentle" ? t("guide.gentleHeading") : t("guide.defaultHeading")),
  );
  const clearSpeechTimer = () => {
    if (speechTimer !== undefined) {
      window.clearTimeout(speechTimer);
    }
    speechTimer = undefined;
  };
  const closeSpeech = () => {
    speech.value = "";
    isSmart.value = false;
    speechProgress.value = "";
    speechActionLabel.value = "";
    speechActionId.value = undefined;
  };
  const showSpeech = (
    message: string,
    heading = "",
    smart = false,
    detail?: Pick<LadiGuideMessage, "progress" | "actionLabel" | "actionId" | "celebration">,
  ) => {
    clearSpeechTimer();
    choosingMood.value = false;
    isSmart.value = smart;
    speechProgress.value = detail?.progress || "";
    speechActionLabel.value = detail?.actionLabel || "";
    speechActionId.value = detail?.actionId;
    customHeading.value = heading;
    speech.value = message;
    if (detail?.celebration === "gift") {
      giftCelebration.value = true;
      randomMotion.value = "does-highfive";
      if (celebrationTimer !== undefined) {
        window.clearTimeout(celebrationTimer);
      }
      celebrationTimer = window.setTimeout(() => {
        giftCelebration.value = false;
        randomMotion.value = "";
      }, LADI_GUIDE_CELEBRATION_DURATION_MS);
    }
    if (!smart) {
      speechTimer = window.setTimeout(closeSpeech, LADI_GUIDE_DEFAULT_SPEECH_DURATION_MS);
    }
  };
  const triggerSpeechAction = () => {
    if (!speechActionId.value) {
      return;
    }
    ladiGuideController.trigger(speechActionId.value);
  };
  const speakCurrentPageIntro = () => {
    if (choosingMood.value) {
      return;
    }
    if (store.piggyBankOpen) {
      showSpeech(t("guide.balance.message"), t("guide.balance.heading"), true);
      return;
    }
    const fallback = pageMessages.value[route.path] ?? fallbackPageMessage();
    if (route.path === "/wishes") {
      showSpeech(pageIntroMessage.value || fallback.message, pageIntroHeading.value || fallback.heading, true, {
        progress: "1 / 6",
        actionLabel: t("common.next"),
        actionId: "savings-interest:start",
      });
      return;
    }
    showSpeech(pageIntroMessage.value || fallback.message, pageIntroHeading.value || fallback.heading);
  };
  const selectMood = (nextMood: GuideMood) => {
    mood.value = nextMood;
    choosingMood.value = false;
    moodPromptPending.value = false;
    const moodCopyKeys: Record<GuideMood, { heading: TranslationKey; message: TranslationKey }> = {
      calm: { heading: "guide.mood.calmHeading", message: "guide.mood.calmMessage" },
      gentle: { heading: "guide.gentleHeading", message: "guide.mood.gentleMessage" },
      happy: { heading: "guide.mood.happyHeading", message: "guide.mood.happyMessage" },
    };
    const moodCopy = moodCopyKeys[nextMood];
    customHeading.value = t(moodCopy.heading);
    speech.value = t(moodCopy.message);
    clearSpeechTimer();
    speechTimer = window.setTimeout(speakCurrentPageIntro, LADI_GUIDE_MOOD_SELECTION_DELAY_MS);
  };
  const handleGuideMessage = (detail: LadiGuideMessage) => {
    if (detail.pageIntro) {
      pageIntroHeading.value = detail.heading || t("guide.defaultHeading");
      pageIntroMessage.value = detail.message;
      if (choosingMood.value) {
        return;
      }
    }
    if (moodPromptPending.value) {
      return;
    }
    showSpeech(detail.message, detail.heading, detail.smart, detail);
  };
  const handleGuidedClick = (event: MouseEvent) => {
    if (moodPromptPending.value) {
      return;
    }
    if (event.target instanceof Element && event.target.closest("[data-ladi-ignore]")) {
      return;
    }
    const target = event.target instanceof Element ? event.target.closest<HTMLElement>("[data-ladi-tip]") : null;
    const message = target?.dataset.ladiTip?.trim();
    if (message) {
      showSpeech(message, target?.dataset.ladiHeading);
    }
  };
  const hideGuide = () => {
    closeSpeech();
    choosingMood.value = false;
    isHidden.value = true;
  };
  const revealGuide = () => {
    isHidden.value = false;
    randomMotion.value = "does-emerge";
    customHeading.value = t("guide.welcomeBack.heading");
    speech.value = mood.value === "gentle" ? t("guide.welcomeBack.gentle") : t("guide.welcomeBack.message");
    if (motionTimer !== undefined) {
      window.clearTimeout(motionTimer);
    }
    motionTimer = window.setTimeout(() => {
      randomMotion.value = "";
      scheduleRandomMotion();
    }, LADI_GUIDE_EMERGE_DURATION_MS);
  };
  const scheduleRandomMotion = () => {
    motionTimer = window.setTimeout(
      () => {
        const motions = ["does-wave", "does-hop", "does-peek"] as const;
        randomMotion.value = motions[Math.floor(Math.random() * motions.length)] ?? "does-wave";
        motionTimer = window.setTimeout(() => {
          randomMotion.value = "";
          scheduleRandomMotion();
        }, LADI_GUIDE_MOTION_DURATION_MS);
      },
      LADI_GUIDE_RANDOM_MOTION_BASE_DELAY_MS + Math.round(Math.random() * LADI_GUIDE_RANDOM_MOTION_DELAY_VARIANCE_MS),
    );
  };

  watch(
    () => route.path,
    () => {
      closeSpeech();
      if (!moodPromptPending.value) {
        choosingMood.value = false;
      }
      const intro = pageMessages.value[route.path] ?? fallbackPageMessage();
      pageIntroHeading.value = intro.heading;
      pageIntroMessage.value = intro.message;
    },
  );
  watch(
    () => store.piggyBankOpen,
    (isOpen) => {
      if (isOpen && moodPromptPending.value) {
        if (initialMoodTimer !== undefined) {
          window.clearTimeout(initialMoodTimer);
        }
        initialMoodTimer = undefined;
        moodPromptPending.value = false;
      }
      closeSpeech();
      choosingMood.value = false;
      if (isOpen) {
        showSpeech(t("guide.balance.openMessage"), t("guide.balance.heading"), true);
      }
    },
  );
  onMounted(() => {
    const intro = pageMessages.value[route.path] ?? fallbackPageMessage();
    pageIntroHeading.value = intro.heading;
    pageIntroMessage.value = intro.message;
    unsubscribeGuide = ladiGuideController.subscribe(handleGuideMessage);
    document.addEventListener("click", handleGuidedClick, true);
    scheduleRandomMotion();
    if (!isHidden.value) {
      choosingMood.value = true;
      const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
      initialMoodTimer = window.setTimeout(
        () => {
          clearSpeechTimer();
          customHeading.value = t("guide.mood.promptHeading");
          speech.value = t("guide.mood.promptMessage");
          initialMoodTimer = undefined;
        },
        reduceMotion ? LADI_GUIDE_REDUCED_MOTION_PROMPT_DELAY_MS : LADI_GUIDE_STANDARD_PROMPT_DELAY_MS,
      );
    } else {
      moodPromptPending.value = false;
    }
  });
  onUnmounted(() => {
    clearSpeechTimer();
    if (motionTimer !== undefined) {
      window.clearTimeout(motionTimer);
    }
    if (initialMoodTimer !== undefined) {
      window.clearTimeout(initialMoodTimer);
    }
    if (celebrationTimer !== undefined) {
      window.clearTimeout(celebrationTimer);
    }
    unsubscribeGuide?.();
    document.removeEventListener("click", handleGuidedClick, true);
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
