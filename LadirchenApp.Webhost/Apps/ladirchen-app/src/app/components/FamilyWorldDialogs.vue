<template>
  <v-snackbar v-model="store.snackbar.visible" color="secondary" location="bottom" rounded="lg" :timeout="2600">
    {{ snackbarMessage }}
  </v-snackbar>

  <ContributionRewardDialog />
  <GuardianGiftDialog />
  <BehaviorNoticeDialog />
  <GoalCancellationDialog />
  <AsyncFamilySetupDialog v-if="store.familySetupOpen" />
  <AsyncSavingsPiggyDialog v-if="store.piggyBankOpen" />
</template>

<script lang="ts" setup>
import { computed, defineAsyncComponent } from "vue";
import { useI18n } from "vue-i18n";
import { useFamilyWorldStore } from "@/stores/family-world";
import BehaviorNoticeDialog from "./BehaviorNoticeDialog.vue";
import ContributionRewardDialog from "./ContributionRewardDialog.vue";
import GoalCancellationDialog from "./GoalCancellationDialog.vue";
import GuardianGiftDialog from "./GuardianGiftDialog.vue";

const AsyncFamilySetupDialog = defineAsyncComponent(() => import("@/features/family/components/FamilySetupDialog.vue"));
const AsyncSavingsPiggyDialog = defineAsyncComponent(
  () => import("@/features/savings/components/SavingsPiggyDialog.vue"),
);
const store = useFamilyWorldStore();
const { t } = useI18n();
const snackbarMessage = computed(() =>
  store.snackbar.messageKey ? t(store.snackbar.messageKey, store.snackbar.params) : "",
);
</script>
