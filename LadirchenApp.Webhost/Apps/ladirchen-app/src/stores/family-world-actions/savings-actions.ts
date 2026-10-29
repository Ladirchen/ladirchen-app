import type { FamilyCurrency, NewGoal, SavingGoalOwnerId } from "@/domain/savings/types";
import { createDomainId } from "@/domain/shared/identifiers";
import type { FamilyMemberId, SavingGoalId } from "@/domain/shared/identifiers";
import { REWARD_REVEAL_DELAY_MS } from "@/shared/runtime-timing";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";
import { createUuid } from "./family-world-store-utils";

export const savingsActions = {
  async setFamilyCurrency(this: FamilyWorldStoreContext, currency: FamilyCurrency) {
    await executeBackendFamilyCommand(this, "savings.set-currency", { currency }, "notifications.save.savings");
  },
  async setLadirchenExchangeRate(this: FamilyWorldStoreContext, rate: number) {
    await executeBackendFamilyCommand(this, "savings.set-exchange-rate", { rate }, "notifications.save.savings");
  },
  async setMaximumSavingsInterestRate(this: FamilyWorldStoreContext, percent: number) {
    await executeBackendFamilyCommand(
      this,
      "progression.set-maximum-savings-rate",
      { percent },
      "notifications.save.savings",
    );
  },
  async toggleCheer(this: FamilyWorldStoreContext, id: SavingGoalId) {
    await executeBackendFamilyCommand(this, "savings.toggle-cheer", { id }, "notifications.save.savings", () => {
      const goal = this.goals.find((item) => item.id === id);
      this.notify(goal?.cheered ? "notifications.goals.cheered" : "notifications.goals.cheerRemoved");
    });
  },
  async addGoal(this: FamilyWorldStoreContext, input: NewGoal, requestedOwnerId?: SavingGoalOwnerId) {
    const ownerId = requestedOwnerId ?? (this.viewerRole === "guardian" ? this.signedInMemberId : this.activeChildId);
    const id = createDomainId.savingGoal(createUuid());
    await executeBackendFamilyCommand(
      this,
      "savings.add-goal",
      { id, ownerId, ...input },
      "notifications.save.savings",
      () => {
        this.activeGoalId = id;
        this.notify(ownerId === "family" ? "notifications.goals.familyAdded" : "notifications.goals.personalAdded");
      },
    );
  },
  async updateGoal(this: FamilyWorldStoreContext, id: SavingGoalId, input: NewGoal) {
    await executeBackendFamilyCommand(this, "savings.update-goal", { id, ...input }, "notifications.save.savings", () =>
      this.notify("notifications.goals.updated"),
    );
  },
  async supportGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    const giftId = createDomainId.guardianGift(createUuid());
    const before = this.goals.find((item) => item.id === id);
    const savedBefore = before?.saved ?? 0;
    await executeBackendFamilyCommand(
      this,
      "savings.support-goal",
      { amount, giftId, id, senderName: this.signedInMember.name.replace(" (du)", "") },
      "notifications.save.savings",
      () => {
        const credited = Math.max(0, (this.goals.find((item) => item.id === id)?.saved ?? 0) - savedBefore);
        const name = this.members.find((member) => member.id === before?.ownerId)?.name ?? "";
        this.notify(
          before?.ownerId === "family" ? "notifications.goals.familyGifted" : "notifications.goals.childGifted",
          before?.ownerId === "family" ? { amount: credited } : { amount: credited, name },
        );
      },
    );
  },
  async giftLadirchenToGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    const giftId = createDomainId.guardianGift(createUuid());
    const before = this.goals.find((item) => item.id === id);
    const savedBefore = before?.saved ?? 0;
    await executeBackendFamilyCommand(
      this,
      "savings.gift-to-goal",
      { amount, giftId, id, senderName: this.signedInMember.name },
      "notifications.save.savings",
      () => {
        const credited = Math.max(0, (this.goals.find((item) => item.id === id)?.saved ?? 0) - savedBefore);
        const name = this.members.find((member) => member.id === before?.ownerId)?.name ?? "";
        this.notify(
          before?.ownerId === "family" ? "notifications.goals.ownFamilyGifted" : "notifications.goals.childGifted",
          before?.ownerId === "family" ? { amount: credited } : { amount: credited, name },
        );
      },
    );
  },
  async giftLadirchenToFamilyMember(this: FamilyWorldStoreContext, memberId: FamilyMemberId, amount: number) {
    const giftId = createDomainId.guardianGift(createUuid());
    const name = this.members.find((member) => member.id === memberId)?.name ?? "";
    await executeBackendFamilyCommand(
      this,
      "savings.gift-to-member",
      { amount, giftId, recipientId: memberId, senderName: this.signedInMember.name },
      "notifications.save.savings",
      () => this.notify("notifications.goals.memberGifted", { amount: Math.round(amount), name }),
    );
  },
  async giftLadirchenToChild(this: FamilyWorldStoreContext, childId: FamilyMemberId, amount: number, reason: string) {
    const giftId = createDomainId.guardianGift(createUuid());
    const name = this.members.find((member) => member.id === childId)?.name ?? "";
    await executeBackendFamilyCommand(
      this,
      "savings.special-gift",
      { amount, childId, giftId, reason, senderName: this.signedInMember.name.replace(" (du)", "") },
      "notifications.save.savings",
      () => this.notify("notifications.goals.specialGifted", { amount: Math.round(amount), name }),
    );
  },
  async revealNextGuardianGift(this: FamilyWorldStoreContext) {
    if (this.viewerRole !== "child" || this.guardianGiftAnimation.visible) {
      return;
    }

    const gift = this.pendingGuardianGifts.find((item) => item.childId === this.activeChildId);

    if (!gift) {
      return;
    }

    await executeBackendFamilyCommand(
      this,
      "savings.consume-guardian-gift",
      { id: gift.id },
      "notifications.save.savings",
      () => {
        this.guardianGiftAnimation = {
          amount: gift.amount,
          destination: gift.destination ?? "goal",
          goalTitle: gift.goalTitle,
          guardianName: gift.guardianName,
          version: this.guardianGiftAnimation.version + 1,
          visible: true,
        };
      },
    );
  },
  dismissGuardianGift(this: FamilyWorldStoreContext) {
    this.guardianGiftAnimation.visible = false;
    this.$familyWorld.scheduler.schedule(() => this.revealNextGuardianGift(), REWARD_REVEAL_DELAY_MS);
  },
  async saveToGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    const before = this.goals.find((item) => item.id === id)?.saved ?? 0;
    await executeBackendFamilyCommand(this, "savings.deposit", { amount, id }, "notifications.save.savings", () => {
      const credited = Math.max(0, (this.goals.find((item) => item.id === id)?.saved ?? 0) - before);
      this.notify("notifications.goals.deposited", { amount: credited });
    });
  },
  async withdrawFromGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    const before = this.goals.find((item) => item.id === id)?.saved ?? 0;
    await executeBackendFamilyCommand(this, "savings.withdraw", { amount, id }, "notifications.save.savings", () => {
      const withdrawn = Math.max(0, before - (this.goals.find((item) => item.id === id)?.saved ?? 0));
      this.notify("notifications.goals.withdrawn", { amount: withdrawn });
    });
  },
  async cancelGoal(this: FamilyWorldStoreContext, id: SavingGoalId) {
    const goal = this.goals.find((item) => item.id === id);
    const balanceBefore = this.balance;
    await executeBackendFamilyCommand(this, "savings.cancel-goal", { id }, "notifications.save.savings", () => {
      const returned = Math.max(0, this.balance - balanceBefore);
      const forfeitedBonus = Math.max(0, (goal?.saved ?? 0) - returned);
      this.notify(forfeitedBonus > 0 ? "notifications.goals.cancelledWithBonus" : "notifications.goals.cancelled", {
        amount: returned,
      });

      if (this.activeGoalId === id) {
        this.activeGoalId =
          this.goals.find((item) => item.ownerId === this.activeChildId)?.id ?? this.goals[0]?.id ?? this.activeGoalId;
      }
    });
  },
  async consentToSavingsVoucher(this: FamilyWorldStoreContext, id: SavingGoalId) {
    await executeBackendFamilyCommand(this, "savings.consent-voucher", { id }, "notifications.save.savings");
  },
  async redeemSavingsVoucher(this: FamilyWorldStoreContext, id: SavingGoalId) {
    await executeBackendFamilyCommand(this, "savings.redeem-voucher", { id }, "notifications.save.savings");
  },
} satisfies FamilyWorldActionGroup;
