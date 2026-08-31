import { defineStore } from 'pinia';

import { createContributions, createFamilyMembers, createFamilyPets, createHouseAccessories, createPromotions, createSavingGoals, createShopRewards } from '../data/fixtures';
import { HOUSE_THEMES } from '../data/house-catalog';
import type { AvatarAppearance } from '../domain/avatar';
import { isPromotionAvailable } from '../domain/promotions';
import { shopRedemptionIsOpen } from '../domain/shop';
import type { Contribution, FamilyCurrency, FamilyMember, FamilyPet, HouseAccessory, NewContribution, NewGoal, NewPromotion, NewShopReward, SavingGoal, ViewerRole } from '../domain/types';

const onboardingWasCompleted = () =>
  typeof localStorage !== 'undefined' && localStorage.getItem('ladirchen-family-setup') === 'completed';

const supportedFamilyCurrencies: FamilyCurrency[] = ['CHF', 'EUR', 'HUF'];
const loadFamilyCurrency = (): FamilyCurrency => {
  if (typeof localStorage === 'undefined') return 'CHF';
  const stored = localStorage.getItem('ladirchen-family-currency') as FamilyCurrency | null;
  return stored && supportedFamilyCurrencies.includes(stored) ? stored : 'CHF';
};
const loadLadirchenExchangeRate = (): number => {
  if (typeof localStorage === 'undefined') return 10;
  const stored = Number(localStorage.getItem('ladirchen-exchange-rate'));
  return Number.isFinite(stored) && stored >= 1 ? stored : 10;
};

const loadStoredList = <T>(key: string, fallback: () => T[]): T[] => {
  if (typeof localStorage === 'undefined') return fallback();
  try {
    const stored = JSON.parse(localStorage.getItem(key) ?? 'null');
    return Array.isArray(stored) && stored.length > 0 ? stored as T[] : fallback();
  } catch {
    return fallback();
  }
};

const loadHouseAccessories = (): HouseAccessory[] => {
  const catalog = createHouseAccessories();
  const stored = loadStoredList<HouseAccessory>('ladirchen-house-accessories', () => []);
  const storedById = new Map(stored.map((accessory) => [accessory.id, accessory]));
  return catalog.map((accessory) => {
    const saved = storedById.get(accessory.id);
    return saved ? { ...accessory, owned: saved.owned, equipped: saved.equipped } : accessory;
  });
};

const shopRewardIsAvailable = (availableUntil?: string) => {
  if (!availableUntil) return true;
  return new Date(`${availableUntil}T23:59:59`).getTime() >= Date.now();
};

interface PendingGuardianGift {
  id: string;
  childId: string;
  guardianName: string;
  goalTitle: string;
  destination: 'balance' | 'goal';
  amount: number;
}

export const usePrototypeStore = defineStore('ladirchenPrototype', {
  state: () => ({
    signedInMemberId: 'laura',
    viewerRole: 'child' as ViewerRole,
    activeChildId: loadStoredList<FamilyMember>('ladirchen-family-members', createFamilyMembers)
      .find((member) => member.role === 'child')?.id ?? 'laura',
    activeGoalId: 'bike',
    onboardingCompleted: onboardingWasCompleted(),
    familySetupOpen: !onboardingWasCompleted(),
    balances: { laura: 340, adam: 220, daniel: 175 } as Record<string, number>,
    familyCurrencyCode: loadFamilyCurrency(),
    ladirchenPerCurrencyUnit: loadLadirchenExchangeRate(),
    perfectRatingBonusPercent: 5,
    baseSavingsRatePercent: 1,
    streakBonusRate: 0.5,
    completionBonusRate: 3.5,
    ratingBonusRate: 2.5,
    maxSavingsRatePercent: 8,
    simulatedEnergy: null as number | null,
    piggyBankOpen: false,
    rewardAnimation: { visible: false, value: 0, version: 0 },
    guardianGiftAnimation: { visible: false, guardianName: '', goalTitle: '', destination: 'goal' as 'balance' | 'goal', amount: 0, version: 0 },
    pendingGuardianGifts: loadStoredList<PendingGuardianGift>('ladirchen-pending-gifts', () => []),
    completedWeeklyStreak: 0,
    currentWeekDays: 4,
    currentWeekTarget: 7,
    houseLevel: 0,
    houseThemeId: HOUSE_THEMES[0]?.id ?? 'sunny-dollhouse',
    revealVersion: 0,
    snackbar: { visible: false, message: '' },
    members: loadStoredList<FamilyMember>('ladirchen-family-members', createFamilyMembers),
    pets: loadStoredList<FamilyPet>('ladirchen-family-pets', createFamilyPets),
    contributions: createContributions(),
    goals: createSavingGoals(),
    accessories: loadHouseAccessories(),
    promotions: createPromotions(),
    shopRewards: createShopRewards(),
  }),

  getters: {
    activeChild(state): FamilyMember {
      return state.members.find((member) => member.id === state.activeChildId) ?? state.members[0];
    },
    signedInMember(state): FamilyMember {
      return state.members.find((member) => member.id === state.signedInMemberId) ?? state.members[0];
    },
    balanceFor: (state) => (memberId: string): number => state.balances[memberId] ?? 0,
    displayNameFor: (state) => (memberId: string): string => {
      const member = state.members.find((item) => item.id === memberId);
      return member?.nickname?.trim() || member?.name || 'Familienmitglied';
    },
    balance(): number {
      return this.balanceFor(this.activeChildId);
    },
    totalVisibleSavedFor: (state) => (memberId: string): number => state.goals
      .filter((goal) => goal.ownerId === memberId && goal.visibility !== 'private')
      .reduce((sum, goal) => sum + goal.saved, 0),
    averageTaskRatingFor: (state) => (memberId: string): number => {
      const rated = state.contributions.filter(
        (contribution) => contribution.assigneeId === memberId && contribution.stars,
      );
      return rated.length === 0
        ? 0
        : rated.reduce((sum, contribution) => sum + (contribution.stars ?? 0), 0) / rated.length;
    },
    pendingCountFor: (state) => (memberId: string): number => state.contributions.filter(
      (contribution) => contribution.assigneeId === memberId && contribution.status === 'pending',
    ).length,
    openCountFor: (state) => (memberId: string): number => state.contributions.filter(
      (contribution) => contribution.assigneeId === memberId && contribution.status === 'available',
    ).length,
    dailyBaseContributions(state): Contribution[] {
      return state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === state.activeChildId,
      );
    },
    dailyEnergy(state): number {
      if (state.simulatedEnergy !== null) return state.simulatedEnergy;
      const total = this.dailyBaseContributions.reduce((sum, contribution) => sum + contribution.energy, 0);
      if (total === 0) return 60;
      const completed = this.dailyBaseContributions
        .filter((contribution) => contribution.status === 'approved')
        .reduce((sum, contribution) => sum + contribution.energy, 0);
      return Math.min(100, Math.round((completed / total) * 100));
    },
    approvedBaseCount(): number {
      return this.dailyBaseContributions.filter((contribution) => contribution.status === 'approved').length;
    },
    pendingContributions(state): Contribution[] {
      return state.contributions.filter((contribution) => contribution.status === 'pending');
    },
    contributionProgress: (state) => (memberId: string): number => {
      const contributions = state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === memberId,
      );
      const total = contributions.reduce((sum, contribution) => sum + contribution.energy, 0);
      const completed = contributions
        .filter((contribution) => contribution.status === 'approved')
        .reduce((sum, contribution) => sum + contribution.energy, 0);
      return total === 0 ? 60 : Math.min(100, Math.round((completed / total) * 100));
    },
    familyEnergy(): number {
      if (this.simulatedEnergy !== null) return this.simulatedEnergy;
      const children = this.members.filter((member) => member.role === 'child');
      if (children.length === 0) return 60;
      return Math.round(
        children.reduce((sum, child) => sum + this.contributionProgress(child.id), 0) / children.length,
      );
    },
    houseMeetsMinimumEnergy(): boolean {
      return this.familyEnergy >= 60;
    },
    currentDailyStreak(): number {
      return this.completedWeeklyStreak * this.currentWeekTarget + this.currentWeekDays;
    },
    rewardForContribution: (state) => (contributionId: string): number => {
      const contribution = state.contributions.find((item) => item.id === contributionId);
      const promotion = state.promotions.find(
        (item) => item.contributionId === contributionId && isPromotionAvailable(item),
      );
      return (contribution?.reward ?? 0) * (promotion?.multiplier ?? 1) + (promotion?.teamworkBonus ?? 0);
    },
    activeGoal(state): SavingGoal {
      const visibleGoals = state.goals.filter(
        (goal) =>
          goal.visibility === 'family' ||
          (state.viewerRole === 'child' && goal.ownerId === state.activeChildId) ||
          (state.viewerRole === 'guardian' && goal.visibility === 'guardians'),
      );
      return visibleGoals.find((goal) => goal.id === state.activeGoalId) ??
        visibleGoals.find((goal) => goal.ownerId === state.activeChildId) ??
        visibleGoals[0] ??
        state.goals[0];
    },
    ownSavingGoals(state): SavingGoal[] {
      return state.goals.filter(
        (goal) => goal.ownerId === state.activeChildId &&
          (state.viewerRole === 'child' || goal.visibility !== 'private'),
      );
    },
    totalSaved(): number {
      return this.ownSavingGoals.reduce((sum, goal) => sum + goal.saved, 0);
    },
    totalInterestEarned(): number {
      return this.ownSavingGoals.reduce((sum, goal) => sum + (goal.interestEarned ?? 0), 0);
    },
    withdrawableGoalBalance: () => (goal?: SavingGoal): number => {
      if (!goal) return 0;
      if (goal.saved >= goal.target) return goal.saved;
      return Math.max(0, goal.saved - (goal.starterBonus ?? 0));
    },
    reservedBalance(state): number {
      return state.shopRewards
        .filter((reward) => reward.status === 'requested' && reward.requesterId === state.activeChildId)
        .reduce((sum, reward) => sum + reward.price, 0);
    },
    availableBalance(): number {
      return Math.max(0, this.balance - this.reservedBalance);
    },
    familyCurrencyValue: (state) => (ladirchen: number): number =>
      Number((ladirchen / state.ladirchenPerCurrencyUnit).toFixed(2)),
    averageTaskRating(): number {
      return this.averageTaskRatingFor(this.activeChildId);
    },
    savingsInterestRateFor: (state) => (memberId: string): number => {
      const contributions = state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === memberId,
      );
      const totalEnergy = contributions.reduce((sum, contribution) => sum + contribution.energy, 0);
      const completedEnergy = contributions
        .filter((contribution) => contribution.status === 'approved')
        .reduce((sum, contribution) => sum + contribution.energy, 0);
      const completion = totalEnergy === 0 ? 0.6 : completedEnergy / totalEnergy;
      const rated = contributions.filter((contribution) => contribution.stars);
      const rating = rated.length === 0
        ? 0
        : rated.reduce((sum, contribution) => sum + (contribution.stars ?? 0), 0) / rated.length / 5;
      const streak = memberId === state.activeChildId
        ? state.completedWeeklyStreak * state.currentWeekTarget + state.currentWeekDays
        : state.members.find((member) => member.id === memberId)?.weeklyStreak ?? 0;
      return Number(Math.min(state.maxSavingsRatePercent,
        state.baseSavingsRatePercent +
        streak * state.streakBonusRate +
        completion * state.completionBonusRate +
        rating * state.ratingBonusRate
      ).toFixed(2));
    },
    savingsInterestRate(): number {
      return this.savingsInterestRateFor(this.activeChildId);
    },
    activeWorldEffects(state): string[] {
      return state.contributions
        .filter((contribution) => contribution.status === 'approved' && contribution.worldEffect)
        .map((contribution) => contribution.worldEffect as string);
    },
  },

  actions: {
    notify(message: string) {
      this.snackbar.message = message;
      this.snackbar.visible = true;
    },
    setFamilyCurrency(currency: FamilyCurrency) {
      if (!supportedFamilyCurrencies.includes(currency)) return;
      this.familyCurrencyCode = currency;
      if (typeof localStorage !== 'undefined') localStorage.setItem('ladirchen-family-currency', currency);
    },
    setLadirchenExchangeRate(rate: number) {
      if (!Number.isFinite(rate) || rate < 1) return;
      this.ladirchenPerCurrencyUnit = Math.round(rate);
      if (typeof localStorage !== 'undefined') localStorage.setItem('ladirchen-exchange-rate', String(this.ladirchenPerCurrencyUnit));
    },
    switchSession(memberId: string) {
      const member = this.members.find((item) => item.id === memberId);
      if (!member) return;
      this.signedInMemberId = member.id;
      this.viewerRole = member.role;
      if (member.role === 'child') {
        this.activeChildId = member.id;
        this.revealNextGuardianGift();
      }
      this.notify(`Prototyp-Sitzung: Angemeldet als ${member.name}.`);
    },
    selectChildForGuardian(memberId: string) {
      if (this.viewerRole !== 'guardian') return;
      const child = this.members.find((member) => member.id === memberId && member.role === 'child');
      if (!child) return;
      this.activeChildId = child.id;
      this.notify(`${child.name} ist für die Detailansichten ausgewählt.`);
    },
    saveOwnAppearance(appearance: AvatarAppearance) {
      const member = this.members.find((item) => item.id === this.activeChildId && item.role === 'child');
      if (!member) return;
      member.appearance = { ...appearance };
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-family-members', JSON.stringify(this.members));
      }
      this.notify('Dein neues Profil wurde gespeichert.');
    },
    setOwnNickname(nickname: string) {
      if (this.viewerRole !== 'child' || this.signedInMemberId !== this.activeChildId) return;
      const member = this.members.find((item) => item.id === this.activeChildId && item.role === 'child');
      if (!member) return;
      member.nickname = nickname.trim().slice(0, 18) || undefined;
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-family-members', JSON.stringify(this.members));
      }
      this.notify(member.nickname ? `Dein Spitzname ist jetzt ${member.nickname}.` : 'Dein Spitzname wurde entfernt.');
    },
    submitContribution(id: string) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !contribution || contribution.assigneeId !== this.activeChildId || contribution.status !== 'available') return;
      contribution.status = 'pending';
      this.notify('Beitrag eingereicht. Die Welt reagiert nach der Bestätigung.');
    },
    claimContribution(id: string) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !contribution || contribution.assigneeId || contribution.status !== 'available') return;
      contribution.assigneeId = this.activeChildId;
      this.notify(`„${contribution.title}“ gehört jetzt zu deinen Beiträgen.`);
    },
    assignContribution(id: string, childId?: string) {
      if (this.viewerRole !== 'guardian') return;
      const contribution = this.contributions.find((item) => item.id === id);
      if (!contribution || contribution.status !== 'available') return;
      if (childId && !this.members.some((member) => member.id === childId && member.role === 'child')) return;
      contribution.assigneeId = childId || undefined;
      contribution.invitedChildIds = [];
      const childName = childId ? this.members.find((member) => member.id === childId)?.name : undefined;
      this.notify(childName ? `„${contribution.title}“ wurde ${childName} zugewiesen.` : `„${contribution.title}“ ist jetzt für alle Kinder offen.`);
    },
    approveContribution(id: string, stars: number) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (this.viewerRole !== 'guardian' || !contribution?.assigneeId || contribution.status !== 'pending') return;
      contribution.status = 'approved';
      contribution.stars = stars;
      const baseReward = this.rewardForContribution(id);
      const ratingBonus = stars === 5 && this.perfectRatingBonusPercent > 0
        ? Math.max(1, Math.round(baseReward * (this.perfectRatingBonusPercent / 100)))
        : 0;
      const reward = baseReward + ratingBonus;
      this.balances[contribution.assigneeId] = this.balanceFor(contribution.assigneeId) + reward;
      this.playRewardAnimation(reward);
      this.notify(
        ratingBonus > 0
          ? `${reward} Ladirchen inklusive ${this.perfectRatingBonusPercent} % Sternebonus wurden gutgeschrieben.`
          : `${reward} Ladirchen wurden gutgeschrieben.`,
      );
    },
    returnContribution(id: string) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (this.viewerRole !== 'guardian' || !contribution || contribution.status !== 'pending') return;
      contribution.status = 'available';
      this.notify('Der Beitrag wurde mit der Bitte um Nachbesserung zurückgegeben.');
    },
    addContribution(input: NewContribution) {
      if (this.viewerRole !== 'guardian') return;
      this.contributions.push({
        id: `contribution-${Date.now()}`,
        ...input,
        area: 'Familie',
        status: 'available',
        energy: input.kind === 'basic' ? input.energy : 0,
        assigneeId: input.assigneeId || undefined,
        dueLabel: input.kind === 'basic' ? 'Täglich' : 'Freiwillig',
        worldEffect: input.kind === 'basic' ? 'sparkle' : undefined,
      });
      this.notify('Beitrag wurde lokal hinzugefügt.');
    },
    setContributionPartners(id: string, childIds: string[]) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (!contribution || contribution.kind !== 'extra') return;
      const allowedIds = new Set(this.members
        .filter((member) => member.role === 'child' && member.id !== contribution.assigneeId)
        .map((member) => member.id));
      contribution.invitedChildIds = childIds.filter((childId) => allowedIds.has(childId));
      const names = this.members
        .filter((member) => contribution.invitedChildIds?.includes(member.id))
        .map((member) => member.name);
      this.notify(names.length > 0 ? `${names.join(' und ')} wurden zur Spezialaufgabe eingeladen.` : 'Die Einladungen wurden entfernt.');
    },
    toggleCheer(id: string) {
      const goal = this.goals.find((item) => item.id === id);
      if (!goal) return;
      goal.cheered = !goal.cheered;
      this.notify(goal.cheered ? 'Deine Unterstützung ist für die Familie sichtbar.' : 'Unterstützung zurückgenommen.');
    },
    addGoal(input: NewGoal) {
      const id = `goal-${Date.now()}`;
      this.goals.push({ id, ...input, ownerId: this.activeChildId, saved: 5, starterBonus: 5, shared: false, cheered: false });
      this.activeGoalId = id;
      this.notify('Neues Sparziel angelegt: 5 L geschützter Startbonus wurden gutgeschrieben.');
    },
    updateGoal(id: string, input: NewGoal) {
      const goal = this.goals.find((item) => item.id === id && item.ownerId === this.activeChildId);
      if (!goal) return;
      goal.title = input.title;
      goal.icon = input.icon;
      goal.target = Math.max(goal.saved, input.target);
      goal.visibility = input.visibility;
      this.notify('Sparziel wurde aktualisiert.');
    },
    supportGoal(id: string, amount: number) {
      if (this.viewerRole !== 'guardian') return;
      const goal = this.goals.find((item) => item.id === id && item.ownerId !== 'family');
      if (!goal) return;
      const safeAmount = Math.max(0, Math.min(Math.round(amount), goal.target - goal.saved));
      if (safeAmount === 0) return;
      goal.saved += safeAmount;
      const childName = this.members.find((member) => member.id === goal.ownerId)?.name ?? 'das Kind';
      const guardianName = this.signedInMember.name.replace(' (du)', '');
      this.pendingGuardianGifts.push({
        id: `gift-${Date.now()}`,
        childId: goal.ownerId,
        guardianName,
        goalTitle: goal.title,
        destination: 'goal',
        amount: safeAmount,
      });
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-pending-gifts', JSON.stringify(this.pendingGuardianGifts));
      }
      this.notify(`${safeAmount} Ladirchen wurden ${childName}s Sparziel geschenkt.`);
    },
    giftLadirchenToGoal(id: string, amount: number) {
      if (this.viewerRole !== 'child') return;
      const goal = this.goals.find((item) =>
        item.id === id &&
        item.ownerId !== this.activeChildId &&
        item.ownerId !== 'family' &&
        item.visibility === 'family',
      );
      const recipient = goal
        ? this.members.find((member) => member.id === goal.ownerId && member.role === 'child')
        : undefined;
      if (!goal || !recipient) return;
      const safeAmount = Math.max(0, Math.min(
        Math.round(amount),
        this.availableBalance,
        goal.target - goal.saved,
      ));
      if (safeAmount === 0) return;
      this.balances[this.activeChildId] = this.balance - safeAmount;
      goal.saved += safeAmount;
      this.pendingGuardianGifts.push({
        id: `gift-${Date.now()}`,
        childId: recipient.id,
        guardianName: this.signedInMember.name,
        goalTitle: goal.title,
        destination: 'goal',
        amount: safeAmount,
      });
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-pending-gifts', JSON.stringify(this.pendingGuardianGifts));
      }
      this.notify(`${safeAmount} Ladirchen wurden ${recipient.name}s Sparziel geschenkt.`);
    },
    giftLadirchenToChild(childId: string, amount: number, reason: string) {
      if (this.viewerRole !== 'guardian') return;
      const child = this.members.find((member) => member.id === childId && member.role === 'child');
      const safeAmount = Math.max(1, Math.min(10_000, Math.round(amount)));
      const safeReason = reason.trim();
      if (!child || !safeReason) return;
      this.balances[child.id] = this.balanceFor(child.id) + safeAmount;
      this.pendingGuardianGifts.push({
        id: `gift-${Date.now()}`,
        childId: child.id,
        guardianName: this.signedInMember.name.replace(' (du)', ''),
        goalTitle: safeReason,
        destination: 'balance',
        amount: safeAmount,
      });
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-pending-gifts', JSON.stringify(this.pendingGuardianGifts));
      }
      this.notify(`${safeAmount} Ladirchen wurden ${child.name} als besonderes Geschenk gutgeschrieben.`);
    },
    revealNextGuardianGift() {
      if (this.viewerRole !== 'child' || this.guardianGiftAnimation.visible) return;
      const giftIndex = this.pendingGuardianGifts.findIndex((gift) => gift.childId === this.activeChildId);
      if (giftIndex < 0) return;
      const [gift] = this.pendingGuardianGifts.splice(giftIndex, 1);
      this.guardianGiftAnimation = {
        visible: true,
        guardianName: gift.guardianName,
        goalTitle: gift.goalTitle,
        destination: gift.destination ?? 'goal',
        amount: gift.amount,
        version: this.guardianGiftAnimation.version + 1,
      };
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-pending-gifts', JSON.stringify(this.pendingGuardianGifts));
      }
    },
    dismissGuardianGift() {
      this.guardianGiftAnimation.visible = false;
      setTimeout(() => this.revealNextGuardianGift(), 250);
    },
    inviteGuardian(name: string, email: string) {
      this.members.push({
        id: `guardian-${Date.now()}`,
        name,
        email,
        avatar: '🧑',
        color: '#7e8db8',
        role: 'guardian',
        weeklyStreak: 0,
        invitationPending: true,
      });
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-family-members', JSON.stringify(this.members));
      }
      this.notify(`Einladung an ${name} wurde im Prototyp vorgemerkt.`);
    },
    addPromotion(input: NewPromotion) {
      if (this.viewerRole !== 'guardian') return;
      const contribution = this.contributions.find((item) => item.id === input.contributionId);
      if (!contribution) return;
      this.promotions.push({
        id: `promotion-${Date.now()}`,
        ...input,
        title: `Nur heute: ${input.multiplier}-fache Ladirchen`,
        active: true,
      });
      this.notify(`Bonusaktion für „${contribution.title}“ wurde aktiviert.`);
    },
    addShopReward(input: NewShopReward) {
      if (this.viewerRole !== 'guardian') return;
      this.shopRewards.unshift({ id: `shop-reward-${Date.now()}`, ...input, status: 'available' });
      this.notify(`„${input.title}“ wurde in den Familien-Shop gestellt.`);
    },
    requestShopReward(id: string) {
      const reward = this.shopRewards.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !reward || reward.status !== 'available' || reward.quantity < 1 || !shopRewardIsAvailable(reward.availableUntil) || reward.price > this.availableBalance) return;
      if (!shopRedemptionIsOpen()) {
        this.notify('Einlösungen sind heute geschlossen. Morgen ist der Familien-Shop wieder bis 18:00 Uhr geöffnet.');
        return;
      }
      reward.status = 'requested';
      reward.requesterId = this.activeChildId;
      this.notify(`${reward.price} Ladirchen sind für „${reward.title}“ reserviert.`);
    },
    cancelShopRewardRequest(id: string) {
      const reward = this.shopRewards.find((item) => item.id === id && item.requesterId === this.activeChildId);
      if (!reward || reward.status !== 'requested') return;
      reward.status = 'available';
      reward.requesterId = undefined;
      this.notify('Die Reservierung wurde aufgehoben.');
    },
    decideShopReward(id: string, approved: boolean) {
      const reward = this.shopRewards.find((item) => item.id === id);
      if (this.viewerRole !== 'guardian' || !reward || reward.status !== 'requested') return;
      if (approved) {
        if (!shopRedemptionIsOpen()) {
          this.notify('Nach 18:00 Uhr können Belohnungen nicht mehr freigegeben werden.');
          return;
        }
        const requesterId = reward.requesterId;
        if (!requesterId) return;
        this.balances[requesterId] = this.balanceFor(requesterId) - reward.price;
        reward.quantity = Math.max(0, reward.quantity - 1);
        reward.status = reward.quantity > 0 ? 'available' : 'redeemed';
        reward.requesterId = undefined;
        this.notify(`„${reward.title}“ wurde freigegeben.`);
      } else {
        reward.status = 'available';
        reward.requesterId = undefined;
        this.notify('Die Anfrage wurde zurückgegeben.');
      }
    },
    playRewardAnimation(value: number) {
      this.rewardAnimation.value = value;
      this.rewardAnimation.version += 1;
      this.rewardAnimation.visible = true;
      window.setTimeout(() => {
        this.rewardAnimation.visible = false;
      }, 1450);
    },
    openFamilySetup() {
      this.familySetupOpen = true;
    },
    completeFamilySetup(members: FamilyMember[], pets: FamilyPet[]) {
      this.members = members;
      this.pets = pets;
      const activeChildStillExists = members.some(
        (member) => member.id === this.activeChildId && member.role === 'child',
      );
      if (!activeChildStillExists) {
        this.activeChildId = members.find((member) => member.role === 'child')?.id ?? this.activeChildId;
      }
      this.onboardingCompleted = true;
      this.familySetupOpen = false;
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-family-setup', 'completed');
        localStorage.setItem('ladirchen-family-members', JSON.stringify(members));
        localStorage.setItem('ladirchen-family-pets', JSON.stringify(pets));
      }
      this.notify('Eure Familie ist eingerichtet. Willkommen in eurer Familienwelt!');
    },
    saveToGoal(id: string, amount: number) {
      const goal = this.goals.find((item) => item.id === id);
      if (!goal) return;
      const remaining = Math.max(0, goal.target - goal.saved);
      const safeAmount = Math.max(0, Math.min(amount, this.availableBalance, remaining));
      if (safeAmount === 0) return;
      goal.saved += safeAmount;
      this.balances[this.activeChildId] = this.balance - safeAmount;
      this.notify(`${safeAmount} Ladirchen wurden dem Ziel zugeordnet.`);
    },
    withdrawFromGoal(id: string, amount: number) {
      const goal = this.goals.find((item) => item.id === id && item.ownerId === this.activeChildId);
      if (!goal) return;
      const safeAmount = Math.max(0, Math.min(amount, this.withdrawableGoalBalance(goal)));
      if (safeAmount === 0) return;
      goal.saved -= safeAmount;
      this.balances[this.activeChildId] = this.balance + safeAmount;
      this.notify(`${safeAmount} Ladirchen sind wieder frei für den Familien-Shop.`);
    },
    cancelGoal(id: string) {
      const goalIndex = this.goals.findIndex((item) => item.id === id && item.ownerId === this.activeChildId);
      if (goalIndex < 0) return;
      const goal = this.goals[goalIndex];
      const completed = goal.saved >= goal.target;
      const forfeitedBonus = completed ? 0 : Math.min(goal.starterBonus ?? 0, goal.saved);
      const returned = Math.max(0, goal.saved - forfeitedBonus);
      this.balances[this.activeChildId] = this.balance + returned;
      this.goals.splice(goalIndex, 1);
      if (this.activeGoalId === id) {
        this.activeGoalId = this.goals.find((item) => item.ownerId === this.activeChildId)?.id ?? this.goals[0]?.id ?? '';
      }
      this.notify(forfeitedBonus > 0
        ? `Sparziel aufgelöst: ${returned} L sind wieder frei, der 5-L-Startbonus ist verfallen.`
        : `Sparziel aufgelöst: ${returned} L sind wieder frei.`);
    },
    creditWeeklyInterestDemo() {
      let credited = 0;
      for (const goal of this.goals) {
        if (goal.ownerId === 'family' || goal.saved <= 0) continue;
        const remaining = Math.max(0, goal.target - goal.saved);
        const interest = Math.min(
          remaining,
          Math.max(1, Math.round(goal.saved * (this.savingsInterestRateFor(goal.ownerId) / 100))),
        );
        goal.saved += interest;
        goal.interestEarned = (goal.interestEarned ?? 0) + interest;
        credited += interest;
      }
      this.notify(`${credited} Ladirchen Wochenzinsen wurden auf die Sparpläne verteilt.`);
    },
    creditActiveChildInterestDemo(rate: number): number {
      if (this.viewerRole !== 'child') return 0;
      const safeRate = Math.max(0, Math.min(this.maxSavingsRatePercent, rate));
      let credited = 0;
      for (const goal of this.goals) {
        if (goal.ownerId !== this.activeChildId || goal.saved <= 0) continue;
        const remaining = Math.max(0, goal.target - goal.saved);
        const interest = Math.min(remaining, Math.max(1, Math.round(goal.saved * (safeRate / 100))));
        if (interest <= 0) continue;
        goal.saved += interest;
        goal.interestEarned = (goal.interestEarned ?? 0) + interest;
        credited += interest;
      }
      if (credited > 0) {
        this.notify(`${credited} Ladirchen Zinsen sind in deinen Sparplänen angekommen.`);
      } else {
        this.notify('Für eine Zinsauszahlung brauchst du Ladirchen in einem noch nicht erfüllten Sparplan.');
      }
      return credited;
    },
    purchaseAccessory(id: string) {
      const accessory = this.accessories.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !accessory || accessory.owned || accessory.price > this.availableBalance) return;
      accessory.owned = true;
      accessory.equipped = true;
      this.balances[this.activeChildId] = this.balance - accessory.price;
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-house-accessories', JSON.stringify(this.accessories));
      }
      this.notify(`${accessory.title} wurde zur Familienwelt hinzugefügt.`);
    },
    toggleAccessory(id: string) {
      const accessory = this.accessories.find((item) => item.id === id);
      if (!accessory?.owned) return;
      accessory.equipped = !accessory.equipped;
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem('ladirchen-house-accessories', JSON.stringify(this.accessories));
      }
    },
    setSimulatedEnergy(value: number | null) {
      if (this.viewerRole !== 'guardian') return;
      this.simulatedEnergy = value === null ? null : Math.max(0, Math.min(100, Math.round(value)));
    },
    completeWeekDemo(): boolean {
      if (!this.houseMeetsMinimumEnergy) {
        this.notify('Die Hausentwicklung wartet: Die gemeinsame Hausenergie muss mindestens 60 % erreichen.');
        return false;
      }
      this.completedWeeklyStreak += 1;
      this.currentWeekDays = 0;
      this.houseLevel = Math.min(4, this.houseLevel + 1);
      this.revealVersion += 1;
      const member = this.members.find((item) => item.id === this.activeChildId);
      if (member) member.weeklyStreak = this.completedWeeklyStreak;
      this.notify('Wochenserie geschafft: Eine neue Hausstufe wurde enthüllt!');
      return true;
    },
    failWeekDemo() {
      this.completedWeeklyStreak = 0;
      this.currentWeekDays = 0;
      this.houseLevel = Math.max(0, this.houseLevel - 1);
      this.revealVersion += 1;
      const member = this.members.find((item) => item.id === this.activeChildId);
      if (member) member.weeklyStreak = 0;
      this.notify('Das Haus ist eine Stufe zurückgegangen. Alle gekauften Dinge bleiben erhalten.');
    },
  },
});
