<template>
  <div class="page page-padding contributions-page">
    <v-alert v-if="store.permissions.canManageContent" class="guardian-context mb-5" color="info" density="compact" icon="mdi-shield-account-outline" variant="tonal">
      Bezugspersonenansicht: Du kannst Beiträge offen lassen, fest zuweisen oder eine bestehende Zuweisung ändern.
    </v-alert>

    <template v-if="store.viewerRole === 'child'">
      <section v-if="guardianTasksToRate.length" class="mb-6">
        <SectionHeader description="Deine Sterne zeigen den Bezugspersonen, wie sorgfältig die Aufgabe erledigt wurde." title="Du darfst bewerten">
          <template #action><v-chip color="secondary" size="small">{{ guardianTasksToRate.length }}</v-chip></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <v-card v-for="contribution in guardianTasksToRate" :key="contribution.id" class="review-card child-review-card pa-4" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--compact" color="blue-lighten-5" rounded="lg" size="50">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1"><strong>{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">Erledigt von {{ memberName(contribution.assigneeId) }}</p></div>
            </div>
            <p class="text-caption text-medium-emphasis mt-3">Wie sorgfältig wurde die Aufgabe erledigt?</p>
            <v-rating v-model="ratings[contribution.id]" active-color="warning" class="my-2" density="compact" hover />
            <div class="d-grid review-actions ga-2">
              <v-btn rounded="lg" variant="tonal" @click="store.returnContribution(contribution.id)">Noch einmal</v-btn>
              <v-btn class="raised-button" color="primary" rounded="lg" variant="flat" @click="store.approveContribution(contribution.id, ratings[contribution.id] ?? 4)">Sterne vergeben</v-btn>
            </div>
          </v-card>
        </div>
      </section>

      <ContributionFilterPanel
        v-model:kind="filter"
        v-model:scope="scopeFilter"
        v-model:status="statusFilter"
        class="mb-4"
        :open-count="openContributionCount"
      />

      <TransitionGroup class="d-flex flex-column ga-3" name="list" tag="div">
        <v-card v-for="contribution in filteredContributions" :key="contribution.id" class="contribution-item pa-4" :class="{ 'contribution-item--approved': contribution.status === 'approved' }" data-ladi-heading="Ladis Tipp" :data-ladi-tip="contributionTip(contribution)" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <v-avatar class="task-icon-avatar" color="blue-lighten-5" rounded="lg" size="58">{{ contribution.icon }}</v-avatar>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center flex-wrap ga-2 mb-1">
                <strong>{{ contribution.title }}</strong>
                <span class="contribution-label" :class="contribution.kind === 'basic' ? 'contribution-label--basic' : 'contribution-label--special'">
                  <v-icon :icon="contribution.kind === 'basic' ? 'mdi-home-heart' : 'mdi-creation'" size="13" />{{ contribution.kind === 'basic' ? 'Grundbeitrag' : 'Spezialaufgabe' }}
                </span>
                <span class="contribution-label" :class="contribution.assigneeId === store.activeChildId ? 'contribution-label--mine' : 'contribution-label--open'">
                  <v-icon :icon="contribution.assigneeId === store.activeChildId ? 'mdi-account-heart' : 'mdi-account-multiple-outline'" size="13" />{{ assigneeLabel(contribution) }}
                </span>
              </div>
              <p class="text-caption text-medium-emphasis">{{ contribution.description }}</p>
              <div v-if="contribution.status === 'approved'" class="approved-reward-summary mt-3">
                <span class="approved-reward-main"><ContributionMetaIcon kind="reward" /><span><small>Du hast verdient</small><strong>{{ earnedReward(contribution) }} Ladirchen</strong></span></span>
                <span v-if="contribution.kind === 'basic'" class="approved-energy"><ContributionMetaIcon kind="energy" /><span><small>Für euer Haus</small><strong>+{{ contribution.energy }} Energie</strong></span></span>
              </div>
              <div v-else class="mt-3 contribution-meta">
                <span class="contribution-meta-chip contribution-meta-chip--time"><ContributionMetaIcon kind="time" /><span><small>Wann?</small><strong>{{ contribution.dueLabel }}</strong></span></span>
                <span class="contribution-meta-chip contribution-meta-chip--reward"><ContributionMetaIcon kind="reward" /><span><small>Du verdienst</small><strong>{{ store.rewardForContribution(contribution.id) }} Ladirchen</strong></span></span>
                <span v-if="contribution.kind === 'basic'" class="contribution-meta-chip contribution-meta-chip--energy"><ContributionMetaIcon kind="energy" /><span><small>Für euer Haus</small><strong>+{{ contribution.energy }} Energie</strong></span></span>
              </div>
              <ActiveContributionBonus
                v-if="contribution.status !== 'approved' && promotionFor(contribution.id)"
                class="mt-2"
                :deadline="promotionFor(contribution.id)?.deadline ?? '00:00'"
                :multiplier="promotionFor(contribution.id)?.multiplier ?? 1"
              />
              <div v-if="contribution.invitedChildIds?.length" class="invited-team mt-3">
                <span>Gemeinsam mit</span>
                <v-chip v-for="name in invitedChildNames(contribution)" :key="name" color="info" size="x-small" variant="tonal">{{ name }}</v-chip>
              </div>
            </div>
          </div>
          <div class="d-flex align-center justify-end flex-wrap ga-2 mt-3">
            <v-btn v-if="!assignedMember(contribution) && contribution.status === 'available'" class="claim-button" color="primary" rounded="lg" variant="flat" @click="store.claimContribution(contribution.id)">
              <span class="claim-button-icon" aria-hidden="true"><v-icon icon="mdi-rocket-launch-outline" /></span>
              <span>Ich schaffe das!</span>
              <i aria-hidden="true">✦</i>
            </v-btn>
            <template v-else-if="contribution.assigneeId === store.activeChildId">
              <v-btn v-if="contribution.kind === 'extra' && contribution.status === 'available'" prepend-icon="mdi-account-multiple-plus-outline" rounded="lg" size="small" variant="tonal" @click="openTeamInvite(contribution)">Geschwister einladen</v-btn>
              <div v-if="contribution.status === 'approved'" class="approved-celebration" :aria-label="`${contribution.stars ?? 1} von 5 Sternen erhalten`">
                <span class="approved-badge">{{ approvalMessage(contribution.stars) }}</span>
                <span class="earned-stars" aria-hidden="true">
                  <v-icon
                    v-for="star in 5"
                    :key="star"
                    class="earned-star"
                    :class="{ active: star <= (contribution.stars ?? 1) }"
                    icon="mdi-star"
                    size="20"
                    :style="{ '--star-index': star }"
                  />
                </span>
              </div>
              <div v-else-if="contribution.status === 'pending'" class="pending-celebration" role="status">
                <span class="pending-celebration-icon" aria-hidden="true">✨</span>
                <span><strong>Fast geschafft!</strong><small>Jetzt fehlen nur noch deine Sterne.</small></span>
              </div>
              <v-btn v-else class="finish-button" color="info" rounded="lg" variant="flat" @click="store.submitContribution(contribution.id)">
                <span class="finish-check" aria-hidden="true"><v-icon icon="mdi-check-bold" size="22" /></span>
                <span>Ich bin fertig!</span>
              </v-btn>
            </template>
            <div v-else-if="assignedMember(contribution)" class="assigned-member">
              <AvatarFigure :appearance="assigneeAppearance(contribution)" :size="34" />
              <span><small>Wird gemacht von</small><strong>{{ memberName(contribution.assigneeId) }}</strong></span>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
      <v-card v-if="!filteredContributions.length" class="empty-contributions pa-5 text-center" elevation="0" rounded="xl">
        <AnimatedCompletionMark :size="84" />
        <strong>{{ statusFilter === 'open' ? 'Für heute alles erledigt!' : 'Noch nichts abgeschlossen' }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ statusFilter === 'open' ? 'Stark gemacht! Deine abgeschlossenen Aufgaben kannst du jederzeit noch einmal ansehen.' : 'Sobald du eine Aufgabe geschafft hast, findest du sie hier wieder.' }}</p>
        <v-btn class="mt-3" color="primary" rounded="lg" variant="tonal" @click="statusFilter = statusFilter === 'open' ? 'completed' : 'open'; filter = 'all'">
          {{ statusFilter === 'open' ? 'Abgeschlossene ansehen' : 'Offene Aufgaben ansehen' }}
        </v-btn>
      </v-card>
    </template>

    <template v-else>
      <section class="guardian-own-tasks mb-6">
        <SectionHeader description="Eigene Aufgaben werden nach dem Erledigen von den Kindern bewertet." title="Meine Aufgaben">
          <template #action><v-btn color="secondary" prepend-icon="mdi-plus" rounded="lg" size="small" variant="tonal" @click="openOwnTaskDialog">Eigene Aufgabe</v-btn></template>
        </SectionHeader>
        <div v-if="guardianOwnContributions.length" class="d-flex flex-column ga-2">
          <v-card v-for="contribution in guardianOwnContributions" :key="contribution.id" class="basic-row pa-3" elevation="0" rounded="lg">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1 min-w-0"><strong class="text-body-small">{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ contribution.status === 'pending' ? 'Wartet auf Sterne der Kinder' : contribution.status === 'approved' ? `${contribution.stars ?? 1} von 5 Sternen` : 'Bereit zum Erledigen' }}</p></div>
              <v-btn v-if="contribution.status === 'available'" class="finish-button guardian-finish-button" color="info" rounded="lg" size="small" variant="flat" @click="store.submitContribution(contribution.id)">Ich bin fertig!</v-btn>
              <v-chip v-else :color="contribution.status === 'approved' ? 'success' : 'warning'" size="small" variant="tonal">{{ contribution.status === 'approved' ? 'Bewertet' : 'Eingereicht' }}</v-chip>
            </div>
          </v-card>
        </div>
        <v-card v-else class="empty-review pa-4 text-center" elevation="0" rounded="xl"><p class="text-caption text-medium-emphasis">Lege deine erste eigene Aufgabe an.</p></v-card>
      </section>

      <section v-if="store.permissions.canManageContent && childTasksToReview.length" class="mb-6">
        <SectionHeader description="Erst nach der Bestätigung reagiert die Familienwelt." title="Wartet auf Prüfung">
          <template #action><v-chip color="warning" size="small">{{ childTasksToReview.length }}</v-chip></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <v-card v-for="contribution in childTasksToReview" :key="contribution.id" class="review-card pa-4" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--compact" color="blue-lighten-5" rounded="lg" size="50">{{ contribution.icon }}</v-avatar>
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

      <v-card v-else-if="store.permissions.canManageContent" class="empty-review pa-5 mb-6 text-center" color="green-lighten-5" elevation="0" rounded="xl">
        <div class="empty-icon mb-2">🌤️</div>
        <strong>Alles geprüft</strong>
        <p class="text-caption text-medium-emphasis mt-1">Aktuell wartet kein Beitrag auf eine Bestätigung.</p>
      </v-card>

      <template v-if="store.permissions.canManageContent">
        <SectionHeader description="Zeitlich begrenzte Extras für ausgewählte Beiträge." title="Bonusaktionen">
          <template #action><v-btn aria-label="Bonusaktion hinzufügen" color="warning" icon="mdi-lightning-bolt" size="small" variant="tonal" @click="promotionDialog = true" /></template>
        </SectionHeader>
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
              <v-btn :aria-label="`${promotion.title} löschen`" color="error" icon="mdi-delete-outline" size="small" variant="tonal" @click.stop="promotionToDelete = promotion" />
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

        <SectionHeader description="Bezugspersonen können Grund- und Zusatzbeiträge anlegen." title="Beiträge verwalten">
          <template #action><v-btn color="primary" icon="mdi-plus" size="small" variant="tonal" @click="addDialog = true" /></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-2">
          <v-card v-for="contribution in managedContributions" :key="contribution.id" class="basic-row pa-3" elevation="0" rounded="lg">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">{{ contribution.icon }}</v-avatar>
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
                @update:model-value="assignContribution(contribution.id, $event)"
              />
              <v-btn
                :aria-label="`${contribution.title} löschen`"
                color="error"
                icon="mdi-delete-outline"
                size="small"
                variant="tonal"
                @click="requestContributionDelete(contribution)"
              />
            </div>
          </v-card>
        </div>
      </template>
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

    <v-dialog :model-value="Boolean(contributionToDelete)" max-width="390" @update:model-value="contributionToDelete = undefined">
      <v-card v-if="contributionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal"><v-icon icon="mdi-delete-alert-outline" /></v-avatar>
        <v-card-title class="pa-0">Beitrag löschen?</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ contributionToDelete.title }}</v-card-subtitle>
        <v-alert class="mb-4" color="warning" density="compact" variant="tonal">{{ contributionToDelete.status === 'approved' ? 'Der Beitrag verschwindet aus der Liste. Bereits gutgeschriebene Ladirchen bleiben erhalten.' : 'Die Zuweisung wird aufgehoben. Für diesen Beitrag werden keine Ladirchen ausgezahlt.' }}</v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="contributionToDelete = undefined">Abbrechen</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmContributionDelete">Löschen</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(promotionToDelete)" max-width="390" @update:model-value="promotionToDelete = undefined">
      <v-card v-if="promotionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal"><v-icon icon="mdi-calendar-remove-outline" /></v-avatar>
        <v-card-title class="pa-0">Bonusaktion löschen?</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ promotionToDelete.title }}</v-card-subtitle>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="promotionToDelete = undefined">Abbrechen</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmPromotionDelete">Löschen</v-btn>
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
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import ActiveContributionBonus from '../components/ActiveContributionBonus.vue';
import AnimatedCompletionMark from '@/shared/components/AnimatedCompletionMark.vue';
import ContributionFilterPanel from '../components/ContributionFilterPanel.vue';
import ContributionMetaIcon from '../components/ContributionMetaIcon.vue';
import PromotionCountdown from '../components/PromotionCountdown.vue';
import SectionHeader from '@/shared/components/ui/SectionHeader.vue';
import { CONTRIBUTION_IDS } from '@/infrastructure/fixtures/family-world-fixtures';
import { createDefaultAvatarAppearance } from '@/domain/avatar';
import type { Contribution, ContributionId, ContributionKind, FamilyMemberId, NewContribution, NewPromotion, Promotion } from '@/domain/types';
import { isPromotionAvailable } from '@/domain/promotions';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const route = useRoute();
const filter = ref<'all' | ContributionKind>('all');
const scopeFilter = ref<'all' | 'mine' | 'open'>('mine');
const statusFilter = ref<'open' | 'completed'>('open');
const addDialog = ref(false);
const promotionDialog = ref(false);
const teamDialog = ref(false);
const teamContributionId = ref<ContributionId>();
const selectedSiblingIds = ref<FamilyMemberId[]>([]);
const selectedPromotion = ref<Promotion>();
const contributionToDelete = ref<Contribution>();
const promotionToDelete = ref<Promotion>();
const ratings = reactive<Partial<Record<ContributionId, number>>>({});
const newContribution = reactive<Omit<NewContribution, 'assigneeId'> & { assigneeId: FamilyMemberId | '' }>({
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
const assignmentOptions = computed(() => {
  if (!store.permissions.canManageContent) {
    return [{ title: `${store.signedInMember.name} (eigene Aufgabe)`, value: store.signedInMemberId }];
  }
  return [
    { title: 'Noch offen – Kinder wählen selbst', value: '' },
    ...store.members.map((member) => ({
      title: `${member.name}${member.role === 'guardian' ? ' (Bezugsperson)' : ''}`,
      value: member.id,
    })),
  ];
});
const newPromotion = reactive<NewPromotion>({ contributionId: CONTRIBUTION_IDS.dishwasher, multiplier: 2, deadline: '17:00', teamworkBonus: 10 });
const multiplierOptions = [
  { title: 'Doppelte Ladirchen', value: 2 },
  { title: 'Dreifache Ladirchen', value: 3 },
];

const filteredContributions = computed(() =>
  store.contributions.filter(
    (contribution) =>
      (filter.value === 'all' || contribution.kind === filter.value) &&
      (statusFilter.value === 'completed' ? contribution.status === 'approved' : contribution.status !== 'approved') &&
      (scopeFilter.value === 'all' ||
        (scopeFilter.value === 'mine' && contribution.assigneeId === store.activeChildId) ||
        (scopeFilter.value === 'open' && !contribution.assigneeId && contribution.status === 'available')),
  ),
);
const managedContributions = computed(() => [...store.contributions].sort((left, right) =>
  Number(Boolean(left.assigneeId)) - Number(Boolean(right.assigneeId)),
));
const guardianOwnContributions = computed(() => store.contributions.filter(
  (contribution) => contribution.assigneeId === store.signedInMemberId,
));
const guardianTasksToRate = computed(() => store.contributions.filter((contribution) => {
  const assignee = store.members.find((member) => member.id === contribution.assigneeId);
  return contribution.status === 'pending' && assignee?.role === 'guardian';
}));
const childTasksToReview = computed(() => store.pendingContributions.filter((contribution) =>
  store.members.some((member) => member.id === contribution.assigneeId && member.role === 'child'),
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
const promotionFor = (contributionId: ContributionId) =>
  store.promotions.find((promotion) => promotion.contributionId === contributionId &&
    isPromotionAvailable(promotion, store.familyTimeZone, new Date(store.currentTimeMilliseconds)));
const contributionTitle = (contributionId: ContributionId) =>
  store.contributions.find((contribution) => contribution.id === contributionId)?.title ?? 'Beitrag';
const assignedMember = (contribution: Contribution) => store.members.find((member) => member.id === contribution.assigneeId);
const memberName = (memberId?: FamilyMemberId) => store.members.find((member) => member.id === memberId)?.name ?? 'Noch offen';
const assigneeAppearance = (contribution: Contribution) =>
  assignedMember(contribution)?.appearance ?? createDefaultAvatarAppearance();
const assigneeLabel = (contribution: Contribution) => {
  if (!assignedMember(contribution)) return 'Noch frei';
  if (contribution.assigneeId === store.activeChildId) return 'Für dich';
  return memberName(contribution.assigneeId);
};
const approvalMessage = (stars?: number) => {
  if ((stars ?? 1) >= 4) return 'Super gemacht!';
  if (stars === 3) return 'Gut gemacht!';
  if (stars === 2) return 'Mit mehr Sorgfalt!';
  return 'Nächstes Mal klappt’s!';
};
const earnedReward = (contribution: Contribution) => contribution.earnedReward ?? contribution.reward;
const assignContribution = (contributionId: ContributionId, value: unknown) => {
  const member = store.members.find(item => item.id === value);
  store.assignContribution(contributionId, member?.id);
};
const openOwnTaskDialog = () => {
  Object.assign(newContribution, { assigneeId: store.signedInMemberId });
  addDialog.value = true;
};
const requestContributionDelete = (contribution: Contribution) => {
  contributionToDelete.value = contribution;
};
const confirmContributionDelete = () => {
  if (!contributionToDelete.value) return;
  store.deleteContribution(contributionToDelete.value.id);
  contributionToDelete.value = undefined;
};
const confirmPromotionDelete = () => {
  if (!promotionToDelete.value) return;
  store.deletePromotion(promotionToDelete.value.id);
  promotionToDelete.value = undefined;
};
const invitedChildNames = (contribution: Contribution) => store.members
  .filter((member) => member.role === 'child' && contribution.invitedChildIds?.includes(member.id))
  .map((member) => `${member.avatar} ${member.name}`);
const openTeamInvite = (contribution: Contribution) => {
  teamContributionId.value = contribution.id;
  selectedSiblingIds.value = [...(contribution.invitedChildIds ?? [])];
  teamDialog.value = true;
};
const contributionTip = (contribution: Contribution) => {
  const title = contribution.title.toLowerCase();
  if (title.includes('lüften')) return 'Öffne das Fenster ganz weit und stell dir zehn Minuten auf einer Uhr. Danach wieder gut schließen.';
  if (title.includes('tisch')) return 'Zähle zuerst, wie viele Personen mitessen. Dann kommen Teller, Besteck und zuletzt die Gläser auf den Tisch.';
  if (title.includes('geschirr')) return 'Reste zuerst abstreifen, dann große Teile nach unten und Tassen nach oben. Bei scharfen Messern bitte einen Erwachsenen fragen.';
  if (title.includes('katze') || title.includes('anna') || title.includes('elsa')) return 'Fülle zuerst frisches Wasser ein und gib dann die vereinbarte Futtermenge. Danach Hände waschen.';
  if (title.includes('pflanz')) return 'Prüfe die Erde mit einem Finger. Nur gießen, wenn sie sich trocken anfühlt.';
  if (title.includes('wäsche')) return 'Sortiere helle und dunkle Sachen getrennt und prüfe vorher alle Taschen.';
  if (title.includes('müll')) return 'Binde den Beutel gut zu und wasche dir danach gründlich die Hände.';
  return `${contribution.description} Fang mit dem einfachsten Schritt an und hake dann einen Schritt nach dem anderen ab.`;
};
onMounted(() => {
  if (store.viewerRole !== 'child') return;
  window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: 'Beiträge',
    message: 'Grundbeiträge versorgen eure Familienwelt. Freiwillige Zusatzbeiträge bringen dir zusätzliche Ladirchen.',
    pageIntro: true,
  } })), 350);
});
const saveTeamInvite = () => {
  if (!teamContributionId.value) {return;}
  store.setContributionPartners(teamContributionId.value, selectedSiblingIds.value);
  teamDialog.value = false;
};

const addContribution = () => {
  store.addContribution({ ...newContribution, assigneeId: newContribution.assigneeId || undefined });
  addDialog.value = false;
  Object.assign(newContribution, { title: '', description: '', icon: '✨', reward: 10, energy: 10, kind: 'basic', assigneeId: '' });
};
const addPromotion = () => {
  store.addPromotion({ ...newPromotion });
  promotionDialog.value = false;
  Object.assign(newPromotion, { contributionId: CONTRIBUTION_IDS.dishwasher, multiplier: 2, deadline: '17:00', teamworkBonus: 10 });
};
watch(
  () => route.query.new,
  (value) => { if (value === '1' && store.permissions.canManageContent) addDialog.value = true; },
  { immediate: true },
);
watch(
  () => route.query.promotion,
  (value) => { if (value === '1' && store.permissions.canManageContent) promotionDialog.value = true; },
  { immediate: true },
);
</script>

<style scoped>
.empty-contributions {
  @apply d-flex flex-column align-center;
  border: 2px dashed rgba(62, 188, 140, 0.32);
  background:
    radial-gradient(
      circle at 80% 15%,
      rgba(255, 218, 112, 0.22),
      transparent 27%
    ),
    linear-gradient(145deg, #f1fbf6, #fff9e8);
  box-shadow: inset 0 0 0 5px rgba(255, 255, 255, 0.42) !important;
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
.contribution-item {
  @apply position-relative overflow-hidden;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.2s ease;
}
.task-icon-avatar {
  flex: 0 0 58px;
  border: 3px solid rgba(255, 255, 255, 0.9);
  background: linear-gradient(145deg, #e9f7ff, #fff4c9) !important;
  box-shadow:
    0 4px 0 rgba(65, 132, 174, 0.12),
    0 8px 15px rgba(56, 112, 92, 0.07);
  font-size: 31px !important;
  transform: rotate(-3deg);
  animation: task-icon-float 3.4s ease-in-out infinite;
}
.task-icon-avatar--compact {
  flex-basis: 50px;
  font-size: 27px !important;
}
.task-icon-avatar--small {
  flex-basis: 44px;
  font-size: 24px !important;
}
.contribution-item--approved {
  border-color: rgba(57, 177, 125, 0.3);
  background:
    radial-gradient(
      circle at 94% 10%,
      rgba(255, 220, 101, 0.24),
      transparent 24%
    ),
    linear-gradient(145deg, #fffef9, #f0fbf5);
  box-shadow:
    0 5px 0 rgba(50, 145, 105, 0.16),
    0 13px 24px rgba(46, 118, 90, 0.08) !important;
}
.contribution-item--approved::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 13px;
  right: 15px;
  color: rgba(228, 169, 40, 0.55);
  font-size: 13px;
  animation: approved-sparkle 2.2s ease-in-out infinite;
}
.approved-reward-summary {
  @apply d-grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr);
  gap: 8px;
}
.approved-reward-main,
.approved-energy {
  min-width: 0;
  min-height: 67px;
  padding: 8px 10px 8px 7px;
  @apply d-flex align-center;
  gap: 7px;
  border: 2px solid rgba(227, 164, 43, 0.22);
  border-radius: 17px;
  background:
    radial-gradient(
      circle at 90% 8%,
      rgba(255, 255, 255, 0.9),
      transparent 27%
    ),
    linear-gradient(145deg, #fff8dc, #ffe9a5);
  box-shadow:
    0 4px 0 rgba(191, 126, 24, 0.15),
    0 9px 17px rgba(139, 104, 40, 0.08);
}
.approved-energy {
  border-color: rgba(63, 167, 120, 0.2);
  background:
    radial-gradient(
      circle at 90% 8%,
      rgba(255, 255, 255, 0.9),
      transparent 27%
    ),
    linear-gradient(145deg, #e8faf2, #fff4bd);
  box-shadow:
    0 4px 0 rgba(45, 133, 94, 0.13),
    0 9px 17px rgba(58, 116, 88, 0.07);
}
.approved-reward-main > span,
.approved-energy > span,
.approved-reward-main small,
.approved-energy small,
.approved-reward-main strong,
.approved-energy strong {
  @apply d-block min-w-0;
}
.approved-reward-main small,
.approved-energy small {
  color: #856222;
  font-size: 8px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}
.approved-energy small {
  color: #34735c;
}
.approved-reward-main strong,
.approved-energy strong {
  margin-top: 2px;
  color: #8a590d;
  font-size: 15px;
  font-weight: 950;
  line-height: 1.1;
  white-space: nowrap;
}
.approved-energy strong {
  color: #267456;
}
.approved-reward-summary :deep(.contribution-meta-icon) {
  flex: 0 0 38px;
  width: 38px;
  height: 38px;
  animation: approved-reward-icon 2.2s ease-in-out infinite;
}
.contribution-label {
  min-height: 26px;
  padding: 4px 9px;
  @apply d-inline-flex align-center;
  gap: 4px;
  border: 1px solid;
  border-radius: 999px;
  font-size: 9px;
  font-weight: 900;
  line-height: 1;
  box-shadow: 0 3px 0 rgba(68, 125, 104, 0.09);
}
.contribution-label--basic {
  color: #216f56;
  border-color: rgba(47, 163, 119, 0.2);
  background: linear-gradient(145deg, #e7f9f0, #fff);
}
.contribution-label--special {
  color: #396f9d;
  border-color: rgba(74, 143, 207, 0.2);
  background: linear-gradient(145deg, #e9f5ff, #fff);
}
.contribution-label--mine {
  color: #39751e;
  border-color: rgba(97, 170, 61, 0.2);
  background: linear-gradient(145deg, #edf9df, #fff);
  animation: mine-label-breathe 2.6s ease-in-out infinite;
}
.contribution-label--open {
  color: #8b651e;
  border-color: rgba(224, 170, 64, 0.22);
  background: linear-gradient(145deg, #fff6d8, #fff);
}
.assigned-member {
  min-height: 45px;
  padding: 5px 10px 5px 6px;
  @apply d-flex align-center;
  gap: 7px;
  color: #325f51;
  border: 2px solid rgba(61, 151, 116, 0.18);
  border-radius: 16px;
  background: linear-gradient(145deg, #f0faf5, #fff8de);
  box-shadow: 0 4px 0 rgba(54, 130, 101, 0.12);
}
.assigned-member :deep(.avatar-figure) {
  border: 2px solid #fff;
  border-radius: 12px;
  background: #eaf7f1;
  box-shadow: 0 2px 0 rgba(51, 123, 95, 0.1);
}
.assigned-member span,
.assigned-member small,
.assigned-member strong {
  @apply d-block;
}
.assigned-member small {
  color: var(--lad-muted);
  font-size: 8px;
  font-weight: 750;
}
.assigned-member strong {
  margin-top: 1px;
  font-size: 11px;
}
.approved-celebration {
  @apply d-flex align-center flex-wrap justify-end;
  gap: 9px;
}
.approved-badge {
  min-height: 39px;
  padding: 7px 13px;
  @apply d-inline-flex align-center;
  gap: 6px;
  color: #237257;
  border: 2px solid rgba(59, 170, 127, 0.25);
  border-radius: 999px;
  background: linear-gradient(145deg, #edfbf5, #d5f3e4);
  box-shadow:
    0 4px 0 rgba(48, 142, 105, 0.17),
    0 8px 15px rgba(57, 139, 107, 0.09);
  font-size: 12px;
  font-weight: 950;
}
.earned-stars {
  padding: 6px 9px;
  @apply d-inline-flex align-center;
  gap: 1px;
  border: 2px solid rgba(235, 174, 53, 0.22);
  border-radius: 999px;
  background: linear-gradient(145deg, #fffbed, #fff0bc);
  box-shadow: 0 4px 0 rgba(196, 132, 29, 0.13);
}
.earned-star {
  color: #dfe4e1;
  opacity: 0.52;
  transform: scale(0.8);
}
.earned-star.active {
  color: #f1b527;
  opacity: 1;
  filter: drop-shadow(0 2px 1px rgba(177, 114, 11, 0.2));
  animation: earned-star-pop 1.9s ease-in-out infinite;
  animation-delay: calc(var(--star-index) * -120ms);
}
.claim-button {
  min-height: 48px !important;
  padding: 5px 15px 5px 7px !important;
  overflow: visible !important;
  border: 2px solid rgba(255, 255, 255, 0.88) !important;
  border-radius: 17px !important;
  background: linear-gradient(145deg, #62c99d, #2c9b73) !important;
  box-shadow:
    0 5px 0 #227858,
    0 10px 17px rgba(40, 137, 99, 0.18) !important;
  font-weight: 950;
  text-transform: none;
  letter-spacing: 0;
  animation: claim-button-ready 2.6s ease-in-out infinite;
}
.claim-button :deep(.v-btn__content) {
  gap: 7px;
}
.claim-button-icon {
  width: 35px;
  height: 35px;
  @apply d-grid place-center;
  border: 2px solid rgba(255, 255, 255, 0.78);
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    rgba(255, 255, 255, 0.3),
    rgba(255, 235, 151, 0.22)
  );
  animation: claim-launch 2.2s ease-in-out infinite;
}
.claim-button-icon :deep(.v-icon) {
  color: #fff7c7;
  font-size: 22px;
  filter: drop-shadow(0 2px 0 rgba(26, 113, 81, 0.25));
}
.claim-button i {
  color: #fff0a1;
  font-style: normal;
  animation: approved-sparkle 1.4s ease-in-out infinite;
}
.claim-button:active {
  transform: translateY(3px) scale(0.97);
  box-shadow: 0 2px 0 #227858 !important;
}
.finish-button {
  min-height: 48px !important;
  padding: 4px 16px 4px 7px !important;
  overflow: visible !important;
  border: 2px solid rgba(255, 255, 255, 0.88) !important;
  border-radius: 17px !important;
  background: linear-gradient(145deg, #67b7f0, #397fd5) !important;
  box-shadow:
    0 5px 0 #2d6eb5,
    0 10px 18px rgba(47, 119, 194, 0.2) !important;
  font-weight: 900;
  text-transform: none;
  letter-spacing: 0;
}
.finish-button :deep(.v-btn__content) {
  gap: 7px;
}
.finish-button:active {
  transform: translateY(3px) scale(0.97);
  box-shadow: 0 2px 0 #2d6eb5 !important;
}
.finish-check {
  width: 38px;
  height: 38px;
  @apply d-grid place-center;
  color: #2f78c5;
  border: 2px solid rgba(255, 255, 255, 0.78);
  border-radius: 13px;
  background: #fff;
  box-shadow: inset 0 -3px 0 rgba(64, 125, 190, 0.12);
}
.pending-celebration {
  min-height: 49px;
  padding: 7px 12px 7px 7px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 8px;
  color: #785610;
  border: 2px solid rgba(232, 170, 53, 0.27);
  border-radius: 17px;
  background: linear-gradient(145deg, #fff7cf, #eef9e8);
  box-shadow:
    0 4px 0 rgba(192, 132, 32, 0.13),
    0 8px 15px rgba(126, 99, 49, 0.08);
}
.pending-celebration::after {
  content: "✦";
  @apply position-absolute;
  top: 4px;
  right: 7px;
  color: #dc9820;
  font-size: 9px;
  animation: pending-spark 2s ease-in-out infinite;
}
.pending-celebration-icon {
  width: 35px;
  height: 35px;
  @apply d-grid place-center flex-shrink-0;
  border: 2px solid #fff;
  border-radius: 12px;
  background: #ffe194;
  box-shadow: 0 3px 0 rgba(177, 117, 23, 0.13);
  font-size: 18px;
  animation: pending-ready 2.5s ease-in-out infinite;
}
.pending-celebration strong,
.pending-celebration small {
  @apply d-block;
}
.pending-celebration strong {
  font-size: 12px;
}
.pending-celebration small {
  margin-top: 1px;
  color: #7d725b;
  font-size: 8px;
}
.promotion-row {
  border: 1px solid rgba(242, 175, 66, 0.25);
  @apply cursor-pointer;
}
.promotion-detail-icon {
  width: 62px;
  height: 62px;
  @apply d-grid place-center;
  border-radius: 20px;
  background: #ffe7ac;
  font-size: 32px;
}
.promotion-detail-title {
  @apply ma-0;
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
  @apply d-block;
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
  @apply d-flex align-center flex-wrap;
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
  @apply d-grid place-center;
  border-radius: 18px;
  background: #eaf6ff;
  font-size: 30px;
}
.sibling-rule {
  padding: 10px 12px;
  @apply d-flex align-start ga-2;
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
  @apply d-grid;
  grid-template-columns: 1.2fr 1fr 1fr;
  gap: 6px;
}
.contribution-meta-chip {
  min-width: 0;
  min-height: 54px;
  padding: 4px 5px 4px 4px;
  @apply d-flex align-center;
  gap: 4px;
  border: 2px solid rgba(76, 145, 193, 0.15);
  border-radius: 14px;
  background: linear-gradient(145deg, #f1f9ff, #fff);
  box-shadow: 0 3px 0 rgba(65, 129, 173, 0.09);
}
.contribution-meta-chip > span,
.contribution-meta-chip small,
.contribution-meta-chip strong {
  @apply d-block min-w-0;
}
.contribution-meta-chip small {
  color: var(--lad-muted);
  font-size: 7px;
  font-weight: 800;
  line-height: 1.1;
}
.contribution-meta-chip strong {
  margin-top: 2px;
  color: #31566c;
  font-size: 8px;
  font-weight: 950;
  line-height: 1.12;
  white-space: nowrap;
}
.contribution-meta-chip :deep(.contribution-meta-icon) {
  flex: 0 0 30px;
  width: 30px;
  height: 30px;
}
.contribution-meta-chip--reward {
  border-color: rgba(228, 168, 47, 0.18);
  background: linear-gradient(145deg, #fffaf0, #fff2c7);
  box-shadow: 0 3px 0 rgba(193, 132, 28, 0.1);
}
.contribution-meta-chip--reward strong {
  color: #8a5a10;
}
.contribution-meta-chip--energy {
  border-color: rgba(73, 172, 124, 0.17);
  background: linear-gradient(145deg, #f0fbf5, #fff6ce);
  box-shadow: 0 3px 0 rgba(55, 138, 101, 0.1);
}
.contribution-meta-chip--energy strong {
  color: #277258;
}
.section-title {
  @apply ma-0;
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
@keyframes earned-star-pop {
  0%,
  100% {
    transform: scale(0.86) rotate(-5deg);
  }
  50% {
    transform: scale(1.14) rotate(6deg);
    filter: drop-shadow(0 0 6px rgba(242, 185, 50, 0.72));
  }
}
@keyframes approved-sparkle {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.75) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(25deg);
  }
}
@keyframes approved-reward-icon {
  0%,
  100% {
    transform: rotate(-3deg) scale(1);
  }
  50% {
    transform: rotate(4deg) scale(1.08);
  }
}
@keyframes mine-label-breathe {
  0%,
  100% {
    box-shadow: 0 3px 0 rgba(68, 125, 104, 0.09);
  }
  50% {
    box-shadow:
      0 3px 0 rgba(68, 125, 104, 0.09),
      0 0 0 4px rgba(104, 186, 75, 0.08);
  }
}
@keyframes claim-button-ready {
  0%,
  70%,
  100% {
    transform: translateY(0) rotate(0);
  }
  79% {
    transform: translateY(-3px) rotate(-1deg);
  }
  87% {
    transform: translateY(1px) rotate(1deg);
  }
}
@keyframes claim-launch {
  0%,
  62%,
  100% {
    transform: translate(0, 0) rotate(-5deg);
  }
  72% {
    transform: translate(2px, -4px) rotate(7deg) scale(1.08);
  }
  80% {
    transform: translate(-1px, 1px) rotate(-8deg);
  }
  88% {
    transform: translate(1px, -2px) rotate(3deg);
  }
}
@keyframes task-icon-float {
  0%,
  68%,
  100% {
    transform: translateY(0) rotate(-3deg);
  }
  78% {
    transform: translateY(-3px) rotate(3deg) scale(1.04);
  }
  88% {
    transform: translateY(1px) rotate(-1deg);
  }
}
@keyframes pending-ready {
  0%,
  75%,
  100% {
    transform: rotate(0) scale(1);
  }
  84% {
    transform: rotate(-8deg) scale(1.1);
  }
  92% {
    transform: rotate(6deg);
  }
}
@keyframes pending-spark {
  0%,
  100% {
    opacity: 0.3;
    transform: scale(0.7);
  }
  50% {
    opacity: 1;
    transform: scale(1.15) rotate(20deg);
  }
}
@media (prefers-reduced-motion: reduce) {
  .contribution-item--approved::after,
  .contribution-label--mine,
  .earned-star.active,
  .claim-button,
  .claim-button-icon,
  .pending-celebration::after,
  .pending-celebration-icon,
  .task-icon-avatar,
  .approved-reward-summary :deep(.contribution-meta-icon) {
    animation: none;
  }
}
@media (max-width: 380px) {
  .form-columns {
    grid-template-columns: 1fr;
  }
}
@media (max-width: 430px) {
  .contribution-meta {
    grid-template-columns: 1fr 1fr;
  }
  .contribution-meta-chip--energy {
    grid-column: 1 / -1;
  }
}
@media (max-width: 430px) {
  .approved-reward-summary {
    grid-template-columns: 1fr;
  }
}
</style>
