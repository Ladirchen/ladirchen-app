<template>
  <v-dialog v-model="store.familySetupOpen" max-width="500" :persistent="!store.onboardingCompleted" scrollable>
    <v-card class="family-setup-card" rounded="xl">
      <div class="setup-header pa-5 pb-4">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="eyebrow mb-1">{{ store.onboardingCompleted ? 'Familie bearbeiten' : 'Willkommen bei Ladirchen' }}</p>
            <h2>Wer gehört zu eurer Familie?</h2>
          </div>
          <v-btn
            v-if="store.onboardingCompleted"
            aria-label="Einrichtung schließen"
            icon="mdi-close"
            size="small"
            variant="text"
            @click="store.familySetupOpen = false"
          />
        </div>
        <div class="d-flex align-center ga-3 mt-4">
          <v-progress-linear color="primary" height="8" :model-value="stepProgress" rounded />
          <strong class="text-caption flex-shrink-0">{{ step }} / 3</strong>
        </div>
      </div>

      <v-card-text class="pa-5 pt-4">
        <v-window v-model="step">
          <v-window-item :value="1">
            <SetupSectionHeader icon="🧒" title="Kinder" copy="Für jedes Kind entstehen eigene Beiträge, Ladirchen und Sparziele." />
            <div class="d-flex flex-column ga-3 mt-4">
              <v-card v-for="(child, index) in children" :key="child.id" class="setup-person pa-3" elevation="0" rounded="lg">
                <div class="d-flex align-center ga-3">
                  <div class="setup-avatar" :style="{ background: `${child.color}24` }">{{ child.avatar }}</div>
                  <v-text-field v-model="child.name" density="compact" hide-details label="Name des Kindes" variant="outlined" />
                  <v-btn aria-label="Kind entfernen" :disabled="children.length === 1" icon="mdi-delete-outline" size="small" variant="text" @click="children.splice(index, 1)" />
                </div>
              </v-card>
            </div>
            <v-btn class="mt-4" color="primary" prepend-icon="mdi-plus" rounded="lg" variant="tonal" @click="addChild">Kind hinzufügen</v-btn>
          </v-window-item>

          <v-window-item :value="2">
            <SetupSectionHeader icon="🐾" title="Haustiere" copy="Haustiere können später eigene Familienbeiträge bekommen." />
            <div class="d-flex flex-column ga-3 mt-4">
              <v-card v-for="(pet, index) in pets" :key="pet.id" class="setup-person pa-3" elevation="0" rounded="lg">
                <div class="d-flex align-center ga-3">
                  <div class="setup-avatar" :style="{ background: `${pet.color}24` }">{{ pet.avatar }}</div>
                  <div class="d-grid pet-fields ga-2 flex-grow-1">
                    <v-text-field v-model="pet.name" density="compact" hide-details label="Name" variant="outlined" />
                    <v-select v-model="pet.kind" density="compact" hide-details item-title="title" item-value="value" :items="petKinds" label="Tierart" variant="outlined" @update:model-value="updatePetAvatar(pet)" />
                  </div>
                  <v-btn aria-label="Haustier entfernen" icon="mdi-delete-outline" size="small" variant="text" @click="pets.splice(index, 1)" />
                </div>
              </v-card>
            </div>
            <v-btn class="mt-4" color="primary" prepend-icon="mdi-plus" rounded="lg" variant="tonal" @click="addPet">Haustier hinzufügen</v-btn>
          </v-window-item>

          <v-window-item :value="3">
            <SetupSectionHeader icon="🤝" title="Bezugspersonen" copy="Bezugspersonen können Beiträge anlegen, prüfen und die Familienwelt begleiten." />
            <div class="d-flex flex-column ga-3 mt-4">
              <v-card v-for="(guardian, index) in guardians" :key="guardian.id" class="setup-person pa-3" elevation="0" rounded="lg">
                <div class="d-flex align-center ga-3">
                  <div class="setup-avatar" :style="{ background: `${guardian.color}24` }">{{ guardian.avatar }}</div>
                  <v-text-field v-model="guardian.name" density="compact" hide-details label="Name der Bezugsperson" variant="outlined" />
                  <v-btn aria-label="Bezugsperson entfernen" :disabled="guardians.length === 1" icon="mdi-delete-outline" size="small" variant="text" @click="guardians.splice(index, 1)" />
                </div>
              </v-card>
            </div>
            <v-btn class="mt-4" color="primary" prepend-icon="mdi-plus" rounded="lg" variant="tonal" @click="addGuardian">Bezugsperson hinzufügen</v-btn>
          </v-window-item>
        </v-window>
      </v-card-text>

      <v-card-actions class="setup-actions pa-5 pt-3">
        <v-btn :disabled="step === 1" rounded="lg" variant="text" @click="step -= 1">Zurück</v-btn>
        <v-spacer />
        <v-btn v-if="step < 3" color="primary" :disabled="!currentStepIsValid" rounded="lg" variant="flat" @click="step += 1">Weiter</v-btn>
        <v-btn v-else class="raised-button" color="primary" :disabled="!setupIsValid" rounded="lg" variant="flat" @click="finishSetup">Familienwelt starten</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, defineComponent, h, ref, watch } from 'vue';

import { createDefaultAvatarAppearance } from '@/domain/avatar';
import { createDomainId } from '@/domain/types';
import type { FamilyMember, FamilyPet, FamilyPetKindId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const step = ref(1);
const children = ref<FamilyMember[]>([]);
const guardians = ref<FamilyMember[]>([]);
const pets = ref<FamilyPet[]>([]);
const petKinds: Array<{ title: string; value: FamilyPetKindId }> = [
  { title: 'Katze', value: 'cat' },
  { title: 'Hund', value: 'dog' },
  { title: 'Kaninchen', value: 'rabbit' },
  { title: 'Vogel', value: 'bird' },
  { title: 'Anderes Tier', value: 'other' },
];

const SetupSectionHeader = defineComponent({
  props: { icon: { type: String, required: true }, title: { type: String, required: true }, copy: { type: String, required: true } },
  setup: (props) => () => h('div', { class: 'd-flex align-start ga-3' }, [
    h('div', { class: 'setup-section-icon' }, props.icon),
    h('div', [h('h3', { class: 'setup-section-title' }, props.title), h('p', { class: 'text-caption text-medium-emphasis mt-1' }, props.copy)]),
  ]),
});

const stepProgress = computed(() => (step.value / 3) * 100);
const childrenAreValid = computed(() => children.value.length > 0 && children.value.every((child) => child.name.trim()));
const petsAreValid = computed(() => pets.value.every((pet) => pet.name.trim() && pet.kind.trim()));
const guardiansAreValid = computed(() => guardians.value.length > 0 && guardians.value.every((guardian) => guardian.name.trim()));
const currentStepIsValid = computed(() => {
  if (step.value === 1) return childrenAreValid.value;
  if (step.value === 2) return petsAreValid.value;
  return guardiansAreValid.value;
});
const setupIsValid = computed(() => childrenAreValid.value && petsAreValid.value && guardiansAreValid.value);

const newId = (prefix: string, index: number) => `${prefix}-${Date.now()}-${index}`;
const resetDraft = () => {
  children.value = store.members.filter((member) => member.role === 'child').map((member) => ({ ...member }));
  guardians.value = store.members.filter((member) => member.role === 'guardian').map((member) => ({ ...member }));
  pets.value = store.pets.map((pet) => ({ ...pet }));
  step.value = 1;
};
const addChild = () => children.value.push({ id: createDomainId.familyMember(newId('child', children.value.length)), name: '', avatar: '🧒', color: '#7295e8', role: 'child', weeklyStreak: 0, appearance: createDefaultAvatarAppearance() });
const addGuardian = () => guardians.value.push({ id: createDomainId.familyMember(newId('guardian', guardians.value.length)), name: '', avatar: '🧑', color: '#68a985', role: 'guardian', guardianAccess: 'supporter', weeklyStreak: 0 });
const addPet = () => pets.value.push({ id: createDomainId.familyPet(newId('pet', pets.value.length)), name: '', kind: 'cat', kindLabel: 'Katze', avatar: '🐈', color: '#d9a465' });
const updatePetAvatar = (pet: FamilyPet) => {
  const avatars: Record<FamilyPetKindId, string> = { cat: '🐈', dog: '🐕', rabbit: '🐇', bird: '🐦', other: '🐾' };
  pet.kindLabel = petKinds.find(kind => kind.value === pet.kind)?.title ?? 'Anderes Tier';
  pet.avatar = avatars[pet.kind] ?? '🐾';
};
const finishSetup = () => store.completeFamilySetup(
  [...children.value.map((member) => ({ ...member })), ...guardians.value.map((member) => ({ ...member }))],
  pets.value.map((pet) => ({ ...pet })),
);

watch(() => store.familySetupOpen, (isOpen) => {
  if (isOpen) resetDraft();
}, { immediate: true });
</script>

<style scoped>
.family-setup-card {
  max-height: min(760px, 92dvh);
}
.setup-header {
  background: linear-gradient(145deg, #effaf5, #edf6ff);
  border-bottom: 1px solid var(--lad-border);
}
.setup-header h2 {
  @apply ma-0;
  font-size: 23px;
  letter-spacing: -0.035em;
}
.setup-person {
  border: 1px solid var(--lad-border);
}
.setup-avatar,
.setup-section-icon {
  @apply d-grid place-center flex-shrink-0;
  border-radius: 14px;
  font-size: 24px;
}
.setup-avatar {
  width: 45px;
  height: 45px;
}
.setup-section-icon {
  width: 48px;
  height: 48px;
  background: var(--lad-surface-soft);
}
.setup-section-title {
  @apply ma-0;
  font-size: 18px;
}
.pet-fields {
  grid-template-columns: 1fr 1fr;
}
.setup-actions {
  border-top: 1px solid var(--lad-border);
}
@media (max-width: 430px) {
  .pet-fields {
    grid-template-columns: 1fr;
  }
}
</style>
