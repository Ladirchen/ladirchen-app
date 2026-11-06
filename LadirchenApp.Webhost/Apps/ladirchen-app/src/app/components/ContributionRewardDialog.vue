<template>
  <v-dialog :model-value="store.rewardAnimation.visible" max-width="390" persistent>
    <v-card :key="store.rewardAnimation.version" class="contribution-reward-card pa-6 text-center" rounded="xl">
      <div class="reward-confetti" aria-hidden="true">
        <span>✦</span>
        <span>★</span>
        <span>●</span>
        <span>◆</span>
        <span>✧</span>
        <span>★</span>
      </div>
      <div
        class="reward-hero"
        :class="{ 'reward-hero--double': store.rewardAnimation.multiplier > 1 }"
        aria-hidden="true"
      >
        <v-icon v-if="store.rewardAnimation.multiplier > 1" class="reward-rocket" icon="i-mdi:rocket-launch" />
        <LadirchenCoin animated />
        <strong v-if="store.rewardAnimation.multiplier > 1">×{{ store.rewardAnimation.multiplier }}</strong>
      </div>
      <p class="eyebrow reward-eyebrow mt-4 mb-1">{{ t("shell.reward.confirmed") }}</p>
      <h2>{{ t("shell.reward.success") }}</h2>
      <p class="reward-task-title">{{ store.rewardAnimation.title }}</p>
      <div class="reward-results mt-4">
        <span class="reward-result reward-result--coins">
          <small>{{ t("shell.reward.earned") }}</small>
          <strong>{{ t("shell.reward.coins", { value: store.rewardAnimation.value }) }}</strong>
        </span>
        <span v-if="store.rewardAnimation.energy > 0" class="reward-result reward-result--energy">
          <small>{{ t("shell.reward.house") }}</small>
          <strong>{{ t("shell.reward.energy", { value: store.rewardAnimation.energy }) }}</strong>
        </span>
      </div>
      <div
        v-if="store.rewardAnimation.stars > 0"
        class="reward-stars mt-4"
        :aria-label="t('shell.reward.stars', { value: store.rewardAnimation.stars })"
      >
        <v-icon
          v-for="star in 5"
          :key="star"
          :class="{ active: star <= store.rewardAnimation.stars }"
          icon="i-mdi:star"
        />
      </div>
      <p v-if="store.rewardAnimation.multiplier > 1" class="reward-double-copy mt-3">
        <v-icon icon="i-mdi:creation" size="16" />
        {{ t("shell.reward.double") }}
      </p>
      <v-btn
        class="reward-dismiss mt-5"
        color="primary"
        rounded="lg"
        variant="flat"
        width="100%"
        @click="store.dismissRewardAnimation"
      >
        {{ t("shell.reward.dismiss") }}
      </v-btn>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n";
import LadirchenCoin from "@/shared/components/LadirchenCoin.vue";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const { t } = useI18n();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.contribution-reward-card {
  --uno: position-relative overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-color-info-muted) 20%, transparent);
  background:
    radial-gradient(circle at 88% 5%, color-mix(in srgb, var(--lad-color-reward) 30%, transparent), transparent 27%),
    linear-gradient(155deg, var(--lad-surface-soft), var(--lad-surface-soft) 58%, var(--lad-color-reward-soft));
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent),
    0 28px 60px color-mix(in srgb, var(--lad-text) 25%, transparent);
}
.contribution-reward-card h2 {
  --uno: position-relative ma-0;
  color: var(--lad-text);
  font-size: rem(26);
  letter-spacing: -0.04em;
}
.reward-eyebrow {
  color: var(--lad-color-primary-strong);
}
.reward-task-title {
  margin: 4px 0 0;
  color: var(--lad-muted);
  font-size: 0.75rem;
  font-weight: 750;
}
.reward-hero {
  width: 100px;
  height: 100px;
  margin: 14px auto 0;
  --uno: position-relative d-grid place-center;
  border: 4px solid var(--lad-border-on-accent);
  border-radius: 32px;
  background: linear-gradient(145deg, var(--lad-surface-soft), var(--lad-color-reward-pale));
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent),
    0 15px 28px color-mix(in srgb, var(--lad-color-info-deep) 12%, transparent);
  animation: reward-hero-arrive 0.75s cubic-bezier(0.2, 0.9, 0.25, 1) both;
}
.reward-hero :deep(.ladirchen-coin) {
  transform: scale(1.65);
  animation: reward-coin-spin 1.25s 0.45s ease-in-out;
}
.reward-hero--double {
  background:
    radial-gradient(circle at 25% 18%, var(--lad-color-reward-soft), transparent 30%),
    linear-gradient(
      145deg,
      var(--lad-color-info-subtle),
      var(--lad-color-bonus-info) 60%,
      var(--lad-color-bonus-highlight)
    );
  box-shadow:
    0 7px 0 var(--lad-color-bonus-muted),
    0 16px 30px color-mix(in srgb, var(--lad-color-info-deep) 20%, transparent);
}
.reward-hero > strong {
  min-width: 45px;
  height: 35px;
  padding: 4px 8px;
  --uno: position-absolute d-grid place-center;
  right: -17px;
  bottom: -10px;
  color: var(--lad-text-inverse);
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 13px;
  background: linear-gradient(145deg, var(--lad-color-primary-highlight), var(--lad-color-info-shadow));
  box-shadow: 0 3px 0 var(--lad-color-info-deep);
  font-size: rem(19);
  animation: reward-double-pop 0.7s 0.65s cubic-bezier(0.2, 0.9, 0.25, 1) both;
}
.reward-rocket {
  --uno: position-absolute;
  top: -17px;
  left: -16px;
  z-index: 2;
  color: var(--lad-color-reward-pale);
  font-size: rem(34);
  filter: drop-shadow(0 3px 1px color-mix(in srgb, var(--lad-color-bonus-info-strong) 30%, transparent));
  animation: reward-rocket-flight 1.8s 0.35s ease-in-out infinite;
}
.reward-results {
  --uno: d-grid;
  grid-template-columns: 1.25fr 1fr;
  gap: 8px;
}
.reward-result {
  min-height: 68px;
  padding: 9px;
  --uno: d-grid place-center;
  border: 2px solid color-mix(in srgb, var(--lad-color-reward-accent) 20%, transparent);
  border-radius: 17px;
  background: linear-gradient(145deg, var(--lad-color-reward-soft), var(--lad-color-reward-pale));
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-reward-deep) 15%, transparent);
}
.reward-result--energy {
  border-color: color-mix(in srgb, var(--lad-color-primary-muted) 20%, transparent);
  background: linear-gradient(145deg, var(--lad-surface-soft), var(--lad-color-reward-pale));
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-primary-deep) 12%, transparent);
}
.reward-result small,
.reward-result strong {
  --uno: d-block;
}
.reward-result small {
  color: var(--lad-color-accent-warm-strong);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: uppercase;
  letter-spacing: 0.025em;
}
.reward-result strong {
  margin-top: 3px;
  color: var(--lad-color-reward-strong);
  font-size: rem(17);
  font-weight: var(--lad-font-weight-black);
  line-height: 1.08;
}
.reward-result--energy strong {
  color: var(--lad-color-primary-deep);
}
.reward-stars {
  --uno: d-flex justify-center;
  gap: 2px;
}
.reward-stars :deep(.v-icon) {
  color: var(--lad-color-primary-soft);
  opacity: 0.58;
}
.reward-stars :deep(.v-icon.active) {
  color: var(--lad-color-reward-border);
  opacity: 1;
  filter: drop-shadow(0 2px 2px color-mix(in srgb, var(--lad-color-reward-deep) 25%, transparent));
  animation: reward-star-pop 1.7s ease-in-out infinite;
}
.reward-double-copy {
  --uno: d-flex align-center justify-center;
  gap: 5px;
  color: var(--lad-color-info-deep);
  font-size: rem(11);
  font-weight: var(--lad-font-weight-black);
}
.reward-dismiss {
  min-height: 49px;
  background: linear-gradient(145deg, var(--lad-color-primary-highlight), var(--lad-color-primary-strong));
  box-shadow:
    0 5px 0 var(--lad-color-primary-deep),
    0 10px 18px color-mix(in srgb, var(--lad-color-primary-deep) 18%, transparent);
  font-weight: var(--lad-font-weight-black);
  text-transform: none;
  letter-spacing: 0;
}
.reward-confetti span {
  --uno: position-absolute;
  z-index: 0;
  color: var(--lad-color-reward-border);
  font-size: 1rem;
  animation: reward-confetti 1.7s ease-in-out infinite;
}
.reward-confetti span:nth-child(1) {
  top: 8%;
  left: 12%;
}
.reward-confetti span:nth-child(2) {
  top: 21%;
  right: 10%;
  color: var(--lad-color-bonus-info);
  animation-delay: -0.3s;
}
.reward-confetti span:nth-child(3) {
  top: 37%;
  left: 7%;
  color: var(--lad-color-primary-highlight);
  animation-delay: -0.6s;
}
.reward-confetti span:nth-child(4) {
  top: 48%;
  right: 6%;
  color: var(--lad-color-bonus-highlight);
  animation-delay: -0.9s;
}
.reward-confetti span:nth-child(5) {
  top: 68%;
  left: 11%;
  animation-delay: -1.2s;
}
.reward-confetti span:nth-child(6) {
  top: 74%;
  right: 12%;
  color: var(--lad-color-info-muted);
  animation-delay: -1.45s;
}
@keyframes reward-hero-arrive {
  from {
    opacity: 0;
    transform: translateY(-55px) scale(0.45) rotate(-14deg);
  }
  72% {
    transform: translateY(6px) scale(1.1) rotate(4deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes reward-coin-spin {
  to {
    transform: scale(1.65) rotateY(720deg);
  }
}
@keyframes reward-double-pop {
  from {
    opacity: 0;
    transform: scale(0.3) rotate(-14deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes reward-rocket-flight {
  0%,
  65%,
  100% {
    transform: translate(0, 0) rotate(-8deg);
  }
  75% {
    transform: translate(5px, -7px) rotate(5deg) scale(1.12);
  }
  87% {
    transform: translate(-1px, 1px) rotate(-4deg);
  }
}
@keyframes reward-star-pop {
  0%,
  100% {
    transform: scale(0.9) rotate(-4deg);
  }
  50% {
    transform: scale(1.15) rotate(5deg);
  }
}
@keyframes reward-confetti {
  0%,
  100% {
    opacity: 0.25;
    transform: translateY(0) rotate(0) scale(0.7);
  }
  50% {
    opacity: 1;
    transform: translateY(7px) rotate(25deg) scale(1.15);
  }
}
@include respond-down(narrow) {
  .reward-results {
    grid-template-columns: 1fr;
  }
}
@include reduced-motion {
  .reward-hero,
  .reward-hero :deep(.ladirchen-coin),
  .reward-hero > strong,
  .reward-rocket,
  .reward-stars :deep(.v-icon.active),
  .reward-confetti span {
    animation: none;
  }
}
</style>
