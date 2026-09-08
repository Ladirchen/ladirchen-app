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
        </div>
      </template>
    </PageHeader>

    <div class="profile-quick-actions mb-5" :class="{ 'profile-quick-actions--single': store.viewerRole !== 'child' }">
      <button class="profile-action-button appearance-studio-button" type="button" @click="avatarBuilderOpen = true">
        <span class="profile-action-icon appearance-button-icon" aria-hidden="true"><v-icon icon="mdi-palette-outline" /></span>
        <span><strong>Aussehen gestalten</strong><small>Mach deine Figur ganz zu deiner.</small></span>
        <i class="profile-action-spark" aria-hidden="true">✦</i>
      </button>
      <button v-if="store.viewerRole === 'child'" class="profile-action-button nickname-button" type="button" @click="profileEditorOpen = true">
        <span class="profile-action-icon nickname-button-icon" aria-hidden="true"><v-icon icon="mdi-form-textbox" /></span>
        <span><strong>Spitzname ändern</strong><small>{{ store.activeChild.nickname || 'Such dir einen Namen aus.' }}</small></span>
        <i class="profile-action-spark" aria-hidden="true">✧</i>
      </button>
    </div>

    <template v-if="store.viewerRole === 'child'">
      <v-card class="settings-card pa-4" elevation="0" rounded="xl">
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

    <button class="logout-card mt-5" type="button" @click="store.signOut">
      <span class="logout-icon" aria-hidden="true"><v-icon icon="mdi-logout-variant" /></span>
      <span><strong>Abmelden</strong><small>Zurück zur Anmeldung</small></span>
      <v-icon class="logout-arrow" icon="mdi-arrow-right" />
    </button>

    <DevelopmentToolsPanel v-if="isDevelopment" />

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

        <div v-if="store.permissions.canManageFamily" class="profile-editor-actions mt-4">
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
import DevelopmentToolsPanel from '../components/DevelopmentToolsPanel.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const isDevelopment = import.meta.env.DEV;
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
const nicknameChanged = computed(() => nicknameDraft.value.trim() !== (store.activeChild.nickname ?? ''));
const saveNickname = () => store.setOwnNickname(nicknameDraft.value);
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
.profile-quick-actions {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 11px;
}
.profile-quick-actions--single {
  grid-template-columns: 1fr;
}
.profile-action-button {
  width: 100%;
  min-height: 82px;
  padding: 11px 12px 11px 9px;
  @apply position-relative d-flex align-center overflow-hidden text-left cursor-pointer;
  gap: 9px;
  color: var(--lad-text);
  border: 2px solid rgba(73, 151, 198, 0.2);
  border-radius: 21px;
  background:
    radial-gradient(
      circle at 88% 10%,
      rgba(255, 215, 88, 0.3),
      transparent 27%
    ),
    linear-gradient(145deg, #eaf7ff, #edf9f3 58%, #fff4cf);
  box-shadow:
    0 6px 0 rgba(58, 127, 174, 0.14),
    0 12px 20px rgba(58, 107, 137, 0.07);
  font: inherit;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.profile-action-button::before {
  width: 42px;
  height: 160%;
  content: "";
  @apply position-absolute;
  top: -30%;
  left: -65px;
  transform: rotate(18deg);
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.8),
    transparent
  );
  animation: appearance-shine 4s ease-in-out infinite;
}
.profile-action-button:nth-child(2)::before {
  animation-delay: -2s;
}
.profile-action-button:hover {
  transform: translateY(-3px);
  box-shadow:
    0 8px 0 rgba(58, 127, 174, 0.14),
    0 16px 24px rgba(58, 107, 137, 0.1);
}
.profile-action-button:active {
  transform: translateY(3px) scale(0.985);
  box-shadow: 0 2px 0 rgba(58, 127, 174, 0.14);
}
.profile-action-button > span:nth-child(2) {
  @apply flex-grow-1 min-w-0;
  z-index: 1;
}
.profile-action-button strong,
.profile-action-button small {
  @apply d-block;
}
.profile-action-button strong {
  font-size: 13px;
  line-height: 1.2;
}
.profile-action-button small {
  margin-top: 3px;
  overflow: hidden;
  color: var(--lad-muted);
  font-size: 8px;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.profile-action-icon {
  width: 46px;
  height: 46px;
  @apply d-grid place-center flex-shrink-0;
  z-index: 1;
  color: #fff;
  border: 3px solid #fff;
  border-radius: 16px;
  background: linear-gradient(145deg, #6bc3a0, #4387d2);
  box-shadow:
    0 4px 0 #3574aa,
    0 8px 13px rgba(54, 116, 165, 0.18);
  transform: rotate(-5deg);
  animation: appearance-icon-float 2.8s ease-in-out infinite;
}
.profile-action-icon :deep(.v-icon) {
  font-size: 24px;
}
.nickname-button {
  border-color: rgba(184, 128, 198, 0.2);
  background:
    radial-gradient(
      circle at 88% 10%,
      rgba(255, 215, 88, 0.3),
      transparent 27%
    ),
    linear-gradient(145deg, #fff1f8, #f1f0ff 55%, #fff6d8);
  box-shadow:
    0 6px 0 rgba(143, 98, 157, 0.13),
    0 12px 20px rgba(110, 77, 121, 0.06);
}
.nickname-button-icon {
  background: linear-gradient(145deg, #d589c7, #826ec5);
  box-shadow:
    0 4px 0 #6d58a8,
    0 8px 13px rgba(102, 75, 153, 0.17);
  animation-delay: -0.9s;
}
.profile-action-spark {
  @apply position-absolute;
  top: 7px;
  right: 9px;
  z-index: 2;
  color: #e6a52d;
  font-style: normal;
  animation: edit-spark-twinkle 1.7s ease-in-out infinite;
}
.settings-card {
  border: 2px solid rgba(78, 143, 221, 0.16);
  background: linear-gradient(145deg, #fff, #eef7ff);
  box-shadow:
    0 7px 0 rgba(78, 143, 221, 0.12),
    0 13px 22px rgba(58, 96, 137, 0.05) !important;
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
.logout-card {
  width: 100%;
  min-height: 66px;
  padding: 9px 13px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 10px;
  color: #7c4c49;
  border: 2px solid rgba(213, 101, 92, 0.15);
  border-radius: 19px;
  background: linear-gradient(145deg, #fff7f3, #fffdf8);
  box-shadow: 0 5px 0 rgba(177, 83, 77, 0.09);
  font: inherit;
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.logout-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 7px 0 rgba(177, 83, 77, 0.09);
}
.logout-card:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 rgba(177, 83, 77, 0.09);
}
.logout-card > span:nth-child(2) {
  @apply flex-grow-1;
}
.logout-card strong,
.logout-card small {
  @apply d-block;
}
.logout-card strong {
  font-size: 13px;
}
.logout-card small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 8px;
}
.logout-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center flex-shrink-0;
  color: #ba5b52;
  border: 2px solid #fff;
  border-radius: 13px;
  background: #ffe8e2;
  box-shadow: 0 3px 0 rgba(185, 87, 78, 0.11);
}
.logout-arrow {
  color: #b95d55;
  font-size: 20px;
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
@keyframes appearance-shine {
  0%,
  45% {
    left: -70px;
  }
  72%,
  100% {
    left: 115%;
  }
}
@keyframes appearance-icon-float {
  0%,
  100% {
    transform: translateY(1px) rotate(-5deg);
  }
  50% {
    transform: translateY(-3px) rotate(4deg);
  }
}
@media (max-width: 390px) {
  .profile-quick-actions {
    grid-template-columns: 1fr;
  }
}
@media (prefers-reduced-motion: reduce) {
  .profile-nickname-icon,
  .profile-action-button::before,
  .profile-action-icon,
  .profile-action-spark {
    animation: none;
  }
}
</style>
