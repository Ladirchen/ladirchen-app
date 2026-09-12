<template>
  <div class="page page-padding family-page">
    <PageHeader
      description="Hier seht ihr Beiträge, Tagesserien und Ziele der ganzen Familie."
      eyebrow="Gemeinsam wachsen"
      title="Meine Familie"
    >
      <template #icon><AnimatedSectionIcon variant="family" /></template>
    </PageHeader>

    <v-card v-if="store.permissions.canManageFamily" class="family-admin pa-4 mb-5" color="blue-lighten-5" elevation="0" rounded="xl">
      <div class="d-flex align-center ga-3">
        <v-avatar color="info" variant="tonal">⚙️</v-avatar>
        <div class="flex-grow-1">
          <strong>Familie verwalten</strong>
          <p class="text-caption text-medium-emphasis">Kinder, Haustiere und Bezugspersonen bearbeiten.</p>
        </div>
        <v-btn color="info" rounded="lg" size="small" variant="flat" @click="store.openFamilySetup">Bearbeiten</v-btn>
      </div>
    </v-card>

    <v-card v-if="store.permissions.canManageFamily" class="currency-settings pa-4 mb-5" elevation="0" rounded="xl">
      <div class="d-flex align-start ga-3">
        <div class="currency-settings-icon" aria-hidden="true">↔</div>
        <div class="flex-grow-1 min-w-0">
          <strong>Familienwährung festlegen</strong>
          <p class="text-caption text-medium-emphasis mt-1">Bestimme, wie viele Ladirchen einer Einheit eurer Familienwährung entsprechen.</p>
        </div>
      </div>
      <div class="currency-fields mt-4">
        <v-select
          density="compact"
          hide-details
          :items="currencyOptions"
          item-title="title"
          item-value="value"
          label="Währung"
          :model-value="store.familyCurrencyCode"
          variant="outlined"
          @update:model-value="setCurrency"
        />
        <v-text-field
          density="compact"
          hide-details
          label="Ladirchen für 1 Einheit"
          min="1"
          :model-value="store.ladirchenPerCurrencyUnit"
          suffix="L"
          type="number"
          variant="outlined"
          @update:model-value="store.setLadirchenExchangeRate(Number($event))"
        />
      </div>
      <div class="currency-example mt-3">
        <span>Beispiel mit 100 L</span>
        <strong>{{ formatFamilyCurrency(store.familyCurrencyValue(100)) }}</strong>
      </div>
    </v-card>

    <section v-if="store.permissions.canManageFamily" class="guardian-overview mb-6">
      <SectionHeader
        description="Aufgaben, Guthaben und Sparfortschritt zentral zusammengefasst."
        eyebrow="Bezugspersonen-Dashboard"
        title="Alle Kinder im Überblick"
      >
        <template #action><v-chip color="info" size="small" variant="tonal">{{ childMembers.length }} Kinder</v-chip></template>
      </SectionHeader>

      <div class="overview-kpis mb-3">
        <div><span>Hausenergie</span><strong>{{ store.familyEnergy }} %</strong></div>
        <div><span>Wartet auf Prüfung</span><strong>{{ totalPending }}</strong></div>
        <div><span>Offene Beiträge</span><strong>{{ totalOpen }}</strong></div>
      </div>

      <div class="d-flex flex-column ga-3">
        <v-card v-for="(child, childIndex) in childMembers" :key="child.id" class="child-summary pa-4" :class="{ selected: child.id === store.activeChildId }" elevation="0" rounded="xl">
          <div class="d-flex align-center ga-3">
            <AvatarFigure :appearance="appearanceFor(child, childIndex)" :size="54" />
            <div class="flex-grow-1 min-w-0"><strong>{{ child.name }}</strong><p class="text-caption text-medium-emphasis">🔥 {{ streakDays(child.id) }} Tage in Serie</p></div>
            <v-chip v-if="child.id === store.activeChildId" color="primary" size="x-small" variant="tonal">Ausgewählt</v-chip>
          </div>
          <div class="child-metrics mt-3">
            <div><span>Guthaben</span><strong>{{ store.balanceFor(child.id) }} L</strong></div>
            <div><span>Sichtbar gespart</span><strong>{{ store.totalVisibleSavedFor(child.id) }} L</strong></div>
            <div><span>Bewertung</span><strong>{{ ratingLabel(child.id) }}</strong></div>
          </div>
          <div class="d-flex align-center justify-space-between mt-3 mb-1">
            <span class="text-caption text-medium-emphasis">Grundbeiträge</span><strong class="text-caption">{{ store.contributionProgress(child.id) }} %</strong>
          </div>
          <v-progress-linear :color="store.averageTaskRatingFor(child.id) >= 5 ? 'primary' : 'warning'" height="8" :model-value="store.contributionProgress(child.id)" rounded />
          <div class="d-flex align-center justify-space-between flex-wrap ga-2 mt-3">
            <div class="d-flex ga-2"><v-chip color="warning" size="x-small" variant="tonal">{{ store.pendingCountFor(child.id) }} zu prüfen</v-chip><v-chip color="info" size="x-small" variant="tonal">{{ store.openCountFor(child.id) }} offen</v-chip></div>
            <v-btn color="info" rounded="lg" size="small" variant="tonal" @click="store.selectChildForGuardian(child.id)">Details auswählen</v-btn>
          </div>
        </v-card>
      </div>
    </section>

    <SectionHeader description="Tagesserien und sichtbare Ziele auf einen Blick." title="Familienmitglieder">
      <template v-if="store.permissions.canInviteMembers" #action>
        <v-btn color="primary" prepend-icon="mdi-account-plus-outline" rounded="lg" size="small" variant="tonal" @click="inviteDialog = true">Einladen</v-btn>
      </template>
    </SectionHeader>

    <div class="member-grid mb-6">
      <v-card v-for="(member, memberIndex) in store.members" :key="member.id" class="member-card pa-4" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3">
          <AvatarFigure :appearance="appearanceFor(member, memberIndex)" :size="46" />
          <div class="flex-grow-1 min-w-0">
            <strong>{{ member.name }}</strong>
            <p class="text-caption text-medium-emphasis">{{ member.role === 'guardian' ? 'Bezugsperson' : goalTitle(member.id) }}</p>
          </div>
          <v-chip v-if="member.invitationPending" color="info" size="small" variant="tonal">Eingeladen</v-chip>
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
            label="Berechtigung"
            :model-value="member.guardianAccess ?? 'supporter'"
            variant="outlined"
            @update:model-value="store.setGuardianAccess(member.id, String($event) as GuardianAccessLevel)"
          />
        </div>
        <div v-if="member.role === 'child'" class="mt-3">
          <div class="d-flex align-center justify-space-between mb-1">
            <span class="text-caption text-medium-emphasis">Persönliches Wochenziel</span>
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

    <SectionHeader description="Auch sie gehören zur Familienwelt." title="Haustiere" />

    <div class="pet-grid mb-6">
      <v-card v-for="pet in store.pets" :key="pet.id" class="pet-card pa-4" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3">
          <AnimatedPet :pet="pet" :size="58" />
          <div><strong>{{ pet.name }}</strong><p class="text-caption text-medium-emphasis">{{ pet.kindLabel }}</p></div>
        </div>
      </v-card>
    </div>

    <v-card v-if="store.permissions.canManageFamily" class="family-world-controls pa-4" elevation="0" rounded="xl">
      <p class="eyebrow mb-1">Nur im Frontend-Prototyp</p>
      <strong>Wochenabschluss testen</strong>
      <p class="text-caption text-medium-emphasis mt-1 mb-4">Diese Schaltflächen simulieren später automatisch ausgeführte Wochenabschlüsse.</p>
      <v-alert class="mb-4" :color="store.houseMeetsMinimumEnergy ? 'success' : 'warning'" density="compact" variant="tonal">
        {{ store.houseMeetsMinimumEnergy ? `Die Hausenergie liegt bei ${store.familyEnergy} % – die nächste Stufe ist bereit.` : `Die Hausenergie liegt bei ${store.familyEnergy} %. Für die nächste Stufe fehlen noch ${60 - store.familyEnergy} Prozentpunkte.` }}
      </v-alert>
      <div class="d-grid control-actions ga-2">
        <v-btn color="error" rounded="lg" variant="tonal" @click="finishWeek(false)">Serie verpasst</v-btn>
        <v-btn class="raised-button" color="primary" :disabled="!store.houseMeetsMinimumEnergy" rounded="lg" variant="flat" @click="finishWeek(true)">Serie geschafft</v-btn>
      </div>
    </v-card>

    <v-dialog v-model="inviteDialog" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0">Bezugsperson einladen</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-5">Lege fest, welche Bereiche die eingeladene Person verwenden darf.</v-card-subtitle>
        <v-text-field v-model="invite.name" label="Name" variant="outlined" />
        <v-text-field v-model="invite.email" label="E-Mail-Adresse" type="email" variant="outlined" />
        <v-select v-model="invite.guardianAccess" :items="guardianAccessOptions" item-title="title" item-value="value" label="Berechtigung" variant="outlined" />
        <v-alert class="mb-4" color="primary" density="compact" variant="tonal">Zielbegleitung sieht nur öffentliche Kinderziele und kann diese unterstützen. Administration erhält die vollständige Verwaltung.</v-alert>
        <div class="d-grid invite-actions ga-2">
          <v-btn rounded="lg" variant="text" @click="inviteDialog = false">Abbrechen</v-btn>
          <v-btn color="primary" :disabled="!canInvite" rounded="lg" variant="flat" @click="inviteGuardian">Einladung vormerken</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="revealDialog" max-width="410" persistent>
      <v-card class="reveal-card pa-6 text-center" rounded="xl">
        <div class="reveal-rays" :class="{ missed: !lastWeekSuccessful }" />
        <div class="reveal-house">{{ houseLevelIcon }}</div>
        <p class="eyebrow mt-3">Wochenabschluss</p>
        <h2>{{ lastWeekSuccessful ? 'Neue Hausstufe enthüllt!' : 'Neue Woche, neuer Versuch' }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2 mb-5">
          {{ lastWeekSuccessful ? `Eure Familienwelt ist jetzt ein ${houseLevelName}.` : `Das Haus ist jetzt wieder ein ${houseLevelName}. Alle gekauften Dinge sind noch da.` }}
        </p>
        <v-btn color="primary" rounded="lg" variant="flat" width="100%" @click="revealDialog = false">Familienwelt ansehen</v-btn>
      </v-card>
    </v-dialog>

  </div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref } from 'vue';

import AnimatedSectionIcon from '@/shared/components/ui/AnimatedSectionIcon.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import AnimatedPet from '@/features/world/components/AnimatedPet.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import SectionHeader from '@/shared/components/ui/SectionHeader.vue';
import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from '@/features/world/data/house-catalog';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyCurrency, FamilyMember, FamilyMemberId, GuardianAccessLevel } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const revealDialog = ref(false);
const inviteDialog = ref(false);
const lastWeekSuccessful = ref(true);
const invite = reactive<{ name: string; email: string; guardianAccess: GuardianAccessLevel }>({ name: '', email: '', guardianAccess: 'supporter' });
const guardianAccessOptions: Array<{ title: string; value: GuardianAccessLevel }> = [
  { title: 'Zielbegleitung', value: 'supporter' },
  { title: 'Administration', value: 'admin' },
];
const currencyOptions: Array<{ title: string; value: FamilyCurrency }> = [
  { title: 'Schweizer Franken (CHF)', value: 'CHF' },
  { title: 'Euro (EUR)', value: 'EUR' },
  { title: 'Forint (HUF)', value: 'HUF' },
];
const currentLevel = computed(() => HOUSE_STAGES[store.houseLevel] ?? DEFAULT_HOUSE_STAGE);
const houseLevelName = computed(() => currentLevel.value.name);
const houseLevelIcon = computed(() => currentLevel.value.icon);
const canInvite = computed(() => invite.name.trim().length > 1 && /^[^@\s]+@[^\s@][^\s.@]*\.[^\s@]+$/.test(invite.email));
const childMembers = computed(() => store.members.filter((member) => member.role === 'child'));
const totalPending = computed(() => childMembers.value.reduce((sum, child) => sum + store.pendingCountFor(child.id), 0));
const totalOpen = computed(() => childMembers.value.reduce((sum, child) => sum + store.openCountFor(child.id), 0));
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
const accessLabel = (access?: GuardianAccessLevel) => access === 'admin' ? 'Administration' : 'Zielbegleitung';
const goalTitle = (memberId: FamilyMemberId) => store.goals.find((goal) => goal.ownerId === memberId)?.title ?? 'Noch kein sichtbares Ziel';
const streakDays = (memberId: FamilyMemberId) => store.members.find((member) => member.id === memberId)?.weeklyStreak ?? 0;
const ratingLabel = (memberId: FamilyMemberId) => {
  const rating = store.averageTaskRatingFor(memberId);
  return rating > 0 ? `${rating.toFixed(1)} / 5` : '–';
};
const setCurrency = (value: unknown) => store.setFamilyCurrency(String(value) as FamilyCurrency);
const formatFamilyCurrency = (value: number) => new Intl.NumberFormat('de-CH', {
  style: 'currency',
  currency: store.familyCurrencyCode,
}).format(value);
const inviteGuardian = () => {
  store.inviteGuardian(invite.name.trim(), invite.email.trim(), invite.guardianAccess);
  Object.assign(invite, { name: '', email: '', guardianAccess: 'supporter' as GuardianAccessLevel });
  inviteDialog.value = false;
};
const finishWeek = (successful: boolean) => {
  lastWeekSuccessful.value = successful;
  if (successful) {
    if (!store.completeWeekDemo()) return;
  } else {
    store.failWeekDemo();
  }
  revealDialog.value = true;
};
</script>

<style scoped>
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
.pet-card,
.family-world-controls {
  border: 2px solid rgba(64, 158, 121, 0.16);
  background: linear-gradient(145deg, #fff, #f1faf6);
  box-shadow:
    0 6px 0 rgba(64, 158, 121, 0.12),
    0 12px 20px rgba(50, 103, 82, 0.05) !important;
}
.permission-editor {
  padding-top: 10px;
  border-top: 1px solid var(--lad-border);
}
.family-admin {
  border: 2px solid rgba(78, 143, 221, 0.2);
  background:
    radial-gradient(
      circle at 92% 10%,
      rgba(255, 214, 105, 0.2) 0 34px,
      transparent 35px
    ),
    linear-gradient(145deg, #edf7ff, #fff9e7) !important;
  box-shadow: 0 7px 0 rgba(78, 143, 221, 0.13) !important;
}
.currency-settings {
  border: 2px solid rgba(242, 175, 66, 0.28);
  background: linear-gradient(145deg, #fffaf0, #f1faf5);
  box-shadow:
    0 7px 0 rgba(242, 175, 66, 0.14),
    0 13px 21px rgba(130, 93, 36, 0.05) !important;
}
.currency-settings-icon {
  width: 42px;
  height: 42px;
  @apply d-grid place-center;
  flex: 0 0 42px;
  color: #85570e;
  border-radius: 14px;
  background: #ffe9ad;
  font-size: 20px;
  font-weight: 950;
}
.currency-fields {
  @apply d-grid;
  grid-template-columns: 0.9fr 1.1fr;
  gap: 9px;
}
.currency-fields :deep(.v-field) {
  background: rgba(255, 255, 255, 0.78);
}
.currency-example {
  padding: 9px 11px;
  @apply d-flex align-center justify-space-between;
  gap: 10px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.8);
}
.currency-example span {
  color: var(--lad-muted);
  font-size: 10px;
}
.currency-example strong {
  color: var(--lad-mint-dark);
  font-size: 16px;
}
.overview-kpis {
  @apply d-grid;
  grid-template-columns: repeat(3, 1fr);
  @apply ga-2;
}
.overview-kpis > div {
  padding: 11px;
  border: 1px solid var(--lad-border);
  border-radius: 14px;
  background: var(--lad-surface-soft);
}
.overview-kpis span,
.overview-kpis strong,
.child-metrics span,
.child-metrics strong {
  @apply d-block;
}
.overview-kpis span,
.child-metrics span {
  color: var(--lad-muted);
  font-size: 9px;
}
.overview-kpis strong {
  margin-top: 2px;
  font-size: 18px;
}
.child-summary {
  border: 2px solid var(--lad-border);
  background: linear-gradient(145deg, #fff, #f7fbf9);
  box-shadow: 0 6px 0 var(--lad-border) !important;
}
.child-summary.selected {
  transform: translateY(-2px);
  border-color: rgba(62, 188, 140, 0.55);
  background: linear-gradient(145deg, #fff, #e9f9f1);
  box-shadow:
    0 8px 0 rgba(62, 188, 140, 0.22),
    0 13px 20px rgba(48, 118, 90, 0.08) !important;
}
.child-metrics {
  @apply d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 7px;
}
.child-metrics > div {
  @apply pa-2;
  border-radius: 10px;
  background: var(--lad-surface-soft);
}
.child-metrics strong {
  margin-top: 2px;
  font-size: 12px;
}
.control-actions {
  grid-template-columns: 1fr 1fr;
}
.invite-actions {
  grid-template-columns: 1fr 1.4fr;
}
.reveal-card {
  @apply position-relative overflow-hidden;
}
.reveal-rays {
  width: 190px;
  height: 190px;
  @apply position-absolute;
  top: 30px;
  left: 50%;
  transform: translateX(-50%);
  border-radius: 50%;
  background: repeating-conic-gradient(
    from 0deg,
    rgba(255, 201, 92, 0.38) 0deg 14deg,
    transparent 14deg 28deg
  );
  animation: rays-spin 8s linear infinite;
}
.reveal-rays.missed {
  opacity: 0.25;
  filter: grayscale(1);
}
.reveal-house {
  @apply position-relative;
  z-index: 1;
  margin-top: 26px;
  font-size: 88px;
  animation: house-reveal 850ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
.reveal-card h2 {
  @apply position-relative;
  z-index: 1;
  @apply ma-0;
  font-size: 23px;
}
@keyframes house-reveal {
  0% {
    opacity: 0;
    transform: translateY(35px) scale(0.45) rotate(-7deg);
  }
  70% {
    opacity: 1;
    transform: translateY(-8px) scale(1.12) rotate(2deg);
  }
  100% {
    transform: scale(1);
  }
}
@keyframes rays-spin {
  to {
    transform: translateX(-50%) rotate(360deg);
  }
}
@media (max-width: 380px) {
  .currency-fields {
    grid-template-columns: 1fr;
  }
}
</style>
