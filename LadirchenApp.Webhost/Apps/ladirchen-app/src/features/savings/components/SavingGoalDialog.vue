<template>
  <v-dialog :model-value="modelValue" max-width="430" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card class="goal-dialog-card" rounded="xl">
      <header class="goal-dialog-header">
        <div class="goal-dialog-visual" aria-hidden="true">
          <span class="goal-visual-icon">{{ goal.icon }}</span>
          <i /><i /><i />
        </div>
        <div class="goal-dialog-heading">
          <p class="eyebrow mb-1">{{ t('savings.goalDialog.eyebrow') }}</p>
          <v-card-title class="pa-0">{{ t(isEditing ? 'savings.goalDialog.editTitle' : 'savings.goalDialog.createTitle') }}</v-card-title>
          <p>{{ t(isEditing ? 'savings.goalDialog.editDescription' : 'savings.goalDialog.createDescription') }}</p>
        </div>
        <v-btn class="goal-dialog-close" :aria-label="t('savings.goalDialog.close')" icon="i-mdi:close" size="small" variant="text" @click="close" />
      </header>

      <v-card-text class="goal-dialog-content">
        <div class="goal-step">
          <span class="goal-step-number">1</span>
          <div><strong>{{ t('savings.goalDialog.steps.wish.title') }}</strong><small>{{ t('savings.goalDialog.steps.wish.description') }}</small></div>
        </div>
        <div class="goal-icon-picker mb-3" :aria-label="t('savings.goalDialog.iconAria')">
          <button v-for="icon in iconOptions" :key="icon" :class="{ active: goal.icon === icon }" type="button" @click="goal.icon = icon">{{ icon }}</button>
        </div>
        <v-text-field v-model="goal.title" autofocus class="goal-field" density="comfortable" hide-details :label="t('savings.goalDialog.wishLabel')" :placeholder="t('savings.goalDialog.wishPlaceholder')" variant="outlined" />

        <div class="goal-step mt-5">
          <span class="goal-step-number">2</span>
          <div><strong>{{ t('savings.goalDialog.steps.amount.title') }}</strong><small>{{ t('savings.goalDialog.steps.amount.description') }}</small></div>
        </div>
        <div class="goal-target-picker" role="group" :aria-label="t('savings.goalDialog.amountAria')">
          <button :aria-label="t('savings.goalDialog.decrease')" :disabled="goal.target <= minimumTarget" type="button" @click="adjustTarget(-10)">−</button>
          <div>
            <small>{{ t('savings.goalDialog.target') }}</small>
            <output aria-live="polite"><LadirchenCoin small /><strong>{{ validTargetPreview }}</strong></output>
          </div>
          <button :aria-label="t('savings.goalDialog.increase')" type="button" @click="adjustTarget(10)">+</button>
          <i class="target-spark target-spark--one" aria-hidden="true">✦</i>
          <i class="target-spark target-spark--two" aria-hidden="true">✧</i>
        </div>

        <div class="goal-step mt-5">
          <span class="goal-step-number">3</span>
          <div><strong>{{ t('savings.goalDialog.steps.visibility.title') }}</strong><small>{{ t('savings.goalDialog.steps.visibility.description') }}</small></div>
        </div>
        <div class="visibility-options">
          <button v-for="option in visibilityOptions" :key="option.value" :class="{ active: goal.visibility === option.value }" type="button" @click="goal.visibility = option.value">
            <v-icon :icon="option.icon" size="21" /><span><strong>{{ option.shortTitle }}</strong><small>{{ option.subtitle }}</small></span><v-icon v-if="goal.visibility === option.value" class="visibility-check" icon="i-mdi:check-circle" size="18" />
          </button>
        </div>

        <div v-if="!isEditing" class="starter-bonus mt-4">
          <span class="bonus-gift" aria-hidden="true">🎁</span>
          <p><small>{{ t('savings.goalDialog.starter.eyebrow') }}</small><strong>{{ t('savings.goalDialog.starter.title') }}</strong><span>{{ t('savings.goalDialog.starter.description') }}</span></p>
          <span class="bonus-coin" aria-hidden="true"><LadirchenCoin small /><em>5</em></span>
          <span class="bonus-spark bonus-spark--one" aria-hidden="true">✦</span>
          <span class="bonus-spark bonus-spark--two" aria-hidden="true">✧</span>
        </div>

        <p v-if="isEditing && minimumTarget > 10" class="saved-hint mt-4">{{ t('savings.goalDialog.savedHint', { amount: minimumTarget }) }}</p>

        <v-alert v-if="removeConfirmation" class="mt-4" color="error" density="compact" variant="tonal">
          {{ t('savings.goalDialog.removeWarning') }}
        </v-alert>
      </v-card-text>

      <v-card-actions class="goal-dialog-actions">
        <v-btn v-if="isEditing && !removeConfirmation" color="error" rounded="lg" variant="text" @click="removeConfirmation = true">{{ t('savings.goalDialog.remove') }}</v-btn>
        <template v-if="removeConfirmation">
          <v-btn rounded="lg" variant="text" @click="removeConfirmation = false">{{ t('savings.goalDialog.keep') }}</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="remove">{{ t('savings.goalDialog.confirmRemove') }}</v-btn>
        </template>
        <template v-else>
          <v-btn class="create-goal-button" color="primary" :disabled="!isValid" rounded="lg" variant="flat" @click="submit"><span aria-hidden="true">★</span>{{ t(isEditing ? 'common.save' : 'savings.goalDialog.start') }}</v-btn>
        </template>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { ladiGuideController } from '@/shared/services/ladi-guide-controller';

import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import type { GoalVisibility, NewGoal } from '@/domain/savings/types';

const props = withDefaults(defineProps<{
  modelValue: boolean;
  initialGoal?: NewGoal | null;
  minimumTarget?: number;
}>(), {
  initialGoal: null,
  minimumTarget: 10,
});
const emit = defineEmits<{
  'update:modelValue': [value: boolean];
  submit: [goal: NewGoal];
  remove: [];
}>();
const { t } = useI18n();

const initialGoal = (): NewGoal => props.initialGoal
  ? { ...props.initialGoal }
  : { title: '', icon: '✨', target: 100, visibility: 'family' };
const goal = reactive<NewGoal>(initialGoal());
const removeConfirmation = ref(false);
const iconOptions = ['✨', '🚲', '📷', '🎨', '🧱', '🦒', '🎮', '🎵'];
const visibilityOptions = computed<{ icon: string; shortTitle: string; subtitle: string; value: GoalVisibility }[]>(() => [
  { icon: 'i-mdi:home-heart', shortTitle: t('savings.goalDialog.visibility.family.title'), subtitle: t('savings.goalDialog.visibility.family.description'), value: 'family' },
  { icon: 'i-mdi:lock-outline', shortTitle: t('savings.goalDialog.visibility.private.title'), subtitle: t('savings.goalDialog.visibility.private.description'), value: 'private' },
  { icon: 'i-mdi:shield-account-outline', shortTitle: t('savings.goalDialog.visibility.guardians.title'), subtitle: t('savings.goalDialog.visibility.guardians.description'), value: 'guardians' },
]);
const isEditing = computed(() => props.initialGoal !== null);
const isValid = computed(() => goal.title.trim().length > 0 && Number.isFinite(goal.target) && goal.target >= props.minimumTarget);
const validTargetPreview = computed(() => Number.isFinite(goal.target) ? Math.max(0, Math.round(goal.target)) : 0);

const reset = () => Object.assign(goal, initialGoal());
const adjustTarget = (change: number) => {
  goal.target = Math.max(props.minimumTarget, validTargetPreview.value + change);
};
const close = () => emit('update:modelValue', false);
const remove = () => {
  emit('remove');
  close();
};
const submit = () => {
  if (!isValid.value) return;
  emit('submit', { ...goal, title: goal.title.trim(), target: Math.round(goal.target) });
  emit('update:modelValue', false);
};

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    reset();
    removeConfirmation.value = false;
    window.setTimeout(() => ladiGuideController.say({
      heading: t('savings.goalDialog.guideTitle'),
      message: t('savings.goalDialog.guideMessage'),
    }), 180);
  }
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.goal-dialog-card {
  max-height: calc(100dvh - 24px);
  @apply overflow-hidden;
  border: 2px solid var(--lad-border-success);
  background: var(--lad-gradient-surface);
  box-shadow:
    0 10px 0 var(--lad-shadow-raised-success),
    var(--lad-shadow-dialog);
}
.goal-dialog-header {
  padding: 17px 50px 15px 18px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 12px;
  border-bottom: 2px solid
    color-mix(in srgb, var(--lad-color-info) 12%, transparent);
  background:
    radial-gradient(
      circle at 94% 5%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 25%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-soft)
    );
}
.goal-dialog-header::after {
  content: "✦";
  @apply position-absolute;
  top: 9px;
  right: 49px;
  color: var(--lad-color-reward-border);
  animation: goal-twinkle 1.7s ease-in-out infinite;
}
.goal-dialog-visual {
  width: 66px;
  height: 66px;
  @apply position-relative d-grid place-center flex-shrink-0;
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 23px;
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info) 65%,
    var(--lad-color-bonus)
  );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-color-info-strong) 20%, transparent),
    0 10px 18px color-mix(in srgb, var(--lad-color-info-deep) 15%, transparent);
  transform: rotate(-5deg);
  animation: goal-visual-float 2.8s ease-in-out infinite;
}
.goal-visual-icon {
  font-size: 1.9375rem;
  filter: drop-shadow(
    0 2px 0 color-mix(in srgb, var(--lad-surface-raised) 35%, transparent)
  );
}
.goal-dialog-visual i {
  width: 6px;
  height: 6px;
  @apply position-absolute;
  border-radius: 50%;
  background: var(--lad-color-reward-pale);
  box-shadow: 0 0 7px var(--lad-surface-raised);
  animation: goal-twinkle 1.5s ease-in-out infinite;
}
.goal-dialog-visual i:nth-of-type(1) {
  top: 7px;
  right: 8px;
}
.goal-dialog-visual i:nth-of-type(2) {
  bottom: 8px;
  left: 8px;
  animation-delay: -0.5s;
}
.goal-dialog-visual i:nth-of-type(3) {
  top: 28px;
  left: -5px;
  animation-delay: -1s;
}
.goal-dialog-heading {
  @apply min-w-0;
}
.goal-dialog-heading :deep(.v-card-title) {
  font-size: 1.3125rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: -0.035em;
}
.goal-dialog-heading > p:last-child {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.625rem;
  line-height: 1.35;
}
.goal-dialog-close {
  @apply position-absolute;
  top: 10px;
  right: 9px;
}
.goal-dialog-content {
  padding: 17px 18px 12px;
}
.goal-step {
  margin-bottom: 9px;
  @apply d-flex align-center;
  gap: 9px;
}
.goal-step-number {
  width: 28px;
  height: 28px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-text-inverse);
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 10px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary),
    var(--lad-color-primary-strong)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-deep) 20%, transparent);
  font-size: 0.6875rem;
  font-weight: var(--lad-font-weight-black);
  transform: rotate(-4deg);
}
.goal-step > div strong,
.goal-step > div small {
  @apply d-block;
}
.goal-step > div strong {
  font-size: 0.75rem;
}
.goal-step > div small {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.goal-icon-picker {
  @apply d-grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 5px;
}
.goal-icon-picker button {
  aspect-ratio: 1;
  @apply d-grid place-center cursor-pointer;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 12%, transparent);
  border-radius: 11px;
  background: var(--lad-surface-soft);
  font-size: 1.125rem;
  transition:
    transform 0.16s ease,
    background 0.16s ease;
}
.goal-icon-picker button:hover {
  transform: translateY(-2px) rotate(-4deg);
}
.goal-icon-picker button.active {
  border-color: color-mix(
    in srgb,
    var(--lad-color-primary-muted) 40%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-strong) 15%, transparent);
  transform: translateY(-2px) rotate(-4deg);
}
.goal-field :deep(.v-field) {
  border-radius: 16px;
  background: color-mix(in srgb, var(--lad-surface-raised) 90%, transparent);
}
.goal-target-picker {
  min-height: 72px;
  padding: 8px 13px;
  @apply position-relative d-grid align-center overflow-hidden;
  grid-template-columns: 52px 1fr 52px;
  gap: 10px;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-info-strong) 20%, transparent);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 88% 18%,
      color-mix(in srgb, var(--lad-color-reward) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(145deg, var(--lad-surface-soft), var(--lad-surface-soft));
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-color-info-shadow) 10%, transparent),
    0 10px 18px color-mix(in srgb, var(--lad-color-info-shadow) 8%, transparent);
}
.goal-target-picker button {
  width: 46px;
  height: 46px;
  @apply d-grid place-center cursor-pointer;
  z-index: 1;
  color: var(--lad-text-inverse);
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info)
  );
  box-shadow:
    0 4px 0 var(--lad-color-info-strong),
    0 7px 11px color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent);
  font-size: 1.6875rem;
  font-weight: var(--lad-font-weight-black);
  line-height: 1;
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.goal-target-picker button:last-of-type {
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow:
    0 4px 0 var(--lad-color-primary-deep),
    0 7px 11px
      color-mix(in srgb, var(--lad-color-primary-strong) 15%, transparent);
}
.goal-target-picker button:hover:not(:disabled) {
  transform: translateY(-2px) rotate(-3deg) scale(1.04);
}
.goal-target-picker button:active:not(:disabled) {
  transform: translateY(3px) scale(0.96);
  box-shadow: 0 1px 0 var(--lad-color-info-strong);
}
.goal-target-picker button:disabled {
  cursor: default;
  filter: grayscale(0.5);
  opacity: 0.35;
}
.goal-target-picker > div {
  @apply position-relative text-center;
  z-index: 1;
}
.goal-target-picker small {
  @apply d-block;
  color: var(--lad-muted);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-strong);
  text-transform: uppercase;
  letter-spacing: 0.07em;
}
.goal-target-picker output {
  @apply d-flex align-baseline justify-center;
  gap: 5px;
  color: var(--lad-blue-dark);
}
.goal-target-picker output strong {
  font-size: 1.5625rem;
  line-height: 1.15;
}
.goal-target-picker output span {
  color: var(--lad-color-primary-supporting);
  font-size: 0.8125rem;
  font-weight: var(--lad-font-weight-black);
}
.target-spark {
  @apply position-absolute;
  color: var(--lad-color-reward-accent);
  font-style: normal;
  animation: goal-twinkle 1.6s ease-in-out infinite;
}
.target-spark--one {
  top: 7px;
  right: 72px;
}
.target-spark--two {
  bottom: 7px;
  left: 72px;
  animation-delay: -0.8s;
}
.visibility-options {
  @apply d-grid;
  gap: 7px;
}
.visibility-options button {
  min-height: 51px;
  padding: 7px 9px;
  @apply position-relative d-flex align-center text-left cursor-pointer;
  gap: 9px;
  color: var(--lad-text);
  border: 2px solid var(--lad-border-subtle);
  border-radius: 15px;
  background: var(--lad-surface);
}
.visibility-options button > .v-icon:first-child {
  width: 34px;
  height: 34px;
  @apply flex-shrink-0;
  color: var(--lad-color-info-strong);
  border-radius: 11px;
  background: var(--lad-color-info-soft);
}
.visibility-options button > span {
  @apply flex-grow-1 min-w-0;
}
.visibility-options strong,
.visibility-options small {
  @apply d-block;
}
.visibility-options strong {
  font-size: 0.6875rem;
}
.visibility-options small {
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.visibility-options button.active {
  border-color: var(--lad-border-success);
  background: var(--lad-gradient-success);
  box-shadow: 0 3px 0 var(--lad-shadow-raised-success);
}
.visibility-check {
  color: var(--lad-mint-dark);
}
.saved-hint {
  color: var(--lad-muted);
  font-size: 0.625rem;
  line-height: 1.35;
}
.starter-bonus {
  min-height: 76px;
  padding: 10px 12px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 10px;
  border: 2px solid var(--lad-border-warning);
  border-radius: 19px;
  background: var(--lad-gradient-reward);
  box-shadow:
    0 5px 0 var(--lad-shadow-raised-warning),
    var(--lad-shadow-soft);
}
.starter-bonus::before {
  content: "";
  width: 46px;
  height: 150%;
  @apply position-absolute;
  top: -25%;
  left: -65px;
  transform: rotate(17deg);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-surface-raised) 75%, transparent),
    transparent
  );
  animation: bonus-shine 3.1s ease-in-out infinite;
}
.bonus-gift {
  width: 45px;
  height: 45px;
  @apply d-grid place-center flex-shrink-0;
  z-index: 1;
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 15px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-reward-shadow) 12%, transparent);
  font-size: 1.6875rem;
  transform-origin: bottom center;
  animation: bonus-wiggle 2.2s ease-in-out infinite;
}
.starter-bonus p,
.starter-bonus p > * {
  @apply d-block;
}
.starter-bonus p {
  @apply ma-0 flex-grow-1 min-w-0;
  z-index: 1;
}
.starter-bonus p small {
  color: var(--lad-color-reward-ink);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}
.starter-bonus p strong {
  color: var(--lad-color-reward-strong);
  font-size: 0.875rem;
}
.starter-bonus p span {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.bonus-coin {
  width: 49px;
  height: 49px;
  @apply position-relative d-grid place-center flex-shrink-0;
  z-index: 1;
  color: var(--lad-color-reward-strong);
  border: 3px solid var(--lad-color-reward-soft);
  border-radius: 50%;
  background: radial-gradient(
    circle at 36% 29%,
    var(--lad-color-reward-pale) 0 8%,
    var(--lad-color-reward) 9% 53%,
    var(--lad-color-reward-border) 54% 100%
  );
  box-shadow:
    inset 0 -5px 0
      color-mix(in srgb, var(--lad-color-reward-deep) 20%, transparent),
    0 5px 0 var(--lad-color-reward-shadow),
    0 9px 14px color-mix(in srgb, var(--lad-color-reward-ink) 15%, transparent);
  animation: bonus-coin-bounce 2.1s ease-in-out infinite;
}
.bonus-coin b {
  font-size: 1.25rem;
  line-height: 1;
}
.bonus-coin em {
  @apply position-absolute;
  right: -5px;
  bottom: -3px;
  padding: 2px 5px;
  color: var(--lad-text-inverse);
  border: 2px solid var(--lad-border-on-accent);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-color-primary-muted);
  font-size: 0.5625rem;
  font-style: normal;
  font-weight: var(--lad-font-weight-black);
}
.bonus-spark {
  @apply position-absolute;
  z-index: 2;
  color: var(--lad-color-reward-accent);
  animation: goal-twinkle 1.4s ease-in-out infinite;
}
.bonus-spark--one {
  top: 5px;
  right: 58px;
}
.bonus-spark--two {
  right: 8px;
  bottom: 5px;
  animation-delay: -0.7s;
}
.goal-dialog-actions {
  padding: 11px 18px 15px;
  @apply flex-shrink-0 d-flex justify-end ga-2;
  border-top: 1px solid
    color-mix(in srgb, var(--lad-color-primary-supporting) 12%, transparent);
  background: color-mix(in srgb, var(--lad-surface-raised) 90%, transparent);
}
.goal-dialog-actions > :first-child:not(:last-child) {
  @apply me-auto;
}
.create-goal-button {
  min-height: 43px;
  padding-inline: 16px;
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 80%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-color-primary),
    var(--lad-color-primary-strong)
  );
  box-shadow:
    0 4px 0 var(--lad-color-primary-deep),
    0 8px 14px
      color-mix(in srgb, var(--lad-color-primary-strong) 15%, transparent);
  text-transform: none;
  letter-spacing: 0;
}
.create-goal-button :deep(.v-btn__content) {
  gap: 6px;
}
.create-goal-button :deep(.v-btn__content) > span {
  color: var(--lad-color-reward-pale);
  animation: goal-twinkle 1.3s ease-in-out infinite;
}
@keyframes goal-visual-float {
  0%,
  100% {
    transform: translateY(2px) rotate(-5deg);
  }
  50% {
    transform: translateY(-4px) rotate(4deg);
  }
}
@keyframes goal-twinkle {
  0%,
  100% {
    opacity: 0.4;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.18) rotate(18deg);
  }
}
@keyframes bonus-wiggle {
  0%,
  100% {
    transform: rotate(-4deg);
  }
  45% {
    transform: rotate(7deg) scale(1.08);
  }
  60% {
    transform: rotate(-5deg);
  }
}
@keyframes bonus-coin-bounce {
  0%,
  100% {
    transform: translateY(2px) rotate(3deg);
  }
  50% {
    transform: translateY(-5px) rotate(-5deg) scale(1.06);
  }
}
@keyframes bonus-shine {
  0%,
  35% {
    left: -65px;
  }
  70%,
  100% {
    left: 115%;
  }
}
@include respond-down(narrow) {
  .goal-icon-picker {
    grid-template-columns: repeat(4, 1fr);
  }
  .goal-dialog-header {
    padding-left: 14px;
  }
  .goal-dialog-content {
    padding-inline: 14px;
  }
}
@include reduced-motion {
  .goal-dialog-visual,
  .goal-dialog-header::after,
  .target-spark,
  .starter-bonus::before,
  .bonus-gift,
  .bonus-coin,
  .bonus-spark,
  .create-goal-button :deep(.v-btn__content) > span {
    animation: none;
  }
}
</style>
