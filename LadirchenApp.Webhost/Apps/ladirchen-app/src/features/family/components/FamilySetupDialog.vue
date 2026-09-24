<template>
  <v-dialog v-model="store.familySetupOpen" max-width="500" :persistent="!store.onboardingCompleted" scrollable>
    <v-card class="family-setup-card" rounded="xl">
      <div class="setup-header pa-5 pb-4">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="eyebrow mb-1">{{ setupEyebrow }}</p>
            <h2>{{ t('family.setup.title') }}</h2>
          </div>
          <v-btn
            v-if="store.onboardingCompleted"
            :aria-label="t('family.setup.close')"
            icon="i-mdi:close"
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
            <SetupSectionHeader icon="🧒" :title="t('family.setup.children.title')" :copy="t('family.setup.children.description')" />
            <div class="d-flex flex-column ga-3 mt-4">
              <v-card v-for="(child, index) in children" :key="child.id" class="setup-person pa-3" elevation="0" rounded="lg">
                <div class="d-flex align-center ga-3">
                  <div class="setup-avatar" :style="{ background: `${child.color}24` }">{{ child.avatar }}</div>
                  <v-text-field v-model="child.name" density="compact" hide-details :label="t('family.setup.children.name')" variant="outlined" />
                  <v-btn :aria-label="t('family.setup.children.remove')" :disabled="children.length === 1" icon="i-mdi:delete-outline" size="small" variant="text" @click="children.splice(index, 1)" />
                </div>
              </v-card>
            </div>
            <v-btn class="mt-4" color="primary" prepend-icon="i-mdi:plus" rounded="lg" variant="tonal" @click="addChild">{{ t('family.setup.children.add') }}</v-btn>
          </v-window-item>

          <v-window-item :value="2">
            <SetupSectionHeader icon="🐾" :title="t('family.setup.pets.title')" :copy="t('family.setup.pets.description')" />
            <div class="d-flex flex-column ga-3 mt-4">
              <v-card v-for="(pet, index) in pets" :key="pet.id" class="setup-person pa-3" elevation="0" rounded="lg">
                <div class="d-flex align-center ga-3">
                  <div class="setup-avatar" :style="{ background: `${pet.color}24` }">{{ pet.avatar }}</div>
                  <div class="d-grid pet-fields ga-2 flex-grow-1">
                    <v-text-field v-model="pet.name" density="compact" hide-details :label="t('family.setup.name')" variant="outlined" />
                    <v-select v-model="pet.kind" density="compact" hide-details item-title="title" item-value="value" :items="petKinds" :label="t('family.setup.pets.kind')" variant="outlined" @update:model-value="updatePetAvatar(pet)" />
                  </div>
                  <v-btn :aria-label="t('family.setup.pets.remove')" icon="i-mdi:delete-outline" size="small" variant="text" @click="pets.splice(index, 1)" />
                </div>
              </v-card>
            </div>
            <v-btn class="mt-4" color="primary" prepend-icon="i-mdi:plus" rounded="lg" variant="tonal" @click="addPet">{{ t('family.setup.pets.add') }}</v-btn>
          </v-window-item>

          <v-window-item :value="3">
            <SetupSectionHeader icon="🤝" :title="t('family.setup.guardians.title')" :copy="t('family.setup.guardians.description')" />
            <div class="d-flex flex-column ga-3 mt-4">
              <v-card v-for="(guardian, index) in guardians" :key="guardian.id" class="setup-person pa-3" elevation="0" rounded="lg">
                <div class="d-flex align-center ga-3">
                  <div class="setup-avatar" :style="{ background: `${guardian.color}24` }">{{ guardian.avatar }}</div>
                  <v-text-field v-model="guardian.name" density="compact" hide-details :label="t('family.setup.guardians.name')" variant="outlined" />
                  <v-btn :aria-label="t('family.setup.guardians.remove')" :disabled="guardians.length === 1" icon="i-mdi:delete-outline" size="small" variant="text" @click="guardians.splice(index, 1)" />
                </div>
              </v-card>
            </div>
            <v-btn class="mt-4" color="primary" prepend-icon="i-mdi:plus" rounded="lg" variant="tonal" @click="addGuardian">{{ t('family.setup.guardians.add') }}</v-btn>
          </v-window-item>
        </v-window>
      </v-card-text>

      <v-card-actions class="setup-actions pa-5 pt-3">
        <v-btn :disabled="step === SETUP_STEPS.children" rounded="lg" variant="text" @click="step -= 1">{{ t('family.setup.back') }}</v-btn>
        <v-spacer />
        <v-btn v-if="step < SETUP_STEPS.guardians" color="primary" :disabled="!currentStepIsValid" rounded="lg" variant="flat" @click="step += 1">{{ t('common.next') }}</v-btn>
        <v-btn v-else class="raised-button" color="primary" :disabled="!setupIsValid" rounded="lg" variant="flat" @click="finishSetup">{{ t('family.setup.start') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, defineComponent, h, ref, watch } from "vue";
import { useI18n } from "vue-i18n";

import { createDefaultAvatarAppearance } from "@/domain/avatar";
import type { FamilyMember, FamilyPet, FamilyPetKindId } from "@/domain/family/types";
import { createDomainId } from "@/domain/shared/identifiers";
import { percentageOfTotal } from "@/domain/shared/numbers";
import { useFamilyWorldStore } from "@/stores/family-world";
import { familyMemberColorPalette } from "@/theme/color-palette";

const store = useFamilyWorldStore();
const { t } = useI18n();
const SETUP_STEPS = Object.freeze({ children: 1, guardians: 3, pets: 2 });
const step = ref(SETUP_STEPS.children);
const children = ref<FamilyMember[]>([]);
const guardians = ref<FamilyMember[]>([]);
const pets = ref<FamilyPet[]>([]);
const petKindIds: readonly FamilyPetKindId[] = ["cat", "dog", "rabbit", "bird", "other"];
const petKinds = computed(() => petKindIds.map(value => ({
  title: t(`familyPets.kinds.${value}`),
  value,
})));

const SetupSectionHeader = defineComponent({
  props: { icon: { type: String, required: true }, title: { type: String, required: true }, copy: { type: String, required: true } },
  setup: (props) => () => h("div", { class: "d-flex align-start ga-3" }, [
    h("div", { class: "setup-section-icon" }, props.icon),
    h("div", [h("h3", { class: "setup-section-title" }, props.title), h("p", { class: "text-caption text-medium-emphasis mt-1" }, props.copy)]),
  ]),
});

const stepProgress = computed(() => percentageOfTotal(step.value, SETUP_STEPS.guardians));
const childrenAreValid = computed(() => children.value.length > 0 && children.value.every((child) => child.name.trim()));
const petsAreValid = computed(() => pets.value.every((pet) => pet.name.trim() && pet.kind.trim()));
const guardiansAreValid = computed(() => guardians.value.length > 0 && guardians.value.every((guardian) => guardian.name.trim()));
const currentStepIsValid = computed(() => {
  if (step.value === SETUP_STEPS.children) return childrenAreValid.value;
  if (step.value === SETUP_STEPS.pets) return petsAreValid.value;
  return guardiansAreValid.value;
});
const setupIsValid = computed(() => childrenAreValid.value && petsAreValid.value && guardiansAreValid.value);
const setupEyebrow = computed(() => t(store.onboardingCompleted
  ? "family.setup.editEyebrow"
  : "family.setup.welcomeEyebrow"));

const newId = (prefix: string, index: number) => `${prefix}-${Date.now()}-${index}`;
const resetDraft = () => {
  children.value = store.members.filter((member) => member.role === "child").map((member) => ({ ...member }));
  guardians.value = store.members.filter((member) => member.role === "guardian").map((member) => ({ ...member }));
  pets.value = store.pets.map((pet) => ({ ...pet }));
  step.value = SETUP_STEPS.children;
};
const addChild = () => children.value.push({ id: createDomainId.familyMember(newId("child", children.value.length)), name: "", avatar: "🧒", color: familyMemberColorPalette.laura, role: "child", participatesInWeeklyGoal: true, weeklyStreak: 0, appearance: createDefaultAvatarAppearance() });
const addGuardian = () => guardians.value.push({ id: createDomainId.familyMember(newId("guardian", guardians.value.length)), name: "", avatar: "🧑", color: familyMemberColorPalette.defaultGuardian, role: "guardian", guardianAccess: "supporter", participatesInWeeklyGoal: false, weeklyStreak: 0 });
const addPet = () => pets.value.push({ id: createDomainId.familyPet(newId("pet", pets.value.length)), name: "", kind: "cat", avatar: "🐈", color: familyMemberColorPalette.petAnna });
const updatePetAvatar = (pet: FamilyPet) => {
  const avatars: Record<FamilyPetKindId, string> = { cat: "🐈", dog: "🐕", rabbit: "🐇", bird: "🐦", other: "🐾" };
  pet.avatar = avatars[pet.kind] ?? "🐾";
};
const finishSetup = () => store.completeFamilySetup(
  [...children.value.map((member) => ({ ...member })), ...guardians.value.map((member) => ({ ...member }))],
  pets.value.map((pet) => ({ ...pet })),
);

watch(() => store.familySetupOpen, (isOpen) => {
  if (isOpen) resetDraft();
}, { immediate: true });
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.family-setup-card {
  max-height: min(47.5rem, 92dvh);
}
.setup-header {
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-surface-soft)
  );
  border-bottom: rem(1) solid var(--lad-border);
}
.setup-header h2 {
  --uno: ma-0;
  font-size: rem(23);
  letter-spacing: -0.035em;
}
.setup-person {
  border: rem(1) solid var(--lad-border);
}
.setup-avatar,
.setup-section-icon {
  --uno: d-grid place-center flex-shrink-0;
  border-radius: rem(14);
  font-size: 1.5rem;
}
.setup-avatar {
  width: rem(45);
  height: rem(45);
}
.setup-section-icon {
  width: 3rem;
  height: 3rem;
  background: var(--lad-surface-soft);
}
.setup-section-title {
  --uno: ma-0;
  font-size: rem(18);
}
.pet-fields {
  grid-template-columns: 1fr 1fr;
}
.setup-actions {
  border-top: rem(1) solid var(--lad-border);
}
@include respond-down(phone) {
  .pet-fields {
    grid-template-columns: 1fr;
  }
}
</style>
