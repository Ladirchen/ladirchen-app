<template>
  <v-dialog v-model="store.familySetupOpen" max-width="560" scrollable>
    <v-card class="family-setup-card" rounded="xl">
      <div class="setup-header pa-5 pb-4">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="eyebrow mb-1">{{ setupEyebrow }}</p>
            <h2>{{ t("family.setup.manageTitle") }}</h2>
          </div>
          <v-btn
            :aria-label="t('family.setup.close')"
            icon="i-mdi:close"
            size="small"
            variant="text"
            @click="store.familySetupOpen = false"
          />
        </div>
        <v-tabs v-model="activeSection" class="setup-tabs mt-4" color="primary" grow>
          <v-tab v-if="store.canInviteFamilyMembers" value="accounts">{{ t("family.setup.accountsTab") }}</v-tab>
          <v-tab v-if="canEditDetails" value="details">{{ t("family.setup.detailsTab") }}</v-tab>
        </v-tabs>
      </div>

      <v-card-text class="pa-5 pt-4">
        <v-window v-model="activeSection">
          <v-window-item v-if="store.canInviteFamilyMembers" value="accounts">
            <p class="text-caption text-medium-emphasis mb-4">
              {{ t("family.memberAccount.setupDescription") }}
            </p>
            <MemberAccountInvitationForm />
          </v-window-item>

          <v-window-item v-if="canEditDetails" value="details">
            <section aria-labelledby="family-member-details-title">
              <h3 id="family-member-details-title" class="section-title">
                {{ t("family.setup.memberDetailsTitle") }}
              </h3>
              <p class="section-description">{{ t("family.setup.memberDetailsDescription") }}</p>
              <div class="d-flex flex-column ga-3 mt-4">
                <v-card
                  v-for="member in memberDrafts"
                  :key="member.id"
                  class="setup-person pa-3"
                  elevation="0"
                  rounded="lg"
                >
                  <div class="member-heading mb-3">
                    <span class="member-preview" :style="avatarStyle(member.color)">{{ member.avatar }}</span>
                    <span>
                      <strong>{{ member.name }}</strong>
                      <small>{{ t(`family.roles.${member.role}`) }}</small>
                    </span>
                    <v-chip v-if="member.invitationPending" color="info" size="x-small" variant="tonal">
                      {{ t("family.roster.invited") }}
                    </v-chip>
                  </div>
                  <div class="member-fields">
                    <v-text-field
                      v-model="member.name"
                      density="compact"
                      hide-details
                      :label="t('family.setup.name')"
                      variant="outlined"
                    />
                    <v-text-field
                      v-model="member.avatar"
                      density="compact"
                      hide-details
                      :label="t('family.setup.avatar')"
                      variant="outlined"
                    />
                    <v-text-field
                      v-model="member.color"
                      density="compact"
                      hide-details
                      :label="t('family.setup.color')"
                      type="color"
                      variant="outlined"
                    />
                  </div>
                </v-card>
              </div>
            </section>

            <section class="mt-6" aria-labelledby="family-pet-details-title">
              <div class="section-heading">
                <div>
                  <h3 id="family-pet-details-title" class="section-title">{{ t("family.setup.pets.title") }}</h3>
                  <p class="section-description">{{ t("family.setup.pets.description") }}</p>
                </div>
                <v-btn
                  color="primary"
                  prepend-icon="i-mdi:plus"
                  rounded="lg"
                  size="small"
                  variant="tonal"
                  @click="addPet"
                >
                  {{ t("family.setup.pets.add") }}
                </v-btn>
              </div>
              <div class="d-flex flex-column ga-3 mt-4">
                <v-card
                  v-for="(pet, index) in petDrafts"
                  :key="pet.id"
                  class="setup-person pa-3"
                  elevation="0"
                  rounded="lg"
                >
                  <div class="pet-fields">
                    <span class="member-preview" :style="avatarStyle(pet.color)">{{ pet.avatar }}</span>
                    <v-text-field
                      v-model="pet.name"
                      density="compact"
                      hide-details
                      :label="t('family.setup.name')"
                      variant="outlined"
                    />
                    <v-select
                      v-model="pet.kind"
                      density="compact"
                      hide-details
                      :items="petKinds"
                      item-title="title"
                      item-value="value"
                      :label="t('family.setup.pets.kind')"
                      variant="outlined"
                      @update:model-value="updatePetAvatar(pet)"
                    />
                    <v-text-field
                      v-model="pet.avatar"
                      density="compact"
                      hide-details
                      :label="t('family.setup.avatar')"
                      variant="outlined"
                    />
                    <v-text-field
                      v-model="pet.color"
                      density="compact"
                      hide-details
                      :label="t('family.setup.color')"
                      type="color"
                      variant="outlined"
                    />
                    <v-btn
                      :aria-label="t('family.setup.pets.remove')"
                      icon="i-mdi:delete-outline"
                      size="small"
                      variant="text"
                      @click="petDrafts.splice(index, 1)"
                    />
                  </div>
                </v-card>
              </div>
            </section>

            <v-btn
              block
              class="mt-6"
              color="primary"
              :disabled="!detailsAreValid"
              :loading="saving"
              rounded="lg"
              @click="saveDetails"
            >
              {{ t("family.setup.save") }}
            </v-btn>
          </v-window-item>
        </v-window>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { FamilyMember, FamilyPet, FamilyPetKindId } from "@/domain/family/types";
import { createDomainId } from "@/domain/shared/identifiers";
import MemberAccountInvitationForm from "@/features/family/components/MemberAccountInvitationForm.vue";
import { isHexColor } from "@/application/contracts/validation/runtime-validation";
import { useFamilyWorldStore } from "@/stores/family-world";
import { familyMemberColorPalette } from "@/theme/color-palette";

type SetupSection = "accounts" | "details";

const PET_AVATARS: Readonly<Record<FamilyPetKindId, string>> = Object.freeze({
  bird: "🐦",
  cat: "🐈",
  dog: "🐕",
  other: "🐾",
  rabbit: "🐇",
});
const PET_KIND_IDS: ReadonlyArray<FamilyPetKindId> = ["cat", "dog", "rabbit", "bird", "other"];
const store = useFamilyWorldStore();
const { t } = useI18n();
const activeSection = ref<SetupSection>("accounts");
const memberDrafts = ref<FamilyMember[]>([]);
const petDrafts = ref<FamilyPet[]>([]);
const saving = ref(false);
const petKinds = computed(() =>
  PET_KIND_IDS.map((value) => ({
    title: t(`familyPets.kinds.${value}`),
    value,
  })),
);
const canEditDetails = computed(() => store.onboardingCompleted && store.permissions.canManageFamily);
const detailsAreValid = computed(
  () =>
    memberDrafts.value.every(
      (member) => member.name.trim().length > 0 && member.avatar.trim().length > 0 && isHexColor(member.color),
    ) &&
    petDrafts.value.every((pet) => pet.name.trim().length > 0 && pet.avatar.trim().length > 0 && isHexColor(pet.color)),
);
const setupEyebrow = computed(() =>
  t(store.onboardingCompleted ? "family.setup.editEyebrow" : "family.setup.welcomeEyebrow"),
);

function avatarStyle(color: string) {
  return { background: `${color}24` };
}

function resetDetails() {
  memberDrafts.value = store.members.map((member) => ({ ...member }));
  petDrafts.value = store.pets.map((pet) => ({ ...pet }));
}

function addPet() {
  petDrafts.value.push({
    avatar: PET_AVATARS.cat,
    color: familyMemberColorPalette.petAnna,
    id: createDomainId.familyPet(crypto.randomUUID()),
    kind: "cat",
    name: "",
  });
}

function updatePetAvatar(pet: FamilyPet) {
  pet.avatar = PET_AVATARS[pet.kind];
}

async function saveDetails() {
  if (!detailsAreValid.value || saving.value) {
    return;
  }

  saving.value = true;

  try {
    await store.completeFamilySetup(
      memberDrafts.value.map((member) => ({ ...member })),
      petDrafts.value.map((pet) => ({ ...pet })),
    );
  } finally {
    saving.value = false;
  }
}

watch(
  () => store.familySetupOpen,
  (isOpen) => {
    if (!isOpen) {
      return;
    }

    activeSection.value = canEditDetails.value ? "details" : "accounts";
    resetDetails();
  },
  { immediate: true },
);

watch(activeSection, (section) => {
  if (section === "details") {
    resetDetails();
  }
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.family-setup-card {
  max-height: min(50rem, 92dvh);
  overflow: hidden;
}

.setup-header {
  border-bottom: rem(1) solid var(--lad-border);
  background: linear-gradient(145deg, var(--lad-color-primary-soft), var(--lad-surface-raised));
}

.setup-header h2 {
  color: var(--lad-text);
  font-size: rem(22);
  letter-spacing: -0.04em;
}

.eyebrow {
  color: var(--lad-color-primary-deep);
  font-size: rem(9);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.setup-tabs {
  border-radius: rem(14);
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
}

.section-heading,
.member-heading {
  --uno: d-flex align-center justify-space-between;
  gap: 0.75rem;
}

.section-title {
  color: var(--lad-text);
  font-size: rem(17);
}

.section-description {
  margin-top: 0.25rem;
  color: var(--lad-muted);
  font-size: rem(10);
}

.setup-person {
  border: rem(1) solid var(--lad-border);
}

.member-heading {
  justify-content: flex-start;
}

.member-heading > span:nth-child(2) {
  --uno: flex-grow-1 min-w-0;
}

.member-heading strong,
.member-heading small {
  --uno: d-block;
}

.member-heading small {
  color: var(--lad-muted);
  font-size: rem(9);
}

.member-preview {
  width: rem(43);
  height: rem(43);
  --uno: d-grid place-center flex-shrink-0;
  border-radius: rem(14);
  font-size: rem(22);
}

.member-fields {
  --uno: d-grid;
  grid-template-columns: minmax(0, 1fr) 5.5rem 5rem;
  gap: 0.5rem;
}

.pet-fields {
  --uno: d-grid align-center;
  grid-template-columns: auto minmax(0, 1fr) 7rem 5rem 5rem auto;
  gap: 0.5rem;
}

@include respond-down(phone) {
  .member-fields,
  .pet-fields {
    grid-template-columns: 1fr;
  }

  .member-preview {
    justify-self: start;
  }
}
</style>
