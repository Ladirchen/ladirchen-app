<template>
  <div class="page page-padding profile-page">
    <PageHeader
      description="Dein Profil, deine Fortschritte und deine Einstellungen."
      eyebrow="Persönlicher Bereich"
      title="Ich"
      tone="blue"
    >
      <template #action>
        <div class="profile-header-avatar">
          <AvatarFigure :appearance="activeAppearance" :size="72" />
          <button class="profile-edit-trigger" aria-label="Profil bearbeiten" type="button" @click="profileEditorOpen = true">
            <svg viewBox="0 0 36 36" aria-hidden="true">
              <path class="edit-note" d="M8 7h17l4 4v18H8Z" />
              <path class="edit-note-fold" d="M25 7v5h5" />
              <path class="edit-line" d="M12 14h8m-8 5h6" />
              <g class="edit-pencil">
                <path d="m14 27 2-6L27 10l4 4-11 11Z" />
                <path d="m14 27 5-2-3-3Z" />
              </g>
              <path class="edit-spark" d="m8 3 1.3 3L12 7.2 9.3 8.5 8 12 6.7 8.5 4 7.2 6.7 6Z" />
            </svg>
          </button>
        </div>
      </template>
    </PageHeader>

    <template v-if="store.viewerRole === 'child'">
      <v-card class="ladi-card pa-4" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3">
          <LadiMascot :score="store.averageTaskRating" :size="78" />
          <div><p class="eyebrow mb-1">Mein Ladi-Level</p><strong>{{ ladiStage.name }}</strong><p class="text-caption text-medium-emphasis mt-1">{{ ladiStage.description }}</p></div>
        </div>
      </v-card>

      <v-card class="settings-card pa-4 mt-4" elevation="0" rounded="xl">
        <strong>Meine Einstellungen</strong>
        <div class="setting-row mt-3"><span><v-icon size="19">mdi-bell-outline</v-icon> Erinnerungen</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
        <div class="setting-row"><span><v-icon size="19">mdi-eye-outline</v-icon> Familienziele sehen</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
      </v-card>
    </template>

    <v-card v-else-if="store.isFamilyAdmin" class="settings-card pa-4 mt-5" elevation="0" rounded="xl">
      <div class="d-flex align-center justify-space-between ga-2">
        <strong>Meine Berechtigungen</strong>
        <v-chip color="info" size="small" variant="tonal">Administration</v-chip>
      </div>
      <div class="permission-list mt-3"><span>✓ Familie und Berechtigungen verwalten</span><span>✓ Beiträge erstellen und bewerten</span><span>✓ Sparziele und Shop-Elemente verwalten</span><span>✓ Bonusaktionen planen</span></div>
    </v-card>

    <v-dialog v-model="profileEditorOpen" max-width="420">
      <v-card class="profile-editor-card pa-5" rounded="xl">
        <div class="d-flex align-start justify-space-between ga-3">
          <div class="d-flex align-center ga-3">
            <AvatarFigure :appearance="activeAppearance" :size="76" />
            <div>
              <p class="eyebrow mb-1">Profil bearbeiten</p>
              <h2>{{ store.displayNameFor(store.signedInMemberId) }}</h2>
              <p class="text-caption text-medium-emphasis mt-1">{{ profileDescription }}</p>
            </div>
          </div>
          <v-btn aria-label="Profil bearbeiten schließen" icon="mdi-close" size="small" variant="text" @click="profileEditorOpen = false" />
        </div>

        <div v-if="store.viewerRole === 'child'" class="profile-editor-nickname mt-5">
          <div class="profile-editor-label">
            <span class="profile-nickname-icon" aria-hidden="true">✨</span>
            <div><strong>Mein Spitzname</strong><small>So nennt dich deine Familie.</small></div>
          </div>
          <div class="nickname-actions mt-3">
            <v-text-field v-model="nicknameDraft" counter="18" density="compact" hide-details label="Spitzname" maxlength="18" variant="outlined" />
            <v-btn color="secondary" :disabled="!nicknameChanged" rounded="lg" variant="flat" @click="saveNickname">Speichern</v-btn>
          </div>
        </div>

        <div class="profile-editor-actions mt-4">
          <v-btn color="primary" prepend-icon="mdi-palette-outline" rounded="lg" variant="flat" width="100%" @click="openAvatarBuilder">Aussehen gestalten</v-btn>
          <v-btn v-if="store.permissions.canManageFamily" color="info" prepend-icon="mdi-account-group-outline" rounded="lg" variant="tonal" width="100%" @click="store.openFamilySetup">Familie verwalten</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <AvatarBuilderDialog v-model="avatarBuilderOpen" :initial-appearance="activeAppearance" :profile-role="store.viewerRole" :user-name="store.displayNameFor(store.signedInMemberId)" @save="store.saveOwnAppearance" />
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue';

import AvatarBuilderDialog from '@/features/avatar/components/AvatarBuilderDialog.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import { getLadiStage } from '@/domain/ladi';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const avatarBuilderOpen = ref(false);
const profileEditorOpen = ref(false);
const nicknameDraft = ref(store.activeChild.nickname ?? '');
const activeAppearance = computed(() => store.signedInMember.appearance ?? (store.viewerRole === 'guardian' ? createGuardianAvatarAppearance() : createDefaultAvatarAppearance()));
const profileDescription = computed(() => {
  if (store.viewerRole === 'child') {return 'So sehen dich alle in eurer Familienwelt.';}
  return store.isFamilyAdmin
    ? 'Du begleitest die Kinder und verwaltest die Familienwelt.'
    : 'Du begleitest die Kinder in eurer Familienwelt.';
});
const ladiStage = computed(() => getLadiStage(store.averageTaskRating));
const nicknameChanged = computed(() => nicknameDraft.value.trim() !== (store.activeChild.nickname ?? ''));
const saveNickname = () => store.setOwnNickname(nicknameDraft.value);
const openAvatarBuilder = () => {
  profileEditorOpen.value = false;
  avatarBuilderOpen.value = true;
};
watch(() => store.activeChildId, () => { nicknameDraft.value = store.activeChild.nickname ?? ''; });
</script>

<style scoped>
.profile-header-avatar {
  width: 74px;
  height: 74px;
  @apply position-relative d-grid place-center;
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.7);
  box-shadow: 0 4px 0 rgba(78, 143, 221, 0.13);
}
.profile-header-avatar :deep(.avatar-figure) {
  border: 0;
  background: transparent;
  box-shadow: none;
}
.profile-edit-trigger {
  width: 39px;
  height: 39px;
  padding: 3px;
  @apply position-absolute d-grid place-center cursor-pointer;
  right: -7px;
  bottom: -5px;
  color: #287f68;
  border: 2px solid white;
  border-radius: 14px;
  background: linear-gradient(145deg, #effbf5, #fff3c9);
  box-shadow:
    0 4px 0 rgba(43, 126, 102, 0.24),
    0 7px 14px rgba(51, 99, 85, 0.16);
  transition:
    transform 160ms ease,
    box-shadow 160ms ease;
}
.profile-edit-trigger:hover {
  transform: translateY(-2px) rotate(-2deg);
  box-shadow:
    0 5px 0 rgba(43, 126, 102, 0.24),
    0 9px 16px rgba(51, 99, 85, 0.18);
}
.profile-edit-trigger:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 rgba(43, 126, 102, 0.24);
}
.profile-edit-trigger:focus-visible {
  outline: 3px solid rgba(78, 143, 221, 0.28);
  outline-offset: 2px;
}
.profile-edit-trigger svg {
  @apply w-100 h-100 d-block overflow-visible;
}
.edit-note {
  fill: #fffdf4;
  stroke: #4d9a82;
  stroke-linejoin: round;
  stroke-width: 1.6;
}
.edit-note-fold,
.edit-line {
  fill: none;
  stroke: #8ac8b4;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.5;
}
.edit-pencil {
  fill: #f3ad3d;
  stroke: #a96617;
  stroke-linejoin: round;
  stroke-width: 1.4;
  transform-box: fill-box;
  transform-origin: center;
  animation: edit-pencil-write 2.7s ease-in-out infinite;
}
.edit-spark {
  fill: #ffd45f;
  transform-box: fill-box;
  transform-origin: center;
  animation: edit-spark-twinkle 2.2s ease-in-out infinite;
}
.profile-editor-card {
  background: linear-gradient(145deg, #f2fbf7, #fff7df);
}
.profile-editor-card h2 {
  @apply ma-0;
  font-size: 21px;
  letter-spacing: -0.03em;
}
.profile-editor-nickname {
  padding: 13px;
  border: 1px solid rgba(77, 149, 119, 0.17);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.72);
}
.profile-editor-label {
  @apply d-flex align-center ga-2;
}
.profile-editor-label strong,
.profile-editor-label small {
  @apply d-block;
}
.profile-editor-label strong {
  font-size: 12px;
}
.profile-editor-label small {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 9px;
}
.profile-nickname-icon {
  width: 31px;
  height: 31px;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 11px;
  background: #fff0c7;
  font-size: 16px;
  animation: nickname-spark 3s ease-in-out infinite;
}
.profile-editor-actions {
  @apply d-grid ga-2;
}
.ladi-card,
.settings-card {
  border: 2px solid rgba(78, 143, 221, 0.16);
  box-shadow:
    0 7px 0 rgba(78, 143, 221, 0.12),
    0 13px 22px rgba(58, 96, 137, 0.05) !important;
}
.ladi-card {
  @apply position-relative overflow-hidden;
  background:
    radial-gradient(
      circle at 94% 10%,
      rgba(255, 211, 94, 0.22) 0 38px,
      transparent 39px
    ),
    linear-gradient(145deg, #fffaf0, #edf9f4);
}
.settings-card {
  background: linear-gradient(145deg, #fff, #eef7ff);
}
.nickname-actions {
  @apply d-grid;
  grid-template-columns: minmax(0, 1fr) auto;
  @apply align-center ga-2;
}
.setting-row {
  min-height: 55px;
  @apply d-flex align-center justify-space-between;
  border-top: 1px dashed rgba(78, 143, 221, 0.2);
}
.setting-row span {
  @apply d-flex align-center ga-2;
  font-size: 13px;
  @apply font-weight-bold;
}
.setting-row span :deep(.v-icon) {
  width: 33px;
  height: 33px;
  border: 2px solid #fff;
  border-radius: 11px;
  color: #3d82c3;
  background: #e3f2ff;
  box-shadow: 0 3px 0 rgba(78, 143, 221, 0.15);
}
.permission-list {
  @apply d-grid;
  gap: 10px;
  color: var(--lad-muted);
  font-size: 13px;
}
@keyframes nickname-spark {
  0%,
  100% {
    transform: rotate(-4deg) scale(0.95);
  }
  50% {
    transform: rotate(5deg) scale(1.08);
  }
}
@keyframes edit-pencil-write {
  0%,
  100% {
    transform: translate(0) rotate(0);
  }
  45% {
    transform: translate(-1.5px, 1.5px) rotate(-3deg);
  }
  65% {
    transform: translate(1px, -1px) rotate(2deg);
  }
}
@keyframes edit-spark-twinkle {
  0%,
  100% {
    opacity: 0.5;
    transform: scale(0.75) rotate(-8deg);
  }
  50% {
    opacity: 1;
    transform: scale(1.18) rotate(8deg);
  }
}
@media (prefers-reduced-motion: reduce) {
  .profile-nickname-icon,
  .edit-pencil,
  .edit-spark {
    animation: none;
  }
}
</style>
