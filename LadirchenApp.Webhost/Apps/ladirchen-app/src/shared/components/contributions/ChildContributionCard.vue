<template>
  <BrandedCard class="contribution-item pa-4" :class="{ 'contribution-item--approved': contribution.status === 'approved' }" :data-ladi-heading="t('contributions.ladiTip')" :data-ladi-tip="tip" tone="contributions">
    <div class="d-flex align-start ga-3">
      <v-avatar class="task-icon-avatar" color="blue-lighten-5" rounded="lg" size="58">{{ contribution.icon }}</v-avatar>
      <div class="flex-grow-1 min-w-0">
        <div class="d-flex align-center flex-wrap ga-2 mb-1">
          <strong>{{ contribution.title }}</strong>
          <span class="contribution-label" :class="contribution.kind === 'basic' ? 'contribution-label--basic' : 'contribution-label--special'"><v-icon :icon="contribution.kind === 'basic' ? 'i-mdi:home-heart' : 'i-mdi:creation'" size="13" />{{ contribution.kind === 'basic' ? t('contributions.kind.basic') : t('contributions.kind.extra') }}</span>
          <span class="contribution-label" :class="contribution.assigneeId === activeChildId ? 'contribution-label--mine' : 'contribution-label--open'"><v-icon :icon="contribution.assigneeId === activeChildId ? 'i-mdi:account-heart' : 'i-mdi:account-multiple-outline'" size="13" />{{ assigneeLabel }}</span>
        </div>
        <p class="text-caption text-medium-emphasis">{{ contribution.description }}</p>
        <div v-if="contribution.status === 'approved'" class="approved-reward-summary mt-3">
          <MetricCard class="approved-reward-main" compact tone="reward"><ContributionMetaIcon kind="reward" /><span><small>{{ t('contributions.reward.earned') }}</small><strong>{{ t('contributions.reward.coins', { value: earnedReward }) }}</strong></span></MetricCard>
          <MetricCard v-if="contribution.kind === 'basic'" class="approved-energy" compact tone="energy"><ContributionMetaIcon kind="energy" /><span><small>{{ t('contributions.reward.house') }}</small><strong>{{ t('contributions.reward.energy', { value: contribution.energy }) }}</strong></span></MetricCard>
        </div>
        <div v-else class="mt-3 contribution-meta">
          <MetricCard class="contribution-meta-chip contribution-meta-chip--time" compact tone="info"><ContributionMetaIcon kind="time" /><span><small>{{ t('contributions.reward.when') }}</small><strong>{{ contribution.dueLabel }}</strong></span></MetricCard>
          <MetricCard class="contribution-meta-chip contribution-meta-chip--reward" compact tone="reward"><ContributionMetaIcon kind="reward" /><span><small>{{ t('contributions.reward.youEarn') }}</small><strong>{{ t('contributions.reward.coins', { value: reward }) }}</strong></span></MetricCard>
          <MetricCard v-if="contribution.kind === 'basic'" class="contribution-meta-chip contribution-meta-chip--energy" compact tone="energy"><ContributionMetaIcon kind="energy" /><span><small>{{ t('contributions.reward.house') }}</small><strong>{{ t('contributions.reward.energy', { value: contribution.energy }) }}</strong></span></MetricCard>
        </div>
        <ActiveContributionBonus v-if="contribution.status !== 'approved' && promotion" class="mt-2" :deadline="promotion.deadline" :multiplier="promotion.multiplier" :time-zone="familyTimeZone" />
        <div v-if="invitedChildNames.length" class="invited-team mt-3"><span>{{ t('contributions.togetherWith') }}</span><v-chip v-for="name in invitedChildNames" :key="name" color="info" size="x-small" variant="tonal">{{ name }}</v-chip></div>
      </div>
    </div>
    <div class="d-flex align-center justify-end flex-wrap ga-2 mt-3">
      <v-btn v-if="!assignedMember && contribution.status === 'available'" class="claim-button" color="primary" rounded="lg" variant="flat" @click="emit('claim', contribution)"><span class="claim-button-icon" aria-hidden="true"><v-icon icon="i-mdi:rocket-launch-outline" /></span><span>{{ t('contributions.claim') }}</span><i aria-hidden="true">✦</i></v-btn>
      <template v-else-if="contribution.assigneeId === activeChildId">
        <v-btn v-if="allowInvite && contribution.kind === 'extra' && contribution.status === 'available'" prepend-icon="i-mdi:account-multiple-plus-outline" rounded="lg" size="small" variant="tonal" @click="emit('invite', contribution)">{{ t('contributions.inviteSiblings') }}</v-btn>
        <div v-if="contribution.status === 'approved'" class="approved-celebration" :aria-label="t('contributions.starsReceived', { value: contribution.stars ?? 1 })"><span class="approved-badge">{{ approvalMessage }}</span><span class="earned-stars" aria-hidden="true"><v-icon v-for="star in 5" :key="star" class="earned-star" :class="{ active: star <= (contribution.stars ?? 1) }" icon="i-mdi:star" size="20" :style="{ '--star-index': star }" /></span></div>
        <div v-else-if="contribution.status === 'pending'" class="pending-celebration" role="status"><span class="pending-celebration-icon" aria-hidden="true">✨</span><span><strong>{{ t('contributions.pending.title') }}</strong><small>{{ t('contributions.pending.description') }}</small></span></div>
        <v-btn v-else class="finish-button" color="info" rounded="lg" variant="flat" @click="emit('submit', contribution)"><span class="finish-check" aria-hidden="true"><v-icon icon="i-mdi:check" size="24" /></span><span>{{ t('contributions.finish') }}</span></v-btn>
      </template>
      <div v-else-if="assignedMember" class="assigned-member"><AvatarFigure :appearance="resolveFamilyMemberAvatarAppearance(assignedMember, familyMembers)" :size="34" /><span><small>{{ t('contributions.assignedTo') }}</small><strong>{{ assignedMember.name }}</strong></span></div>
    </div>
  </BrandedCard>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

import { resolveFamilyMemberAvatarAppearance } from '@/domain/avatar';
import type { Contribution, Promotion } from '@/domain/contributions/types';
import type { FamilyMember, IanaTimeZone } from '@/domain/family/types';
import type { FamilyMemberId } from '@/domain/shared/identifiers';
import AvatarFigure from '@/shared/components/avatar/AvatarFigure.vue';
import ActiveContributionBonus from './ActiveContributionBonus.vue';
import ContributionMetaIcon from './ContributionMetaIcon.vue';
import BrandedCard from '@/shared/components/ui/BrandedCard.vue';
import MetricCard from '@/shared/components/ui/MetricCard.vue';

const props = withDefaults(defineProps<{
  activeChildId: FamilyMemberId;
  allowInvite?: boolean;
  contribution: Contribution;
  familyMembers: ReadonlyArray<FamilyMember>;
  familyTimeZone: IanaTimeZone;
  promotion?: Promotion;
  reward: number;
  tip: string;
}>(), {
  allowInvite: true,
});
const emit = defineEmits<{
  claim: [contribution: Contribution];
  invite: [contribution: Contribution];
  submit: [contribution: Contribution];
}>();
const { t } = useI18n();
const assignedMember = computed(() => props.familyMembers.find(member => member.id === props.contribution.assigneeId));
const assigneeLabel = computed(() => !assignedMember.value
  ? t('contributions.assignment.free')
  : props.contribution.assigneeId === props.activeChildId
    ? t('contributions.assignment.forYou')
    : assignedMember.value.name);
const earnedReward = computed(() => props.contribution.earnedReward ?? props.reward);
const invitedChildNames = computed(() => (props.contribution.invitedChildIds ?? []).map(id => props.familyMembers.find(member => member.id === id)?.name).filter((name): name is string => Boolean(name)));
const approvalMessage = computed(() => {
  const stars = props.contribution.stars ?? 1;
  if (stars >= 4) return t('contributions.approval.great');
  if (stars === 3) return t('contributions.approval.good');
  if (stars === 2) return t('contributions.approval.careful');
  return t('contributions.approval.nextTime');
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.contribution-item {
  @apply position-relative overflow-hidden;
}
.contribution-item--approved {
  border-color: color-mix(in srgb, var(--lad-color-primary) 30%, transparent);
  background:
    radial-gradient(
      circle at 94% 10%,
      color-mix(in srgb, var(--lad-color-reward) 25%, transparent),
      transparent 24%
    ),
    linear-gradient(145deg, var(--lad-surface), var(--lad-surface-soft));
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-color-primary-strong) 15%, transparent),
    0 13px 24px
      color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.task-icon-avatar {
  @include task-icon-tile(rem(58), 2rem, -3deg, false);
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
}
.contribution-label--open {
  color: var(--lad-color-reward-ink);
  border-color: color-mix(
    in srgb,
    var(--lad-color-reward-border) 20%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-surface-raised)
  );
}
.contribution-meta {
  @apply d-grid;
  grid-template-columns: minmax(0, 1.2fr) repeat(2, minmax(0, 1fr));
  gap: 6px;
}
.contribution-meta-chip > span {
  @apply min-w-0;
}
.contribution-meta-chip :deep(.contribution-meta-icon) {
  width: 30px;
  height: 30px;
  flex: 0 0 30px;
}
.contribution-meta-chip strong {
  white-space: normal;
  font-size: 0.5rem;
}
.approved-reward-summary {
  @apply d-grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr);
  gap: 8px;
}
.approved-reward-main,
.approved-energy {
  min-height: 67px;
}
.approved-reward-main small,
.approved-energy small {
  font-size: 0.5rem;
}
.approved-reward-main strong,
.approved-energy strong {
  font-size: rem(15);
}
.invited-team,
.approved-celebration {
  @apply d-flex align-center flex-wrap;
  gap: 7px;
}
.invited-team > span {
  color: var(--lad-muted);
  font-size: rem(10);
  font-weight: 800;
}
.assigned-member,
.pending-celebration {
  min-height: 45px;
  padding: 5px 10px;
  @apply d-flex align-center;
  gap: 7px;
  border-radius: 16px;
  background: var(--lad-surface-soft);
}
.assigned-member span,
.assigned-member small,
.assigned-member strong,
.pending-celebration strong,
.pending-celebration small {
  @apply d-block;
}
.assigned-member small,
.pending-celebration small {
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.assigned-member strong,
.pending-celebration strong {
  font-size: rem(11);
}
.approved-badge,
.earned-stars {
  padding: 7px 11px;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-soft);
}
.earned-star {
  color: var(--lad-color-primary-soft);
  opacity: 0.52;
}
.earned-star.active {
  color: var(--lad-color-reward-border);
  opacity: 1;
}
.claim-button {
  @include action-button(
    linear-gradient(
      145deg,
      var(--lad-color-primary-highlight),
      var(--lad-color-primary-strong)
    ),
    var(--lad-color-primary-deep)
  );
}
.finish-button {
  @include action-button;
}
.claim-button-icon {
  @include action-button-icon(rem(35), var(--lad-color-primary-strong));
}
.finish-check {
  @include action-button-icon;
}
.pending-celebration-icon {
  width: 35px;
  height: 35px;
  @apply d-grid place-center;
  border-radius: 12px;
  background: var(--lad-surface-raised);
}
</style>
