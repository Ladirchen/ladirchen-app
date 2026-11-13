<template>
  <v-dialog :model-value="store.houseLevelAnimation.visible" max-width="410" persistent>
    <v-card :key="store.houseLevelAnimation.version" class="house-level-card pa-6 text-center" rounded="xl">
      <div class="house-level-confetti" aria-hidden="true">
        <span>✦</span><span>★</span><span>◆</span><span>✧</span><span>★</span>
      </div>
      <div class="house-level-icon" aria-hidden="true">{{ stage.icon }}</div>
      <p class="eyebrow mt-4 mb-1">{{ t("shell.houseUpgrade.eyebrow") }}</p>
      <h2>{{ t("shell.houseUpgrade.title") }}</h2>
      <strong class="house-level-name">{{ t(stage.nameKey) }}</strong>
      <p class="text-body-small text-medium-emphasis mt-3">
        {{ t("shell.houseUpgrade.description") }}
      </p>
      <v-btn
        class="mt-5"
        color="primary"
        rounded="lg"
        variant="flat"
        width="100%"
        @click="store.dismissHouseLevelAnimation"
      >
        {{ t("shell.houseUpgrade.dismiss") }}
      </v-btn>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from "@/domain/house/catalog";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const { t } = useI18n();
const stage = computed(() => HOUSE_STAGES[store.houseLevelAnimation.level] ?? DEFAULT_HOUSE_STAGE);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.house-level-card {
  --uno: position-relative overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-color-primary) 24%, transparent);
  background:
    radial-gradient(circle at 50% 18%, color-mix(in srgb, var(--lad-color-reward) 34%, transparent), transparent 30%),
    linear-gradient(155deg, var(--lad-surface-soft), var(--lad-color-reward-soft));
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-color-primary) 14%, transparent),
    0 28px 60px color-mix(in srgb, var(--lad-text) 25%, transparent);
}

.house-level-card h2 {
  --uno: ma-0;
  font-size: rem(27);
  letter-spacing: -0.04em;
}

.house-level-icon {
  width: 7rem;
  height: 7rem;
  margin: 0.75rem auto 0;
  --uno: d-grid place-center;
  border: 4px solid var(--lad-border-on-accent);
  border-radius: 2rem;
  background: var(--lad-surface-raised);
  box-shadow: 0 7px 0 color-mix(in srgb, var(--lad-color-primary) 16%, transparent);
  font-size: 4rem;
}

.house-level-name {
  --uno: d-block;
  margin-top: 0.35rem;
  color: var(--lad-color-primary-deep);
}

.house-level-confetti span {
  --uno: position-absolute;
  color: var(--lad-color-reward);
  animation: house-confetti 1.4s both var(--lad-easing-pop);
}

.house-level-confetti span:nth-child(1) {
  top: 8%;
  left: 9%;
}
.house-level-confetti span:nth-child(2) {
  top: 16%;
  right: 10%;
  animation-delay: 100ms;
}
.house-level-confetti span:nth-child(3) {
  top: 42%;
  left: 5%;
  animation-delay: 180ms;
}
.house-level-confetti span:nth-child(4) {
  top: 48%;
  right: 6%;
  animation-delay: 260ms;
}
.house-level-confetti span:nth-child(5) {
  top: 4%;
  left: 48%;
  animation-delay: 340ms;
}

@keyframes house-confetti {
  from {
    opacity: 0;
    transform: scale(0) rotate(-25deg);
  }
  55% {
    opacity: 1;
    transform: scale(1.4) rotate(15deg);
  }
  to {
    opacity: 0.8;
    transform: scale(1) rotate(0);
  }
}

@include reduced-motion {
  .house-level-confetti span {
    animation: none;
  }
}
</style>
