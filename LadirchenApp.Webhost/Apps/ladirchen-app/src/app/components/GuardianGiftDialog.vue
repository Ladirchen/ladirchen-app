<template>
  <v-dialog :model-value="store.guardianGiftAnimation.visible" max-width="390" persistent>
    <v-card :key="store.guardianGiftAnimation.version" class="guardian-gift-card pa-6 text-center" rounded="xl">
      <div class="gift-confetti" aria-hidden="true">
        <span>✦</span>
        <span>●</span>
        <span>★</span>
        <span>◆</span>
        <span>✦</span>
      </div>
      <div class="gift-coin"><LadirchenCoin /></div>
      <p class="eyebrow mt-4 mb-1">{{ t("shell.gift.eyebrow") }}</p>
      <h2>{{ t("shell.gift.title", { guardian: store.guardianGiftAnimation.guardianName }) }}</h2>
      <strong class="gift-amount">+{{ store.guardianGiftAnimation.amount }}</strong>
      <p v-if="store.guardianGiftAnimation.destination === 'goal'" class="text-body-small text-medium-emphasis mt-2">
        {{ t("shell.gift.goal", { goal: store.guardianGiftAnimation.goalTitle }) }}
      </p>
      <p v-else class="text-body-small text-medium-emphasis mt-2">{{ guardianGiftBalanceMessage }}</p>
      <v-btn class="mt-5" color="primary" rounded="lg" variant="flat" width="100%" @click="store.dismissGuardianGift">
        {{ t("shell.gift.dismiss") }}
      </v-btn>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import LadirchenCoin from "@/shared/components/LadirchenCoin.vue";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const { t } = useI18n();
const guardianGiftBalanceMessage = computed(() =>
  store.guardianGiftAnimation.goalTitle
    ? t("shell.gift.balance", { goal: store.guardianGiftAnimation.goalTitle })
    : t("shell.gift.balanceDefault"),
);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.guardian-gift-card {
  --uno: position-relative overflow-hidden;
  background: linear-gradient(160deg, var(--lad-surface), var(--lad-color-reward-soft) 72%);
  border: 1px solid color-mix(in srgb, var(--lad-color-primary) 25%, transparent);
}
.guardian-gift-card h2 {
  --uno: position-relative ma-0;
  font-size: rem(23);
  letter-spacing: -0.035em;
}
.gift-coin {
  width: 82px;
  height: 82px;
  --uno: position-relative d-grid place-center;
  z-index: 2;
  margin: 15px auto 0;

  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
  box-shadow: 0 10px 28px color-mix(in srgb, var(--lad-color-reward-shadow) 20%, transparent);
  animation: gift-coin-arrive 850ms var(--lad-easing-pop);
}
.gift-coin :deep(.ladirchen-coin) {
  transform: scale(1.65);
  animation: gift-coin-spin 1.2s 700ms ease-in-out;
}
.gift-amount {
  --uno: d-block position-relative mt-3;
  color: var(--lad-color-primary-strong);
  font-size: rem(35);
  animation: gift-amount-pop 700ms 500ms both var(--lad-easing-pop);
}
.gift-confetti span {
  --uno: position-absolute;
  z-index: 1;
  color: var(--lad-color-reward-border);
  font-size: rem(18);
  animation: gift-confetti 1.8s infinite ease-in-out;
}
.gift-confetti span:nth-child(1) {
  top: 11%;
  left: 13%;
}
.gift-confetti span:nth-child(2) {
  top: 25%;
  right: 12%;
  color: var(--lad-color-bonus-info);
  animation-delay: 0.2s;
}
.gift-confetti span:nth-child(3) {
  top: 8%;
  right: 30%;
  color: var(--lad-color-danger-soft);
  animation-delay: 0.45s;
}
.gift-confetti span:nth-child(4) {
  top: 41%;
  left: 8%;
  color: var(--lad-color-primary);
  animation-delay: 0.7s;
}
.gift-confetti span:nth-child(5) {
  top: 49%;
  right: 7%;
  animation-delay: 0.9s;
}
@keyframes gift-coin-arrive {
  from {
    opacity: 0;
    transform: translateY(-70px) scale(0.4) rotate(-180deg);
  }
  70% {
    transform: translateY(8px) scale(1.12) rotate(12deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes gift-coin-spin {
  to {
    transform: scale(1.65) rotateY(720deg);
  }
}
@keyframes gift-amount-pop {
  from {
    opacity: 0;
    transform: scale(0.4);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes gift-confetti {
  0%,
  100% {
    opacity: 0.35;
    transform: translateY(0) rotate(0);
  }
  50% {
    opacity: 1;
    transform: translateY(7px) rotate(25deg);
  }
}
@include reduced-motion {
  .gift-coin,
  .gift-coin :deep(.ladirchen-coin),
  .gift-amount,
  .gift-confetti span {
    animation: none;
  }
}
</style>
