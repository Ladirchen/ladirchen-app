<template>
  <div class="page page-padding contributions-page">
    <v-alert v-if="store.permissions.canManageContent" class="guardian-context mb-5" color="info" density="compact" icon="i-mdi:shield-account-outline" variant="tonal">
      {{ t('contributions.guardianContext') }}
    </v-alert>

    <template v-if="store.viewerRole === 'child'">
      <section v-if="guardianTasksToRate.length" class="mb-6">
        <SectionHeader :description="t('contributions.childReview.description')" :title="t('contributions.childReview.title')">
          <template #action><v-chip color="secondary" size="small">{{ guardianTasksToRate.length }}</v-chip></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <BrandedCard v-for="contribution in guardianTasksToRate" :key="contribution.id" class="review-card child-review-card pa-4" tone="contributions">
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
          </BrandedCard>
        </div>
      </section>

      <ContributionFilterPanel
        v-model:kind="filter"
        v-model:scope="scopeFilter"
        v-model:status="statusFilter"
        class="mb-4"
        :open-count="openContributionCount"
      />

      <TransitionGroup class="contribution-card-list d-flex flex-column ga-3" name="list" tag="div">
        <ChildContributionCard
          v-for="contribution in filteredContributions"
          :key="contribution.id"
          :active-child-id="store.activeChildId"
          :contribution="contribution"
          :family-members="store.members"
          :family-time-zone="store.familyTimeZone"
          :promotion="promotionFor(contribution.id)"
          :reward="store.rewardForContribution(contribution.id)"
          :tip="contributionTip(contribution)"
          @claim="store.claimContribution($event.id)"
          @invite="openTeamInvite"
          @submit="store.submitContribution($event.id)"
        />
      </TransitionGroup>
      <BrandedCard v-if="!filteredContributions.length" class="empty-contributions pa-5 text-center" tone="contributions">
        <AnimatedCompletionMark :size="84" />
        <strong>{{ emptyStateTitle }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ emptyStateDescription }}</p>
        <v-btn class="mt-3" color="primary" rounded="lg" variant="tonal" @click="toggleEmptyStateFilter">
          {{ emptyStateActionLabel }}
        </v-btn>
      </BrandedCard>
    </template>

    <template v-else>
      <section class="guardian-own-tasks mb-6">
        <SectionHeader :description="t('contributions.guardianOwn.description')" :title="t('contributions.guardianOwn.title')">
          <template #action><v-btn color="secondary" prepend-icon="i-mdi:plus" rounded="lg" size="small" variant="tonal" @click="openOwnTaskDialog">{{ t('contributions.guardianOwn.add') }}</v-btn></template>
        </SectionHeader>
        <div v-if="guardianOwnContributions.length" class="d-flex flex-column ga-2">
          <BrandedCard v-for="contribution in guardianOwnContributions" :key="contribution.id" class="basic-row pa-3" tone="contributions">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1 min-w-0"><strong class="text-body-small">{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ guardianContributionStatus(contribution) }}</p></div>
              <v-btn v-if="contribution.status === 'available'" class="finish-button guardian-finish-button" color="info" rounded="lg" size="small" variant="flat" @click="store.submitContribution(contribution.id)"><span class="finish-check" aria-hidden="true"><v-icon icon="i-mdi:check" size="24" /></span><span>{{ t('contributions.finish') }}</span></v-btn>
              <v-chip v-else :color="guardianStatusColor(contribution)" size="small" variant="tonal">{{ guardianStatusLabel(contribution) }}</v-chip>
            </div>
          </BrandedCard>
        </div>
        <BrandedCard v-else class="empty-review pa-4 text-center" tone="contributions"><p class="text-caption text-medium-emphasis">{{ t('contributions.guardianOwn.empty') }}</p></BrandedCard>
      </section>

      <section v-if="store.permissions.canManageContent && childTasksToReview.length" class="mb-6">
        <SectionHeader :description="t('contributions.review.description')" :title="t('contributions.review.title')">
          <template #action><v-chip color="warning" size="small">{{ childTasksToReview.length }}</v-chip></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <BrandedCard v-for="contribution in childTasksToReview" :key="contribution.id" class="review-card pa-4" tone="contributions">
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
          </BrandedCard>
        </div>
      </section>

      <BrandedCard v-else-if="store.permissions.canManageContent" class="empty-review pa-5 mb-6 text-center" tone="contributions">
        <div class="empty-icon mb-2">🌤️</div>
        <strong>{{ t('contributions.review.emptyTitle') }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ t('contributions.review.emptyDescription') }}</p>
      </BrandedCard>

      <template v-if="store.permissions.canManageContent">
        <SectionHeader :description="t('contributions.promotions.description')" :title="t('contributions.promotions.title')">
          <template #action><v-btn :aria-label="t('contributions.promotions.add')" color="warning" icon="i-mdi:lightning-bolt" size="small" variant="tonal" @click="promotionDialog = true" /></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-2 mb-6">
          <BrandedCard v-for="promotion in activePromotions" :key="promotion.id" class="promotion-row pa-3" interactive tone="contributions" role="button" tabindex="0" @click="selectedPromotion = promotion" @keydown.enter="selectedPromotion = promotion">
            <div class="d-flex align-center ga-3">
              <v-avatar color="warning" size="40" variant="tonal">⚡</v-avatar>
              <div class="flex-grow-1">
                <strong class="text-body-small">{{ promotion.title }}</strong>
                <p class="text-caption text-medium-emphasis">{{ contributionTitle(promotion.contributionId) }} · {{ t('contributions.promotions.includingTeamwork', { value: promotion.teamworkBonus }) }}</p>
                <PromotionCountdown class="mt-1" :deadline="promotion.deadline" :time-zone="store.familyTimeZone" />
              </div>
              <v-chip color="warning" size="small">×{{ promotion.multiplier }}</v-chip>
              <v-btn :aria-label="t('contributions.promotions.deleteAria', { title: promotion.title })" color="error" icon="i-mdi:delete-outline" size="small" variant="tonal" @click.stop="promotionToDelete = promotion" />
            </div>
          </BrandedCard>
        </div>

        <BrandedCard class="rating-setting pa-4 mb-6" tone="contributions">
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
        </BrandedCard>

        <SectionHeader :description="t('contributions.manage.description')" :title="t('contributions.manage.title')">
          <template #action><v-btn color="primary" icon="i-mdi:plus" size="small" variant="tonal" @click="addDialog = true" /></template>
        </SectionHeader>
        <div class="d-flex flex-column ga-2">
          <BrandedCard v-for="contribution in managedContributions" :key="contribution.id" class="basic-row pa-3" tone="contributions">
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1 min-w-0"><strong class="text-body-small">{{ contribution.title }}</strong><p class="text-caption text-medium-emphasis">{{ managedContributionDescription(contribution) }} · {{ t('contributions.reward.coins', { value: contribution.reward }) }}</p></div>
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
                icon="i-mdi:delete-outline"
                size="small"
                variant="tonal"
                @click="requestContributionDelete(contribution)"
              />
            </div>
          </BrandedCard>
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
        <v-text-field v-model.number="newPromotion.teamworkBonus" :label="t('contributions.form.teamworkBonus')" min="0" type="number" variant="outlined" />
        <v-slider v-if="newContribution.kind === 'basic'" v-model="newContribution.energy" color="primary" :label="t('contributions.form.energyShare')" max="50" min="5" step="5" thumb-label />
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="addDialog = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" :disabled="!newContribution.title.trim()" rounded="lg" variant="flat" @click="addContribution">{{ t('common.add') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(contributionToDelete)" max-width="390" @update:model-value="contributionToDelete = undefined">
      <v-card v-if="contributionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal"><v-icon icon="i-mdi:delete-alert-outline" /></v-avatar>
        <v-card-title class="pa-0">{{ t('contributions.deleteContribution.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ contributionToDelete.title }}</v-card-subtitle>
        <v-alert class="mb-4" color="warning" density="compact" variant="tonal">{{ contributionDeleteWarning }}</v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="contributionToDelete = undefined">{{ t('common.cancel') }}</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmContributionDelete">{{ t('common.delete') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog :model-value="Boolean(promotionToDelete)" max-width="390" @update:model-value="promotionToDelete = undefined">
      <v-card v-if="promotionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal"><v-icon icon="i-mdi:calendar-remove-outline" /></v-avatar>
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
          <v-icon color="info" size="18">i-mdi:shield-check-outline</v-icon>
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
        <PromotionCountdown class="mt-3" :deadline="selectedPromotion.deadline" :time-zone="store.familyTimeZone" />
        <div class="promotion-reward mt-4">
          <span>{{ t('contributions.promotions.reachableReward') }}</span>
          <strong>{{ t('contributions.reward.coins', { value: store.rewardForContribution(selectedPromotion.contributionId) }) }}</strong>
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
import { computed } from "vue";
import { useI18n } from "vue-i18n";

import AnimatedCompletionMark from "@/shared/components/AnimatedCompletionMark.vue";
import ChildContributionCard from "@/shared/components/contributions/ChildContributionCard.vue";
import ContributionFilterPanel from "@/features/contributions/components/ContributionFilterPanel.vue";
import PromotionCountdown from "@/shared/components/contributions/PromotionCountdown.vue";
import SectionHeader from "@/shared/components/ui/SectionHeader.vue";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";
import { useContributionsPage } from "@/features/contributions/composables/use-contributions-page";
import type { Contribution } from "@/domain/contributions/types";

const {
  activePromotions, addContribution, addDialog, addPromotion, assignContribution, assignmentOptions,
  childTasksToReview, confirmContributionDelete, confirmPromotionDelete, contributionTip, contributionTitle,
  contributionToDelete, filter, filteredContributions, guardianContributionStatus, guardianOwnContributions,
  guardianTasksToRate, kindOptions, managedContributions, memberName, multiplierOptions, newContribution,
  newPromotion, openContributionCount, openOwnTaskDialog, openTeamInvite, promotionContributionOptions,
  promotionDialog, promotionFor, promotionToDelete, ratings, requestContributionDelete, saveTeamInvite,
  scopeFilter, selectedPromotion, selectedSiblingIds, siblingOptions, statusFilter, store, teamContribution,
  teamDialog,
} = useContributionsPage();
const { t } = useI18n();
const emptyStateIsOpen = computed(() => statusFilter.value === "open");
const emptyStateTitle = computed(() => t(emptyStateIsOpen.value ? "contributions.empty.openTitle" : "contributions.empty.completedTitle"));
const emptyStateDescription = computed(() => t(emptyStateIsOpen.value ? "contributions.empty.openDescription" : "contributions.empty.completedDescription"));
const emptyStateActionLabel = computed(() => t(emptyStateIsOpen.value ? "contributions.empty.viewCompleted" : "contributions.empty.viewOpen"));
const contributionDeleteWarning = computed(() => t(contributionToDelete.value?.status === "approved"
  ? "contributions.deleteContribution.approved"
  : "contributions.deleteContribution.open"));
const toggleEmptyStateFilter = () => {
  statusFilter.value = emptyStateIsOpen.value ? "completed" : "open";
  filter.value = "all";
};
const guardianStatusColor = (contribution: Contribution): "success" | "warning" => contribution.status === "approved" ? "success" : "warning";
const guardianStatusLabel = (contribution: Contribution) => t(contribution.status === "approved" ? "contributions.guardianOwn.rated" : "contributions.guardianOwn.submitted");
const managedContributionDescription = (contribution: Contribution) => contribution.kind === "basic"
  ? t("contributions.manage.energyPoints", { value: contribution.energy })
  : t("contributions.kind.extra");
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.empty-contributions {
  --uno: d-flex flex-column align-center;
  border: 2px dashed
    color-mix(in srgb, var(--lad-color-primary) 30%, transparent);
  background:
    radial-gradient(
      circle at 80% 15%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 20%, transparent),
      transparent 27%
    ),
    linear-gradient(145deg, var(--lad-surface-soft), var(--lad-surface));
  box-shadow: inset 0 0 0 5px
    color-mix(in srgb, var(--lad-surface-raised) 40%, transparent);
}
.contribution-card-list {
  padding-inline: rem(17);
}
.guardian-context {
  border: 1px solid color-mix(in srgb, var(--lad-color-info) 20%, transparent);
}
.review-card,
.basic-row {
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border);
}
.task-icon-avatar {
  @include task-icon-tile;
}
.task-icon-avatar--compact {
  flex-basis: 50px;
  font-size: rem(27);
}
.task-icon-avatar--small {
  flex-basis: 44px;
  font-size: 1.5rem;
}
.finish-button {
  @include action-button;
}
.finish-check {
  @include action-button-icon;
}
.promotion-row {
  border: 1px solid
    color-mix(in srgb, var(--lad-color-reward-border) 25%, transparent);
  --uno: cursor-pointer;
}
.promotion-detail-icon {
  width: 62px;
  height: 62px;
  --uno: d-grid place-center;
  border-radius: 20px;
  background: var(--lad-color-reward-pale);
  font-size: 2rem;
}
.promotion-detail-title {
  --uno: ma-0;
  font-size: rem(22);
  letter-spacing: -0.03em;
}
.promotion-reward {
  padding: 14px;
  border-radius: 16px;
  background: var(--lad-color-reward-soft);
}
.promotion-reward span,
.promotion-reward strong,
.promotion-reward small {
  --uno: d-block;
}
.promotion-reward span,
.promotion-reward small {
  color: var(--lad-muted);
  font-size: rem(11);
}
.promotion-reward strong {
  color: var(--lad-color-reward-ink);
  font-size: rem(27);
}
.team-dialog-icon {
  width: 58px;
  height: 58px;
  --uno: d-grid place-center;
  border-radius: 18px;
  background: var(--lad-surface-soft);
  font-size: rem(30);
}
.sibling-rule {
  padding: 10px 12px;
  --uno: d-flex align-start ga-2;
  color: var(--lad-muted);
  border-radius: 12px;
  background: var(--lad-surface-soft);
  font-size: rem(11);
  line-height: 1.4;
}
.rating-setting {
  border: 1px solid var(--lad-border);
}
.bonus-value {
  color: var(--lad-color-reward-deep);
  font-size: rem(18);
}
.section-title {
  @include section-title;
}
.review-actions {
  grid-template-columns: 0.8fr 1.2fr;
}
.empty-icon {
  font-size: rem(42);
}
.form-columns {
  grid-template-columns: 1fr 1fr;
}
.assignment-select {
  max-width: 150px;
  flex: 0 0 150px;
  font-size: rem(10);
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
@include reduced-motion {
  .task-icon-avatar {
    animation: none;
  }
}
@include respond-down(compact) {
  .form-columns {
    grid-template-columns: 1fr;
  }
}
</style>
