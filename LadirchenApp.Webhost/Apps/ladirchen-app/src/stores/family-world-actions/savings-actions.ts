import type { FamilyCurrency, NewGoal, SavingGoalOwnerId } from "@/domain/savings/types";
import { calculateSavingsCredit } from "@/domain/savings/interest";
import { normalizeSpecialGiftAmount, SAVINGS_RULES } from "@/domain/savings/rules";
import { createDomainId } from "@/domain/shared/identifiers";
import type { FamilyMemberId, SavingGoalId } from "@/domain/shared/identifiers";
import { createUuid, supportedFamilyCurrencies } from "./family-world-store-utils";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { REWARD_REVEAL_DELAY_MS } from "@/shared/runtime-timing";

export const savingsActions = {
  setFamilyCurrency(this: FamilyWorldStoreContext, currency: FamilyCurrency) {
    if (!this.permissions.canManageFamily) {return;}
    if (!supportedFamilyCurrencies.includes(currency)) {return;}
    this.familyCurrencyCode = currency;
    this.persistSavings();
  },
  setLadirchenExchangeRate(this: FamilyWorldStoreContext, rate: number) {
    if (!this.permissions.canManageFamily) {return;}
    if (!Number.isFinite(rate) || rate < 1) {return;}
    this.ladirchenPerCurrencyUnit = Math.round(rate);
    this.persistSavings();
  },
  toggleCheer(this: FamilyWorldStoreContext, id: SavingGoalId) {
    const goal = this.goals.find((item) => item.id === id);
    if (!goal) {return;}
    if (this.viewerRole === "guardian") {
      const canViewGoal = (goal.visibility === "family" && (goal.ownerId !== "family" || this.permissions.canViewFamilyGoals)) ||
        (goal.visibility === "guardians" && this.permissions.canViewGuardianGoals);
      if (!this.permissions.canSupportChildGoals || !canViewGoal) {return;}
    }
    goal.cheered = !goal.cheered;
    this.persistSavings();
    this.notify(goal.cheered ? "notifications.goals.cheered" : "notifications.goals.cheerRemoved");
  },
  addGoal(this: FamilyWorldStoreContext, input: NewGoal, requestedOwnerId?: SavingGoalOwnerId) {
    const ownerId = requestedOwnerId ?? (this.viewerRole === "guardian" ? this.signedInMemberId : this.activeChildId);
    const isFamilyGoal = ownerId === "family";
    const isOwnGoal = ownerId === this.signedInMemberId;
    const isManagedChildGoal = this.permissions.canManageGoals && this.members.some((member) => member.id === ownerId && member.role === "child");
    if (!isFamilyGoal && !isOwnGoal && !isManagedChildGoal) {return;}
    const id = createDomainId.savingGoal(createUuid());
    const starterBonus = isFamilyGoal ? 0 : SAVINGS_RULES.goalStarterBonus;
    this.goals.push({
      id,
      ...input,
      ownerId,
      saved: starterBonus,
      starterBonus,
      shared: isFamilyGoal,
      visibility: isFamilyGoal ? "family" : input.visibility,
      cheered: false,
    });
    this.activeGoalId = id;
    this.persistSavings();
    this.notify(isFamilyGoal ? "notifications.goals.familyAdded" : "notifications.goals.personalAdded");
  },
  updateGoal(this: FamilyWorldStoreContext, id: SavingGoalId, input: NewGoal) {
    if (this.viewerRole === "guardian" && !this.permissions.canManageGoals) {return;}
    const goal = this.goals.find((item) => item.id === id && item.ownerId === this.activeChildId);
    if (!goal) {return;}
    goal.title = input.title;
    goal.icon = input.icon;
    goal.target = Math.max(goal.saved, input.target);
    goal.visibility = input.visibility;
    this.persistSavings();
    this.notify("notifications.goals.updated");
  },
  supportGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    if (!this.permissions.canSupportChildGoals) {return;}
    const goal = this.goals.find((item) =>
      item.id === id &&
      item.visibility !== "private" &&
      (item.ownerId === "family"
        ? this.permissions.canViewFamilyGoals
        : this.permissions.canViewGuardianGoals || item.visibility === "family"),
    );
    if (!goal) {return;}
    const safeAmount = Math.max(0, Math.min(Math.round(amount), goal.target - goal.saved));
    if (safeAmount === 0) {return;}
    goal.saved += safeAmount;
    if (goal.ownerId === "family") {
      this.persistSavings();
      this.notify("notifications.goals.familyGifted", { amount: safeAmount });
      return;
    }
    const childName = this.members.find((member) => member.id === goal.ownerId)?.name ?? "";
    const guardianName = this.signedInMember.name.replace(" (du)", "");
    this.pendingGuardianGifts.push({
      id: createDomainId.guardianGift(createUuid()),
      childId: goal.ownerId,
      guardianName,
      goalTitle: goal.title,
      destination: "goal",
      amount: safeAmount,
    });
    this.persistSavings();
    this.notify("notifications.goals.childGifted", { amount: safeAmount, name: childName });
  },
  giftLadirchenToGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    if (this.viewerRole !== "child") {return;}
    const goal = this.goals.find((item) =>
      item.id === id &&
      item.ownerId !== this.activeChildId &&
      item.visibility === "family",
    );
    const recipient = goal
      && goal.ownerId !== "family"
      ? this.members.find((member) => member.id === goal.ownerId && member.role === "child")
      : undefined;
    if (!goal || (goal.ownerId !== "family" && !recipient)) {return;}
    const safeAmount = Math.max(0, Math.min(
      Math.round(amount),
      this.availableBalance,
      goal.target - goal.saved,
    ));
    if (safeAmount === 0) {return;}
    this.balances[this.activeChildId] = this.balance - safeAmount;
    goal.saved += safeAmount;
    if (recipient) {
      this.pendingGuardianGifts.push({
        id: createDomainId.guardianGift(createUuid()),
        childId: recipient.id,
        guardianName: this.signedInMember.name,
        goalTitle: goal.title,
        destination: "goal",
        amount: safeAmount,
      });
    }
    this.persistSavings();
    this.notify(goal.ownerId === "family" ? "notifications.goals.ownFamilyGifted" : "notifications.goals.childGifted", goal.ownerId === "family"
      ? { amount: safeAmount }
      : { amount: safeAmount, name: recipient?.name ?? "" });
  },
  giftLadirchenToFamilyMember(this: FamilyWorldStoreContext, memberId: FamilyMemberId, amount: number) {
    if (this.viewerRole !== "child") {return;}
    const recipient = this.members.find((member) =>
      member.id === memberId && member.role === "child" && member.id !== this.activeChildId,
    );
    const safeAmount = Math.max(0, Math.min(Math.round(amount), this.availableBalance));
    if (!recipient || safeAmount === 0) {return;}
    this.balances[this.activeChildId] = this.balance - safeAmount;
    this.balances[recipient.id] = this.balanceFor(recipient.id) + safeAmount;
    this.pendingGuardianGifts.push({
      id: createDomainId.guardianGift(createUuid()),
      childId: recipient.id,
      guardianName: this.signedInMember.name,
      goalTitle: "",
      destination: "balance",
      amount: safeAmount,
    });
    this.persistSavings();
    this.notify("notifications.goals.memberGifted", { amount: safeAmount, name: recipient.name });
  },
  giftLadirchenToChild(this: FamilyWorldStoreContext, childId: FamilyMemberId, amount: number, reason: string) {
    if (!this.permissions.canManageContent) {return;}
    const child = this.members.find((member) => member.id === childId && member.role === "child");
    const safeAmount = normalizeSpecialGiftAmount(amount);
    const safeReason = reason.trim();
    if (!child || !safeReason) {return;}
    this.balances[child.id] = this.balanceFor(child.id) + safeAmount;
    this.pendingGuardianGifts.push({
      id: createDomainId.guardianGift(createUuid()),
      childId: child.id,
      guardianName: this.signedInMember.name.replace(" (du)", ""),
      goalTitle: safeReason,
      destination: "balance",
      amount: safeAmount,
    });
    this.persistSavings();
    this.notify("notifications.goals.specialGifted", { amount: safeAmount, name: child.name });
  },
  revealNextGuardianGift(this: FamilyWorldStoreContext) {
    if (this.viewerRole !== "child" || this.guardianGiftAnimation.visible) {return;}
    const giftIndex = this.pendingGuardianGifts.findIndex((gift) => gift.childId === this.activeChildId);
    if (giftIndex < 0) {return;}
    const [gift] = this.pendingGuardianGifts.splice(giftIndex, 1);
    if (!gift) {return;}
    this.guardianGiftAnimation = {
      visible: true,
      guardianName: gift.guardianName,
      goalTitle: gift.goalTitle,
      destination: gift.destination ?? "goal",
      amount: gift.amount,
      version: this.guardianGiftAnimation.version + 1,
    };
    this.persistSavings();
  },
  dismissGuardianGift(this: FamilyWorldStoreContext) {
    this.guardianGiftAnimation.visible = false;
    this.$familyWorld.scheduler.schedule(() => this.revealNextGuardianGift(), REWARD_REVEAL_DELAY_MS);
  },
  saveToGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    const goal = this.goals.find((item) => item.id === id);
    if (!goal) {return;}
    const remaining = Math.max(0, goal.target - goal.saved);
    const safeAmount = Math.max(0, Math.min(amount, this.availableBalance, remaining));
    if (safeAmount === 0) {return;}
    goal.saved += safeAmount;
    this.balances[this.activeChildId] = this.balance - safeAmount;
    this.persistSavings();
    this.notify("notifications.goals.deposited", { amount: safeAmount });
  },
  withdrawFromGoal(this: FamilyWorldStoreContext, id: SavingGoalId, amount: number) {
    const goal = this.goals.find((item) => item.id === id && item.ownerId === this.activeChildId);
    if (!goal) {return;}
    const safeAmount = Math.max(0, Math.min(amount, this.withdrawableGoalBalance(goal)));
    if (safeAmount === 0) {return;}
    goal.saved -= safeAmount;
    this.balances[this.activeChildId] = this.balance + safeAmount;
    this.persistSavings();
    this.notify("notifications.goals.withdrawn", { amount: safeAmount });
  },
  cancelGoal(this: FamilyWorldStoreContext, id: SavingGoalId) {
    const goalIndex = this.goals.findIndex((item) => item.id === id && item.ownerId === this.activeChildId);
    if (goalIndex < 0) {return;}
    const goal = this.goals[goalIndex];
    if (!goal) {return;}
    const completed = goal.saved >= goal.target;
    const forfeitedBonus = completed ? 0 : Math.min(goal.starterBonus ?? 0, goal.saved);
    const returned = Math.max(0, goal.saved - forfeitedBonus);
    this.balances[this.activeChildId] = this.balance + returned;
    this.goals.splice(goalIndex, 1);
    if (this.activeGoalId === id) {
      this.activeGoalId = this.goals.find((item) => item.ownerId === this.activeChildId)?.id ?? this.goals[0]?.id ?? this.activeGoalId;
    }
    this.persistSavings();
    this.notify(forfeitedBonus > 0 ? "notifications.goals.cancelledWithBonus" : "notifications.goals.cancelled", { amount: returned });
  },
  creditWeeklyInterestDemo(this: FamilyWorldStoreContext) {
    let credited = 0;
    for (const goal of this.goals) {
      if (goal.ownerId === "family" || goal.saved <= 0) {continue;}
      const interest = calculateSavingsCredit(goal.saved, goal.target, this.savingsInterestRateFor(goal.ownerId));
      goal.saved += interest;
      goal.interestEarned = (goal.interestEarned ?? 0) + interest;
      credited += interest;
    }
    if (credited > 0) {this.persistSavings();}
    this.notify("notifications.goals.weeklyInterest", { amount: credited });
  },
  creditActiveChildInterestDemo(this: FamilyWorldStoreContext, rate: number): number {
    if (this.viewerRole !== "child") {return 0;}
    const safeRate = Math.max(0, Math.min(this.maxSavingsRatePercent, rate));
    let credited = 0;
    for (const goal of this.goals) {
      if (goal.ownerId !== this.activeChildId || goal.saved <= 0) {continue;}
      const interest = calculateSavingsCredit(goal.saved, goal.target, safeRate);
      if (interest <= 0) {continue;}
      goal.saved += interest;
      goal.interestEarned = (goal.interestEarned ?? 0) + interest;
      credited += interest;
    }
    if (credited > 0) {
      this.persistSavings();
      this.notify("notifications.goals.interestCredited", { amount: credited });
    } else {
      this.notify("notifications.goals.interestUnavailable");
    }
    return credited;
  },
} satisfies FamilyWorldActionGroup;
