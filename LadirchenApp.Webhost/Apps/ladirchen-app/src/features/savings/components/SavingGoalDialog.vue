<template>
  <v-dialog :model-value="modelValue" max-width="430" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card class="goal-dialog-card" rounded="xl">
      <header class="goal-dialog-header">
        <div class="goal-dialog-visual" aria-hidden="true">
          <span class="goal-visual-icon">{{ goal.icon }}</span>
          <i /><i /><i />
        </div>
        <div class="goal-dialog-heading">
          <p class="eyebrow mb-1">Clever sparen</p>
          <v-card-title class="pa-0">{{ isEditing ? 'Sparziel bearbeiten' : 'Neues Sparziel' }}</v-card-title>
          <p>{{ isEditing ? 'Passe deinen Wunsch ganz einfach an.' : 'Aus einem Wunsch wird Schritt für Schritt ein Ziel.' }}</p>
        </div>
        <v-btn class="goal-dialog-close" aria-label="Dialog schließen" icon="mdi-close" size="small" variant="text" @click="close" />
      </header>

      <v-card-text class="goal-dialog-content">
        <div class="goal-step">
          <span class="goal-step-number">1</span>
          <div><strong>Was wünschst du dir?</strong><small>Wähle ein Bild und gib deinem Ziel einen Namen.</small></div>
        </div>
        <div class="goal-icon-picker mb-3" aria-label="Symbol für das Sparziel auswählen">
          <button v-for="icon in iconOptions" :key="icon" :class="{ active: goal.icon === icon }" type="button" @click="goal.icon = icon">{{ icon }}</button>
        </div>
        <v-text-field v-model="goal.title" autofocus class="goal-field" density="comfortable" hide-details label="Mein Wunsch" placeholder="Zum Beispiel: Neues Fahrrad" variant="outlined" />

        <div class="goal-step mt-5">
          <span class="goal-step-number">2</span>
          <div><strong>Wie viele Ladirchen brauchst du?</strong><small>Du kannst später jederzeit weiter einzahlen.</small></div>
        </div>
        <div class="goal-target-picker" role="group" aria-label="Zielbetrag auswählen">
          <button aria-label="Zehn Ladirchen weniger" :disabled="goal.target <= minimumTarget" type="button" @click="adjustTarget(-10)">−</button>
          <div>
            <small>Zielbetrag</small>
            <output aria-live="polite"><strong>{{ validTargetPreview }}</strong><span>L</span></output>
          </div>
          <button aria-label="Zehn Ladirchen mehr" type="button" @click="adjustTarget(10)">+</button>
          <i class="target-spark target-spark--one" aria-hidden="true">✦</i>
          <i class="target-spark target-spark--two" aria-hidden="true">✧</i>
        </div>

        <div class="goal-step mt-5">
          <span class="goal-step-number">3</span>
          <div><strong>Wer darf dein Ziel sehen?</strong><small>Du entscheidest, wer dich unterstützen kann.</small></div>
        </div>
        <div class="visibility-options">
          <button v-for="option in visibilityOptions" :key="option.value" :class="{ active: goal.visibility === option.value }" type="button" @click="goal.visibility = option.value">
            <v-icon :icon="option.icon" size="21" /><span><strong>{{ option.shortTitle }}</strong><small>{{ option.subtitle }}</small></span><v-icon v-if="goal.visibility === option.value" class="visibility-check" icon="mdi-check-circle" size="18" />
          </button>
        </div>

        <div v-if="!isEditing" class="starter-bonus mt-4">
          <span class="bonus-gift" aria-hidden="true">🎁</span>
          <p><small>Dein Start</small><strong>5 L geschenkt!</strong><span>Bleibt bis zum Ziel geschützt.</span></p>
          <span class="bonus-coin" aria-hidden="true"><b>L</b><em>5</em></span>
          <span class="bonus-spark bonus-spark--one" aria-hidden="true">✦</span>
          <span class="bonus-spark bonus-spark--two" aria-hidden="true">✧</span>
        </div>

        <p v-if="isEditing && minimumTarget > 10" class="saved-hint mt-4">Bereits gespart: {{ minimumTarget }} L. Der Zielbetrag kann deshalb nicht darunter liegen.</p>

        <v-alert v-if="removeConfirmation" class="mt-4" color="error" density="compact" variant="tonal">
          Nicht geschützte Ladirchen werden zurückgegeben. Ein noch gesperrter 5-L-Startbonus verfällt.
        </v-alert>
      </v-card-text>

      <v-card-actions class="goal-dialog-actions">
        <v-btn v-if="isEditing && !removeConfirmation" color="error" rounded="lg" variant="text" @click="removeConfirmation = true">Sparziel auflösen</v-btn>
        <template v-if="removeConfirmation">
          <v-btn rounded="lg" variant="text" @click="removeConfirmation = false">Behalten</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="remove">Jetzt auflösen</v-btn>
        </template>
        <template v-else>
          <v-btn class="create-goal-button" color="primary" :disabled="!isValid" rounded="lg" variant="flat" @click="submit"><span aria-hidden="true">★</span>{{ isEditing ? 'Speichern' : 'Ziel starten' }}</v-btn>
        </template>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';

import type { GoalVisibility, NewGoal } from '@/domain/types';

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

const initialGoal = (): NewGoal => props.initialGoal
  ? { ...props.initialGoal }
  : { title: '', icon: '✨', target: 100, visibility: 'family' };
const goal = reactive<NewGoal>(initialGoal());
const removeConfirmation = ref(false);
const iconOptions = ['✨', '🚲', '📷', '🎨', '🧱', '🦒', '🎮', '🎵'];
const visibilityOptions: { icon: string; shortTitle: string; subtitle: string; value: GoalVisibility }[] = [
  { icon: 'mdi-home-heart', shortTitle: 'Familie', subtitle: 'Alle dürfen es sehen', value: 'family' },
  { icon: 'mdi-lock-outline', shortTitle: 'Nur ich', subtitle: 'Bleibt ganz privat', value: 'private' },
  { icon: 'mdi-shield-account-outline', shortTitle: 'Mit Erwachsenen', subtitle: 'Bezugspersonen helfen', value: 'guardians' },
];
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
    window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
      heading: 'Ladis Spartipp',
      message: 'Wähle ein klares Ziel und einen Betrag, der gut zu deinem Wunsch passt. Kleine regelmäßige Schritte bringen dich oft leichter ans Ziel als ein großer Sprung.',
    } })), 180);
  }
});
</script>

<style scoped>
.goal-dialog-card {
  max-height: calc(100dvh - 24px);
  @apply overflow-hidden;
  border: 2px solid rgba(75, 143, 117, 0.2);
  background: linear-gradient(180deg, #fffdf8, #f9fcfa) !important;
  box-shadow:
    0 10px 0 rgba(57, 110, 90, 0.13),
    0 28px 65px rgba(43, 76, 65, 0.25) !important;
}
.goal-dialog-header {
  padding: 17px 50px 15px 18px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 12px;
  border-bottom: 2px solid rgba(78, 143, 221, 0.12);
  background:
    radial-gradient(
      circle at 94% 5%,
      rgba(255, 219, 105, 0.27),
      transparent 27%
    ),
    linear-gradient(145deg, #eaf7ff, #fff7dc);
}
.goal-dialog-header::after {
  content: "✦";
  @apply position-absolute;
  top: 9px;
  right: 49px;
  color: #e9a82f;
  animation: goal-twinkle 1.7s ease-in-out infinite;
}
.goal-dialog-visual {
  width: 66px;
  height: 66px;
  @apply position-relative d-grid place-center flex-shrink-0;
  border: 3px solid #fff;
  border-radius: 23px;
  background: linear-gradient(145deg, #77c9f2, #567fdb 65%, #8a65ca);
  box-shadow:
    0 6px 0 rgba(60, 99, 174, 0.22),
    0 10px 18px rgba(67, 101, 157, 0.14);
  transform: rotate(-5deg);
  animation: goal-visual-float 2.8s ease-in-out infinite;
}
.goal-visual-icon {
  font-size: 31px;
  filter: drop-shadow(0 2px 0 rgba(255, 255, 255, 0.35));
}
.goal-dialog-visual i {
  width: 6px;
  height: 6px;
  @apply position-absolute;
  border-radius: 50%;
  background: #fff4a8;
  box-shadow: 0 0 7px #fff;
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
  font-size: 21px;
  font-weight: 950;
  letter-spacing: -0.035em;
}
.goal-dialog-heading > p:last-child {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.35;
}
.goal-dialog-close {
  @apply position-absolute;
  top: 10px;
  right: 9px;
}
.goal-dialog-content {
  padding: 17px 18px 12px !important;
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
  color: #fff;
  border: 2px solid #fff;
  border-radius: 10px;
  background: linear-gradient(145deg, #54b68e, #2c8c68);
  box-shadow: 0 3px 0 rgba(34, 118, 85, 0.2);
  font-size: 11px;
  font-weight: 950;
  transform: rotate(-4deg);
}
.goal-step > div strong,
.goal-step > div small {
  @apply d-block;
}
.goal-step > div strong {
  font-size: 12px;
}
.goal-step > div small {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 9px;
}
.goal-icon-picker {
  @apply d-grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 5px;
}
.goal-icon-picker button {
  aspect-ratio: 1;
  @apply d-grid place-center cursor-pointer;
  border: 2px solid rgba(78, 143, 221, 0.12);
  border-radius: 11px;
  background: #f3f9fc;
  font-size: 18px;
  transition:
    transform 0.16s ease,
    background 0.16s ease;
}
.goal-icon-picker button:hover {
  transform: translateY(-2px) rotate(-4deg);
}
.goal-icon-picker button.active {
  border-color: rgba(66, 153, 116, 0.45);
  background: linear-gradient(145deg, #e4f8ee, #fff2c8);
  box-shadow: 0 3px 0 rgba(52, 139, 103, 0.15);
  transform: translateY(-2px) rotate(-4deg);
}
.goal-field :deep(.v-field) {
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.88);
}
.goal-target-picker {
  min-height: 72px;
  padding: 8px 13px;
  @apply position-relative d-grid align-center overflow-hidden;
  grid-template-columns: 52px 1fr 52px;
  gap: 10px;
  border: 2px solid rgba(69, 143, 188, 0.2);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 88% 18%,
      rgba(255, 217, 91, 0.28),
      transparent 27%
    ),
    linear-gradient(145deg, #eef8ff, #edf9f2);
  box-shadow:
    0 5px 0 rgba(55, 125, 160, 0.11),
    0 10px 18px rgba(55, 125, 160, 0.07);
}
.goal-target-picker button {
  width: 46px;
  height: 46px;
  @apply d-grid place-center cursor-pointer;
  z-index: 1;
  color: #fff;
  border: 3px solid #fff;
  border-radius: 16px;
  background: linear-gradient(145deg, #6abbed, #3e82d1);
  box-shadow:
    0 4px 0 #2f6cb3,
    0 7px 11px rgba(48, 112, 181, 0.15);
  font-size: 27px;
  font-weight: 950;
  line-height: 1;
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.goal-target-picker button:last-of-type {
  background: linear-gradient(145deg, #61c49b, #328e6b);
  box-shadow:
    0 4px 0 #267153,
    0 7px 11px rgba(43, 130, 96, 0.15);
}
.goal-target-picker button:hover:not(:disabled) {
  transform: translateY(-2px) rotate(-3deg) scale(1.04);
}
.goal-target-picker button:active:not(:disabled) {
  transform: translateY(3px) scale(0.96);
  box-shadow: 0 1px 0 #2f6cb3;
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
  font-size: 9px;
  font-weight: 850;
  text-transform: uppercase;
  letter-spacing: 0.07em;
}
.goal-target-picker output {
  @apply d-flex align-baseline justify-center;
  gap: 5px;
  color: var(--lad-blue-dark);
}
.goal-target-picker output strong {
  font-size: 25px;
  line-height: 1.15;
}
.goal-target-picker output span {
  color: #47826d;
  font-size: 13px;
  font-weight: 950;
}
.target-spark {
  @apply position-absolute;
  color: #e4a42b;
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
  border: 2px solid rgba(74, 134, 111, 0.12);
  border-radius: 15px;
  background: #fff;
}
.visibility-options button > .v-icon:first-child {
  width: 34px;
  height: 34px;
  @apply flex-shrink-0;
  color: #4b829f;
  border-radius: 11px;
  background: #e9f5fb;
}
.visibility-options button > span {
  @apply flex-grow-1 min-w-0;
}
.visibility-options strong,
.visibility-options small {
  @apply d-block;
}
.visibility-options strong {
  font-size: 11px;
}
.visibility-options small {
  color: var(--lad-muted);
  font-size: 8px;
}
.visibility-options button.active {
  border-color: rgba(60, 174, 129, 0.45);
  background: linear-gradient(145deg, #effaf5, #fff9e7);
  box-shadow: 0 3px 0 rgba(48, 142, 105, 0.13);
}
.visibility-check {
  color: var(--lad-mint-dark);
}
.saved-hint {
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.35;
}
.starter-bonus {
  min-height: 76px;
  padding: 10px 12px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 10px;
  border: 2px solid rgba(232, 171, 58, 0.3);
  border-radius: 19px;
  background:
    radial-gradient(
      circle at 88% 12%,
      rgba(255, 217, 96, 0.42),
      transparent 35%
    ),
    linear-gradient(145deg, #fff9d8, #e3f8ed 72%);
  box-shadow:
    0 5px 0 rgba(184, 126, 34, 0.12),
    0 10px 18px rgba(129, 100, 49, 0.08);
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
    rgba(255, 255, 255, 0.75),
    transparent
  );
  animation: bonus-shine 3.1s ease-in-out infinite;
}
.bonus-gift {
  width: 45px;
  height: 45px;
  @apply d-grid place-center flex-shrink-0;
  z-index: 1;
  border: 2px solid #fff;
  border-radius: 15px;
  background: rgba(255, 255, 255, 0.68);
  box-shadow: 0 4px 0 rgba(190, 132, 33, 0.12);
  font-size: 27px;
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
  color: #8a6728;
  font-size: 8px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}
.starter-bonus p strong {
  color: #74500f;
  font-size: 14px;
}
.starter-bonus p span {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 8px;
}
.bonus-coin {
  width: 49px;
  height: 49px;
  @apply position-relative d-grid place-center flex-shrink-0;
  z-index: 1;
  color: #8a5707;
  border: 3px solid #fff5c8;
  border-radius: 50%;
  background: radial-gradient(
    circle at 36% 29%,
    #fff7ad 0 8%,
    #ffd65e 9% 53%,
    #efa925 54% 100%
  );
  box-shadow:
    inset 0 -5px 0 rgba(184, 113, 17, 0.2),
    0 5px 0 #c98a20,
    0 9px 14px rgba(160, 107, 24, 0.16);
  animation: bonus-coin-bounce 2.1s ease-in-out infinite;
}
.bonus-coin b {
  font-size: 20px;
  line-height: 1;
}
.bonus-coin em {
  @apply position-absolute;
  right: -5px;
  bottom: -3px;
  padding: 2px 5px;
  color: #fff;
  border: 2px solid #fff;
  border-radius: 999px;
  background: #48a77f;
  font-size: 9px;
  font-style: normal;
  font-weight: 950;
}
.bonus-spark {
  @apply position-absolute;
  z-index: 2;
  color: #dc9b24;
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
  padding: 11px 18px 15px !important;
  @apply flex-shrink-0 d-flex justify-end ga-2;
  border-top: 1px solid rgba(70, 129, 108, 0.12);
  background: rgba(255, 255, 255, 0.9);
}
.goal-dialog-actions > :first-child:not(:last-child) {
  @apply me-auto;
}
.create-goal-button {
  min-height: 43px !important;
  padding-inline: 16px !important;
  border: 2px solid rgba(255, 255, 255, 0.82) !important;
  background: linear-gradient(145deg, #55bb91, #2c956d) !important;
  box-shadow:
    0 4px 0 #23795a,
    0 8px 14px rgba(39, 131, 96, 0.16) !important;
  text-transform: none;
  letter-spacing: 0;
}
.create-goal-button :deep(.v-btn__content) {
  gap: 6px;
}
.create-goal-button :deep(.v-btn__content) > span {
  color: #fff1a5;
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
@media (max-width: 390px) {
  .goal-icon-picker {
    grid-template-columns: repeat(4, 1fr);
  }
  .goal-dialog-header {
    padding-left: 14px;
  }
  .goal-dialog-content {
    padding-inline: 14px !important;
  }
}
@media (prefers-reduced-motion: reduce) {
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
