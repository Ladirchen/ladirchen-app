<template>
  <div class="page page-padding profile-page">
    <PageHeader
      :description="t('profile.header.description')"
      :eyebrow="t('profile.header.eyebrow')"
      :title="t('profile.header.title')"
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
        <span><strong>{{ t('profile.appearance.title') }}</strong><small>{{ t('profile.appearance.description') }}</small></span>
        <i class="profile-action-spark" aria-hidden="true">✦</i>
      </button>
      <button v-if="store.viewerRole === 'child'" class="profile-action-button nickname-button" type="button" @click="profileEditorOpen = true">
        <span class="profile-action-icon nickname-button-icon" aria-hidden="true"><v-icon icon="mdi-form-textbox" /></span>
        <span><strong>{{ t('profile.nickname.title') }}</strong><small>{{ store.activeChild.nickname || t('profile.nickname.fallback') }}</small></span>
        <i class="profile-action-spark" aria-hidden="true">✧</i>
      </button>
    </div>

    <v-card class="settings-card pa-4" elevation="0" rounded="xl">
      <strong>{{ t('profile.settings.title') }}</strong>
      <div class="setting-row mt-3">
        <span><v-icon size="19">mdi-translate</v-icon> {{ t('profile.language.label') }}</span>
        <v-select v-model="activeLocale" class="language-select" density="compact" hide-details :items="languageOptions" item-title="title" item-value="value" variant="outlined" />
      </div>
      <template v-if="store.viewerRole === 'child'">
        <div class="setting-row"><span><v-icon size="19">mdi-bell-outline</v-icon> {{ t('profile.settings.reminders') }}</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
        <div class="setting-row"><span><v-icon size="19">mdi-eye-outline</v-icon> {{ t('profile.settings.familyGoals') }}</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
      </template>
    </v-card>

    <v-card v-if="store.isFamilyAdmin" class="settings-card pa-4 mt-5" elevation="0" rounded="xl">
      <div class="d-flex align-center justify-space-between ga-2">
        <strong>{{ t('profile.permissions.title') }}</strong>
        <v-chip color="info" size="small" variant="tonal">{{ t('profile.permissions.administration') }}</v-chip>
      </div>
      <div class="permission-list mt-3"><span>✓ {{ t('profile.permissions.family') }}</span><span>✓ {{ t('profile.permissions.contributions') }}</span><span>✓ {{ t('profile.permissions.goalsAndShop') }}</span><span>✓ {{ t('profile.permissions.promotions') }}</span></div>
    </v-card>

    <button class="logout-card mt-5" type="button" @click="store.signOut">
      <span class="logout-icon" aria-hidden="true"><v-icon icon="mdi-logout-variant" /></span>
      <span><strong>{{ t('profile.logout.title') }}</strong><small>{{ t('profile.logout.description') }}</small></span>
      <v-icon class="logout-arrow" icon="mdi-arrow-right" />
    </button>

    <DevelopmentToolsPanel v-if="isDevelopment" />

    <v-dialog v-model="profileEditorOpen" max-width="420">
      <v-card class="profile-editor-card pa-5" rounded="xl">
        <div class="d-flex align-start justify-space-between ga-3">
          <div class="d-flex align-center ga-3">
            <AvatarFigure :appearance="activeAppearance" :size="76" />
            <div>
              <p class="eyebrow mb-1">{{ t('profile.editor.title') }}</p>
              <h2>{{ store.displayNameFor(store.signedInMemberId) }}</h2>
              <p class="text-caption text-medium-emphasis mt-1">{{ profileDescription }}</p>
            </div>
          </div>
          <v-btn :aria-label="t('profile.editor.close')" icon="mdi-close" size="small" variant="text" @click="profileEditorOpen = false" />
        </div>

        <div v-if="store.viewerRole === 'child'" class="profile-editor-nickname mt-5">
          <div class="profile-editor-label">
            <span class="profile-nickname-icon" aria-hidden="true">✨</span>
            <div><strong>{{ t('profile.editor.nicknameTitle') }}</strong><small>{{ t('profile.editor.nicknameDescription') }}</small></div>
          </div>
          <div class="nickname-actions mt-3">
            <v-text-field v-model="nicknameDraft" counter="18" density="compact" hide-details :label="t('profile.editor.nicknameLabel')" maxlength="18" variant="outlined" />
            <v-btn color="secondary" :disabled="!nicknameChanged" rounded="lg" variant="flat" @click="saveNickname">{{ t('common.save') }}</v-btn>
          </div>
        </div>

        <div v-if="store.permissions.canManageFamily" class="profile-editor-actions mt-4">
          <v-btn v-if="store.permissions.canManageFamily" color="info" prepend-icon="mdi-account-group-outline" rounded="lg" variant="tonal" width="100%" @click="store.openFamilySetup">{{ t('profile.editor.manageFamily') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <AvatarBuilderDialog v-model="avatarBuilderOpen" :initial-appearance="activeAppearance" :profile-role="store.viewerRole" :user-name="store.displayNameFor(store.signedInMemberId)" @save="store.saveOwnAppearance" />
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

import AvatarBuilderDialog from '@/features/avatar/components/AvatarBuilderDialog.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import DevelopmentToolsPanel from '../components/DevelopmentToolsPanel.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import { useFamilyWorldStore } from '@/stores/family-world';
import { setActiveLocale } from '@/plugins/i18n';
import type { SupportedLocale } from '@/plugins/i18n';
import { DEFAULT_LOCALE, isSupportedLocale, localeOptions } from '@/locales';

const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const isDevelopment = import.meta.env.DEV;
const avatarBuilderOpen = ref(false);
const profileEditorOpen = ref(false);
const nicknameDraft = ref(store.activeChild.nickname ?? '');
const activeLocale = computed<SupportedLocale>({
  get: () => isSupportedLocale(locale.value) ? locale.value : DEFAULT_LOCALE,
  set: setActiveLocale,
});
const languageOptions = localeOptions.map(option => ({ title: option.label, value: option.code }));
const activeAppearance = computed(() => store.signedInMember.appearance ?? (store.viewerRole === 'guardian' ? createGuardianAvatarAppearance() : createDefaultAvatarAppearance()));
const profileDescription = computed(() => {
  if (store.viewerRole === 'child') {return t('profile.editor.childDescription');}
  return store.isFamilyAdmin
    ? t('profile.editor.administratorDescription')
    : t('profile.editor.guardianDescription');
});
const nicknameChanged = computed(() => nicknameDraft.value.trim() !== (store.activeChild.nickname ?? ''));
const saveNickname = () => store.setOwnNickname(nicknameDraft.value);
watch(() => store.activeChildId, () => { nicknameDraft.value = store.activeChild.nickname ?? ''; });
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.profile-header-avatar {
  width: 4.625rem;
  height: 4.625rem;
  @apply position-relative d-grid place-center;
  border-radius: 1.5rem;
  background: color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-palette-blue) 12%, transparent);
}
.profile-header-avatar :deep(.avatar-figure) {
  border: 0;
  background: transparent;
  box-shadow: none;
}
.profile-editor-card {
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
}
.profile-editor-card h2 {
  @apply ma-0;
  font-size: 1.3125rem;
  letter-spacing: -0.03em;
}
.profile-editor-nickname {
  padding: 0.8125rem;
  border: 0.0625rem solid
    color-mix(in srgb, var(--lad-palette-teal-550) 18%, transparent);
  border-radius: 1.125rem;
  background: color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
}
.profile-editor-label {
  @apply d-flex align-center ga-2;
}
.profile-editor-label strong,
.profile-editor-label small {
  @apply d-block;
}
.profile-editor-label strong {
  font-size: 0.75rem;
}
.profile-editor-label small {
  margin-top: 0.0625rem;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.profile-nickname-icon {
  width: 1.9375rem;
  height: 1.9375rem;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 0.6875rem;
  background: var(--lad-palette-amber-100);
  font-size: 1rem;
  animation: nickname-spark 3s ease-in-out infinite;
}
.profile-editor-actions {
  @apply d-grid ga-2;
}
.profile-quick-actions {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.6875rem;
}
.profile-quick-actions--single {
  grid-template-columns: 1fr;
}
.profile-action-button {
  width: 100%;
  min-height: 5.125rem;
  padding: 0.6875rem 0.75rem 0.6875rem 0.5625rem;
  @apply position-relative d-flex align-center overflow-hidden text-left cursor-pointer;
  gap: 0.5625rem;
  color: var(--lad-text);
  border: 0.125rem solid
    color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
  border-radius: 1.3125rem;
  background:
    radial-gradient(
      circle at 88% 10%,
      color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-background) 58%,
      var(--lad-palette-amber-100)
    );
  box-shadow:
    0 0.375rem 0
      color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent),
    0 0.75rem 1.25rem
      color-mix(in srgb, var(--lad-palette-blue-600) 8%, transparent);
  font: inherit;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.profile-action-button::before {
  width: 2.625rem;
  height: 160%;
  content: "";
  @apply position-absolute;
  top: -30%;
  left: -4.0625rem;
  transform: rotate(18deg);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent),
    transparent
  );
  animation: appearance-shine 4s ease-in-out infinite;
}
.profile-action-button:nth-child(2)::before {
  animation-delay: -2s;
}
.profile-action-button:hover {
  transform: translateY(-0.1875rem);
  box-shadow:
    0 0.5rem 0
      color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent),
    0 1rem 1.5rem
      color-mix(in srgb, var(--lad-palette-blue-600) 10%, transparent);
}
.profile-action-button:active {
  transform: translateY(0.1875rem) scale(0.985);
  box-shadow: 0 0.125rem 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent);
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
  font-size: 0.8125rem;
  line-height: 1.2;
}
.profile-action-button small {
  margin-top: 0.1875rem;
  overflow: hidden;
  color: var(--lad-muted);
  font-size: 0.5rem;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.profile-action-icon {
  width: 2.875rem;
  height: 2.875rem;
  @apply d-grid place-center flex-shrink-0;
  z-index: 1;
  color: var(--lad-palette-white);
  border: 0.1875rem solid var(--lad-palette-white);
  border-radius: 1rem;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-blue)
  );
  box-shadow:
    0 0.25rem 0 var(--lad-palette-blue-strong),
    0 0.5rem 0.8125rem
      color-mix(in srgb, var(--lad-palette-blue-strong) 18%, transparent);
  transform: rotate(-5deg);
  animation: appearance-icon-float 2.8s ease-in-out infinite;
}
.profile-action-icon :deep(.v-icon) {
  font-size: 1.5rem;
}
.nickname-button {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-purple-350) 20%,
    transparent
  );
  background:
    radial-gradient(
      circle at 88% 10%,
      color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-surface),
      var(--lad-palette-background) 55%,
      var(--lad-palette-amber-100)
    );
  box-shadow:
    0 0.375rem 0
      color-mix(in srgb, var(--lad-palette-muted-500) 12%, transparent),
    0 0.75rem 1.25rem
      color-mix(in srgb, var(--lad-palette-pink-650) 5%, transparent);
}
.nickname-button-icon {
  background: linear-gradient(
    145deg,
    var(--lad-palette-pink-300),
    var(--lad-palette-violet-400)
  );
  box-shadow:
    0 0.25rem 0 var(--lad-palette-violet-500),
    0 0.5rem 0.8125rem
      color-mix(in srgb, var(--lad-palette-violet-500) 18%, transparent);
  animation-delay: -0.9s;
}
.profile-action-spark {
  @apply position-absolute;
  top: 0.4375rem;
  right: 0.5625rem;
  z-index: 2;
  color: var(--lad-palette-amber-450);
  font-style: normal;
  animation: edit-spark-twinkle 1.7s ease-in-out infinite;
}
.settings-card {
  border: 0.125rem solid
    color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 0.4375rem 0 color-mix(in srgb, var(--lad-palette-blue) 12%, transparent),
    0 0.8125rem 1.375rem
      color-mix(in srgb, var(--lad-palette-blue-600) 5%, transparent) !important;
}
.nickname-actions {
  @apply d-grid;
  grid-template-columns: minmax(0, 1fr) auto;
  @apply align-center ga-2;
}
.setting-row {
  min-height: 3.4375rem;
  @apply d-flex align-center justify-space-between;
  border-top: 0.0625rem dashed
    color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
}
.setting-row span {
  @apply d-flex align-center ga-2;
  font-size: 0.8125rem;
  @apply font-weight-bold;
}
.setting-row span :deep(.v-icon) {
  width: 2.0625rem;
  height: 2.0625rem;
  border: 0.125rem solid var(--lad-palette-white);
  border-radius: 0.6875rem;
  color: var(--lad-palette-blue-strong);
  background: var(--lad-palette-background);
  box-shadow: 0 0.1875rem 0
    color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
}
.language-select {
  max-width: 9.375rem;
}
.permission-list {
  @apply d-grid;
  gap: 0.625rem;
  color: var(--lad-muted);
  font-size: 0.8125rem;
}
.logout-card {
  width: 100%;
  min-height: 4.125rem;
  padding: 0.5625rem 0.8125rem;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 0.625rem;
  color: var(--lad-palette-orange-650);
  border: 0.125rem solid
    color-mix(in srgb, var(--lad-palette-red-400) 15%, transparent);
  border-radius: 1.1875rem;
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-surface)
  );
  box-shadow: 0 0.3125rem 0
    color-mix(in srgb, var(--lad-palette-red-500) 8%, transparent);
  font: inherit;
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.logout-card:hover {
  transform: translateY(-0.125rem);
  box-shadow: 0 0.4375rem 0
    color-mix(in srgb, var(--lad-palette-red-500) 8%, transparent);
}
.logout-card:active {
  transform: translateY(0.125rem);
  box-shadow: 0 0.125rem 0
    color-mix(in srgb, var(--lad-palette-red-500) 8%, transparent);
}
.logout-card > span:nth-child(2) {
  @apply flex-grow-1;
}
.logout-card strong,
.logout-card small {
  @apply d-block;
}
.logout-card strong {
  font-size: 0.8125rem;
}
.logout-card small {
  margin-top: 0.125rem;
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.logout-icon {
  width: 2.5rem;
  height: 2.5rem;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-palette-red-500);
  border: 0.125rem solid var(--lad-palette-white);
  border-radius: 0.8125rem;
  background: var(--lad-palette-amber-100);
  box-shadow: 0 0.1875rem 0
    color-mix(in srgb, var(--lad-palette-red-500) 10%, transparent);
}
.logout-arrow {
  color: var(--lad-palette-red-500);
  font-size: 1.25rem;
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
    left: -4.375rem;
  }
  72%,
  100% {
    left: 115%;
  }
}
@keyframes appearance-icon-float {
  0%,
  100% {
    transform: translateY(0.0625rem) rotate(-5deg);
  }
  50% {
    transform: translateY(-0.1875rem) rotate(4deg);
  }
}
@include respond-down(narrow) {
  .profile-quick-actions {
    grid-template-columns: 1fr;
  }
}
@include reduced-motion {
  .profile-nickname-icon,
  .profile-action-button::before,
  .profile-action-icon,
  .profile-action-spark {
    animation: none;
  }
}
</style>
