import { isPromotionAvailable } from '@/domain/promotions';
import { createDomainId } from '@/domain/types';
import type { Contribution, ContributionId, FamilyMemberId, NewContribution, NewPromotion, PromotionId } from '@/domain/types';
import { createUuid } from './family-world-store-utils';
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from '../family-world-store-context';

export const contributionsActions = {
  submitContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    const contribution = this.contributions.find((item) => item.id === id);
    if (!contribution || contribution.assigneeId !== this.signedInMemberId || contribution.status !== 'available') {return;}
    contribution.status = 'pending';
    this.persistContributions();
    this.notify(this.viewerRole === 'guardian' ? 'notifications.contributions.guardianSubmitted' : 'notifications.contributions.childSubmitted');
  },
  claimContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    const contribution = this.contributions.find((item) => item.id === id);
    const hasValidAssignee = this.members.some((member) => member.id === contribution?.assigneeId && member.role === 'child');
    if (this.viewerRole !== 'child' || !contribution || hasValidAssignee || contribution.status !== 'available') {return;}
    contribution.assigneeId = this.activeChildId;
    this.persistContributions();
    this.notify('notifications.contributions.claimed', { title: contribution.title });
  },
  assignContribution(this: FamilyWorldStoreContext, id: ContributionId, memberId?: FamilyMemberId) {
    if (!this.permissions.canManageContent) {return;}
    const contribution = this.contributions.find((item) => item.id === id);
    if (!contribution || contribution.status !== 'available') {return;}
    if (memberId && !this.members.some((member) => member.id === memberId)) {return;}
    contribution.assigneeId = memberId || undefined;
    contribution.invitedChildIds = [];
    this.persistContributions();
    const memberName = memberId ? this.members.find((member) => member.id === memberId)?.name : undefined;
    this.notify(memberName ? 'notifications.contributions.assigned' : 'notifications.contributions.opened', memberName ? { title: contribution.title, name: memberName } : { title: contribution.title });
  },
  approveContribution(this: FamilyWorldStoreContext, id: ContributionId, stars: number) {
    const contribution = this.contributions.find((item) => item.id === id);
    const assignee = this.members.find((member) => member.id === contribution?.assigneeId);
    const canRateChild = this.permissions.canManageContent && assignee?.role === 'child';
    const canRateGuardian = this.viewerRole === 'child' && assignee?.role === 'guardian';
    if ((!canRateChild && !canRateGuardian) || !contribution?.assigneeId || contribution.status !== 'pending') {return;}
    const approvalTime = new Date();
    const promotion = this.promotions.find(item => item.contributionId === id && isPromotionAvailable(item, this.familyTimeZone, approvalTime));
    const baseReward = contribution.reward * (promotion?.multiplier ?? 1) + (promotion?.teamworkBonus ?? 0);
    contribution.status = 'approved';
    contribution.stars = Math.max(1, Math.min(5, Math.round(stars)));
    const ratingBonus = stars === 5 && this.perfectRatingBonusPercent > 0
      ? Math.max(1, Math.round(baseReward * (this.perfectRatingBonusPercent / 100)))
      : 0;
    const reward = baseReward + ratingBonus;
    contribution.earnedReward = baseReward;
    contribution.earnedRatingBonus = ratingBonus;
    contribution.earnedPromotionMultiplier = promotion?.multiplier;
    contribution.approvedAt = approvalTime.toISOString();
    contribution.rewardCelebrated = false;
    this.balances[contribution.assigneeId] = this.balanceFor(contribution.assigneeId) + reward;
    this.persistContributions();
    this.persistSavings();
    if (contribution.assigneeId === this.signedInMemberId) {this.playRewardAnimation(contribution);}
    this.notify(ratingBonus > 0 ? 'notifications.contributions.rewardWithBonus' : 'notifications.contributions.reward', ratingBonus > 0
      ? { reward, percent: this.perfectRatingBonusPercent }
      : { reward });
  },
  returnContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    const contribution = this.contributions.find((item) => item.id === id);
    const assignee = this.members.find((member) => member.id === contribution?.assigneeId);
    const canReturnChild = this.permissions.canManageContent && assignee?.role === 'child';
    const canReturnGuardian = this.viewerRole === 'child' && assignee?.role === 'guardian';
    if ((!canReturnChild && !canReturnGuardian) || !contribution || contribution.status !== 'pending') {return;}
    contribution.status = 'available';
    this.persistContributions();
    this.notify('notifications.contributions.returned');
  },
  addContribution(this: FamilyWorldStoreContext, input: NewContribution) {
    const isOwnGuardianTask = this.viewerRole === 'guardian' && input.assigneeId === this.signedInMemberId;
    if (!this.permissions.canManageContent && !isOwnGuardianTask) {return;}
    this.contributions.push({
      id: createDomainId.contribution(createUuid()),
      ...input,
      area: '',
      areaKey: 'contributions.dynamic.familyArea',
      status: 'available',
      energy: input.kind === 'basic' ? input.energy : 0,
      assigneeId: input.assigneeId || undefined,
      dueLabel: '',
      dueLabelKey: input.kind === 'basic' ? 'contributions.dynamic.daily' : 'contributions.dynamic.optional',
      worldEffect: input.kind === 'basic' ? 'sparkle' : undefined,
    });
    this.persistContributions();
    this.notify('notifications.contributions.added');
  },
  deleteContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    if (!this.permissions.canManageContent) {return;}
    const contributionIndex = this.contributions.findIndex((item) => item.id === id);
    const contribution = this.contributions[contributionIndex];
    if (!contribution) {return;}
    this.contributions.splice(contributionIndex, 1);
    this.promotions = this.promotions.filter((promotion) => promotion.contributionId !== id);
    this.persistContributions();
    this.notify(contribution.status === 'approved' ? 'notifications.contributions.deletedApproved' : 'notifications.contributions.deletedOpen', { title: contribution.title });
  },
  deletePromotion(this: FamilyWorldStoreContext, id: PromotionId) {
    if (!this.permissions.canManageContent) {return;}
    const promotion = this.promotions.find((item) => item.id === id);
    if (!promotion) {return;}
    this.promotions = this.promotions.filter((item) => item.id !== id);
    this.persistContributions();
    this.notify('notifications.contributions.promotionDeleted', { title: promotion.title });
  },
  setContributionPartners(this: FamilyWorldStoreContext, id: ContributionId, childIds: FamilyMemberId[]) {
    const contribution = this.contributions.find((item) => item.id === id);
    if (!contribution || contribution.kind !== 'extra') {return;}
    const allowedIds = new Set(this.members
      .filter((member) => member.role === 'child' && member.id !== contribution.assigneeId)
      .map((member) => member.id));
    contribution.invitedChildIds = childIds.filter((childId) => allowedIds.has(childId));
    this.persistContributions();
    const names = this.members
      .filter((member) => contribution.invitedChildIds?.includes(member.id))
      .map((member) => member.name);
    this.notify(names.length > 0 ? 'notifications.contributions.partnersInvited' : 'notifications.contributions.partnersRemoved', names.length > 0 ? { names: names.join(', ') } : {});
  },
  addPromotion(this: FamilyWorldStoreContext, input: NewPromotion) {
    if (!this.permissions.canManageContent) {return;}
    const contribution = this.contributions.find((item) => item.id === input.contributionId);
    if (!contribution) {return;}
    this.promotions.push({
      id: createDomainId.promotion(createUuid()),
      ...input,
      title: '',
      titleKey: 'contributions.promotions.multiplierTitle',
      active: true,
    });
    this.persistContributions();
    this.notify('notifications.contributions.promotionAdded', { title: contribution.title });
  },
  revealNextContributionReward(this: FamilyWorldStoreContext) {
    if (this.rewardAnimation.visible) {return;}
    const contribution = this.contributions.find(item =>
      item.status === 'approved' &&
      item.assigneeId === this.signedInMemberId &&
      item.earnedReward !== undefined &&
      item.rewardCelebrated === false,
    );
    if (!contribution) {return;}
    this.playRewardAnimation(contribution);
  },
  playRewardAnimation(this: FamilyWorldStoreContext, contribution: Contribution) {
    this.rewardAnimation.contributionId = contribution.id;
    this.rewardAnimation.value = contribution.earnedReward ?? contribution.reward;
    this.rewardAnimation.energy = contribution.kind === 'basic' ? contribution.energy : 0;
    this.rewardAnimation.multiplier = contribution.earnedPromotionMultiplier ?? 1;
    this.rewardAnimation.stars = contribution.stars ?? 0;
    this.rewardAnimation.title = contribution.title;
    this.rewardAnimation.version += 1;
    this.rewardAnimation.visible = true;
  },
  dismissRewardAnimation(this: FamilyWorldStoreContext) {
    const contribution = this.contributions.find(item => item.id === this.rewardAnimation.contributionId);
    if (contribution) {
      contribution.rewardCelebrated = true;
      this.persistContributions();
    }
    this.rewardAnimation.visible = false;
    this.rewardAnimation.contributionId = undefined;
    window.setTimeout(() => this.revealNextContributionReward(), 250);
  },
} satisfies FamilyWorldActionGroup;
