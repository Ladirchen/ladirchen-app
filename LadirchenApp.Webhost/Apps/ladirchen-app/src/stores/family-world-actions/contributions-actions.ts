import type { Contribution, NewContribution, NewPromotion } from "@/domain/contributions/types";
import { createDomainId } from "@/domain/shared/identifiers";
import type { ContributionId, FamilyMemberId, PromotionId } from "@/domain/shared/identifiers";
import { REWARD_REVEAL_DELAY_MS } from "@/shared/runtime-timing";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";
import { createUuid } from "./family-world-store-utils";

export const contributionsActions = {
  async setPerfectRatingBonusPercent(this: FamilyWorldStoreContext, percent: number) {
    await executeBackendFamilyCommand(
      this,
      "contributions.set-perfect-rating-bonus",
      { percent },
      "notifications.save.contributions",
    );
  },
  async submitContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    await executeBackendFamilyCommand(this, "contributions.submit", { id }, "notifications.save.contributions", () =>
      this.notify(
        this.viewerRole === "guardian"
          ? "notifications.contributions.guardianSubmitted"
          : "notifications.contributions.childSubmitted",
      ),
    );
  },
  async claimContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    const title = this.contributions.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(this, "contributions.claim", { id }, "notifications.save.contributions", () =>
      this.notify("notifications.contributions.claimed", { title }),
    );
  },
  async assignContribution(this: FamilyWorldStoreContext, id: ContributionId, memberId?: FamilyMemberId) {
    const title = this.contributions.find((item) => item.id === id)?.title ?? "";
    const name = memberId ? this.members.find((member) => member.id === memberId)?.name : undefined;
    await executeBackendFamilyCommand(
      this,
      "contributions.assign",
      { id, memberId },
      "notifications.save.contributions",
      () =>
        this.notify(
          name ? "notifications.contributions.assigned" : "notifications.contributions.opened",
          name ? { name, title } : { title },
        ),
    );
  },
  async approveContribution(this: FamilyWorldStoreContext, id: ContributionId, stars: number) {
    await executeBackendFamilyCommand(
      this,
      "contributions.approve",
      { id, stars },
      "notifications.save.contributions",
      () => {
        const approved = this.contributions.find((item) => item.id === id);
        const ratingBonus = approved?.earnedRatingBonus ?? 0;
        const reward = (approved?.earnedReward ?? 0) + ratingBonus;
        this.notify(
          ratingBonus > 0 ? "notifications.contributions.rewardWithBonus" : "notifications.contributions.reward",
          ratingBonus > 0 ? { percent: this.perfectRatingBonusPercent, reward } : { reward },
        );

        if (approved?.assigneeId === this.signedInMemberId) {
          this.playRewardAnimation(approved);
        }
      },
    );
  },
  async returnContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    await executeBackendFamilyCommand(this, "contributions.return", { id }, "notifications.save.contributions", () =>
      this.notify("notifications.contributions.returned"),
    );
  },
  async addContribution(this: FamilyWorldStoreContext, input: NewContribution) {
    const id = createDomainId.contribution(createUuid());
    await executeBackendFamilyCommand(
      this,
      "contributions.add",
      { id, ...input },
      "notifications.save.contributions",
      () => this.notify("notifications.contributions.added"),
    );
  },
  async deleteContribution(this: FamilyWorldStoreContext, id: ContributionId) {
    const contribution = this.contributions.find((item) => item.id === id);
    await executeBackendFamilyCommand(this, "contributions.delete", { id }, "notifications.save.contributions", () =>
      this.notify(
        contribution?.status === "approved"
          ? "notifications.contributions.deletedApproved"
          : "notifications.contributions.deletedOpen",
        { title: contribution?.title ?? "" },
      ),
    );
  },
  async deletePromotion(this: FamilyWorldStoreContext, id: PromotionId) {
    const title = this.promotions.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(
      this,
      "contributions.delete-promotion",
      { id },
      "notifications.save.contributions",
      () => this.notify("notifications.contributions.promotionDeleted", { title }),
    );
  },
  async setContributionPartners(this: FamilyWorldStoreContext, id: ContributionId, childIds: FamilyMemberId[]) {
    const names = this.members.filter((member) => childIds.includes(member.id)).map((member) => member.name);
    await executeBackendFamilyCommand(
      this,
      "contributions.set-partners",
      { id, partnerIds: childIds },
      "notifications.save.contributions",
      () =>
        this.notify(
          names.length > 0
            ? "notifications.contributions.partnersInvited"
            : "notifications.contributions.partnersRemoved",
          names.length > 0 ? { names: names.join(", ") } : {},
        ),
    );
  },
  async addPromotion(this: FamilyWorldStoreContext, input: NewPromotion) {
    const id = createDomainId.promotion(createUuid());
    const title = this.contributions.find((item) => item.id === input.contributionId)?.title ?? "";
    await executeBackendFamilyCommand(
      this,
      "contributions.add-promotion",
      { id, ...input },
      "notifications.save.contributions",
      () => this.notify("notifications.contributions.promotionAdded", { title }),
    );
  },
  revealNextContributionReward(this: FamilyWorldStoreContext) {
    if (this.rewardAnimation.visible) {
      return;
    }

    const contribution = this.contributions.find(
      (item) =>
        item.status === "approved" &&
        item.assigneeId === this.signedInMemberId &&
        item.earnedReward !== undefined &&
        item.rewardCelebrated === false,
    );

    if (contribution) {
      this.playRewardAnimation(contribution);
    }
  },
  playRewardAnimation(this: FamilyWorldStoreContext, contribution: Contribution) {
    this.rewardAnimation.contributionId = contribution.id;
    this.rewardAnimation.value = contribution.earnedReward ?? contribution.reward;
    this.rewardAnimation.energy = contribution.kind === "basic" ? contribution.energy : 0;
    this.rewardAnimation.multiplier = contribution.earnedPromotionMultiplier ?? 1;
    this.rewardAnimation.stars = contribution.stars ?? 0;
    this.rewardAnimation.title = contribution.title;
    this.rewardAnimation.version += 1;
    this.rewardAnimation.visible = true;
  },
  async dismissRewardAnimation(this: FamilyWorldStoreContext) {
    const contribution = this.contributions.find((item) => item.id === this.rewardAnimation.contributionId);

    if (contribution) {
      await executeBackendFamilyCommand(
        this,
        "contributions.acknowledge-reward",
        { id: contribution.id },
        "notifications.save.contributions",
      );
    }

    this.rewardAnimation.visible = false;
    this.rewardAnimation.contributionId = undefined;
    this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), REWARD_REVEAL_DELAY_MS);
  },
} satisfies FamilyWorldActionGroup;
