<template>
  <div class="page page-padding contributions-page">
    <v-alert v-if="store.permissions.canManageContent" class="guardian-context mb-5" color="info" density="compact" icon="mdi-shield-account-outline" variant="tonal">
      {{ t('contributions.guardianContext') }}
    </v-alert>

    <template v-if="store.viewerRole === 'child'">
      <section v-if="guardianTasksToRate.length" class="mb-6">
        <SectionHeader :description="t('contributions.childReview.description')" :title="t('contributions.childReview.title')">
          <template #action><v-chip color="secondary" size="small">{{ guardianTasksToRate.length }}</v-chip></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <v-card v-for="contribution in guardianTasksToRate" :key="contribution.id" class="review-card child-review-card pa-4" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--compact" color="blue-lighten-5" rounded="lg" size="50">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1"><strong>{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ t('contributions.completedBy', { name: memberName(contribution.assigneeId) }) }}</p></div>
            </div>
            <p class="text-caption text-medium-emphasis mt-3">{{ t('contributions.childReview.question') }}</p>
            <v-rating v-model="ratings[contribution.id]" active-color="warning" class="my-2" density="compact" hover />
            <div class="d-grid review-actions ga-2">
              <v-btn rounded="lg" variant="tonal" @click="store.returnContribution(contribution.id)">{{ t('contributions.childReview.again') }}</v-btn>
              <v-btn class="raised-button" color="primary" rounded="lg" variant="flat" @click="store.approveContribution(contribution.id, ratings[contribution.id] ?? 4)">{{ t('contributions.childReview.rate') }}</v-btn>
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
        <v-card v-for="contribution in filteredContributions" :key="contribution.id" class="contribution-item pa-4" :class="{ 'contribution-item--approved': contribution.status === 'approved' }" :data-ladi-heading="t('contributions.ladiTip')" :data-ladi-tip="contributionTip(contribution)" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <v-avatar class="task-icon-avatar" color="blue-lighten-5" rounded="lg" size="58">{{ contribution.icon }}</v-avatar>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center flex-wrap ga-2 mb-1">
                <strong>{{ contribution.title }}</strong>
                <span class="contribution-label" :class="contribution.kind === 'basic' ? 'contribution-label--basic' : 'contribution-label--special'">
                  <v-icon :icon="contribution.kind === 'basic' ? 'mdi-home-heart' : 'mdi-creation'" size="13" />{{ contribution.kind === 'basic' ? t('contributions.kind.basic') : t('contributions.kind.extra') }}
                </span>
                <span class="contribution-label" :class="contribution.assigneeId === store.activeChildId ? 'contribution-label--mine' : 'contribution-label--open'">
                  <v-icon :icon="contribution.assigneeId === store.activeChildId ? 'mdi-account-heart' : 'mdi-account-multiple-outline'" size="13" />{{ assigneeLabel(contribution) }}
                </span>
              </div>
              <p class="text-caption text-medium-emphasis">{{ contribution.description }}</p>
              <div v-if="contribution.status === 'approved'" class="approved-reward-summary mt-3">
                <span class="approved-reward-main"><ContributionMetaIcon kind="reward" /><span><small>{{ t('contributions.reward.earned') }}</small><strong>{{ t('contributions.reward.coins', { value: earnedReward(contribution) }) }}</strong></span></span>
                <span v-if="contribution.kind === 'basic'" class="approved-energy"><ContributionMetaIcon kind="energy" /><span><small>{{ t('contributions.reward.house') }}</small><strong>{{ t('contributions.reward.energy', { value: contribution.energy }) }}</strong></span></span>
              </div>
              <div v-else class="mt-3 contribution-meta">
                <span class="contribution-meta-chip contribution-meta-chip--time"><ContributionMetaIcon kind="time" /><span><small>{{ t('contributions.reward.when') }}</small><strong>{{ contribution.dueLabel }}</strong></span></span>
                <span class="contribution-meta-chip contribution-meta-chip--reward"><ContributionMetaIcon kind="reward" /><span><small>{{ t('contributions.reward.youEarn') }}</small><strong>{{ t('contributions.reward.coins', { value: store.rewardForContribution(contribution.id) }) }}</strong></span></span>
                <span v-if="contribution.kind === 'basic'" class="contribution-meta-chip contribution-meta-chip--energy"><ContributionMetaIcon kind="energy" /><span><small>{{ t('contributions.reward.house') }}</small><strong>{{ t('contributions.reward.energy', { value: contribution.energy }) }}</strong></span></span>
              </div>
              <ActiveContributionBonus
                v-if="contribution.status !== 'approved' && promotionFor(contribution.id)"
                class="mt-2"
                :deadline="promotionFor(contribution.id)?.deadline ?? '00:00'"
                :multiplier="promotionFor(contribution.id)?.multiplier ?? 1"
              />
              <div v-if="contribution.invitedChildIds?.length" class="invited-team mt-3">
                <span>{{ t('contributions.togetherWith') }}</span>
                <v-chip v-for="name in invitedChildNames(contribution)" :key="name" color="info" size="x-small" variant="tonal">{{ name }}</v-chip>
              </div>
            </div>
          </div>
          <div class="d-flex align-center justify-end flex-wrap ga-2 mt-3">
            <v-btn v-if="!assignedMember(contribution) && contribution.status === 'available'" class="claim-button" color="primary" rounded="lg" variant="flat" @click="store.claimContribution(contribution.id)">
              <span class="claim-button-icon" aria-hidden="true"><v-icon icon="mdi-rocket-launch-outline" /></span>
              <span>{{ t('contributions.claim') }}</span>
              <i aria-hidden="true">✦</i>
            </v-btn>
            <template v-else-if="contribution.assigneeId === store.activeChildId">
              <v-btn v-if="contribution.kind === 'extra' && contribution.status === 'available'" prepend-icon="mdi-account-multiple-plus-outline" rounded="lg" size="small" variant="tonal" @click="openTeamInvite(contribution)">{{ t('contributions.inviteSiblings') }}</v-btn>
              <div v-if="contribution.status === 'approved'" class="approved-celebration" :aria-label="t('contributions.starsReceived', { value: contribution.stars ?? 1 })">
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
                <span><strong>{{ t('contributions.pending.title') }}</strong><small>{{ t('contributions.pending.description') }}</small></span>
              </div>
              <v-btn v-else class="finish-button" color="info" rounded="lg" variant="flat" @click="store.submitContribution(contribution.id)">
                <span class="finish-check" aria-hidden="true"><v-icon icon="mdi-check-bold" size="22" /></span>
                <span>{{ t('contributions.finish') }}</span>
              </v-btn>
            </template>
            <div v-else-if="assignedMember(contribution)" class="assigned-member">
              <AvatarFigure :appearance="assigneeAppearance(contribution)" :size="34" />
              <span><small>{{ t('contributions.assignedTo') }}</small><strong>{{ memberName(contribution.assigneeId) }}</strong></span>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
      <v-card v-if="!filteredContributions.length" class="empty-contributions pa-5 text-center" elevation="0" rounded="xl">
        <AnimatedCompletionMark :size="84" />
        <strong>{{ statusFilter === 'open' ? t('contributions.empty.openTitle') : t('contributions.empty.completedTitle') }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ statusFilter === 'open' ? t('contributions.empty.openDescription') : t('contributions.empty.completedDescription') }}</p>
        <v-btn class="mt-3" color="primary" rounded="lg" variant="tonal" @click="statusFilter = statusFilter === 'open' ? 'completed' : 'open'; filter = 'all'">
          {{ statusFilter === 'open' ? t('contributions.empty.viewCompleted') : t('contributions.empty.viewOpen') }}
        </v-btn>
      </v-card>
    </template>

    <template v-else>
      <section class="guardian-own-tasks mb-6">
        <SectionHeader :description="t('contributions.guardianOwn.description')" :title="t('contributions.guardianOwn.title')">
          <template #action><v-btn color="secondary" prepend-icon="mdi-plus" rounded="lg" size="small" variant="tonal" @click="openOwnTaskDialog">{{ t('contributions.guardianOwn.add') }}</v-btn></template>
        </SectionHeader>
        <div v-if="guardianOwnContributions.length" class="d-flex flex-column ga-2">
          <v-card v-for="contribution in guardianOwnContributions" :key="contribution.id" class="basic-row pa-3" elevation="0" rounded="lg">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1 min-w-0"><strong class="text-body-small">{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ guardianContributionStatus(contribution) }}</p></div>
              <v-btn v-if="contribution.status === 'available'" class="finish-button guardian-finish-button" color="info" rounded="lg" size="small" variant="flat" @click="store.submitContribution(contribution.id)">{{ t('contributions.finish') }}</v-btn>
              <v-chip v-else :color="contribution.status === 'approved' ? 'success' : 'warning'" size="small" variant="tonal">{{ contribution.status === 'approved' ? t('contributions.guardianOwn.rated') : t('contributions.guardianOwn.submitted') }}</v-chip>
            </div>
          </v-card>
        </div>
        <v-card v-else class="empty-review pa-4 text-center" elevation="0" rounded="xl"><p class="text-caption text-medium-emphasis">{{ t('contributions.guardianOwn.empty') }}</p></v-card>
      </section>

      <section v-if="store.permissions.canManageContent && childTasksToReview.length" class="mb-6">
        <SectionHeader :description="t('contributions.review.description')" :title="t('contributions.review.title')">
          <template #action><v-chip color="warning" size="small">{{ childTasksToReview.length }}</v-chip></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <v-card v-for="contribution in childTasksToReview" :key="contribution.id" class="review-card pa-4" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--compact" color="blue-lighten-5" rounded="lg" size="50">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1"><strong>{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ memberName(contribution.assigneeId) }} · +{{ t('contributions.reward.coins', { value: store.rewardForContribution(contribution.id) }) }}</p></div>
            </div>
            <div class="d-flex align-center justify-space-between ga-2 mt-3">
              <p class="text-caption text-medium-emphasis">{{ t('contributions.review.question') }}</p>
              <v-chip color="warning" size="x-small" variant="tonal">5 ★ = +{{ store.perfectRatingBonusPercent }} %</v-chip>
            </div>
            <v-rating v-model="ratings[contribution.id]" active-color="warning" class="my-2" density="compact" hover />
            <div class="d-grid review-actions ga-2">
              <v-btn rounded="lg" variant="tonal" @click="store.returnContribution(contribution.id)">{{ t('contributions.review.return') }}</v-btn>
              <v-btn class="raised-button" color="primary" rounded="lg" variant="flat" @click="store.approveContribution(contribution.id, ratings[contribution.id] ?? 4)">{{ t('contributions.review.confirm') }}</v-btn>
            </div>
          </v-card>
        </div>
      </section>

      <v-card v-else-if="store.permissions.canManageContent" class="empty-review pa-5 mb-6 text-center" color="green-lighten-5" elevation="0" rounded="xl">
        <div class="empty-icon mb-2">🌤️</div>
        <strong>{{ t('contributions.review.emptyTitle') }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ t('contributions.review.emptyDescription') }}</p>
      </v-card>

      <template v-if="store.permissions.canManageContent">
        <SectionHeader :description="t('contributions.promotions.description')" :title="t('contributions.promotions.title')">
          <template #action><v-btn :aria-label="t('contributions.promotions.add')" color="warning" icon="mdi-lightning-bolt" size="small" variant="tonal" @click="promotionDialog = true" /></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-2 mb-6">
          <v-card v-for="promotion in activePromotions" :key="promotion.id" class="promotion-row pa-3" color="amber-lighten-5" elevation="0" rounded="lg" role="button" tabindex="0" @click="selectedPromotion = promotion" @keydown.enter="selectedPromotion = promotion">
            <div class="d-flex align-center ga-3">
              <v-avatar color="warning" size="40" variant="tonal">⚡</v-avatar>
              <div class="flex-grow-1">
                <strong class="text-body-small">{{ promotion.title }}</strong>
                <p class="text-caption text-medium-emphasis">{{ contributionTitle(promotion.contributionId) }} · Team +{{ promotion.teamworkBonus }} L</p>
                <PromotionCountdown class="mt-1" :deadline="promotion.deadline" />
              </div>
              <v-chip color="warning" size="small">×{{ promotion.multiplier }}</v-chip>
              <v-btn :aria-label="t('contributions.promotions.deleteAria', { title: promotion.title })" color="error" icon="mdi-delete-outline" size="small" variant="tonal" @click.stop="promotionToDelete = promotion" />
            </div>
          </v-card>
        </div>

        <v-card class="rating-setting pa-4 mb-6" color="surface" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <v-avatar color="warning" variant="tonal">★</v-avatar>
            <div class="flex-grow-1">
              <div class="d-flex align-center justify-space-between ga-3">
                <div><strong>{{ t('contributions.ratingBonus.title') }}</strong><p class="text-caption text-medium-emphasis">{{ t('contributions.ratingBonus.description') }}</p></div>
                <strong class="bonus-value">{{ store.perfectRatingBonusPercent }} %</strong>
              </div>
              <v-slider v-model="store.perfectRatingBonusPercent" class="mt-3" color="warning" hide-details max="25" min="0" step="1" thumb-label />
            </div>
          </div>
        </v-card>

        <SectionHeader :description="t('contributions.manage.description')" :title="t('contributions.manage.title')">
          <template #action><v-btn color="primary" icon="mdi-plus" size="small" variant="tonal" @click="addDialog = true" /></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-2">
          <v-card v-for="contribution in managedContributions" :key="contribution.id" class="basic-row pa-3" elevation="0" rounded="lg">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1 min-w-0"><strong class="text-body-small">{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ contribution.kind === 'basic' ? t('contributions.manage.energyPoints', { value: contribution.energy }) : t('contributions.kind.extra') }} · {{ contribution.reward }} L</p></div>
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
                :aria-label="t('contributions.manage.deleteAria', { title: contribution.title })"
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
        <v-card-title class="pa-0 mb-1">{{ t('contributions.form.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ t('contributions.form.prototypeHint') }}</v-card-subtitle>
        <v-text-field v-model="newContribution.title" :label="t('common.title')" variant="outlined" />
        <v-textarea v-model="newContribution.description" :label="t('contributions.form.shortDescription')" rows="2" variant="outlined" />
        <v-select v-model="newContribution.assigneeId" :items="assignmentOptions" item-title="title" item-value="value" :label="t('contributions.form.assignment')" variant="outlined" />
        <div class="d-grid form-columns ga-3">
          <v-select v-model="newContribution.kind" :items="kindOptions" item-title="title" item-value="value" :label="t('contributions.form.kind')" variant="outlined" />
          <v-text-field v-model.number="newContribution.reward" :label="t('contributions.form.coins')" min="0" type="number" variant="outlined" />
        </div>
        <v-text-field v-model.number="newPromotion.teamworkBonus" :label="t('contributions.form.teamworkBonus')" min="0" suffix="L" type="number" variant="outlined" />
        <v-slider v-if="newContribution.kind === 'basic'" v-model="newContribution.energy" color="primary" :label="t('contributions.form.energyShare')" max="50" min="5" step="5" thumb-label />
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="addDialog = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" :disabled="!newContribution.title.trim()" rounded="lg" variant="flat" @click="addContribution">{{ t('common.add') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(contributionToDelete)" max-width="390" @update:model-value="contributionToDelete = undefined">
      <v-card v-if="contributionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal"><v-icon icon="mdi-delete-alert-outline" /></v-avatar>
        <v-card-title class="pa-0">{{ t('contributions.deleteContribution.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ contributionToDelete.title }}</v-card-subtitle>
        <v-alert class="mb-4" color="warning" density="compact" variant="tonal">{{ contributionToDelete.status === 'approved' ? t('contributions.deleteContribution.approved') : t('contributions.deleteContribution.open') }}</v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="contributionToDelete = undefined">{{ t('common.cancel') }}</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmContributionDelete">{{ t('common.delete') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(promotionToDelete)" max-width="390" @update:model-value="promotionToDelete = undefined">
      <v-card v-if="promotionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal"><v-icon icon="mdi-calendar-remove-outline" /></v-avatar>
        <v-card-title class="pa-0">{{ t('contributions.deletePromotion.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ promotionToDelete.title }}</v-card-subtitle>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="promotionToDelete = undefined">{{ t('common.cancel') }}</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmPromotionDelete">{{ t('common.delete') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="teamDialog" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <div class="team-dialog-icon mb-3">🤝</div>
        <v-card-title class="pa-0 mb-1">{{ t('contributions.team.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ t('contributions.team.description', { title: teamContribution?.title }) }}</v-card-subtitle>
        <v-select
          v-model="selectedSiblingIds"
          chips
          closable-chips
          :items="siblingOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.team.selectChildren')"
          multiple
          variant="outlined"
        />
        <div class="sibling-rule mb-4">
          <v-icon color="info" size="18">mdi-shield-check-outline</v-icon>
          <span>{{ t('contributions.team.rule') }}</span>
        </div>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="teamDialog = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="info" rounded="lg" variant="flat" @click="saveTeamInvite">{{ t('contributions.team.save') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(selectedPromotion)" max-width="410" @update:model-value="selectedPromotion = undefined">
      <v-card v-if="selectedPromotion" class="pa-5" rounded="xl">
        <div class="promotion-detail-icon">⚡</div>
        <p class="eyebrow mt-3 mb-1">{{ t('contributions.promotions.singular') }}</p>
        <h2 class="promotion-detail-title">{{ selectedPromotion.title }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2">{{ t('contributions.promotions.deadline', { title: contributionTitle(selectedPromotion.contributionId), deadline: selectedPromotion.deadline }) }}</p>
        <PromotionCountdown class="mt-3" :deadline="selectedPromotion.deadline" />
        <div class="promotion-reward mt-4">
          <span>{{ t('contributions.promotions.reachableReward') }}</span>
          <strong>{{ store.rewardForContribution(selectedPromotion.contributionId) }} L</strong>
          <small>{{ t('contributions.promotions.includingTeamwork', { value: selectedPromotion.teamworkBonus }) }}</small>
        </div>
        <v-btn class="mt-5" color="warning" rounded="lg" variant="flat" width="100%" @click="selectedPromotion = undefined">{{ t('common.understood') }}</v-btn>
      </v-card>
    </v-dialog>

    <v-dialog v-model="promotionDialog" max-width="440">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">{{ t('contributions.promotions.startTitle') }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ t('contributions.promotions.startDescription') }}</v-card-subtitle>
        <v-select
          v-model="newPromotion.contributionId"
          :items="promotionContributionOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.promotions.contribution')"
          variant="outlined"
        />
        <div class="d-grid form-columns ga-3">
          <v-select v-model="newPromotion.multiplier" :items="multiplierOptions" item-title="title" item-value="value" :label="t('contributions.promotions.bonus')" variant="outlined" />
          <v-text-field v-model="newPromotion.deadline" :label="t('contributions.promotions.validUntil')" type="time" variant="outlined" />
        </div>
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="promotionDialog = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="warning" :disabled="!newPromotion.contributionId || !newPromotion.deadline" rounded="lg" variant="flat" @click="addPromotion">{{ t('contributions.promotions.start') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

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
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const route = useRoute();
const { t } = useI18n();
const localize = useLocalizedDomainContent();
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
  kind: 'basic',
  assigneeId: '',
});
const kindOptions = computed(() => [
  { title: t('contributions.kind.basic'), value: 'basic' },
  { title: t('contributions.kind.extraOptional'), value: 'extra' },
]);
const assignmentOptions = computed(() => {
  if (!store.permissions.canManageContent) {
    return [{ title: t('contributions.assignment.own', { name: store.signedInMember.name }), value: store.signedInMemberId }];
  }
  return [
    { title: t('contributions.assignment.open'), value: '' },
    ...store.members.map((member) => ({
      title: `${member.name}${member.role === 'guardian' ? t('contributions.assignment.guardianSuffix') : ''}`,
      value: member.id,
    })),
  ];
});
const newPromotion = reactive<NewPromotion>({ contributionId: CONTRIBUTION_IDS.dishwasher, multiplier: 2, deadline: '17:00', teamworkBonus: 10 });
const multiplierOptions = computed(() => [
  { title: t('contributions.promotions.double'), value: 2 },
  { title: t('contributions.promotions.triple'), value: 3 },
]);
const localizedContributions = computed(() => store.contributions.map(localize.contribution));
const localizedPromotions = computed(() => store.promotions.map(localize.promotion));
const activePromotions = computed(() => localizedPromotions.value.filter(item => item.active));

const filteredContributions = computed(() =>
  localizedContributions.value.filter(
    (contribution) =>
      (filter.value === 'all' || contribution.kind === filter.value) &&
      (statusFilter.value === 'completed' ? contribution.status === 'approved' : contribution.status !== 'approved') &&
      (scopeFilter.value === 'all' ||
        (scopeFilter.value === 'mine' && contribution.assigneeId === store.activeChildId) ||
        (scopeFilter.value === 'open' && !contribution.assigneeId && contribution.status === 'available')),
  ),
);
const managedContributions = computed(() => [...localizedContributions.value].sort((left, right) =>
  Number(Boolean(left.assigneeId)) - Number(Boolean(right.assigneeId)),
));
const guardianOwnContributions = computed(() => localizedContributions.value.filter(
  (contribution) => contribution.assigneeId === store.signedInMemberId,
));
const guardianTasksToRate = computed(() => localizedContributions.value.filter((contribution) => {
  const assignee = store.members.find((member) => member.id === contribution.assigneeId);
  return contribution.status === 'pending' && assignee?.role === 'guardian';
}));
const childTasksToReview = computed(() => localizedContributions.value.filter((contribution) =>
  contribution.status === 'pending' &&
  store.members.some((member) => member.id === contribution.assigneeId && member.role === 'child'),
));
const openContributionCount = computed(() => store.contributions.filter(
  (contribution) => !contribution.assigneeId && contribution.status === 'available',
).length);
const promotionContributionOptions = computed(() =>
  localizedContributions.value.map((contribution) => ({ title: contribution.title, value: contribution.id })),
);
const teamContribution = computed(() => localizedContributions.value.find((contribution) => contribution.id === teamContributionId.value));
const siblingOptions = computed(() => store.members
  .filter((member) => member.role === 'child' && member.id !== teamContribution.value?.assigneeId)
  .map((member) => ({ title: `${member.avatar} ${member.name}`, value: member.id })),
);
const promotionFor = (contributionId: ContributionId) =>
  localizedPromotions.value.find((promotion) => promotion.contributionId === contributionId && isPromotionAvailable(promotion, store.familyTimeZone, new Date(store.currentTimeMilliseconds)));
const contributionTitle = (contributionId: ContributionId) =>
  localizedContributions.value.find((contribution) => contribution.id === contributionId)?.title ?? t('contributions.singular');
const assignedMember = (contribution: Contribution) => store.members.find((member) => member.id === contribution.assigneeId);
const memberName = (memberId?: FamilyMemberId) => store.members.find((member) => member.id === memberId)?.name ?? t('contributions.assignment.stillOpen');
const assigneeAppearance = (contribution: Contribution) =>
  assignedMember(contribution)?.appearance ?? createDefaultAvatarAppearance();
const assigneeLabel = (contribution: Contribution) => {
  if (!assignedMember(contribution)) return t('contributions.assignment.free');
  if (contribution.assigneeId === store.activeChildId) return t('contributions.assignment.forYou');
  return memberName(contribution.assigneeId);
};
const approvalMessage = (stars?: number) => {
  if ((stars ?? 1) >= 4) return t('contributions.approval.great');
  if (stars === 3) return t('contributions.approval.good');
  if (stars === 2) return t('contributions.approval.careful');
  return t('contributions.approval.nextTime');
};
const guardianContributionStatus = (contribution: Contribution): string => {
  if (contribution.status === 'pending') return t('contributions.guardianOwn.awaitingRating');
  if (contribution.status === 'approved') return t('contributions.starsReceived', { value: contribution.stars ?? 1 });
  return t('contributions.guardianOwn.ready');
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
  const seedKey = contribution.translationKey?.split('.').at(-1);
  if (seedKey === 'roomAir') return t('contributions.tips.air');
  if (seedKey === 'setTable' || seedKey === 'adamBreakfast') return t('contributions.tips.table');
  if (seedKey === 'dishwasher') return t('contributions.tips.dishes');
  if (seedKey === 'feedPet' || seedKey === 'danielCats') return t('contributions.tips.pets');
  if (seedKey === 'waterPlants') return t('contributions.tips.plants');
  if (seedKey === 'danielLaundry') return t('contributions.tips.laundry');
  if (seedKey === 'takeTrash') return t('contributions.tips.trash');
  return t('contributions.tips.default', { description: contribution.description });
};
onMounted(() => {
  if (store.viewerRole !== 'child') return;
  window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: t('guide.pages.contributions.heading'),
    message: t('guide.pages.contributions.message'),
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

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.empty-contributions {
  @apply d-flex flex-column align-center;
  border: 2px dashed
    color-mix(in srgb, var(--lad-palette-mint) 30%, transparent);
  background:
    radial-gradient(
      circle at 80% 15%,
      color-mix(in srgb, var(--lad-palette-amber-250) 20%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-surface)
    );
  box-shadow: inset 0 0 0 5px
    color-mix(in srgb, var(--lad-palette-white) 40%, transparent) !important;
}
.guardian-context {
  border: 1px solid color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
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
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  ) !important;
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-blue-550) 12%, transparent),
    0 8px 15px color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent);
  font-size: 1.9375rem !important;
  transform: rotate(-3deg);
  animation: task-icon-float 3.4s ease-in-out infinite;
}
.task-icon-avatar--compact {
  flex-basis: 50px;
  font-size: 1.6875rem !important;
}
.task-icon-avatar--small {
  flex-basis: 44px;
  font-size: 1.5rem !important;
}
.contribution-item--approved {
  border-color: color-mix(in srgb, var(--lad-palette-mint) 30%, transparent);
  background:
    radial-gradient(
      circle at 94% 10%,
      color-mix(in srgb, var(--lad-palette-yellow) 25%, transparent),
      transparent 24%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-surface),
      var(--lad-palette-background)
    );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent),
    0 13px 24px color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent) !important;
}
.contribution-item--approved::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 13px;
  right: 15px;
  color: color-mix(in srgb, var(--lad-palette-amber-450) 60%, transparent);
  font-size: 0.8125rem;
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
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-amber-500) 20%, transparent);
  border-radius: 17px;
  background:
    radial-gradient(
      circle at 90% 8%,
      color-mix(in srgb, var(--lad-palette-white) 90%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-amber-100),
      var(--lad-palette-amber-150)
    );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-amber-550) 15%, transparent),
    0 9px 17px color-mix(in srgb, var(--lad-palette-amber-650) 8%, transparent);
}
.approved-energy {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-teal-550) 20%,
    transparent
  );
  background:
    radial-gradient(
      circle at 90% 8%,
      color-mix(in srgb, var(--lad-palette-white) 90%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-150)
    );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent),
    0 9px 17px color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent);
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
  color: var(--lad-palette-amber-650);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: uppercase;
  letter-spacing: 0.025em;
}
.approved-energy small {
  color: var(--lad-palette-teal-700);
}
.approved-reward-main strong,
.approved-energy strong {
  margin-top: 2px;
  color: var(--lad-palette-amber-700);
  font-size: 0.9375rem;
  font-weight: var(--lad-font-weight-black);
  line-height: 1.1;
  white-space: nowrap;
}
.approved-energy strong {
  color: var(--lad-palette-teal-700);
}
.approved-reward-summary :deep(.contribution-meta-icon) {
  flex: 0 0 38px;
  width: 38px;
  height: 38px;
  animation: approved-reward-icon 2.2s ease-in-out infinite;
}
.contribution-label {
  @include contribution-label;
}
.contribution-label--basic {
  @include contribution-label-basic;
}
.contribution-label--special {
  @include contribution-label-special;
}
.contribution-label--mine {
  @include contribution-label-mine;
  animation: mine-label-breathe 2.6s ease-in-out infinite;
}
.contribution-label--open {
  color: var(--lad-palette-amber-650);
  border-color: color-mix(
    in srgb,
    var(--lad-palette-amber-450) 20%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-white)
  );
}
.assigned-member {
  min-height: 45px;
  padding: 5px 10px 5px 6px;
  @apply d-flex align-center;
  gap: 7px;
  color: var(--lad-palette-muted-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 18%, transparent);
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent);
}
.assigned-member :deep(.avatar-figure) {
  border: 2px solid var(--lad-palette-white);
  border-radius: 12px;
  background: var(--lad-palette-background);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-teal-700) 10%, transparent);
}
.assigned-member span,
.assigned-member small,
.assigned-member strong {
  @apply d-block;
}
.assigned-member small {
  color: var(--lad-muted);
  font-size: 0.5rem;
  font-weight: 750;
}
.assigned-member strong {
  margin-top: 1px;
  font-size: 0.6875rem;
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
  color: var(--lad-palette-teal-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 25%, transparent);
  border-radius: var(--lad-radius-pill);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-teal-150)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 18%, transparent),
    0 8px 15px color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
}
.earned-stars {
  padding: 6px 9px;
  @apply d-inline-flex align-center;
  gap: 1px;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-amber-450) 20%, transparent);
  border-radius: var(--lad-radius-pill);
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-150)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-amber-550) 12%, transparent);
}
.earned-star {
  color: var(--lad-palette-teal-150);
  opacity: 0.52;
  transform: scale(0.8);
}
.earned-star.active {
  color: var(--lad-palette-amber-450);
  opacity: 1;
  filter: drop-shadow(
    0 2px 1px color-mix(in srgb, var(--lad-palette-amber-600) 20%, transparent)
  );
  animation: earned-star-pop 1.9s ease-in-out infinite;
  animation-delay: calc(var(--star-index) * -120ms);
}
.claim-button {
  min-height: 48px !important;
  padding: 5px 15px 5px 7px !important;
  overflow: visible !important;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent) !important;
  border-radius: 17px !important;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  ) !important;
  box-shadow:
    0 5px 0 var(--lad-palette-teal-700),
    0 10px 17px
      color-mix(in srgb, var(--lad-palette-mint-strong) 18%, transparent) !important;
  font-weight: var(--lad-font-weight-black);
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
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    color-mix(in srgb, var(--lad-palette-white) 30%, transparent),
    color-mix(in srgb, var(--lad-palette-amber-150) 20%, transparent)
  );
  animation: claim-launch 2.2s ease-in-out infinite;
}
.claim-button-icon :deep(.v-icon) {
  color: var(--lad-palette-amber-100);
  font-size: 1.375rem;
  filter: drop-shadow(
    0 2px 0 color-mix(in srgb, var(--lad-palette-teal-700) 25%, transparent)
  );
}
.claim-button i {
  color: var(--lad-palette-amber-150);
  font-style: normal;
  animation: approved-sparkle 1.4s ease-in-out infinite;
}
.claim-button:active {
  transform: translateY(3px) scale(0.97);
  box-shadow: 0 2px 0 var(--lad-palette-teal-700) !important;
}
.finish-button {
  min-height: 48px !important;
  padding: 4px 16px 4px 7px !important;
  overflow: visible !important;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent) !important;
  border-radius: 17px !important;
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-350),
    var(--lad-palette-blue-strong)
  ) !important;
  box-shadow:
    0 5px 0 var(--lad-palette-blue-strong),
    0 10px 18px
      color-mix(in srgb, var(--lad-palette-blue-strong) 20%, transparent) !important;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.finish-button :deep(.v-btn__content) {
  gap: 7px;
}
.finish-button:active {
  transform: translateY(3px) scale(0.97);
  box-shadow: 0 2px 0 var(--lad-palette-blue-strong) !important;
}
.finish-check {
  width: 38px;
  height: 38px;
  @apply d-grid place-center;
  color: var(--lad-palette-blue-strong);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  border-radius: 13px;
  background: var(--lad-palette-white);
  box-shadow: inset 0 -3px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 12%, transparent);
}
.pending-celebration {
  min-height: 49px;
  padding: 7px 12px 7px 7px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 8px;
  color: var(--lad-palette-amber-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-amber-450) 25%, transparent);
  border-radius: 17px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-background)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-amber-550) 12%, transparent),
    0 8px 15px color-mix(in srgb, var(--lad-palette-orange-600) 8%, transparent);
}
.pending-celebration::after {
  content: "✦";
  @apply position-absolute;
  top: 4px;
  right: 7px;
  color: var(--lad-palette-amber-500);
  font-size: 0.5625rem;
  animation: pending-spark 2s ease-in-out infinite;
}
.pending-celebration-icon {
  width: 35px;
  height: 35px;
  @apply d-grid place-center flex-shrink-0;
  border: 2px solid var(--lad-palette-white);
  border-radius: 12px;
  background: var(--lad-palette-amber-250);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-amber-600) 12%, transparent);
  font-size: 1.125rem;
  animation: pending-ready 2.5s ease-in-out infinite;
}
.pending-celebration strong,
.pending-celebration small {
  @apply d-block;
}
.pending-celebration strong {
  font-size: 0.75rem;
}
.pending-celebration small {
  margin-top: 1px;
  color: var(--lad-palette-muted-600);
  font-size: 0.5rem;
}
.promotion-row {
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-amber-450) 25%, transparent);
  @apply cursor-pointer;
}
.promotion-detail-icon {
  width: 62px;
  height: 62px;
  @apply d-grid place-center;
  border-radius: 20px;
  background: var(--lad-palette-amber-150);
  font-size: 2rem;
}
.promotion-detail-title {
  @apply ma-0;
  font-size: 1.375rem;
  letter-spacing: -0.03em;
}
.promotion-reward {
  padding: 14px;
  border-radius: 16px;
  background: var(--lad-palette-amber-100);
}
.promotion-reward span,
.promotion-reward strong,
.promotion-reward small {
  @apply d-block;
}
.promotion-reward span,
.promotion-reward small {
  color: var(--lad-muted);
  font-size: 0.6875rem;
}
.promotion-reward strong {
  color: var(--lad-palette-amber-650);
  font-size: 1.6875rem;
}
.invited-team {
  @apply d-flex align-center flex-wrap;
  gap: 5px;
}
.invited-team > span {
  color: var(--lad-muted);
  font-size: 0.625rem;
  font-weight: 800;
}
.team-dialog-icon {
  width: 58px;
  height: 58px;
  @apply d-grid place-center;
  border-radius: 18px;
  background: var(--lad-palette-background);
  font-size: 1.875rem;
}
.sibling-rule {
  padding: 10px 12px;
  @apply d-flex align-start ga-2;
  color: var(--lad-muted);
  border-radius: 12px;
  background: var(--lad-palette-background);
  font-size: 0.6875rem;
  line-height: 1.4;
}
.rating-setting {
  border: 1px solid var(--lad-border);
}
.bonus-value {
  color: var(--lad-palette-amber-600);
  font-size: 1.125rem;
}
.contribution-meta {
  @include contribution-meta-grid;
}
.contribution-meta-chip {
  @include contribution-meta-chip;
}
.contribution-meta-chip--reward {
  @include contribution-meta-chip-reward;
}
.contribution-meta-chip--energy {
  @include contribution-meta-chip-energy;
}
.section-title {
  @apply ma-0;
  font-size: 1.125rem;
  letter-spacing: -0.025em;
}
.review-actions {
  grid-template-columns: 0.8fr 1.2fr;
}
.empty-icon {
  font-size: 2.625rem;
}
.form-columns {
  grid-template-columns: 1fr 1fr;
}
.assignment-select {
  max-width: 150px;
  flex: 0 0 150px;
  font-size: 0.625rem;
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
    filter: drop-shadow(
      0 0 6px color-mix(in srgb, var(--lad-palette-amber-450) 70%, transparent)
    );
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
    box-shadow: 0 3px 0
      color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
  }
  50% {
    box-shadow:
      0 3px 0 color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent),
      0 0 0 4px color-mix(in srgb, var(--lad-palette-green-500) 8%, transparent);
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
@include reduced-motion {
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
@include respond-down(compact) {
  .form-columns {
    grid-template-columns: 1fr;
  }
}
@include respond-down(phone) {
  .contribution-meta {
    @include contribution-meta-grid;
  }
  .contribution-meta-chip--energy {
    @include contribution-meta-chip-energy;
  }
}
@include respond-down(phone) {
  .approved-reward-summary {
    grid-template-columns: 1fr;
  }
}
</style>
