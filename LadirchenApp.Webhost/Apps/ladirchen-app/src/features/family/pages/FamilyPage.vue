<template>
  <div class="page page-padding family-page">
    <section class="family-hero mb-5">
      <div class="family-hero-copy">
        <p class="family-hero-kicker">{{ t('family.hero.eyebrow') }}</p>
        <h1>{{ t('family.hero.title') }}</h1>
        <span>{{ t('family.hero.description') }}</span>
      </div>
      <div class="family-hero-avatars" :aria-label="t('family.hero.membersAria')">
        <span v-for="(member, memberIndex) in store.members.slice(0, 3)" :key="member.id">
          <AvatarFigure :appearance="appearanceFor(member, memberIndex)" :size="58" />
        </span>
        <i v-if="store.members.length > 3">+{{ store.members.length - 3 }}</i>
      </div>
    </section>

    <v-card v-if="store.permissions.canManageFamily" class="family-admin pa-4 mb-5" color="blue-lighten-5" elevation="0" rounded="xl">
      <div class="d-flex align-center ga-3">
        <v-avatar color="info" variant="tonal">⚙️</v-avatar>
        <div class="flex-grow-1">
          <strong>{{ t('family.admin.title') }}</strong>
          <p class="text-caption text-medium-emphasis">{{ t('family.admin.description') }}</p>
        </div>
        <v-btn color="info" rounded="lg" size="small" variant="flat" @click="store.openFamilySetup">{{ t('family.admin.edit') }}</v-btn>
      </div>
    </v-card>

    <section class="family-roster mb-6">
      <div class="family-section-heading">
        <div><p>{{ t('family.roster.eyebrow') }}</p><h2>{{ t('family.roster.title') }}</h2><span>{{ t('family.roster.count', { count: store.members.length }) }}</span></div>
        <v-btn v-if="store.permissions.canInviteMembers" color="primary" prepend-icon="mdi-account-plus-outline" rounded="lg" size="small" variant="tonal" @click="inviteDialog = true">{{ t('family.roster.invite') }}</v-btn>
      </div>
      <div class="member-grid">
        <v-card v-for="(member, memberIndex) in store.members" :key="member.id" class="member-card pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-center ga-3">
            <AvatarFigure :appearance="appearanceFor(member, memberIndex)" :size="46" />
            <div class="flex-grow-1 min-w-0">
              <strong>{{ member.name }}</strong>
              <p class="text-caption text-medium-emphasis">{{ member.role === 'guardian' ? t('family.roles.guardian') : goalTitle(member.id) }}</p>
            </div>
            <v-chip v-if="member.invitationPending" color="info" size="small" variant="tonal">{{ t('family.roster.invited') }}</v-chip>
            <v-chip v-else-if="member.role === 'child'" color="warning" size="small" variant="tonal">🔥 {{ streakDays(member.id) }}</v-chip>
            <v-chip v-else-if="store.permissions.canManageFamily" :color="member.guardianAccess === 'admin' ? 'info' : 'primary'" size="x-small" variant="tonal">{{ accessLabel(member.guardianAccess) }}</v-chip>
          </div>
          <div v-if="store.permissions.canManageFamily && member.role === 'guardian' && member.id !== store.signedInMemberId" class="permission-editor mt-3">
            <v-select
              density="compact"
              hide-details
              :items="guardianAccessOptions"
              item-title="title"
              item-value="value"
              :label="t('family.permissions.label')"
              :model-value="member.guardianAccess ?? 'supporter'"
              variant="outlined"
              @update:model-value="setGuardianAccess(member.id, $event)"
            />
          </div>
          <div v-if="member.role === 'guardian' && (store.permissions.canManageFamily || member.id === store.signedInMemberId)" class="weekly-participation mt-3">
            <div>
              <strong>{{ t('family.weekly.participate') }}</strong>
              <span>{{ t(member.participatesInWeeklyGoal ? 'family.weekly.active' : 'family.weekly.inactive') }}</span>
            </div>
            <v-switch
              color="primary"
              density="compact"
              hide-details
              :model-value="member.participatesInWeeklyGoal ?? false"
              @update:model-value="store.setWeeklyGoalParticipation(member.id, Boolean($event))"
            />
          </div>
          <div v-if="member.role === 'child' || member.participatesInWeeklyGoal" class="mt-3">
            <div class="d-flex align-center justify-space-between mb-1">
              <span class="text-caption text-medium-emphasis">{{ t(member.role === 'guardian' ? 'family.weekly.familyProgress' : 'family.weekly.personalProgress') }}</span>
              <strong class="text-caption">{{ store.contributionProgress(member.id) }} %</strong>
            </div>
            <v-progress-linear
              :color="store.averageTaskRatingFor(member.id) >= 5 ? 'primary' : 'warning'"
              height="7"
              :model-value="store.contributionProgress(member.id)"
              rounded
            />
          </div>
        </v-card>
      </div>
    </section>

    <section class="family-pets mb-6">
      <div class="family-section-heading family-section-heading--pets">
        <div><p>{{ t('family.pets.eyebrow') }}</p><h2>{{ t('family.pets.title') }}</h2><span>{{ t('family.pets.description') }}</span></div>
      </div>
      <div class="pet-grid">
        <v-card v-for="pet in store.pets" :key="pet.id" class="pet-card pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-center ga-3">
            <AnimatedPet :pet="pet" :size="58" />
            <div><strong>{{ pet.name }}</strong><p class="text-caption text-medium-emphasis">{{ t(`familyPets.kinds.${pet.kind}`) }}</p></div>
          </div>
        </v-card>
      </div>
    </section>

    <v-dialog v-model="inviteDialog" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0">{{ t('family.invite.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-5">{{ t('family.invite.description') }}</v-card-subtitle>
        <v-text-field v-model="invite.name" :label="t('family.setup.name')" variant="outlined" />
        <v-text-field v-model="invite.email" :label="t('family.invite.email')" type="email" variant="outlined" />
        <v-select v-model="invite.guardianAccess" :items="guardianAccessOptions" item-title="title" item-value="value" :label="t('family.permissions.label')" variant="outlined" />
        <v-alert class="mb-4" color="primary" density="compact" variant="tonal">{{ t('family.invite.notice') }}</v-alert>
        <div class="d-grid invite-actions ga-2">
          <v-btn rounded="lg" variant="text" @click="inviteDialog = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" :disabled="!canInvite" rounded="lg" variant="flat" @click="inviteGuardian">{{ t('family.invite.submit') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import AnimatedPet from '@/features/world/components/AnimatedPet.vue';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyMemberId, GuardianAccessLevel } from '@/domain/types';
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';
import { isGuardianAccessLevel } from '@/application/contracts/family-aggregate-validation';

const store = useFamilyWorldStore();
const { t } = useI18n();
const localize = useLocalizedDomainContent();
const inviteDialog = ref(false);
const invite = reactive<{ name: string; email: string; guardianAccess: GuardianAccessLevel }>({ name: '', email: '', guardianAccess: 'supporter' });
const guardianAccessOptions = computed<Array<{ title: string; value: GuardianAccessLevel }>>(() => [
  { title: t('family.permissions.supporter'), value: 'supporter' },
  { title: t('family.permissions.admin'), value: 'admin' },
]);
const canInvite = computed(() => invite.name.trim().length > 1 && /^[^@\s]+@[^\s@][^\s.@]*\.[^\s@]+$/.test(invite.email));
const appearanceFor = (member: FamilyMember, index: number): AvatarAppearance => {
  if (member.appearance) return member.appearance;
  if (member.role === 'guardian') return createGuardianAvatarAppearance(index % 2 === 0 ? 'grandma' : 'grandpa');
  const appearance = createDefaultAvatarAppearance();
  const variants: Array<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColorId: 'outfit-blue' },
    { hair: 'short', hairColorId: 'hair-black', outfit: 'overalls', outfitColorId: 'outfit-gold' },
    { hair: 'curls', hairColorId: 'hair-brown', outfit: 'space', outfitColorId: 'outfit-ocean' },
  ];
  return { ...appearance, ...(variants[index % variants.length] ?? {}) };
};
const accessLabel = (access?: GuardianAccessLevel) => t(access === 'admin' ? 'family.permissions.admin' : 'family.permissions.supporter');
const goalTitle = (memberId: FamilyMemberId) => {
  const goal = store.goals.find(item => item.ownerId === memberId);
  return goal ? localize.goal(goal).title : t('family.roster.noGoal');
};
const streakDays = (memberId: FamilyMemberId) => store.members.find((member) => member.id === memberId)?.weeklyStreak ?? 0;
const setGuardianAccess = (memberId: FamilyMemberId, value: unknown) => {
  if (isGuardianAccessLevel(value)) {store.setGuardianAccess(memberId, value);}
};
const inviteGuardian = () => {
  store.inviteGuardian(invite.name.trim(), invite.email.trim(), invite.guardianAccess);
  invite.name = '';
  invite.email = '';
  invite.guardianAccess = 'supporter';
  inviteDialog.value = false;
};
onMounted(() => {
  if (store.viewerRole !== 'child') return;
  window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: t('family.guide.title'),
    message: t('family.guide.message'),
    pageIntro: true,
  } })), 350);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.family-hero {
  min-height: 150px;
  padding: 23px 22px;
  @apply position-relative d-flex align-center justify-space-between overflow-hidden;
  gap: 16px;
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  border-radius: 27px;
  background:
    radial-gradient(
      circle at 88% 8%,
      color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent),
      transparent 26%
    ),
    radial-gradient(
      circle at 76% 92%,
      color-mix(in srgb, var(--lad-palette-teal-400) 18%, transparent),
      transparent 31%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-white),
      var(--lad-palette-background) 58%,
      var(--lad-palette-background)
    );
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-blue) 10%, transparent),
    0 16px 28px color-mix(in srgb, var(--lad-palette-blue-600) 8%, transparent);
}
.family-hero::after {
  content: "✦";
  @apply position-absolute;
  top: 15px;
  right: 18px;
  color: var(--lad-palette-amber-450);
  font-size: 0.8125rem;
  animation: family-spark 2.2s ease-in-out infinite;
}
.family-hero-copy {
  max-width: 245px;
  z-index: 1;
}
.family-hero-kicker {
  margin: 0 0 5px;
  color: var(--lad-palette-blue);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.family-hero h1 {
  margin: 0;
  color: var(--lad-palette-text);
  font-size: 1.8125rem;
  line-height: 1;
  letter-spacing: -0.05em;
}
.family-hero-copy > span {
  @apply d-block;
  margin-top: 9px;
  color: var(--lad-muted);
  font-size: 0.6875rem;
  line-height: 1.45;
}
.family-hero-avatars {
  min-width: 128px;
  height: 88px;
  @apply position-relative d-flex align-end justify-center;
  z-index: 1;
}
.family-hero-avatars > span {
  width: 52px;
  height: 67px;
  @apply d-grid place-center overflow-hidden;
  margin-left: -17px;
  border: 3px solid var(--lad-palette-white);
  border-radius: 19px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-teal-600) 15%, transparent),
    0 9px 15px color-mix(in srgb, var(--lad-palette-muted-700) 8%, transparent);
  animation: family-avatar-bob 3.2s ease-in-out infinite;
}
.family-hero-avatars > span:first-child {
  margin-left: 0;
}
.family-hero-avatars > span:nth-child(2) {
  animation-delay: -0.8s;
}
.family-hero-avatars > span:nth-child(3) {
  animation-delay: -1.6s;
}
.family-hero-avatars > span :deep(.avatar-figure) {
  margin-top: 7px;
}
.family-hero-avatars > i {
  min-width: 29px;
  height: 29px;
  @apply position-absolute d-grid place-center;
  right: -3px;
  bottom: 1px;
  color: var(--lad-palette-white);
  border: 3px solid var(--lad-palette-white);
  border-radius: 10px;
  background: var(--lad-palette-mint);
  box-shadow: 0 3px 0 var(--lad-palette-teal-600);
  font-size: 0.5625rem;
  font-style: normal;
  font-weight: var(--lad-font-weight-black);
}
.family-roster,
.family-pets {
  padding: 17px;
  @apply position-relative overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  border-radius: 27px;
  background:
    radial-gradient(
      circle at 94% 3%,
      color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent),
      transparent 22%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-surface),
      var(--lad-palette-background)
    );
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-blue) 10%, transparent),
    0 15px 24px color-mix(in srgb, var(--lad-palette-blue-600) 5%, transparent);
}
.family-pets {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-teal-550) 15%,
    transparent
  );
  background:
    radial-gradient(
      circle at 92% 4%,
      color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent),
      transparent 23%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-surface),
      var(--lad-palette-background)
    );
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-teal-550) 10%, transparent),
    0 15px 24px color-mix(in srgb, var(--lad-palette-muted-700) 5%, transparent);
}
.family-section-heading {
  margin-bottom: 14px;
  @apply d-flex align-center justify-space-between;
  gap: 12px;
}
.family-section-heading p {
  margin: 0 0 3px;
  color: var(--lad-palette-blue);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.09em;
  text-transform: uppercase;
}
.family-section-heading h2 {
  margin: 0;
  font-size: 1.25rem;
  letter-spacing: -0.035em;
}
.family-section-heading span {
  @apply d-block;
  margin-top: 3px;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.family-section-heading--pets p {
  color: var(--lad-mint-dark);
}
.member-grid {
  @apply d-grid;
  gap: 10px;
}
.pet-grid {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}
.member-card,
.pet-card {
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 15%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-palette-teal-550) 12%, transparent),
    0 12px 20px color-mix(in srgb, var(--lad-palette-muted-700) 5%, transparent) !important;
}
.permission-editor {
  padding-top: 10px;
  border-top: 1px solid var(--lad-border);
}
.weekly-participation {
  padding: 9px 10px;
  @apply d-flex align-center justify-space-between;
  gap: 10px;
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 15%, transparent);
  border-radius: 13px;
  background: color-mix(
    in srgb,
    var(--lad-palette-background) 80%,
    transparent
  );
}
.weekly-participation strong,
.weekly-participation span {
  @apply d-block;
}
.weekly-participation strong {
  font-size: 0.625rem;
}
.weekly-participation span {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.family-admin {
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
  background:
    radial-gradient(
      circle at 92% 10%,
      color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent) 0 34px,
      transparent 35px
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-surface)
    ) !important;
  box-shadow: 0 7px 0
    color-mix(in srgb, var(--lad-palette-blue) 12%, transparent) !important;
}
.invite-actions {
  grid-template-columns: 1fr 1.4fr;
}
@keyframes family-avatar-bob {
  0%,
  100% {
    transform: translateY(1px) rotate(-2deg);
  }
  50% {
    transform: translateY(-4px) rotate(2deg);
  }
}
@keyframes family-spark {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.75) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.18) rotate(22deg);
  }
}
@include reduced-motion {
  .family-hero::after,
  .family-hero-avatars > span {
    animation: none;
  }
}
@include respond-down(phone) {
  .family-hero {
    min-height: 138px;
    padding: 19px 16px;
  }
  .family-hero-copy {
    max-width: 205px;
  }
  .family-hero-avatars {
    min-width: 105px;
    transform: scale(0.88);
    transform-origin: right center;
  }
  .family-roster,
  .family-pets {
    padding: 14px;
  }
  .family-section-heading {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
