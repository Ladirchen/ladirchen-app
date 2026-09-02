<template>
  <v-dialog :model-value="modelValue" max-width="420" @update:model-value="emit('update:modelValue', $event)">
    <v-card class="pa-5" rounded="xl">
      <div class="d-flex align-start justify-space-between ga-3 mb-4">
        <div>
          <p class="eyebrow mb-1">Clever sparen</p>
          <v-card-title class="pa-0">{{ isEditing ? 'Sparziel bearbeiten' : 'Neues Sparziel' }}</v-card-title>
          <p class="text-caption text-medium-emphasis mt-1">{{ isEditing ? 'Passe Wunsch, Zielbetrag oder Sichtbarkeit direkt an.' : 'Lege fest, wofür du sparen möchtest und wer das Ziel sehen darf.' }}</p>
        </div>
        <v-btn aria-label="Dialog schließen" icon="mdi-close" size="small" variant="text" @click="close" />
      </div>

      <v-text-field v-model="goal.title" autofocus label="Wunsch" variant="outlined" />
      <v-text-field v-model.number="goal.target" label="Zielbetrag" :min="minimumTarget" suffix="L" type="number" variant="outlined" />
      <v-select
        v-model="goal.visibility"
        :items="visibilityOptions"
        item-title="title"
        item-value="value"
        label="Sichtbarkeit"
        variant="outlined"
      />

      <div v-if="!isEditing" class="starter-bonus mb-4">
        <span aria-hidden="true">🎁</span>
        <p><strong>5 L geschützter Startbonus</strong><small>Der Bonus bleibt bis zum Erreichen des Ziels im Sparplan. Bei vorzeitigem Auflösen verfällt er.</small></p>
      </div>

      <div class="visibility-hint mb-5">
        <v-icon :icon="visibilityHint.icon" size="20" />
        <span>{{ visibilityHint.text }}</span>
      </div>

      <p v-if="isEditing && minimumTarget > 10" class="saved-hint mb-5">Bereits gespart: {{ minimumTarget }} L. Der Zielbetrag kann deshalb nicht darunter liegen.</p>

      <v-alert v-if="removeConfirmation" class="mb-3" color="error" density="compact" variant="tonal">
        Nicht geschützte Ladirchen werden zurückgegeben. Ein noch gesperrter 5-L-Startbonus verfällt.
      </v-alert>

      <div class="goal-dialog-actions">
        <v-btn v-if="isEditing && !removeConfirmation" color="error" rounded="lg" variant="text" @click="removeConfirmation = true">Sparziel auflösen</v-btn>
        <template v-if="removeConfirmation">
          <v-btn rounded="lg" variant="text" @click="removeConfirmation = false">Behalten</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="remove">Jetzt auflösen</v-btn>
        </template>
        <template v-else>
          <v-btn rounded="lg" variant="text" @click="close">Abbrechen</v-btn>
          <v-btn color="primary" :disabled="!isValid" rounded="lg" variant="flat" @click="submit">{{ isEditing ? 'Änderungen speichern' : 'Sparziel anlegen' }}</v-btn>
        </template>
      </div>
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
const visibilityOptions: { title: string; value: GoalVisibility }[] = [
  { title: 'Für die ganze Familie sichtbar', value: 'family' },
  { title: 'Nur für mich sichtbar', value: 'private' },
  { title: 'Nur für mich und Bezugspersonen sichtbar', value: 'guardians' },
];
const isEditing = computed(() => props.initialGoal !== null);
const isValid = computed(() => goal.title.trim().length > 0 && Number.isFinite(goal.target) && goal.target >= props.minimumTarget);
const visibilityHint = computed(() => {
  if (goal.visibility === 'private') return { icon: 'mdi-lock-outline', text: 'Nur du kannst dieses Sparziel sehen.' };
  if (goal.visibility === 'guardians') return { icon: 'mdi-shield-account-outline', text: 'Du und deine Bezugspersonen können dieses Ziel sehen.' };
  return { icon: 'mdi-home-heart', text: 'Alle Kinder und Bezugspersonen in deiner Familie können es sehen.' };
});

const reset = () => Object.assign(goal, initialGoal());
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
  }
});
</script>

<style scoped>
.visibility-hint {
  min-height: 46px;
  padding: 10px 12px;
  @apply d-flex align-center;
  gap: 9px;
  color: var(--lad-muted);
  border: 1px solid var(--lad-border);
  border-radius: 14px;
  background: var(--lad-surface-soft);
  font-size: 11px;
  line-height: 1.35;
}
.saved-hint {
  margin-top: -10px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.35;
}
.starter-bonus {
  padding: 10px 12px;
  @apply d-flex align-center;
  gap: 10px;
  border: 1px solid rgba(62, 188, 140, 0.24);
  border-radius: 14px;
  background: linear-gradient(145deg, #e8f9f0, #fff7d8);
}
.starter-bonus > span {
  font-size: 25px;
}
.starter-bonus p,
.starter-bonus strong,
.starter-bonus small {
  @apply d-block;
}
.starter-bonus p {
  @apply ma-0;
}
.starter-bonus strong {
  color: var(--lad-mint-dark);
  font-size: 12px;
}
.starter-bonus small {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.35;
}
.goal-dialog-actions {
  @apply d-flex justify-end ga-2;
}
.goal-dialog-actions > :first-child:not(:last-child) {
  @apply me-auto;
}
</style>
