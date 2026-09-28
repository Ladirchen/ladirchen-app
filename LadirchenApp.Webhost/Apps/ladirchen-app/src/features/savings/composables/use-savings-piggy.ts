import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";

import { isFamilyCurrency } from "@/application/contracts/family-aggregate-validation";
import { resolveFamilyMemberAvatarAppearance } from "@/domain/avatar";
import type { AvatarAppearance } from "@/domain/avatar";
import { getLadiStage, LADI_STAGES } from "@/domain/ladi";
import { SAVINGS_RULES } from "@/domain/savings/rules";
import type { FamilyCurrency } from "@/domain/savings/types";
import type { FamilyMemberId, SavingGoalId } from "@/domain/shared/identifiers";
import { percentageOfTotal } from "@/domain/shared/numbers";
import { isInstantInIsoWeek } from "@/domain/shared/zoned-calendar";
import { useLocalizedDomainContent } from "@/shared/composables/use-localized-domain-content";
import { useFamilyWorldStore } from "@/stores/family-world";
import { ladiGuideController } from "@/shared/services/ladi-guide-controller";

const PIGGY_GUIDE_DELAY_MS = 320;
const TRANSFER_ANIMATION_DURATION_MS = 1100;

export const useSavingsPiggy = () => {
  const store = useFamilyWorldStore();
  const { locale, t } = useI18n();
  const localize = useLocalizedDomainContent();
  const selectedGoalId = ref<SavingGoalId>();
  const selectedMemberId = ref<FamilyMemberId>();
  const transferDestination = ref<"goal" | "member">("goal");
  const amount = ref<number>(SAVINGS_RULES.defaultTransferAmount);
  const transferDirection = ref<"deposit" | "withdraw" | "gift" | "">("");
  const piggyGuideStep = ref(-1);
  let transferTimer: number | undefined;
  let guidanceTimer: number | undefined;
  const currencyOptions = computed<Array<{ title: string; value: FamilyCurrency }>>(() => [
    { title: t("savings.piggy.currencies.CHF"), value: "CHF" },
    { title: t("savings.piggy.currencies.EUR"), value: "EUR" },
    { title: t("savings.piggy.currencies.HUF"), value: "HUF" },
  ]);
  const guardianChildren = computed(() => store.members.filter(member => member.role === "child").map((member) => {
    const now = new Date(store.currentTimeMilliseconds);
    const completedThisWeek = store.contributions.filter(contribution => contribution.assigneeId === member.id &&
      contribution.status === "approved" && contribution.approvedAt !== undefined &&
      isInstantInIsoWeek(contribution.approvedAt, now, store.familyTimeZone));
    const reserved = store.shopRewards.filter(reward => reward.status === "requested" && reward.requesterId === member.id)
      .reduce((sum, reward) => sum + reward.price, 0);
    const available = Math.max(0, store.balanceFor(member.id) - reserved);
    const saved = store.totalVisibleSavedFor(member.id);
    const ladiStage = getLadiStage(store.averageTaskRatingFor(member.id));
    return {
      available, completedThisWeek: completedThisWeek.length, id: member.id,
      ladiLevel: LADI_STAGES.length - LADI_STAGES.indexOf(ladiStage),
      name: member.nickname?.trim() || member.name, saved, total: available + saved,
      weekEarned: completedThisWeek.reduce((sum, contribution) => sum +
        (contribution.earnedReward ?? contribution.reward) + (contribution.earnedRatingBonus ?? 0), 0),
    };
  }));
  const selectedGoal = computed(() => store.ownSavingGoals.find(goal => goal.id === selectedGoalId.value));
  const selectedGoalWithdrawable = computed(() => store.withdrawableGoalBalance(selectedGoal.value));
  const maxDeposit = computed(() => Math.min(store.availableBalance, Math.max(0, (selectedGoal.value?.target ?? 0) - (selectedGoal.value?.saved ?? 0))));
  const transferAmountMaximum = computed(() => transferDestination.value === "member" ? store.availableBalance : Math.max(maxDeposit.value, selectedGoalWithdrawable.value));
  const goalOptions = computed(() => store.ownSavingGoals.map(localize.goal).map(goal => ({ icon: goal.icon, saved: goal.saved, target: goal.target, title: goal.title, value: goal.id })));
  const memberOptions = computed(() => store.members.filter(member => member.role === "child" && member.id !== store.activeChildId)
    .map(member => ({ icon: member.avatar, title: member.nickname?.trim() || member.name, value: member.id })));
  const selectedMemberName = computed(() => memberOptions.value.find(member => member.value === selectedMemberId.value)?.title ?? t("savings.piggy.transfer.memberFallback"));
  const memberAppearance = (memberId: unknown): AvatarAppearance => {
    const member = store.members.find(candidate => candidate.id === memberId) ?? store.activeChild;
    return resolveFamilyMemberAvatarAppearance(member, store.members);
  };
  const formattedFamilyValue = computed(() => new Intl.NumberFormat(locale.value, { style: "currency", currency: store.familyCurrencyCode }).format(store.familyCurrencyValue(store.availableBalance)));
  const formattedGuardianFamilyValue = computed(() => new Intl.NumberFormat(locale.value, { style: "currency", currency: store.familyCurrencyCode }).format(store.familyCurrencyValue(100)));
  const formatRate = (value: number) => value.toLocaleString(locale.value, { minimumFractionDigits: 1, maximumFractionDigits: 2 });
  const setCurrency = (value: unknown) => { if (isFamilyCurrency(value)) {store.setFamilyCurrency(value);} };
  const goalProgress = (saved: number, target: number) => Math.round(percentageOfTotal(saved, target));
  const adjustTransferAmount = (change: number) => { amount.value = Math.max(0, Math.min(transferAmountMaximum.value, amount.value + change)); };
  const setTransferDestination = (destination: "goal" | "member") => {
    transferDestination.value = destination;
    amount.value = Math.min(SAVINGS_RULES.defaultTransferAmount, transferAmountMaximum.value);
  };
  const playTransfer = (direction: "deposit" | "withdraw" | "gift") => {
    if (transferTimer !== undefined) {window.clearTimeout(transferTimer);}
    transferDirection.value = direction;
    transferTimer = window.setTimeout(() => {
      transferDirection.value = "";
      transferTimer = undefined;
    }, TRANSFER_ANIMATION_DURATION_MS);
  };
  const deposit = () => {
    if (!selectedGoalId.value) {return;}
    store.saveToGoal(selectedGoalId.value, amount.value);
    playTransfer("deposit");
    amount.value = SAVINGS_RULES.defaultTransferAmount;
  };
  const withdraw = () => {
    if (!selectedGoalId.value) {return;}
    store.withdrawFromGoal(selectedGoalId.value, amount.value);
    playTransfer("withdraw");
    amount.value = SAVINGS_RULES.defaultTransferAmount;
  };
  const giftToMember = () => {
    if (!selectedMemberId.value || amount.value <= 0) {return;}
    const recipientName = memberOptions.value.find(member => member.value === selectedMemberId.value)?.title ?? t("savings.piggy.transfer.memberFallback");
    store.giftLadirchenToFamilyMember(selectedMemberId.value, amount.value);
    playTransfer("gift");
    ladiGuideController.say({
      heading: t("savings.piggy.guide.giftTitle"), message: t("savings.piggy.guide.giftMessage", { name: recipientName }), celebration: "gift",
    });
    amount.value = Math.min(SAVINGS_RULES.defaultTransferAmount, store.availableBalance);
  };
  const piggyGuideSteps = computed(() => [
    { heading: t("savings.piggy.guide.balanceTitle"), message: t("savings.piggy.guide.balanceMessage", { available: store.availableBalance, saved: store.totalSaved }) },
    { heading: t("savings.piggy.guide.transferTitle"), message: t("savings.piggy.guide.transferMessage") },
    { heading: t("savings.piggy.guide.safeTitle"), message: t("savings.piggy.guide.safeMessage") },
  ]);
  const nextPiggyGuide = () => {
    piggyGuideStep.value = piggyGuideStep.value >= piggyGuideSteps.value.length - 1 ? 0 : piggyGuideStep.value + 1;
    const step = piggyGuideSteps.value[piggyGuideStep.value];
    if (!step) {return;}
    ladiGuideController.say({
      heading: step.heading, message: step.message, smart: true,
      progress: `${piggyGuideStep.value + 1} / ${piggyGuideSteps.value.length}`,
      actionLabel: piggyGuideStep.value === piggyGuideSteps.value.length - 1 ? t("savings.piggy.guide.again") : t("common.next"),
      actionId: "piggy:next",
    });
  };
  watch(() => store.piggyBankOpen, (isOpen) => {
    if (guidanceTimer !== undefined) {window.clearTimeout(guidanceTimer);}
    if (!isOpen || store.viewerRole === "guardian") {return;}
    selectedGoalId.value = store.ownSavingGoals.some(goal => goal.id === store.activeGoal.id) ? store.activeGoal.id : store.ownSavingGoals[0]?.id;
    selectedMemberId.value = memberOptions.value[0]?.value;
    transferDestination.value = "goal";
    amount.value = Math.min(SAVINGS_RULES.defaultTransferAmount, store.availableBalance);
    piggyGuideStep.value = -1;
    guidanceTimer = window.setTimeout(() => {
      nextPiggyGuide();
      guidanceTimer = undefined;
    }, PIGGY_GUIDE_DELAY_MS);
  });
  let unregisterGuideAction: (() => void) | undefined;
  onMounted(() => { unregisterGuideAction = ladiGuideController.registerAction("piggy:next", nextPiggyGuide); });
  onUnmounted(() => {
    if (transferTimer !== undefined) {window.clearTimeout(transferTimer);}
    if (guidanceTimer !== undefined) {window.clearTimeout(guidanceTimer);}
    unregisterGuideAction?.();
  });

  return {
    adjustTransferAmount, amount, currencyOptions, deposit, formatRate, formattedFamilyValue,
    formattedGuardianFamilyValue, giftToMember, goalOptions, goalProgress, guardianChildren,
    maxDeposit, memberAppearance, memberOptions, selectedGoalId, selectedGoalWithdrawable,
    selectedMemberId, selectedMemberName, setCurrency, setTransferDestination, store, transferAmountMaximum,
    transferDestination, transferDirection, withdraw,
  };
};
