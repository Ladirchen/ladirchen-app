<template>
  <BrandedCard class="time-reward-card pa-4" tone="shop">
    <div class="d-flex align-start justify-space-between ga-3">
      <div class="d-flex align-center ga-3 min-w-0">
        <span class="time-reward-icon" aria-hidden="true">{{ reward.icon }}</span>
        <div class="min-w-0">
          <strong class="d-block text-truncate">{{ reward.title }}</strong>
          <span class="text-caption text-medium-emphasis">{{ ownerName }}</span>
        </div>
      </div>
      <v-chip :color="statusColor" size="small" variant="tonal">{{ statusLabel }}</v-chip>
    </div>

    <div class="time-reward-countdown my-4" role="timer" :aria-label="t('shop.timer.remaining')">
      {{ formattedRemaining }}
    </div>

    <div class="d-flex flex-wrap justify-center ga-2">
      <v-btn
        v-if="reward.canPause && effectiveStatus === 'running'"
        prepend-icon="i-mdi:pause"
        rounded="lg"
        size="small"
        variant="tonal"
        @click="store.pauseShopTimer(reward.requestId)"
      >
        {{ t("shop.timer.pause") }}
      </v-btn>
      <v-btn
        v-if="reward.canResume && effectiveStatus === 'paused'"
        color="primary"
        prepend-icon="i-mdi:play"
        rounded="lg"
        size="small"
        variant="flat"
        @click="store.resumeShopTimer(reward.requestId)"
      >
        {{ t("shop.timer.resume") }}
      </v-btn>
      <v-btn
        v-if="reward.canReset"
        prepend-icon="i-mdi:restart"
        rounded="lg"
        size="small"
        variant="text"
        @click="store.resetShopTimer(reward.requestId)"
      >
        {{ t("shop.timer.reset") }}
      </v-btn>
      <v-btn
        v-if="reward.canCancel"
        color="error"
        prepend-icon="i-mdi:close-circle-outline"
        rounded="lg"
        size="small"
        variant="text"
        @click="cancelTimeReward"
      >
        {{ t("shop.timer.cancel") }}
      </v-btn>
    </div>
  </BrandedCard>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { ShopTimeReward } from "@/domain/shop";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";
import { useFamilyWorldStore } from "@/stores/family-world";

interface Props {
  readonly ownerName: string;
  readonly reward: ShopTimeReward;
}

const props = defineProps<Props>();
const store = useFamilyWorldStore();
const { t } = useI18n();
const now = ref(Date.now());
const timer = window.setInterval(() => {
  now.value = Date.now();
}, 1_000);

const remainingSeconds = computed(() => {
  if (props.reward.status !== "running" || !props.reward.endsAt) {
    return props.reward.remainingSeconds;
  }

  return Math.max(0, Math.ceil((Date.parse(props.reward.endsAt) - now.value) / 1_000));
});
const effectiveStatus = computed(() => (remainingSeconds.value === 0 ? "expired" : props.reward.status));
const formattedRemaining = computed(() => {
  const seconds = remainingSeconds.value;
  const hours = Math.floor(seconds / 3_600);
  const minutes = Math.floor((seconds % 3_600) / 60);
  const remaining = seconds % 60;

  return [hours, minutes, remaining].map((value) => String(value).padStart(2, "0")).join(":");
});
const statusColor = computed(() => {
  if (effectiveStatus.value === "running") {
    return "success";
  }

  return effectiveStatus.value === "paused" ? "warning" : "default";
});
const statusLabel = computed(() => t(`shop.timer.status.${effectiveStatus.value}`));

function cancelTimeReward() {
  if (window.confirm(t("shop.timer.cancelConfirm"))) {
    void store.cancelShopTimer(props.reward.requestId);
  }
}

onBeforeUnmount(() => window.clearInterval(timer));
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.time-reward-card {
  min-width: min(100%, rem(270));
}
.time-reward-icon {
  display: grid;
  width: rem(42);
  height: rem(42);
  place-items: center;
  border-radius: rem(14);
  background: var(--lad-surface-soft);
  font-size: rem(24);
}
.time-reward-countdown {
  color: var(--lad-color-primary-deep);
  font-size: clamp(2rem, 8vw, 3rem);
  font-variant-numeric: tabular-nums;
  font-weight: var(--lad-font-weight-heavy);
  letter-spacing: 0.06em;
  text-align: center;
}
</style>
