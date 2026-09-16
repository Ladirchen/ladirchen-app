<template>
  <div class="page page-padding contributions-page">
    <div class="page-heading d-flex align-start justify-space-between ga-3">
      <div><p class="eyebrow">Mitwirken</p><h1>Beiträge</h1><p>Grundbeiträge versorgen die Familienwelt. Zusatzbeiträge sind freiwillig und bringen zusätzliche Ladirchen.</p></div>
      <v-btn v-if="store.viewerRole === 'guardian'" aria-label="Beitrag direkt hinzufügen" color="primary" icon="mdi-plus" variant="flat" @click="addDialog = true" />
    </div>

    <v-alert v-if="store.viewerRole === 'guardian'" class="guardian-context mb-5" color="info" density="compact" icon="mdi-shield-account-outline" variant="tonal">
      Bezugspersonenansicht: Du kannst Beiträge offen lassen, fest zuweisen oder eine bestehende Zuweisung ändern.
    </v-alert>

    <template v-if="store.viewerRole === 'child'">
      <section class="contribution-finder mb-4" aria-labelledby="contribution-finder-title">
        <div class="finder-heading">
          <div class="finder-mascot" aria-hidden="true">
            <svg class="finder-eyes" viewBox="0 0 42 28">
              <g class="finder-eye finder-eye--left">
                <ellipse cx="12" cy="14" rx="9" ry="11" />
                <circle class="finder-pupil" cx="14" cy="15" r="3" />
                <circle class="finder-glint" cx="15" cy="14" r="1" />
              </g>
              <g class="finder-eye finder-eye--right">
                <ellipse cx="30" cy="14" rx="9" ry="11" />
                <circle class="finder-pupil" cx="32" cy="15" r="3" />
                <circle class="finder-glint" cx="33" cy="14" r="1" />
              </g>
            </svg>
          </div>
          <div>
            <h2 id="contribution-finder-title">Was möchtest du sehen?</h2>
            <p>Wähle zuerst, für wen die Aufgaben sind.</p>
          </div>
        </div>

        <div class="scope-choices" role="group" aria-label="Aufgaben auswählen">
          <button
            v-for="option in scopeOptions"
            :key="option.value"
            :aria-pressed="scopeFilter === option.value"
            :class="{ active: scopeFilter === option.value }"
            type="button"
            @click="scopeFilter = option.value"
          >
            <v-icon :icon="option.icon" size="25" />
            <strong>{{ option.title }}</strong>
            <span>{{ option.description }}</span>
            <b v-if="option.value === 'open'">{{ openContributionCount }}</b>
          </button>
        </div>

        <div class="kind-filter">
          <span class="kind-filter-label">Welche Art?</span>
          <div class="kind-choices" role="group" aria-label="Aufgabenart auswählen">
            <button
              v-for="option in kindFilterOptions"
              :key="option.value"
              :aria-pressed="filter === option.value"
              :class="{ active: filter === option.value }"
              type="button"
              @click="filter = option.value"
            >
              <span aria-hidden="true">{{ option.icon }}</span>{{ option.title }}
            </button>
          </div>
        </div>
      </section>

      <TransitionGroup class="d-flex flex-column ga-3" name="list" tag="div">
        <v-card v-for="contribution in filteredContributions" :key="contribution.id" class="contribution-item pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <v-avatar color="blue-lighten-5" rounded="lg" size="50">{{ contribution.icon }}</v-avatar>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center flex-wrap ga-2 mb-1">
                <strong>{{ contribution.title }}</strong>
                <v-chip :color="contribution.kind === 'basic' ? 'primary' : 'info'" size="x-small" variant="tonal">
                  {{ contribution.kind === 'basic' ? 'Grundbeitrag' : 'Spezialaufgabe' }}
                </v-chip>
                <v-chip :color="!contribution.assigneeId ? 'warning' : contribution.assigneeId === store.activeChildId ? 'success' : undefined" size="x-small" variant="tonal">{{ assigneeLabel(contribution) }}</v-chip>
              </div>
              <p class="text-caption text-medium-emphasis">{{ contribution.description }}</p>
              <div class="d-flex align-center flex-wrap ga-3 mt-3 contribution-meta">
                <span>🕒 {{ contribution.dueLabel }}</span>
                <span>🪙 {{ store.rewardForContribution(contribution.id) }} L</span>
                <span v-if="contribution.kind === 'basic'">⚡ {{ contribution.energy }} Energie</span>
                <v-chip v-if="promotionFor(contribution.id)" color="warning" size="x-small" variant="tonal">×{{ promotionFor(contribution.id)?.multiplier }}</v-chip>
                <PromotionCountdown v-if="promotionFor(contribution.id)" :deadline="promotionFor(contribution.id)?.deadline ?? '00:00'" />
              </div>
              <div v-if="contribution.invitedChildIds?.length" class="invited-team mt-3">
                <span>Gemeinsam mit</span>
                <v-chip v-for="name in invitedChildNames(contribution)" :key="name" color="info" size="x-small" variant="tonal">{{ name }}</v-chip>
              </div>
            </div>
          </div>
          <div class="d-flex align-center justify-end flex-wrap ga-2 mt-3">
            <v-btn v-if="!contribution.assigneeId && contribution.status === 'available'" class="raised-button" color="primary" prepend-icon="mdi-hand-back-right-outline" rounded="lg" variant="flat" @click="store.claimContribution(contribution.id)">Zu mir nehmen</v-btn>
            <template v-else-if="contribution.assigneeId === store.activeChildId">
              <v-btn v-if="contribution.kind === 'extra' && contribution.status === 'available'" prepend-icon="mdi-account-multiple-plus-outline" rounded="lg" size="small" variant="tonal" @click="openTeamInvite(contribution)">Geschwister einladen</v-btn>
              <v-chip v-if="contribution.status === 'approved'" color="success" prepend-icon="mdi-check" variant="tonal">Bestätigt</v-chip>
              <v-chip v-else-if="contribution.status === 'pending'" color="warning" prepend-icon="mdi-clock-outline" variant="tonal">Wartet auf Prüfung</v-chip>
              <v-btn v-else class="raised-button" color="info" rounded="lg" variant="flat" @click="store.submitContribution(contribution.id)">Als erledigt einreichen</v-btn>
            </template>
            <v-chip v-else color="surface-variant" prepend-icon="mdi-account-check-outline" variant="tonal">Bereits vergeben</v-chip>
          </div>
        </v-card>
      </TransitionGroup>
      <v-card v-if="!filteredContributions.length" class="empty-contributions pa-5 text-center" elevation="0" rounded="xl">
        <div class="empty-contributions-icon" aria-hidden="true">🌈</div>
        <strong>Hier ist gerade alles geschafft!</strong>
        <p class="text-caption text-medium-emphasis mt-1">Bei den freien Aufgaben wartet vielleicht noch etwas auf dich.</p>
        <v-btn v-if="scopeFilter !== 'open'" class="mt-3" color="primary" rounded="lg" variant="tonal" @click="scopeFilter = 'open'; filter = 'all'">Freie Aufgaben zeigen</v-btn>
      </v-card>
    </template>

    <template v-else>
      <section v-if="store.pendingContributions.length" class="mb-6">
        <div class="d-flex align-end justify-space-between mb-3">
          <div><h2 class="section-title">Wartet auf Prüfung</h2><p class="text-caption text-medium-emphasis">Erst nach der Bestätigung reagiert die Familienwelt.</p></div>
          <v-chip color="warning" size="small">{{ store.pendingContributions.length }}</v-chip>
        </div>
        <div class="d-flex flex-column ga-3">
          <v-card v-for="contribution in store.pendingContributions" :key="contribution.id" class="review-card pa-4" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <v-avatar color="blue-lighten-5" rounded="lg">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1"><strong>{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ memberName(contribution.assigneeId) }} · +{{ store.rewardForContribution(contribution.id) }} Ladirchen</p></div>
            </div>
            <div class="d-flex align-center justify-space-between ga-2 mt-3">
              <p class="text-caption text-medium-emphasis">Wie sorgfältig wurde der Beitrag erledigt?</p>
              <v-chip color="warning" size="x-small" variant="tonal">5 ★ = +{{ store.perfectRatingBonusPercent }} %</v-chip>
            </div>
            <v-rating v-model="ratings[contribution.id]" active-color="warning" class="my-2" density="compact" hover />
            <div class="d-grid review-actions ga-2">
              <v-btn rounded="lg" variant="tonal" @click="store.returnContribution(contribution.id)">Zurückgeben</v-btn>
              <v-btn class="raised-button" color="primary" rounded="lg" variant="flat" @click="store.approveContribution(contribution.id, ratings[contribution.id] ?? 4)">Bestätigen</v-btn>
            </div>
          </v-card>
        </div>
      </section>

      <v-card v-else class="empty-review pa-5 mb-6 text-center" color="green-lighten-5" elevation="0" rounded="xl">
        <div class="empty-icon mb-2">🌤️</div>
        <strong>Alles geprüft</strong>
        <p class="text-caption text-medium-emphasis mt-1">Aktuell wartet kein Beitrag auf eine Bestätigung.</p>
      </v-card>

      <div class="d-flex align-center justify-space-between mb-3">
        <div><h2 class="section-title">Bonusaktionen</h2><p class="text-caption text-medium-emphasis">Zeitlich begrenzte Extras für ausgewählte Beiträge.</p></div>
        <v-btn aria-label="Bonusaktion hinzufügen" color="warning" icon="mdi-lightning-bolt" size="small" variant="tonal" @click="promotionDialog = true" />
      </div>
      <div class="d-flex flex-column ga-2 mb-6">
        <v-card v-for="promotion in store.promotions.filter((item) => item.active)" :key="promotion.id" class="promotion-row pa-3" color="amber-lighten-5" elevation="0" rounded="lg" role="button" tabindex="0" @click="selectedPromotion = promotion" @keydown.enter="selectedPromotion = promotion">
          <div class="d-flex align-center ga-3">
            <v-avatar color="warning" size="40" variant="tonal">⚡</v-avatar>
            <div class="flex-grow-1">
              <strong class="text-body-small">{{ promotion.title }}</strong>
              <p class="text-caption text-medium-emphasis">{{ contributionTitle(promotion.contributionId) }} · Team +{{ promotion.teamworkBonus }} L</p>
              <PromotionCountdown class="mt-1" :deadline="promotion.deadline" />
            </div>
            <v-chip color="warning" size="small">×{{ promotion.multiplier }}</v-chip>
          </div>
        </v-card>
      </div>

      <v-card class="rating-setting pa-4 mb-6" color="surface" elevation="0" rounded="xl">
        <div class="d-flex align-start ga-3">
          <v-avatar color="warning" variant="tonal">★</v-avatar>
          <div class="flex-grow-1">
            <div class="d-flex align-center justify-space-between ga-3">
              <div><strong>Bonus für volle Sterne</strong><p class="text-caption text-medium-emphasis">Wird zusätzlich zur normalen und zur Aktionsbelohnung vergeben.</p></div>
              <strong class="bonus-value">{{ store.perfectRatingBonusPercent }} %</strong>
            </div>
            <v-slider v-model="store.perfectRatingBonusPercent" class="mt-3" color="warning" hide-details max="25" min="0" step="1" thumb-label />
          </div>
        </div>
      </v-card>

      <div class="d-flex align-center justify-space-between mb-3">
        <div><h2 class="section-title">Beiträge verwalten</h2><p class="text-caption text-medium-emphasis">Bezugspersonen können Grund- und Zusatzbeiträge anlegen.</p></div>
        <v-btn color="primary" icon="mdi-plus" size="small" variant="tonal" @click="addDialog = true" />
      </div>
      <div class="d-flex flex-column ga-2">
        <v-card v-for="contribution in managedContributions" :key="contribution.id" class="basic-row pa-3" elevation="0" rounded="lg">
          <div class="d-flex align-center ga-3">
            <v-avatar color="surface-variant" rounded="lg" size="40">{{ contribution.icon }}</v-avatar>
            <div class="flex-grow-1 min-w-0"><strong class="text-body-small">{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ contribution.kind === 'basic' ? `${contribution.energy} Energiepunkte` : 'Spezialaufgabe' }} · {{ contribution.reward }} L</p></div>
            <v-select
              class="assignment-select"
              density="compact"
              hide-details
              :disabled="contribution.status !== 'available'"
              :items="assignmentOptions"
              item-title="title"
              item-value="value"
              :model-value="contribution.assigneeId ?? ''"
              variant="outlined"
              @update:model-value="store.assignContribution(contribution.id, String($event) || undefined)"
            />
          </div>
        </v-card>
      </div>
    </template>

    <v-dialog v-model="addDialog" max-width="440">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">Beitrag hinzufügen</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">Nur lokal im Frontend-Prototyp</v-card-subtitle>
        <v-text-field v-model="newContribution.title" label="Titel" variant="outlined" />
        <v-textarea v-model="newContribution.description" label="Kurze Beschreibung" rows="2" variant="outlined" />
        <v-select v-model="newContribution.assigneeId" :items="assignmentOptions" item-title="title" item-value="value" label="Zuweisung (optional)" variant="outlined" />
        <div class="d-grid form-columns ga-3">
          <v-select v-model="newContribution.kind" :items="kindOptions" item-title="title" item-value="value" label="Art" variant="outlined" />
          <v-text-field v-model.number="newContribution.reward" label="Ladirchen" min="0" type="number" variant="outlined" />
        </div>
        <v-text-field v-model.number="newPromotion.teamworkBonus" label="Teamarbeitsbonus" min="0" suffix="L" type="number" variant="outlined" />
        <v-slider v-if="newContribution.kind === 'basic'" v-model="newContribution.energy" color="primary" label="Energieanteil" max="50" min="5" step="5" thumb-label />
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="addDialog = false">Abbrechen</v-btn>
          <v-btn color="primary" :disabled="!newContribution.title.trim()" rounded="lg" variant="flat" @click="addContribution">Hinzufügen</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="teamDialog" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <div class="team-dialog-icon mb-3">🤝</div>
        <v-card-title class="pa-0 mb-1">Geschwister einladen</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ teamContribution?.title }} gemeinsam erledigen</v-card-subtitle>
        <v-select
          v-model="selectedSiblingIds"
          chips
          closable-chips
          :items="siblingOptions"
          item-title="title"
          item-value="value"
          label="Kinder auswählen"
          multiple
          variant="outlined"
        />
        <div class="sibling-rule mb-4">
          <v-icon color="info" size="18">mdi-shield-check-outline</v-icon>
          <span>Nur Geschwister können mitmachen. Bezugspersonen werden nicht als Teilnehmer angeboten.</span>
        </div>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="teamDialog = false">Abbrechen</v-btn>
          <v-btn color="info" rounded="lg" variant="flat" @click="saveTeamInvite">Einladung speichern</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(selectedPromotion)" max-width="410" @update:model-value="selectedPromotion = undefined">
      <v-card v-if="selectedPromotion" class="pa-5" rounded="xl">
        <div class="promotion-detail-icon">⚡</div>
        <p class="eyebrow mt-3 mb-1">Bonusaktion</p>
        <h2 class="promotion-detail-title">{{ selectedPromotion.title }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2">{{ contributionTitle(selectedPromotion.contributionId) }} muss bis {{ selectedPromotion.deadline }} Uhr erledigt sein.</p>
        <PromotionCountdown class="mt-3" :deadline="selectedPromotion.deadline" />
        <div class="promotion-reward mt-4">
          <span>Erreichbare Belohnung</span>
          <strong>{{ store.rewardForContribution(selectedPromotion.contributionId) }} L</strong>
          <small>inklusive +{{ selectedPromotion.teamworkBonus }} L für Teamarbeit</small>
        </div>
        <v-btn class="mt-5" color="warning" rounded="lg" variant="flat" width="100%" @click="selectedPromotion = undefined">Verstanden</v-btn>
      </v-card>
    </v-dialog>

    <v-dialog v-model="promotionDialog" max-width="440">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">Bonusaktion starten</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">Die Aktion ist sofort für die Familie sichtbar.</v-card-subtitle>
        <v-select
          v-model="newPromotion.contributionId"
          :items="promotionContributionOptions"
          item-title="title"
          item-value="value"
          label="Beitrag"
          variant="outlined"
        />
        <div class="d-grid form-columns ga-3">
          <v-select v-model="newPromotion.multiplier" :items="multiplierOptions" item-title="title" item-value="value" label="Bonus" variant="outlined" />
          <v-text-field v-model="newPromotion.deadline" label="Gültig bis" type="time" variant="outlined" />
        </div>
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="promotionDialog = false">Abbrechen</v-btn>
          <v-btn color="warning" :disabled="!newPromotion.contributionId || !newPromotion.deadline" rounded="lg" variant="flat" @click="addPromotion">Aktion starten</v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

import PromotionCountdown from '../components/PromotionCountdown.vue';
import type { Contribution, ContributionKind, Promotion } from '../domain/types';
import { isPromotionAvailable } from '../domain/promotions';
import { usePrototypeStore } from '../stores/prototype';

const store = usePrototypeStore();
const route = useRoute();
const filter = ref<'all' | ContributionKind>('all');
const scopeFilter = ref<'all' | 'mine' | 'open'>('mine');
const addDialog = ref(false);
const promotionDialog = ref(false);
const teamDialog = ref(false);
const teamContributionId = ref('');
const selectedSiblingIds = ref<string[]>([]);
const selectedPromotion = ref<Promotion>();
const ratings = reactive<Record<string, number>>({});
const newContribution = reactive({
  title: '',
  description: '',
  icon: '✨',
  reward: 10,
  energy: 10,
  kind: 'basic' as ContributionKind,
  assigneeId: '',
});
const kindOptions = [
  { title: 'Grundbeitrag', value: 'basic' },
  { title: 'Spezialaufgabe (freiwillig)', value: 'extra' },
];
const childOptions = computed(() =>
  store.members.filter((member) => member.role === 'child').map((member) => ({ title: member.name, value: member.id })),
);
const assignmentOptions = computed(() => [
  { title: 'Noch offen – Kinder wählen selbst', value: '' },
  ...childOptions.value,
]);
const newPromotion = reactive({ contributionId: 'dishwasher', multiplier: 2, deadline: '17:00', teamworkBonus: 10 });
const multiplierOptions = [
  { title: 'Doppelte Ladirchen', value: 2 },
  { title: 'Dreifache Ladirchen', value: 3 },
];
const scopeOptions: Array<{ value: 'mine' | 'open' | 'all'; title: string; description: string; icon: string }> = [
  { value: 'mine', title: 'Meine Aufgaben', description: 'Schon für dich', icon: 'mdi-account-star-outline' },
  { value: 'open', title: 'Freie Aufgaben', description: 'Such dir eine aus', icon: 'mdi-hand-wave-outline' },
  { value: 'all', title: 'Alle Aufgaben', description: 'Alles ansehen', icon: 'mdi-view-grid-outline' },
];
const kindFilterOptions: Array<{ value: 'all' | ContributionKind; title: string; icon: string }> = [
  { value: 'all', title: 'Alle', icon: '✨' },
  { value: 'basic', title: 'Hausenergie', icon: '⚡' },
  { value: 'extra', title: 'Extra-Ladirchen', icon: '🪙' },
];

const filteredContributions = computed(() =>
  store.contributions.filter(
    (contribution) =>
      (filter.value === 'all' || contribution.kind === filter.value) &&
      (scopeFilter.value === 'all' ||
        (scopeFilter.value === 'mine' && contribution.assigneeId === store.activeChildId) ||
        (scopeFilter.value === 'open' && !contribution.assigneeId && contribution.status === 'available')),
  ),
);
const managedContributions = computed(() => [...store.contributions].sort((left, right) =>
  Number(Boolean(left.assigneeId)) - Number(Boolean(right.assigneeId)),
));
const openContributionCount = computed(() => store.contributions.filter(
  (contribution) => !contribution.assigneeId && contribution.status === 'available',
).length);
const promotionContributionOptions = computed(() =>
  store.contributions.map((contribution) => ({ title: contribution.title, value: contribution.id })),
);
const teamContribution = computed(() => store.contributions.find((contribution) => contribution.id === teamContributionId.value));
const siblingOptions = computed(() => store.members
  .filter((member) => member.role === 'child' && member.id !== teamContribution.value?.assigneeId)
  .map((member) => ({ title: `${member.avatar} ${member.name}`, value: member.id })),
);
const promotionFor = (contributionId: string) =>
  store.promotions.find((promotion) => promotion.contributionId === contributionId && isPromotionAvailable(promotion));
const contributionTitle = (contributionId: string) =>
  store.contributions.find((contribution) => contribution.id === contributionId)?.title ?? 'Beitrag';
const memberName = (memberId?: string) => store.members.find((member) => member.id === memberId)?.name ?? 'Noch offen';
const assigneeLabel = (contribution: Contribution) => {
  if (!contribution.assigneeId) return 'Noch frei';
  if (contribution.assigneeId === store.activeChildId) return 'Für dich';
  return memberName(contribution.assigneeId);
};
const invitedChildNames = (contribution: Contribution) => store.members
  .filter((member) => member.role === 'child' && contribution.invitedChildIds?.includes(member.id))
  .map((member) => `${member.avatar} ${member.name}`);
const openTeamInvite = (contribution: Contribution) => {
  teamContributionId.value = contribution.id;
  selectedSiblingIds.value = [...(contribution.invitedChildIds ?? [])];
  teamDialog.value = true;
};
const saveTeamInvite = () => {
  store.setContributionPartners(teamContributionId.value, selectedSiblingIds.value);
  teamDialog.value = false;
};

const addContribution = () => {
  store.addContribution({ ...newContribution });
  addDialog.value = false;
  Object.assign(newContribution, { title: '', description: '', icon: '✨', reward: 10, energy: 10, kind: 'basic', assigneeId: '' });
};
const addPromotion = () => {
  store.addPromotion({ ...newPromotion });
  promotionDialog.value = false;
  Object.assign(newPromotion, { contributionId: 'dishwasher', multiplier: 2, deadline: '17:00', teamworkBonus: 10 });
};
watch(
  () => route.query.new,
  (value) => { if (value === '1' && store.viewerRole === 'guardian') addDialog.value = true; },
  { immediate: true },
);
</script>

<style scoped>
.contribution-finder {
  padding: 15px;
  border: 1px solid rgba(62, 188, 140, 0.25);
  border-radius: 24px;
  background: linear-gradient(145deg, #f0fbf6 0%, #fffaf0 100%);
  box-shadow: 0 4px 0 rgba(62, 188, 140, 0.1);
}
.finder-heading {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 13px;
}
.finder-heading h2 {
  margin: 0;
  font-size: 16px;
  letter-spacing: -0.02em;
}
.finder-heading p {
  margin: 1px 0 0;
  color: var(--lad-muted);
  font-size: 10px;
}
.finder-mascot {
  width: 39px;
  height: 39px;
  display: grid;
  place-items: center;
  flex: 0 0 39px;
  border-radius: 14px;
  background: white;
  box-shadow: 0 3px 0 rgba(62, 188, 140, 0.14);
}
.finder-eyes {
  width: 31px;
  height: 24px;
  overflow: visible;
}
.finder-eye {
  transform-box: fill-box;
  transform-origin: center;
}
.finder-eye > ellipse {
  fill: #fff;
  stroke: #91a6a0;
  stroke-width: 1.2;
}
.finder-pupil {
  fill: #263a42;
  transform-box: fill-box;
  transform-origin: center;
  animation: finder-look 8.6s ease-in-out infinite;
}
.finder-glint {
  fill: white;
}
.finder-eye--left {
  animation: finder-wink 7.4s 1.1s ease-in-out infinite;
}
.finder-eye--right {
  animation: finder-blink 11.3s 3.2s ease-in-out infinite;
}
.scope-choices {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
}
.scope-choices button {
  min-width: 0;
  min-height: 86px;
  padding: 10px 6px 8px;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  color: var(--lad-text);
  border: 2px solid transparent;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.8);
  box-shadow: 0 3px 0 rgba(72, 105, 91, 0.1);
  cursor: pointer;
  font: inherit;
  transition:
    transform 150ms ease,
    border-color 150ms ease,
    background 150ms ease;
}
.scope-choices button:hover {
  transform: translateY(-1px);
}
.scope-choices button.active {
  color: #16745a;
  border-color: var(--lad-mint);
  background: #e2f7ed;
  box-shadow: 0 4px 0 #b7e5cf;
}
.scope-choices button strong {
  font-size: 11px;
  line-height: 1.2;
}
.scope-choices button span {
  color: var(--lad-muted);
  font-size: 8px;
  line-height: 1.2;
}
.scope-choices button b {
  min-width: 20px;
  height: 20px;
  padding: 0 5px;
  position: absolute;
  top: 6px;
  right: 6px;
  display: grid;
  place-items: center;
  color: #8a5908;
  border-radius: 10px;
  background: #ffe4a8;
  font-size: 10px;
}
.kind-filter {
  margin-top: 14px;
  padding-top: 12px;
  display: flex;
  align-items: center;
  gap: 10px;
  border-top: 1px dashed rgba(62, 111, 91, 0.18);
}
.kind-filter-label {
  flex: 0 0 auto;
  color: var(--lad-muted);
  font-size: 9px;
  font-weight: 850;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}
.kind-choices {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.kind-choices button {
  padding: 7px 10px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: #53645d;
  border: 1px solid rgba(73, 111, 96, 0.16);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.76);
  cursor: pointer;
  font: inherit;
  font-size: 9px;
  font-weight: 850;
}
.kind-choices button.active {
  color: #185f4d;
  border-color: #9ed9bf;
  background: white;
  box-shadow: 0 2px 0 #cceadb;
}
.empty-contributions {
  border: 1px dashed rgba(62, 188, 140, 0.35);
  background: #f5fbf7;
}
.empty-contributions-icon {
  font-size: 38px;
}
.guardian-context {
  border: 1px solid rgba(78, 143, 221, 0.2);
}
.contribution-item,
.review-card,
.basic-row {
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.promotion-row {
  border: 1px solid rgba(242, 175, 66, 0.25);
  cursor: pointer;
}
.promotion-detail-icon {
  width: 62px;
  height: 62px;
  display: grid;
  place-items: center;
  border-radius: 20px;
  background: #ffe7ac;
  font-size: 32px;
}
.promotion-detail-title {
  margin: 0;
  font-size: 22px;
  letter-spacing: -0.03em;
}
.promotion-reward {
  padding: 14px;
  border-radius: 16px;
  background: #fff4d6;
}
.promotion-reward span,
.promotion-reward strong,
.promotion-reward small {
  display: block;
}
.promotion-reward span,
.promotion-reward small {
  color: var(--lad-muted);
  font-size: 11px;
}
.promotion-reward strong {
  color: #985e08;
  font-size: 27px;
}
.invited-team {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 5px;
}
.invited-team > span {
  color: var(--lad-muted);
  font-size: 10px;
  font-weight: 800;
}
.team-dialog-icon {
  width: 58px;
  height: 58px;
  display: grid;
  place-items: center;
  border-radius: 18px;
  background: #eaf6ff;
  font-size: 30px;
}
.sibling-rule {
  padding: 10px 12px;
  display: flex;
  align-items: flex-start;
  gap: 8px;
  color: var(--lad-muted);
  border-radius: 12px;
  background: #f1f7fa;
  font-size: 11px;
  line-height: 1.4;
}
.rating-setting {
  border: 1px solid var(--lad-border);
}
.bonus-value {
  color: #a5680c;
  font-size: 18px;
}
.contribution-meta {
  color: var(--lad-muted);
  font-size: 11px;
}
.section-title {
  margin: 0;
  font-size: 18px;
  letter-spacing: -0.025em;
}
.review-actions {
  grid-template-columns: 0.8fr 1.2fr;
}
.empty-icon {
  font-size: 42px;
}
.form-columns {
  grid-template-columns: 1fr 1fr;
}
.assignment-select {
  max-width: 150px;
  flex: 0 0 150px;
  font-size: 10px;
}
.list-enter-active,
.list-leave-active {
  transition:
    opacity 200ms ease,
    transform 200ms ease;
}
.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
@keyframes finder-wink {
  0%,
  43%,
  47%,
  100% {
    transform: scaleY(1);
  }
  44.5%,
  46% {
    transform: scaleY(0.08);
  }
}
@keyframes finder-blink {
  0%,
  68%,
  71%,
  100% {
    transform: scaleY(1);
  }
  69%,
  70% {
    transform: scaleY(0.08);
  }
}
@keyframes finder-look {
  0%,
  18%,
  100% {
    transform: translateX(0);
  }
  28%,
  45% {
    transform: translateX(-2px);
  }
  58%,
  75% {
    transform: translateX(2px);
  }
}
@media (prefers-reduced-motion: reduce) {
  .finder-eye,
  .finder-pupil {
    animation: none;
  }
}
@media (max-width: 380px) {
  .form-columns {
    grid-template-columns: 1fr;
  }
  .scope-choices button {
    min-height: 80px;
    padding-inline: 3px;
  }
  .scope-choices button strong {
    font-size: 10px;
  }
  .kind-filter {
    align-items: flex-start;
    flex-direction: column;
    gap: 7px;
  }
}
</style>
