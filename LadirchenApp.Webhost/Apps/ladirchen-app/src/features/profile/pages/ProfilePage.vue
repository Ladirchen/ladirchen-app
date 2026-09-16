<template>
  <div class="page page-padding profile-page">
    <PageHeader
      :description="t('profile.header.description')"
      :eyebrow="t('profile.header.eyebrow')"
      :title="t('profile.header.title')"
      tone="profile"
    >
      <template #action>
        <div class="profile-header-avatar">
          <AvatarFigure :appearance="activeAppearance" :size="72" />
        </div>
      </template>
    </PageHeader>

    <div class="profile-quick-actions mb-5" :class="{ 'profile-quick-actions--single': store.viewerRole !== 'child' }">
      <button class="profile-action-button appearance-studio-button" type="button" @click="avatarBuilderOpen = true">
        <span class="profile-action-icon appearance-button-icon" aria-hidden="true"><v-icon icon="i-mdi:palette-outline" /></span>
        <span><strong>{{ t('profile.appearance.title') }}</strong><small>{{ t('profile.appearance.description') }}</small></span>
        <i class="profile-action-spark" aria-hidden="true">✦</i>
      </button>
      <button v-if="store.viewerRole === 'child'" class="profile-action-button nickname-button" type="button" @click="profileEditorOpen = true">
        <span class="profile-action-icon nickname-button-icon" aria-hidden="true"><v-icon icon="i-mdi:form-textbox" /></span>
        <span><strong>{{ t('profile.nickname.title') }}</strong><small>{{ store.activeChild.nickname || t('profile.nickname.fallback') }}</small></span>
        <i class="profile-action-spark" aria-hidden="true">✧</i>
      </button>
    </div>

    <BrandedCard class="settings-card pa-4" tone="profile">
      <strong>{{ t('profile.settings.title') }}</strong>
      <div class="setting-row mt-3">
        <span><i class="setting-icon" aria-hidden="true"><v-icon icon="i-mdi:translate" size="19" /></i>{{ t('profile.language.label') }}</span>
        <div class="language-options" role="group" :aria-label="t('profile.language.label')">
          <button
            v-for="option in languageOptions"
            :key="option.value"
            :aria-pressed="activeLocale === option.value"
            :class="{ active: activeLocale === option.value }"
            type="button"
            @click="activeLocale = option.value"
          >
            <i aria-hidden="true">{{ option.icon }}</i>
            <strong>{{ option.value.toUpperCase() }}</strong>
            <small>{{ option.title }}</small>
          </button>
        </div>
      </div>
      <template v-if="store.viewerRole === 'child'">
        <div class="setting-row"><span><i class="setting-icon" aria-hidden="true"><v-icon icon="i-mdi:bell-outline" size="19" /></i>{{ t('profile.settings.reminders') }}</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
        <div class="setting-row"><span><i class="setting-icon" aria-hidden="true"><v-icon icon="i-mdi:eye-outline" size="19" /></i>{{ t('profile.settings.familyGoals') }}</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
      </template>
    </BrandedCard>

    <BrandedCard v-if="store.isFamilyAdmin" class="settings-card pa-4 mt-5" tone="profile">
      <div class="d-flex align-center justify-space-between ga-2">
        <strong>{{ t('profile.permissions.title') }}</strong>
        <v-chip color="info" size="small" variant="tonal">{{ t('profile.permissions.administration') }}</v-chip>
      </div>
      <div class="permission-list mt-3"><span>✓ {{ t('profile.permissions.family') }}</span><span>✓ {{ t('profile.permissions.contributions') }}</span><span>✓ {{ t('profile.permissions.goalsAndShop') }}</span><span>✓ {{ t('profile.permissions.promotions') }}</span></div>
    </BrandedCard>

    <button class="logout-card mt-5" type="button" @click="store.signOut">
      <span class="logout-icon" aria-hidden="true"><v-icon icon="i-mdi:logout-variant" /></span>
      <span><strong>{{ t('profile.logout.title') }}</strong><small>{{ t('profile.logout.description') }}</small></span>
      <v-icon class="logout-arrow" icon="i-mdi:arrow-right" />
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
          <v-btn :aria-label="t('profile.editor.close')" icon="i-mdi:close" size="small" variant="text" @click="profileEditorOpen = false" />
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
          <v-btn v-if="store.permissions.canManageFamily" color="info" prepend-icon="i-mdi:account-group-outline" rounded="lg" variant="tonal" width="100%" @click="store.openFamilySetup">{{ t('profile.editor.manageFamily') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <AvatarBuilderDialog v-model="avatarBuilderOpen" :initial-appearance="activeAppearance" :profile-role="store.viewerRole" :user-name="store.displayNameFor(store.signedInMemberId)" @save="store.saveOwnAppearance" />
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

import AvatarBuilderDialog from '@/shared/components/avatar/AvatarBuilderDialog.vue';
import AvatarFigure from '@/shared/components/avatar/AvatarFigure.vue';
import DevelopmentToolsPanel from '../components/DevelopmentToolsPanel.vue';
import BrandedCard from '@/shared/components/ui/BrandedCard.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import { resolveFamilyMemberAvatarAppearance } from '@/domain/avatar';
import { useFamilyWorldStore } from '@/stores/family-world';
import { setActiveLocale } from '@/plugins/i18n';
import type { SupportedLocale } from '@/plugins/i18n';
import { DEFAULT_LOCALE, isSupportedLocale, localeOptions } from '@/locales';

const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const localDevelopmentHostnames = new Set(['localhost', '127.0.0.1', '::1', '[::1]']);
const isDevelopment = import.meta.env.DEV || localDevelopmentHostnames.has(window.location.hostname);
const avatarBuilderOpen = ref(false);
const profileEditorOpen = ref(false);
const nicknameDraft = ref(store.activeChild.nickname ?? '');
const activeLocale = computed<SupportedLocale>({
  get: () => isSupportedLocale(locale.value) ? locale.value : DEFAULT_LOCALE,
  set: setActiveLocale,
});
const languageOptions = localeOptions.map(option => ({
  icon: option.icon,
  title: option.label,
  value: option.code,
}));
const activeAppearance = computed(() => resolveFamilyMemberAvatarAppearance(store.signedInMember, store.members));
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
  width: rem(74);
  height: rem(74);
  @apply position-relative d-grid place-center;
  border-radius: 1.5rem;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-color-info) 12%, transparent);
}
.profile-header-avatar :deep(.avatar-figure) {
  border: 0;
  background: transparent;
  box-shadow: none;
}
.profile-editor-card {
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
}
.profile-editor-card h2 {
  @apply ma-0;
  font-size: rem(21);
  letter-spacing: -0.03em;
}
.profile-editor-nickname {
  padding: rem(13);
  border: rem(1) solid
    color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent);
  border-radius: rem(18);
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
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
  margin-top: rem(1);
  color: var(--lad-muted);
  font-size: rem(9);
}
.profile-nickname-icon {
  width: rem(31);
  height: rem(31);
  @apply d-grid place-center flex-shrink-0;
  border-radius: rem(11);
  background: var(--lad-color-reward-soft);
  font-size: 1rem;
  animation: nickname-spark 3s ease-in-out infinite;
}
.profile-editor-actions {
  @apply d-grid ga-2;
}
.profile-quick-actions {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: rem(11);
}
.profile-quick-actions--single {
  grid-template-columns: 1fr;
}
.profile-action-button {
  width: 100%;
  min-height: rem(82);
  padding: rem(11) 0.75rem rem(11) rem(9);
  @apply position-relative d-flex align-center overflow-hidden text-left cursor-pointer;
  gap: rem(9);
  color: var(--lad-text);
  border: rem(2) solid
    color-mix(in srgb, var(--lad-color-info) 20%, transparent);
  border-radius: rem(21);
  background:
    radial-gradient(
      circle at 88% 10%,
      color-mix(in srgb, var(--lad-color-reward) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-surface-soft) 58%,
      var(--lad-color-reward-soft)
    );
  box-shadow:
    0 rem(6) 0
      color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent),
    0 0.75rem 1.25rem
      color-mix(in srgb, var(--lad-color-info-deep) 8%, transparent);
  font: inherit;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.profile-action-button::before {
  width: rem(42);
  height: 160%;
  content: "";
  @apply position-absolute;
  top: -30%;
  left: -rem(65);
  transform: rotate(18deg);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-surface-raised) 80%, transparent),
    transparent
  );
  animation: appearance-shine 4s ease-in-out infinite;
}
.profile-action-button:nth-child(2)::before {
  animation-delay: -2s;
}
.profile-action-button:hover {
  transform: translateY(-rem(3));
  box-shadow:
    0 0.5rem 0 color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent),
    0 1rem 1.5rem
      color-mix(in srgb, var(--lad-color-info-deep) 10%, transparent);
}
.profile-action-button:active {
  transform: translateY(rem(3)) scale(0.985);
  box-shadow: 0 rem(2) 0
    color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent);
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
  font-size: rem(13);
  line-height: 1.2;
}
.profile-action-button small {
  margin-top: rem(3);
  @apply overflow-hidden;
  color: var(--lad-muted);
  font-size: 0.5rem;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.profile-action-icon {
  width: rem(46);
  height: rem(46);
  @apply d-grid place-center flex-shrink-0;
  z-index: 1;
  color: var(--lad-text-inverse);
  border: rem(3) solid var(--lad-border-on-accent);
  border-radius: 1rem;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-info)
  );
  box-shadow:
    0 0.25rem 0 var(--lad-color-info-strong),
    0 0.5rem rem(13)
      color-mix(in srgb, var(--lad-color-info-strong) 18%, transparent);
  transform: rotate(-5deg);
  animation: appearance-icon-float 2.8s ease-in-out infinite;
}
.profile-action-icon :deep(.v-icon) {
  font-size: 1.5rem;
}
.nickname-button {
  border-color: color-mix(
    in srgb,
    var(--lad-color-bonus-highlight) 20%,
    transparent
  );
  background:
    radial-gradient(
      circle at 88% 10%,
      color-mix(in srgb, var(--lad-color-reward) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface),
      var(--lad-surface-soft) 55%,
      var(--lad-color-reward-soft)
    );
  box-shadow:
    0 rem(6) 0
      color-mix(in srgb, var(--lad-neutral-decorative) 12%, transparent),
    0 0.75rem 1.25rem
      color-mix(in srgb, var(--lad-color-accent-pink-strong) 5%, transparent);
}
.nickname-button-icon {
  background: linear-gradient(
    145deg,
    var(--lad-color-accent-pink),
    var(--lad-color-bonus)
  );
  box-shadow:
    0 0.25rem 0 var(--lad-color-bonus-muted),
    0 0.5rem rem(13)
      color-mix(in srgb, var(--lad-color-bonus-muted) 18%, transparent);
  animation-delay: -0.9s;
}
.profile-action-spark {
  @apply position-absolute;
  top: rem(7);
  right: rem(9);
  z-index: 2;
  color: var(--lad-color-reward-border);
  font-style: normal;
  animation: edit-spark-twinkle 1.7s ease-in-out infinite;
}
.nickname-actions {
  @apply d-grid;
  grid-template-columns: minmax(0, 1fr) auto;
  @apply align-center ga-2;
}
.setting-row {
  min-height: rem(55);
  @apply d-flex align-center justify-space-between;
  border-top: rem(1) dashed
    color-mix(in srgb, var(--lad-color-info) 20%, transparent);
}
.setting-row > span {
  @apply d-flex align-center ga-2;
  font-size: rem(13);
  @apply font-weight-bold;
}
.setting-icon {
  width: rem(33);
  height: rem(33);
  @apply d-grid place-center flex-shrink-0;
  border: rem(2) solid var(--lad-border-on-accent);
  border-radius: rem(11);
  background: var(--lad-surface-raised);
  box-shadow: 0 rem(3) 0
    color-mix(in srgb, var(--lad-color-info) 22%, transparent);
  font-style: normal;
}
.setting-icon :deep(.v-icon) {
  color: var(--lad-color-info-deep);
  background-color: currentColor;
  opacity: 1;
}
.language-options {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: rem(6);
}
.language-options button {
  min-width: rem(70);
  min-height: 2.75rem;
  padding: rem(5) 0.5rem;
  @apply d-grid align-center cursor-pointer;
  grid-template-columns: auto auto;
  column-gap: rem(5);
  color: var(--lad-text-strong);
  border: rem(2) solid
    color-mix(in srgb, var(--lad-color-info) 18%, transparent);
  border-radius: rem(14);
  background: var(--lad-surface-raised);
  box-shadow: 0 rem(3) 0
    color-mix(in srgb, var(--lad-color-info-deep) 10%, transparent);
  font: inherit;
  transition:
    transform 0.15s ease,
    border-color 0.15s ease,
    background 0.15s ease,
    box-shadow 0.15s ease;
}
.language-options button:hover {
  transform: translateY(-rem(2));
}
.language-options button.active {
  color: var(--lad-color-primary-deep);
  border-color: var(--lad-color-primary-highlight);
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-color-primary-soft)
  );
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-color-primary-deep) 18%, transparent);
}
.language-options button i {
  grid-row: 1 / 3;
  font-size: rem(18);
  font-style: normal;
}
.language-options button strong {
  align-self: end;
  font-size: rem(10);
  line-height: 1;
}
.language-options button small {
  align-self: start;
  color: var(--lad-muted);
  font-size: rem(7);
  line-height: 1.15;
}
.permission-list {
  @apply d-grid;
  gap: rem(10);
  color: var(--lad-muted);
  font-size: rem(13);
}
.logout-card {
  width: 100%;
  min-height: rem(66);
  padding: rem(9) rem(13);
  @apply d-flex align-center text-left cursor-pointer;
  gap: rem(10);
  color: var(--lad-color-accent-warm-deep);
  border: rem(2) solid
    color-mix(in srgb, var(--lad-color-danger) 15%, transparent);
  border-radius: rem(19);
  background: linear-gradient(145deg, var(--lad-surface), var(--lad-surface));
  box-shadow: 0 rem(5) 0
    color-mix(in srgb, var(--lad-color-danger-muted) 8%, transparent);
  font: inherit;
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.logout-card:hover {
  transform: translateY(-rem(2));
  box-shadow: 0 rem(7) 0
    color-mix(in srgb, var(--lad-color-danger-muted) 8%, transparent);
}
.logout-card:active {
  transform: translateY(rem(2));
  box-shadow: 0 rem(2) 0
    color-mix(in srgb, var(--lad-color-danger-muted) 8%, transparent);
}
.logout-card > span:nth-child(2) {
  @apply flex-grow-1;
}
.logout-card strong,
.logout-card small {
  @apply d-block;
}
.logout-card strong {
  font-size: rem(13);
}
.logout-card small {
  margin-top: rem(2);
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.logout-icon {
  width: 2.5rem;
  height: 2.5rem;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-color-danger-muted);
  border: rem(2) solid var(--lad-border-on-accent);
  border-radius: rem(13);
  background: var(--lad-color-reward-soft);
  box-shadow: 0 rem(3) 0
    color-mix(in srgb, var(--lad-color-danger-muted) 10%, transparent);
}
.logout-arrow {
  color: var(--lad-color-danger-muted);
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
    left: -rem(70);
  }
  72%,
  100% {
    left: 115%;
  }
}
@keyframes appearance-icon-float {
  0%,
  100% {
    transform: translateY(rem(1)) rotate(-5deg);
  }
  50% {
    transform: translateY(-rem(3)) rotate(4deg);
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
