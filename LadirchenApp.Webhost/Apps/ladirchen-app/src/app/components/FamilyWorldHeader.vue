<template>
  <header class="family-world-header d-flex align-center justify-space-between">
    <RouterLink
      :aria-label="t('common.familyWorldAria')"
      class="family-world-brand d-flex align-end text-decoration-none"
      to="/app"
    >
      <span class="family-world-logo position-relative flex-shrink-0 overflow-hidden" aria-hidden="true">
        <img alt="" src="/ladirchen-logo.png" />
      </span>
      <span class="family-world-wordmark">{{ t("common.appName") }}</span>
    </RouterLink>
    <div
      class="header-stats d-grid align-center"
      :class="{ 'header-stats--guardian': store.viewerRole === 'guardian' }"
    >
      <button
        v-if="store.viewerRole === 'guardian' && store.signedInMember?.guardianAccess !== 'external'"
        class="header-stat header-guardian-week d-flex align-center cursor-pointer"
        :aria-label="guardianWeekAriaLabel"
        aria-haspopup="dialog"
        :data-ladi-heading="t('shell.week.heading')"
        :data-ladi-tip="guardianWeekTip"
        type="button"
        @click="store.piggyBankOpen = true"
      >
        <span class="header-stat-symbol"><LadirchenCoin animated small /></span>
        <span class="header-guardian-week-copy">
          <small>{{ t("shell.week.label") }}</small>
          <strong>
            {{ t("shell.week.summary", { earned: childrenWeekEarned, completed: childrenWeekCompleted }) }}
          </strong>
          <i>{{ t("shell.week.assets", { assets: childrenFamilyCurrency, balance: headerBalance }) }}</i>
        </span>
      </button>
      <template v-else-if="store.viewerRole === 'child'">
        <button
          class="header-stat header-streak d-flex align-center justify-center cursor-pointer"
          :aria-label="t('shell.streak.aria', { days: store.currentParticipationStreakDays })"
          aria-haspopup="dialog"
          :data-ladi-heading="t('shell.streak.heading')"
          :data-ladi-tip="t('shell.streak.tip', { days: store.currentParticipationStreakDays })"
          type="button"
          @click="streakDialog = true"
        >
          <span class="header-stat-symbol"><AnimatedStreakFlame :size="30" /></span>
          <strong>{{ store.currentParticipationStreakDays }}</strong>
          <span class="header-stat-label">{{ t("shell.streak.days") }}</span>
        </button>
        <button
          class="header-stat header-balance d-flex align-center justify-center cursor-pointer"
          :aria-label="
            t('shell.balance.aria', { name: store.displayNameFor(headerBalanceMemberId), balance: headerBalance })
          "
          aria-haspopup="dialog"
          :data-ladi-heading="t('shell.balance.heading')"
          :data-ladi-tip="t('shell.balance.tip', { balance: headerBalance })"
          type="button"
          @click="store.piggyBankOpen = true"
        >
          <span class="header-stat-symbol"><LadirchenCoin animated small /></span>
          <strong>{{ headerBalance }}</strong>
        </button>
      </template>
    </div>
    <div class="header-session d-flex align-center justify-space-between ga-2">
      <RouterLink
        v-if="store.signedInMember"
        :aria-label="t('shell.currentUser.aria', { name: currentUserName })"
        class="header-current-user d-flex align-center ga-1 text-decoration-none"
        :title="t('shell.currentUser.aria', { name: currentUserName })"
        to="/app/profile"
      >
        <span class="i-mdi:account-circle-outline flex-shrink-0" aria-hidden="true"></span>
        <span class="text-truncate">{{ currentUserName }}</span>
      </RouterLink>
      <ApplicationVersion />
    </div>
  </header>

  <AsyncWeeklyStreakDialog v-if="streakDialog" v-model="streakDialog" />
</template>

<script lang="ts" setup>
import { computed, defineAsyncComponent, ref } from "vue";
import { useI18n } from "vue-i18n";
import AnimatedStreakFlame from "@/features/streaks/components/AnimatedStreakFlame.vue";
import LadirchenCoin from "@/shared/components/LadirchenCoin.vue";
import ApplicationVersion from "@/shared/components/ApplicationVersion.vue";
import { useFamilyWorldStore } from "@/stores/family-world";

const AsyncWeeklyStreakDialog = defineAsyncComponent(
  () => import("@/features/streaks/components/WeeklyStreakDialog.vue"),
);
const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const streakDialog = ref(false);
const currentUserName = computed(() => store.displayNameFor(store.signedInMemberId));
const headerBalanceMemberId = computed(() =>
  store.viewerRole === "guardian" ? store.signedInMemberId : store.activeChildId,
);
const headerBalance = computed(() =>
  store.viewerRole === "guardian" ? store.balanceFor(store.signedInMemberId) : store.availableBalance,
);
const childMembers = computed(() => store.members.filter((member) => member.role === "child"));
const childrenWeekEarned = computed(() =>
  childMembers.value.reduce((sum, child) => sum + store.currentWeekEarnedFor(child.id), 0),
);
const childrenWeekCompleted = computed(() =>
  childMembers.value.reduce((sum, child) => sum + store.currentWeekApprovedContributionCountFor(child.id), 0),
);
const childrenTotalAssets = computed(() =>
  childMembers.value.reduce(
    (sum, child) =>
      sum +
      store.balanceFor(child.id) +
      store.goals.filter((goal) => goal.ownerId === child.id).reduce((goalSum, goal) => goalSum + goal.saved, 0),
    0,
  ),
);
const childrenFamilyCurrency = computed(() =>
  new Intl.NumberFormat(locale.value, {
    style: "currency",
    currency: store.familyCurrencyCode,
    maximumFractionDigits: 2,
  }).format(store.familyCurrencyValue(childrenTotalAssets.value)),
);
const guardianWeekAriaLabel = computed(() =>
  t("shell.week.aria", {
    earned: childrenWeekEarned.value,
    completed: childrenWeekCompleted.value,
    assets: childrenFamilyCurrency.value,
    balance: headerBalance.value,
  }),
);
const guardianWeekTip = computed(() =>
  t("shell.week.tip", { earned: childrenWeekEarned.value, completed: childrenWeekCompleted.value }),
);
</script>
