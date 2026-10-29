import { useTimeoutFn } from "@vueuse/core";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import type { PageViewOption } from "@/shared/components/ui/page-view-option";
import type { GoalVisibility, NewGoal, SavingGoal, SavingGoalOwnerId } from "@/domain/savings/types";
import { DEFAULT_TRANSFER_AMOUNT } from "@/features/savings/transfer-defaults";
import type { FamilyMemberId, SavingGoalId } from "@/domain/shared/identifiers";
import { percentageOfTotal } from "@/domain/shared/numbers";
import { useLocalizedDomainContent } from "@/shared/composables/use-localized-domain-content";
import { useFamilyWorldStore } from "@/stores/family-world";
import { ladiGuideController } from "@/shared/services/ladi-guide-controller";
import { PAGE_INTRO_GUIDE_DELAY_MS } from "@/shared/runtime-timing";

const SAVE_ANIMATION_DURATION_MS = 620;
const SUPPORT_ANIMATION_DURATION_MS = 950;

type WishView = "own" | "children" | "family";

function goalBalanceLabel(goal: SavingGoal) {
  return `${goal.saved} / ${goal.target}`;
}

function remainingGoalAmount(goal: Pick<SavingGoal, "remainingAmount" | "saved" | "target">) {
  return goal.remainingAmount ?? Math.max(0, goal.target - goal.saved);
}

function progress(saved: number, target: number) {
  return percentageOfTotal(saved, target);
}

function depositedAmount(goal: SavingGoal) {
  return Math.max(0, goal.saved - (goal.interestEarned ?? 0));
}

export function useWishesPage() {
  const store = useFamilyWorldStore();
  const route = useRoute();
  const { locale, t } = useI18n();
  const localize = useLocalizedDomainContent();
  const activeTab = ref<WishView>("own");
  const saveDialog = ref(false);
  const goalDialog = ref(false);
  const newGoalOwnerId = ref<SavingGoalOwnerId>(store.activeChildId);
  const supportDialog = ref(false);
  const supportGoalId = ref<SavingGoalId>();
  const supportAmount = ref<number>(DEFAULT_TRANSFER_AMOUNT);
  const supportSending = ref(false);
  const saveAmount = ref<number>(DEFAULT_TRANSFER_AMOUNT);
  const saveMotion = ref(false);
  let saveTimer: number | undefined;
  let supportTimer: number | undefined;

  const localizedGoals = computed(() => store.goals.map(localize.goal));
  const activeGoal = computed(() => localize.goal(store.activeGoal));
  const activeGoalRemaining = computed(() => remainingGoalAmount(activeGoal.value));
  const maxAssignable = computed(() => Math.min(store.availableBalance, activeGoalRemaining.value));
  const personalGoalOwnerId = computed<FamilyMemberId>(() => store.signedInMemberId);
  const childrenGoals = computed(() =>
    localizedGoals.value.filter((goal) => {
      const owner = goal.ownerId === "family" ? undefined : store.members.find((member) => member.id === goal.ownerId);

      return owner?.role === "child" && goal.visibleToMemberIds?.includes(store.signedInMemberId);
    }),
  );
  const personalGoals = computed(() =>
    activeTab.value === "children"
      ? childrenGoals.value
      : localizedGoals.value.filter((goal) => goal.ownerId === store.signedInMemberId),
  );
  const canCreatePersonalGoal = computed(() => activeTab.value === "own");
  const visibleFamilyGoals = computed(() =>
    localizedGoals.value.filter(
      (goal) => goal.ownerId !== store.signedInMemberId && goal.visibleToMemberIds?.includes(store.signedInMemberId),
    ),
  );
  const wishViewOptions = computed<Array<PageViewOption<WishView>>>(() => {
    const ownGoals = localizedGoals.value.filter((goal) => goal.ownerId === store.signedInMemberId);
    const options: Array<PageViewOption<WishView>> = [
      {
        id: "own",
        icon: "i-mdi:account-star-outline",
        subtitle: t("wishes.views.ownCount", { count: ownGoals.length }),
        title: t("wishes.views.own"),
      },
    ];

    if (store.viewerRole === "guardian") {
      options.push({
        id: "children",
        icon: "i-mdi:account-child-outline",
        subtitle: t("wishes.views.visibleCount", { count: childrenGoals.value.length }),
        title: t("wishes.views.children"),
      });
    }

    if (store.permissions.canViewFamilyGoals) {
      options.push({
        id: "family",
        icon: "i-mdi:account-group-outline",
        subtitle: t("wishes.views.visibleCount", { count: visibleFamilyGoals.value.length }),
        title: t("wishes.views.family"),
      });
    }

    return options;
  });
  const supportGoal = computed(() => localizedGoals.value.find((goal) => goal.id === supportGoalId.value));
  const supportMaximum = computed(() => {
    if (!supportGoal.value) {
      return 0;
    }

    const remaining = remainingGoalAmount(supportGoal.value);

    return store.viewerRole === "child" ? Math.min(store.availableBalance, remaining) : remaining;
  });
  const supportExplanation = computed(() =>
    store.viewerRole === "child" ? t("wishes.support.childExplanation") : t("wishes.support.guardianExplanation"),
  );

  function weeklyInterestForGoal(goal: SavingGoal) {
    return goal.projectedInterestCredit ?? 0;
  }

  function formatInterestRate(value: number) {
    return value.toLocaleString(locale.value, { minimumFractionDigits: 1, maximumFractionDigits: 2 });
  }

  function ownerName(ownerId: SavingGoalOwnerId) {
    return ownerId === "family"
      ? t("wishes.owner.myFamily")
      : (store.members.find((member) => member.id === ownerId)?.name ?? t("wishes.owner.family"));
  }

  function visibilityLabel(visibility: GoalVisibility) {
    return t(`wishes.visibility.${visibility}`);
  }

  function saveToGoal() {
    if (saveAmount.value <= 0 || saveMotion.value) {
      return;
    }

    saveMotion.value = true;
    saveTimer = window.setTimeout(() => {
      void store.saveToGoal(store.activeGoal.id, saveAmount.value);
      saveDialog.value = false;
      saveAmount.value = DEFAULT_TRANSFER_AMOUNT;
      saveMotion.value = false;
      saveTimer = undefined;
    }, SAVE_ANIMATION_DURATION_MS);
  }

  function openSave(goalId: SavingGoalId) {
    store.activeGoalId = goalId;
    saveAmount.value = Math.min(DEFAULT_TRANSFER_AMOUNT, store.availableBalance, remainingGoalAmount(store.activeGoal));
    saveDialog.value = true;
  }

  function openSupport(goalId: SavingGoalId) {
    supportGoalId.value = goalId;
    const goal = store.goals.find((item) => item.id === goalId);
    const remaining = goal ? remainingGoalAmount(goal) : 0;
    supportAmount.value = Math.max(
      0,
      Math.min(DEFAULT_TRANSFER_AMOUNT, store.viewerRole === "child" ? store.availableBalance : remaining, remaining),
    );
    supportDialog.value = true;
  }

  function giveSupport() {
    if (!supportGoalId.value || supportSending.value) {
      return;
    }

    const goalId = supportGoalId.value;
    const amount = supportAmount.value;
    const recipient = supportGoal.value ? ownerName(supportGoal.value.ownerId) : t("wishes.owner.yourFamily");
    supportSending.value = true;
    ladiGuideController.say({
      heading: t("wishes.support.guideTitle"),
      message: t("wishes.support.guideMessage", { recipient }),
      celebration: "gift",
    });
    supportTimer = window.setTimeout(() => {
      if (store.viewerRole === "child") {
        void store.giftLadirchenToGoal(goalId, amount);
      } else {
        void store.supportGoal(goalId, amount);
      }

      supportDialog.value = false;
      supportSending.value = false;
      supportTimer = undefined;
    }, SUPPORT_ANIMATION_DURATION_MS);
  }

  function openGoalDialog(ownerId: SavingGoalOwnerId) {
    newGoalOwnerId.value = ownerId;
    goalDialog.value = true;
  }

  function addGoal(goal: NewGoal) {
    void store.addGoal(goal, newGoalOwnerId.value);
  }
  watch(
    () => route.query.new,
    (value) => {
      if (value === "1" && store.permissions.canManageGoals) {
        goalDialog.value = true;
      }
    },
    { immediate: true },
  );
  watch(
    () => route.query.family,
    (value) => {
      if (value !== "1" || !store.permissions.canManageGoals) {
        return;
      }

      activeTab.value = "family";
      openGoalDialog("family");
    },
    { immediate: true },
  );
  watch(
    () => store.permissions.canViewFamilyGoals,
    (canViewFamilyGoals) => {
      if (!canViewFamilyGoals) {
        activeTab.value = "own";
      }
    },
  );
  watch(activeTab, (tab) => {
    if (tab !== "family" || store.viewerRole !== "child") {
      return;
    }

    ladiGuideController.say({
      heading: t("wishes.guide.familyTitle"),
      message: t("wishes.guide.familyMessage"),
    });
  });
  const { start: startPageIntroGuide } = useTimeoutFn(
    () => {
      ladiGuideController.say({
        heading: t("guide.pages.wishes.heading"),
        message: t("guide.pages.wishes.message"),
        pageIntro: true,
      });
    },
    PAGE_INTRO_GUIDE_DELAY_MS,
    { immediate: false },
  );

  onMounted(() => {
    if (store.viewerRole !== "child") {
      return;
    }

    startPageIntroGuide();
  });
  onUnmounted(() => {
    if (saveTimer !== undefined) {
      window.clearTimeout(saveTimer);
    }

    if (supportTimer !== undefined) {
      window.clearTimeout(supportTimer);
    }
  });

  return {
    activeGoal,
    activeTab,
    addGoal,
    canCreatePersonalGoal,
    depositedAmount,
    formatInterestRate,
    giveSupport,
    goalBalanceLabel,
    goalDialog,
    maxAssignable,
    openGoalDialog,
    openSave,
    openSupport,
    ownerName,
    personalGoalOwnerId,
    personalGoals,
    progress,
    saveAmount,
    saveDialog,
    saveMotion,
    saveToGoal,
    store,
    supportAmount,
    supportDialog,
    supportExplanation,
    supportGoal,
    supportMaximum,
    supportSending,
    visibilityLabel,
    visibleFamilyGoals,
    weeklyInterestForGoal,
    wishViewOptions,
  };
}
