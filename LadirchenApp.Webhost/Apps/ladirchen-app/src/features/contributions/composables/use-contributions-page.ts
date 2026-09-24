import { computed, onMounted, reactive, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";

import { isPromotionAvailable } from "@/domain/contributions/promotions";
import type { Contribution, ContributionKind, NewContribution, NewPromotion, Promotion } from "@/domain/contributions/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import { CONTRIBUTION_IDS } from "@/infrastructure/fixtures/family-world-fixtures";
import { useLocalizedDomainContent } from "@/shared/composables/use-localized-domain-content";
import { useFamilyWorldStore } from "@/stores/family-world";
import { ladiGuideController } from "@/shared/services/ladi-guide-controller";
import { PAGE_INTRO_GUIDE_DELAY_MS } from "@/shared/runtime-timing";

export const useContributionsPage = () => {
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
  const selectedSiblingIds = ref<FamilyMemberId[]>([]);
  const selectedPromotion = ref<Promotion>();
  const contributionToDelete = ref<Contribution>();
  const promotionToDelete = ref<Promotion>();
  const ratings = reactive<Partial<Record<ContributionId, number>>>({});
  const newContribution = reactive<Omit<NewContribution, "assigneeId"> & { assigneeId: FamilyMemberId | "" }>({
    title: "", description: "", icon: "✨", reward: 10, energy: 10, kind: "basic", assigneeId: "",
  });
  const kindOptions = computed(() => [
    { title: t("contributions.kind.basic"), value: "basic" },
    { title: t("contributions.kind.extraOptional"), value: "extra" },
  ]);
  const assignmentOptions = computed(() => {
    if (!store.permissions.canManageContent) {
      return [{ title: t("contributions.assignment.own", { name: store.signedInMember.name }), value: store.signedInMemberId }];
    }
    return [
      { title: t("contributions.assignment.open"), value: "" },
      ...store.members.map(member => ({
        title: `${member.name}${member.role === "guardian" ? t("contributions.assignment.guardianSuffix") : ""}`,
        value: member.id,
      })),
    ];
  });
  const newPromotion = reactive<NewPromotion>({ contributionId: CONTRIBUTION_IDS.dishwasher, multiplier: 2, deadline: "17:00", teamworkBonus: 10 });
  const multiplierOptions = computed(() => [
    { title: t("contributions.promotions.double"), value: 2 },
    { title: t("contributions.promotions.triple"), value: 3 },
  ]);
  const localizedContributions = computed(() => store.contributions.map(localize.contribution));
  const localizedPromotions = computed(() => store.promotions.map(localize.promotion));
  const activePromotions = computed(() => localizedPromotions.value.filter(item => item.active));
  const filteredContributions = computed(() => localizedContributions.value.filter(contribution =>
    (filter.value === "all" || contribution.kind === filter.value) &&
    (statusFilter.value === "completed" ? contribution.status === "approved" : contribution.status !== "approved") &&
    (scopeFilter.value === "all" ||
      (scopeFilter.value === "mine" && contribution.assigneeId === store.activeChildId) ||
      (scopeFilter.value === "open" && !contribution.assigneeId && contribution.status === "available")),
  ));
  const managedContributions = computed(() => [...localizedContributions.value].sort((left, right) =>
    Number(Boolean(left.assigneeId)) - Number(Boolean(right.assigneeId)),
  ));
  const guardianOwnContributions = computed(() => localizedContributions.value.filter(contribution => contribution.assigneeId === store.signedInMemberId));
  const guardianTasksToRate = computed(() => localizedContributions.value.filter((contribution) => {
    const assignee = store.members.find(member => member.id === contribution.assigneeId);
    return contribution.status === "pending" && assignee?.role === "guardian";
  }));
  const childTasksToReview = computed(() => localizedContributions.value.filter(contribution =>
    contribution.status === "pending" && store.members.some(member => member.id === contribution.assigneeId && member.role === "child"),
  ));
  const openContributionCount = computed(() => store.contributions.filter(contribution => !contribution.assigneeId && contribution.status === "available").length);
  const promotionContributionOptions = computed(() => localizedContributions.value.map(contribution => ({ title: contribution.title, value: contribution.id })));
  const teamContribution = computed(() => localizedContributions.value.find(contribution => contribution.id === teamContributionId.value));
  const siblingOptions = computed(() => store.members
    .filter(member => member.role === "child" && member.id !== teamContribution.value?.assigneeId)
    .map(member => ({ title: `${member.avatar} ${member.name}`, value: member.id })));
  const promotionFor = (contributionId: ContributionId) => localizedPromotions.value.find(promotion =>
    promotion.contributionId === contributionId && isPromotionAvailable(promotion, store.familyTimeZone, new Date(store.currentTimeMilliseconds)),
  );
  const contributionTitle = (contributionId: ContributionId) => localizedContributions.value.find(contribution => contribution.id === contributionId)?.title ?? t("contributions.singular");
  const memberName = (memberId?: FamilyMemberId) => store.members.find(member => member.id === memberId)?.name ?? t("contributions.assignment.stillOpen");
  const guardianContributionStatus = (contribution: Contribution): string => {
    if (contribution.status === "pending") {return t("contributions.guardianOwn.awaitingRating");}
    if (contribution.status === "approved") {return t("contributions.starsReceived", { value: contribution.stars ?? 1 });}
    return t("contributions.guardianOwn.ready");
  };
  const assignContribution = (contributionId: ContributionId, value: unknown) => {
    const member = store.members.find(item => item.id === value);
    store.assignContribution(contributionId, member?.id);
  };
  const openOwnTaskDialog = () => {
    Object.assign(newContribution, { assigneeId: store.signedInMemberId });
    addDialog.value = true;
  };
  const requestContributionDelete = (contribution: Contribution) => { contributionToDelete.value = contribution; };
  const confirmContributionDelete = () => {
    if (!contributionToDelete.value) {return;}
    store.deleteContribution(contributionToDelete.value.id);
    contributionToDelete.value = undefined;
  };
  const confirmPromotionDelete = () => {
    if (!promotionToDelete.value) {return;}
    store.deletePromotion(promotionToDelete.value.id);
    promotionToDelete.value = undefined;
  };
  const openTeamInvite = (contribution: Contribution) => {
    teamContributionId.value = contribution.id;
    selectedSiblingIds.value = [...(contribution.invitedChildIds ?? [])];
    teamDialog.value = true;
  };
  const contributionTip = (contribution: Contribution) => {
    const seedKey = contribution.translationKey?.split(".").at(-1);
    if (seedKey === "roomAir") {return t("contributions.tips.air");}
    if (seedKey === "setTable" || seedKey === "adamBreakfast") {return t("contributions.tips.table");}
    if (seedKey === "dishwasher") {return t("contributions.tips.dishes");}
    if (seedKey === "feedPet" || seedKey === "danielCats") {return t("contributions.tips.pets");}
    if (seedKey === "waterPlants") {return t("contributions.tips.plants");}
    if (seedKey === "danielLaundry") {return t("contributions.tips.laundry");}
    if (seedKey === "takeTrash") {return t("contributions.tips.trash");}
    return t("contributions.tips.default", { description: contribution.description });
  };
  const saveTeamInvite = () => {
    if (!teamContributionId.value) {return;}
    store.setContributionPartners(teamContributionId.value, selectedSiblingIds.value);
    teamDialog.value = false;
  };
  const addContribution = () => {
    store.addContribution({ ...newContribution, assigneeId: newContribution.assigneeId || undefined });
    addDialog.value = false;
    Object.assign(newContribution, { title: "", description: "", icon: "✨", reward: 10, energy: 10, kind: "basic", assigneeId: "" });
  };
  const addPromotion = () => {
    store.addPromotion({ ...newPromotion });
    promotionDialog.value = false;
    Object.assign(newPromotion, { contributionId: CONTRIBUTION_IDS.dishwasher, multiplier: 2, deadline: "17:00", teamworkBonus: 10 });
  };

  onMounted(() => {
    if (store.viewerRole !== "child") {return;}
    window.setTimeout(() => ladiGuideController.say({
      heading: t("guide.pages.contributions.heading"), message: t("guide.pages.contributions.message"), pageIntro: true,
    }), PAGE_INTRO_GUIDE_DELAY_MS);
  });
  watch(() => route.query.new, value => { if (value === "1" && store.permissions.canManageContent) {addDialog.value = true;} }, { immediate: true });
  watch(() => route.query.promotion, value => { if (value === "1" && store.permissions.canManageContent) {promotionDialog.value = true;} }, { immediate: true });

  return {
    activePromotions, addContribution, addDialog, addPromotion, assignContribution, assignmentOptions,
    childTasksToReview, confirmContributionDelete, confirmPromotionDelete, contributionTip, contributionTitle,
    contributionToDelete, filter, filteredContributions, guardianContributionStatus, guardianOwnContributions,
    guardianTasksToRate, kindOptions, managedContributions, memberName, multiplierOptions, newContribution,
    newPromotion, openContributionCount, openOwnTaskDialog, openTeamInvite, promotionContributionOptions,
    promotionDialog, promotionFor, promotionToDelete, ratings, requestContributionDelete, saveTeamInvite,
    scopeFilter, selectedPromotion, selectedSiblingIds, siblingOptions, statusFilter, store, teamContribution,
    teamDialog,
  };
};
