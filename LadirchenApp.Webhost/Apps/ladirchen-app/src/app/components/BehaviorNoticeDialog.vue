<template>
  <v-dialog :model-value="store.behaviorNoticeAnimation.visible" max-width="390" persistent>
    <v-card :key="store.behaviorNoticeAnimation.version" class="behavior-notice-card pa-6 text-center" rounded="xl">
      <div class="behavior-rain" aria-hidden="true">
        <span>💧</span>
        <span>💧</span>
        <span>💧</span>
      </div>
      <div class="behavior-face" aria-hidden="true">{{ behaviorNoticeFace }}</div>
      <p class="eyebrow mt-3 mb-1">{{ t("shell.behavior.eyebrow") }}</p>
      <h2>{{ behaviorNoticeTitle }}</h2>
      <p class="text-body-2 text-medium-emphasis mt-3">{{ store.behaviorNoticeAnimation.reason }}</p>
      <strong v-if="store.behaviorNoticeAnimation.deductedAmount > 0" class="behavior-deduction d-block mt-4">
        −{{ store.behaviorNoticeAnimation.deductedAmount }} Ladirchen
      </strong>
      <p v-if="store.behaviorNoticeAnimation.level === 'red'" class="behavior-blocked mt-3">
        {{ t("shell.behavior.tasksBlocked") }}
      </p>
      <v-btn class="mt-5" color="primary" rounded="lg" variant="flat" width="100%" @click="store.dismissBehaviorNotice">
        {{ t("shell.behavior.dismiss") }}
      </v-btn>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const { t } = useI18n();
const behaviorNoticeTitle = computed(() => {
  if (store.behaviorNoticeAnimation.level === "yellow") {
    return t("shell.behavior.yellow.title");
  }

  if (store.behaviorNoticeAnimation.level === "orange") {
    return t("shell.behavior.orange.title");
  }

  return t("shell.behavior.red.title");
});
const behaviorNoticeFace = computed(() => (store.behaviorNoticeAnimation.level === "yellow" ? "😟" : "😢"));
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.behavior-notice-card {
  --uno: position-relative overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-color-info-muted) 20%, transparent);
  background: linear-gradient(155deg, var(--lad-surface-soft), var(--lad-color-info-subtle));
  box-shadow: 0 24px 60px color-mix(in srgb, var(--lad-text) 24%, transparent);
}
.behavior-notice-card h2 {
  --uno: position-relative ma-0;
  font-size: rem(25);
  letter-spacing: -0.035em;
}
.behavior-face {
  --uno: position-relative;
  font-size: rem(68);
  line-height: 1;
  animation: behavior-face-arrive 700ms var(--lad-easing-pop);
}
.behavior-deduction {
  color: var(--lad-color-danger);
  font-size: rem(26);
}
.behavior-blocked {
  padding: 10px;
  color: var(--lad-color-danger);
  border-radius: 14px;
  background: color-mix(in srgb, var(--lad-color-danger-soft) 55%, transparent);
  font-size: rem(12);
  font-weight: var(--lad-font-weight-heavy);
}
.behavior-rain span {
  --uno: position-absolute;
  color: var(--lad-color-info-muted);
  font-size: rem(17);
  animation: behavior-rain 1.5s ease-in infinite;
}
.behavior-rain span:nth-child(1) {
  top: 9%;
  left: 16%;
}
.behavior-rain span:nth-child(2) {
  top: 18%;
  right: 13%;
  animation-delay: -0.5s;
}
.behavior-rain span:nth-child(3) {
  top: 47%;
  left: 8%;
  animation-delay: -1s;
}
@keyframes behavior-face-arrive {
  from {
    opacity: 0;
    transform: translateY(-30px) scale(0.5);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes behavior-rain {
  from {
    opacity: 0;
    transform: translateY(-12px);
  }
  35% {
    opacity: 1;
  }
  to {
    opacity: 0;
    transform: translateY(34px);
  }
}
</style>
