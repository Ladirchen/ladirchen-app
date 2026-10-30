import { useTimeoutFn } from "@vueuse/core";
import { computed, onMounted, reactive, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import type {
  Contribution,
  ContributionKind,
  NewContribution,
  NewPromotion,
  Promotion,
} from "@/domain/contributions/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import { useLocalizedDomainContent } from "@/shared/composables/use-localized-domain-content";
import { useFamilyWorldStore } from "@/stores/family-world";
import { ladiGuideController } from "@/shared/services/ladi-guide-controller";
import { PAGE_INTRO_GUIDE_DELAY_MS } from "@/shared/runtime-timing";

export function useContributionsPage() {
  const store = useFamilyWorldStore();
  const route = useRoute();
  const { t } = useI18n();
  const localize = useLocalizedDomainContent();
  const filter = ref<"all" | ContributionKind>("all");
  const scopeFilter = ref<"all" | "mine" | "open">("mine");
  const statusFilter = ref<"open" | "completed">("open");
  const addDialog = ref(false);
  const promotionDialog = ref(false);
  const teamDialog = ref(false);
  const teamContributionId = ref<ContributionId>();
  const selectedPartnerIds = ref<FamilyMemberId[]>([]);
  const selectedPromotion = ref<Promotion>();
  const contributionToDelete = ref<Contribution>();
  const promotionToDelete = ref<Promotion>();
  const ratings = reactive<Partial<Record<ContributionId, number>>>({});
  const newContribution = reactive<
    Omit<NewContribution, "assigneeId"> & {
      assigneeId: FamilyMemberId | "";
    }
  >({
    title: "",
    description: "",
    icon: "✨",
    reward: 10,
    energy: 10,
    kind: "basic",
    assigneeId: "",
  });
  const kindOptions = computed(() => [
    { title: t("contributions.kind.basic"), value: "basic" },
    { title: t("contributions.kind.extraOptional"), value: "extra" },
  ]);
  const assignmentOptions = computed(function callback() {
    if (!store.permissions.canManageContent) {
      return [
        {
          title: t("contributions.assignment.own", { name: store.signedInMember?.name ?? "" }),
          value: store.signedInMemberId,
        },
      ];
    }

    return [
      { title: t("contributions.assignment.open"), value: "" },
      ...store.members.map((member) => ({
        title: `${member.name}${member.role === "guardian" ? t("contributions.assignment.guardianSuffix") : ""}`,
        value: member.id,
      })),
    ];
  });
  const newPromotion = reactive<Omit<NewPromotion, "contributionId"> & { contributionId: ContributionId | "" }>({
    contributionId: "",
    multiplier: 2,
    deadline: "17:00",
    teamworkBonus: 10,
  });
  const multiplierOptions = computed(() => [
    { title: t("contributions.promotions.double"), value: 2 },
    { title: t("contributions.promotions.triple"), value: 3 },
  ]);
  const localizedContributions = computed(() => store.contributions.map(localize.contribution));
  const localizedPromotions = computed(() => store.promotions.map(localize.promotion));
  const activePromotions = computed(() => localizedPromotions.value.filter((item) => item.active));
  const filteredContributions = computed(() =>
    localizedContributions.value.filter(
      (contribution) =>
        (filter.value === "all" || contribution.kind === filter.value) &&
        (statusFilter.value === "completed"
          ? contribution.status === "approved"
          : contribution.status !== "approved") &&
        (scopeFilter.value === "all" ||
          (scopeFilter.value === "mine" && contribution.assigneeId === store.activeChildId) ||
          (scopeFilter.value === "open" && !contribution.assigneeId && contribution.status === "available")),
    ),
  );
  const managedContributions = computed(() =>
    [...localizedContributions.value].sort(
      (left, right) => Number(Boolean(left.assigneeId)) - Number(Boolean(right.assigneeId)),
    ),
  );
  const guardianOwnContributions = computed(() =>
    localizedContributions.value.filter((contribution) => contribution.assigneeId === store.signedInMemberId),
  );
  const guardianTasksToRate = computed(() =>
    localizedContributions.value.filter(function callback(contribution) {
      const assignee = store.members.find((member) => member.id === contribution.assigneeId);

      return contribution.status === "pending" && assignee?.role === "guardian";
    }),
  );
  const reviewableTasks = computed(() =>
    localizedContributions.value.filter(
      (contribution) =>
        contribution.status === "pending" &&
        contribution.assigneeId !== store.signedInMemberId &&
        store.members.some((member) => member.id === contribution.assigneeId),
    ),
  );
  const openContributionCount = computed(
    () =>
      store.contributions.filter((contribution) => !contribution.assigneeId && contribution.status === "available")
        .length,
  );
  const promotionContributionOptions = computed(() =>
    localizedContributions.value.map((contribution) => ({ title: contribution.title, value: contribution.id })),
  );
  const teamContribution = computed(() =>
    localizedContributions.value.find((contribution) => contribution.id === teamContributionId.value),
  );
  const partnerOptions = computed(() =>
    (teamContribution.value?.partnerableMemberIds ?? []).flatMap((memberId) => {
      const member = store.members.find((item) => item.id === memberId);

      return member ? [{ title: `${member.avatar} ${member.name}`, value: member.id }] : [];
    }),
  );
  function promotionFor(contributionId: ContributionId) {
    return localizedPromotions.value.find(
      (promotion) => promotion.contributionId === contributionId && promotion.available,
    );
  }
  function contributionTitle(contributionId: ContributionId) {
    return (
      localizedContributions.value.find((contribution) => contribution.id === contributionId)?.title ??
      t("contributions.singular")
    );
  }
  function memberName(memberId?: FamilyMemberId) {
    return store.members.find((member) => member.id === memberId)?.name ?? t("contributions.assignment.stillOpen");
  }
  function guardianContributionStatus(contribution: Contribution): string {
    if (contribution.status === "pending") {
      return t("contributions.guardianOwn.awaitingRating");
    }

    if (contribution.status === "approved") {
      return t("contributions.starsReceived", { value: contribution.stars ?? 1 });
    }

    return t("contributions.guardianOwn.ready");
  }
  function approveWithRating(contributionId: ContributionId) {
    void store.approveContribution(contributionId, ratings[contributionId] ?? 4);
  }
  function assignContribution(contributionId: ContributionId, value: unknown) {
    const member = store.members.find((item) => item.id === value);
    void store.assignContribution(contributionId, member?.id);
  }
  function openOwnTaskDialog() {
    Object.assign(newContribution, { assigneeId: store.signedInMemberId });
    addDialog.value = true;
  }
  function requestContributionDelete(contribution: Contribution) {
    contributionToDelete.value = contribution;
  }
  function confirmContributionDelete() {
    if (!contributionToDelete.value) {
      return;
    }

    void store.deleteContribution(contributionToDelete.value.id);
    contributionToDelete.value = undefined;
  }
  function confirmPromotionDelete() {
    if (!promotionToDelete.value) {
      return;
    }

    void store.deletePromotion(promotionToDelete.value.id);
    promotionToDelete.value = undefined;
  }
  function openTeamInvite(contribution: Contribution) {
    teamContributionId.value = contribution.id;
    selectedPartnerIds.value = [...(contribution.invitedChildIds ?? [])];
    teamDialog.value = true;
  }
  function contributionTip(contribution: Contribution) {
    const seedKey = contribution.translationKey?.split(".").at(-1);

    if (seedKey === "roomAir") {
      return t("contributions.tips.air");
    }

    if (seedKey === "setTable" || seedKey === "adamBreakfast") {
      return t("contributions.tips.table");
    }

    if (seedKey === "dishwasher") {
      return t("contributions.tips.dishes");
    }

    if (seedKey === "feedPet" || seedKey === "danielCats") {
      return t("contributions.tips.pets");
    }

    if (seedKey === "waterPlants") {
      return t("contributions.tips.plants");
    }

    if (seedKey === "danielLaundry") {
      return t("contributions.tips.laundry");
    }

    if (seedKey === "takeTrash") {
      return t("contributions.tips.trash");
    }

    return t("contributions.tips.default", { description: contribution.description });
  }
  function saveTeamInvite() {
    if (!teamContributionId.value) {
      return;
    }

    void store.setContributionPartners(teamContributionId.value, selectedPartnerIds.value);
    teamDialog.value = false;
  }
  function addContribution() {
    void store.addContribution({ ...newContribution, assigneeId: newContribution.assigneeId || undefined });
    addDialog.value = false;
    Object.assign(newContribution, {
      title: "",
      description: "",
      icon: "✨",
      reward: 10,
      energy: 10,
      kind: "basic",
      assigneeId: "",
    });
  }
  function addPromotion() {
    const contributionId = newPromotion.contributionId;

    if (!contributionId) {
      return;
    }

    void store.addPromotion({ ...newPromotion, contributionId });
    promotionDialog.value = false;
    Object.assign(newPromotion, {
      contributionId: "",
      multiplier: 2,
      deadline: "17:00",
      teamworkBonus: 10,
    });
  }
  const { start: startPageIntroGuide } = useTimeoutFn(
    () => {
      ladiGuideController.say({
        heading: t("guide.pages.contributions.heading"),
        message: t("guide.pages.contributions.message"),
        pageIntro: true,
      });
    },
    PAGE_INTRO_GUIDE_DELAY_MS,
    { immediate: false },
  );
  onMounted(function callback() {
    if (store.viewerRole !== "child") {
      return;
    }

    startPageIntroGuide();
  });
  watch(
    () => route.query.new,
    function callback(value) {
      if (value === "1" && store.permissions.canManageContent) {
        addDialog.value = true;
      }
    },
    { immediate: true },
  );
  watch(
    () => route.query.promotion,
    function callback(value) {
      if (value === "1" && store.permissions.canManageContent) {
        promotionDialog.value = true;
      }
    },
    { immediate: true },
  );

  return {
    activePromotions,
    addContribution,
    addDialog,
    addPromotion,
    approveWithRating,
    assignContribution,
    assignmentOptions,
    reviewableTasks,
    confirmContributionDelete,
    confirmPromotionDelete,
    contributionTip,
    contributionTitle,
    contributionToDelete,
    filter,
    filteredContributions,
    guardianContributionStatus,
    guardianOwnContributions,
    guardianTasksToRate,
    kindOptions,
    managedContributions,
    memberName,
    multiplierOptions,
    newContribution,
    newPromotion,
    openContributionCount,
    openOwnTaskDialog,
    openTeamInvite,
    promotionContributionOptions,
    promotionDialog,
    promotionFor,
    promotionToDelete,
    ratings,
    requestContributionDelete,
    saveTeamInvite,
    scopeFilter,
    selectedPromotion,
    selectedPartnerIds,
    partnerOptions,
    statusFilter,
    store,
    teamContribution,
    teamDialog,
  };
}
