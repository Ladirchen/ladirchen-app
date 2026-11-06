<template>
  <v-dialog :model-value="store.pendingGoalCancellation !== undefined" max-width="420" persistent>
    <v-card v-if="store.pendingGoalCancellation" class="pa-6" rounded="xl">
      <div class="d-flex align-center ga-3 mb-4">
        <v-avatar color="warning" size="52" variant="tonal">
          <v-icon icon="i-mdi:account-group-outline" size="30" />
        </v-avatar>
        <div>
          <p class="eyebrow mb-1">{{ t("shell.cancellation.eyebrow") }}</p>
          <h2 class="text-h6">
            {{ t("shell.cancellation.title", { goal: store.pendingGoalCancellation.title }) }}
          </h2>
        </div>
      </div>
      <p class="text-body-2 text-medium-emphasis">{{ t("shell.cancellation.description") }}</p>
      <div class="d-flex flex-column flex-sm-row justify-end ga-2 mt-6">
        <v-btn
          v-if="pendingCancellationLimits?.canRejectCancellation"
          color="error"
          :disabled="cancellationDecision !== undefined"
          :loading="cancellationDecision === false"
          rounded="lg"
          variant="tonal"
          @click="decideGoalCancellation(false)"
        >
          {{ t("shell.cancellation.reject") }}
        </v-btn>
        <v-btn
          v-if="pendingCancellationLimits?.canApproveCancellation"
          color="primary"
          :disabled="cancellationDecision !== undefined"
          :loading="cancellationDecision === true"
          rounded="lg"
          variant="flat"
          @click="decideGoalCancellation(true)"
        >
          {{ t("shell.cancellation.approve") }}
        </v-btn>
      </div>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const { t } = useI18n();
const cancellationDecision = ref<boolean>();
const pendingCancellationLimits = computed(
  () => store.pendingGoalCancellation?.commandLimits?.[store.signedInMemberId],
);

async function decideGoalCancellation(approved: boolean) {
  const goal = store.pendingGoalCancellation;

  if (!goal || cancellationDecision.value !== undefined) {
    return;
  }

  cancellationDecision.value = approved;

  try {
    await store.decideGoalCancellation(goal.id, approved);
  } finally {
    cancellationDecision.value = undefined;
  }
}
</script>
