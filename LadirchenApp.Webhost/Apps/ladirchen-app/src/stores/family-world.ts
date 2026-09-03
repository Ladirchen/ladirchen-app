import { defineStore } from 'pinia';

import { createContributions, createFamilyMembers, createFamilyPets, createHouseAccessories, createHouseLayoutPlacements, createPromotions, createSavingGoals, createShopRewards, FAMILY_MEMBER_IDS, SAVING_GOAL_IDS } from '@/infrastructure/fixtures/family-world-fixtures';
import { FURNITURE_SETS, HOUSE_ROOMS, HOUSE_STAGES, HOUSE_THEMES } from '@/features/world/data/house-catalog';
import { createGuardianAvatarAppearance, normalizeAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import { calculateAverageEnergy, calculateContributionProgress, MINIMUM_HOUSE_ENERGY_PERCENT } from '@/domain/energy';
import type { FurnitureSetId, HouseStageLevel, HouseZoneId } from '@/domain/house';
import { resolveFamilyPermissions } from '@/domain/family-permissions';
import type { FamilyPermissions } from '@/domain/family-permissions';
import { isPromotionAvailable } from '@/domain/promotions';
import { familyParticipationInterestStrategy } from '@/domain/savings-interest';
import { shopRedemptionIsOpen } from '@/domain/shop';
import { createDomainId } from '@/domain/types';
import type { Contribution, ContributionId, FamilyCurrency, FamilyMember, FamilyMemberId, FamilyPet, FamilyPetKindId, GuardianAccessLevel, GuardianGiftId, HouseAccessory, HouseLayoutPlacement, HouseLayoutPlacementId, NewContribution, NewGoal, NewPromotion, NewShopReward, SavingGoal, SavingGoalId, ShopRewardId, SubscriptionTier, ViewerRole, WorldEffect } from '@/domain/types';
import { FAMILY_WORLD_STORAGE_KEYS, localFamilyWorldPersistence } from '@/infrastructure/local-family-world-storage';

const persistence = localFamilyWorldPersistence;

const onboardingWasCompleted = () =>
  persistence.readText(FAMILY_WORLD_STORAGE_KEYS.familySetup) === 'completed';

const supportedFamilyCurrencies: FamilyCurrency[] = ['CHF', 'EUR', 'HUF'];
const normalizeFamilyMembers = (members: FamilyMember[]): FamilyMember[] => {
  const guardians = members.filter(member => member.role === 'guardian');
  const hasAdministrator = guardians.some(member => member.guardianAccess === 'admin');
  return members.map((member) => {
    if (member.role !== 'guardian') {
      return member.appearance ? { ...member, appearance: normalizeAvatarAppearance(member.appearance) } : member;
    }
    const isFirstGuardian = member.id === guardians[0]?.id;
    const fallbackAppearance = createGuardianAvatarAppearance(isFirstGuardian ? 'adult' : 'grandpa');
    return {
      ...member,
      guardianAccess: member.guardianAccess ?? (!hasAdministrator && isFirstGuardian ? 'admin' : 'supporter'),
      appearance: normalizeAvatarAppearance(member.appearance, fallbackAppearance),
    };
  });
};
const loadFamilyMembers = (): FamilyMember[] => normalizeFamilyMembers(
  persistence.readList<FamilyMember>(FAMILY_WORLD_STORAGE_KEYS.familyMembers, createFamilyMembers),
);
const familyPetKinds: Record<string, { id: FamilyPetKindId; label: string }> = {
  bird: { id: 'bird', label: 'Vogel' },
  cat: { id: 'cat', label: 'Katze' },
  dog: { id: 'dog', label: 'Hund' },
  other: { id: 'other', label: 'Anderes Tier' },
  rabbit: { id: 'rabbit', label: 'Kaninchen' },
  Vogel: { id: 'bird', label: 'Vogel' },
  Katze: { id: 'cat', label: 'Katze' },
  Hund: { id: 'dog', label: 'Hund' },
  'Anderes Tier': { id: 'other', label: 'Anderes Tier' },
  Kaninchen: { id: 'rabbit', label: 'Kaninchen' },
};
const loadFamilyPets = (): FamilyPet[] => persistence
  .readList<FamilyPet>(FAMILY_WORLD_STORAGE_KEYS.familyPets, createFamilyPets)
  .map((pet) => {
    const kind = familyPetKinds[pet.kind] ?? familyPetKinds.other!;
    return { ...pet, kind: kind.id, kindLabel: pet.kindLabel ?? kind.label };
  });
const loadFamilyCurrency = (): FamilyCurrency => {
  const stored = persistence.readText(FAMILY_WORLD_STORAGE_KEYS.familyCurrency) as FamilyCurrency | null;
  return stored && supportedFamilyCurrencies.includes(stored) ? stored : 'CHF';
};
const loadLadirchenExchangeRate = (): number => {
  const stored = persistence.readNumber(FAMILY_WORLD_STORAGE_KEYS.exchangeRate, 10);
  return Number.isFinite(stored) && stored >= 1 ? stored : 10;
};

const loadHouseAccessories = (): HouseAccessory[] => {
  const catalog = createHouseAccessories();
  const stored = persistence.readList<HouseAccessory>(FAMILY_WORLD_STORAGE_KEYS.houseAccessories, () => []);
  const storedById = new Map(stored.map((accessory) => [accessory.id, accessory]));
  return catalog.map((accessory) => {
    const saved = storedById.get(accessory.id);
    return saved ? { ...accessory, owned: saved.owned, equipped: saved.equipped } : accessory;
  });
};

const loadHouseLayout = (): HouseLayoutPlacement[] => {
  const defaults = createHouseLayoutPlacements();
  const stored = persistence.readList<HouseLayoutPlacement>(FAMILY_WORLD_STORAGE_KEYS.houseLayout, () => []);
  const storedById = new Map(stored.map((placement) => [placement.id, placement]));
  return defaults.map((placement) => {
    const saved = storedById.get(placement.id);
    if (!saved) {return placement;}
    return {
      ...placement,
      zoneId: saved.zoneId,
      x: Math.max(4, Math.min(96, saved.x)),
      y: Math.max(8, Math.min(94, saved.y)),
      scale: Math.max(.5, Math.min(1.35, saved.scale)),
    } as HouseLayoutPlacement;
  });
};

const shopRewardIsAvailable = (availableUntil?: string) => {
  if (!availableUntil) {return true;}
  return new Date(`${availableUntil}T23:59:59`).getTime() >= Date.now();
};

interface PendingGuardianGift {
  id: GuardianGiftId;
  childId: FamilyMemberId;
  guardianName: string;
  goalTitle: string;
  destination: 'balance' | 'goal';
  amount: number;
}

export const useFamilyWorldStore = defineStore('ladirchenFamilyWorld', {
  state: () => ({
    signedInMemberId: FAMILY_MEMBER_IDS.laura,
    viewerRole: 'child' as ViewerRole,
    activeChildId: loadFamilyMembers()
      .find((member) => member.role === 'child')?.id ?? FAMILY_MEMBER_IDS.laura,
    activeGoalId: SAVING_GOAL_IDS.bike,
    onboardingCompleted: onboardingWasCompleted(),
    familySetupOpen: !onboardingWasCompleted(),
    balances: { [FAMILY_MEMBER_IDS.laura]: 340, [FAMILY_MEMBER_IDS.adam]: 220, [FAMILY_MEMBER_IDS.daniel]: 175 } as Record<FamilyMemberId, number>,
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
    pendingGuardianGifts: persistence.readList<PendingGuardianGift>(FAMILY_WORLD_STORAGE_KEYS.pendingGifts, () => []),
    completedWeeklyStreak: 0,
    currentWeekDays: 4,
    currentWeekTarget: 7,
    houseLevel: 0 as HouseStageLevel,
    houseThemeId: HOUSE_THEMES[0]?.id ?? 'sunny-dollhouse',
    subscriptionTier: 'pro' as SubscriptionTier,
    houseLayout: loadHouseLayout(),
    revealVersion: 0,
    snackbar: { visible: false, message: '' },
    members: loadFamilyMembers(),
    pets: loadFamilyPets(),
    contributions: createContributions(),
    goals: createSavingGoals(),
    accessories: loadHouseAccessories(),
    promotions: createPromotions(),
    shopRewards: createShopRewards(),
  }),

  getters: {
    activeChild(state): FamilyMember {
      return state.members.find((member) => member.id === state.activeChildId) ?? state.members[0] ?? createFamilyMembers()[0]!;
    },
    signedInMember(state): FamilyMember {
      return state.members.find((member) => member.id === state.signedInMemberId) ?? state.members[0] ?? createFamilyMembers()[0]!;
    },
    permissions(): FamilyPermissions {
      return resolveFamilyPermissions(this.signedInMember);
    },
    isFamilyAdmin(): boolean {
      return this.signedInMember.role === 'guardian' && this.signedInMember.guardianAccess === 'admin';
    },
    canArrangeHouse(state): boolean {
      return state.subscriptionTier === 'pro';
    },
    unlockedHouseRooms(state) {
      return HOUSE_ROOMS.filter((room) => room.minimumHouseLevel <= state.houseLevel);
    },
    ownedFurnitureSetIds(state): FurnitureSetId[] {
      return FURNITURE_SETS
        .filter((set) => set.accessoryIds.every((id) => state.accessories.find((accessory) => accessory.id === id)?.owned))
        .map((set) => set.id);
    },
    balanceFor: (state) => (memberId: FamilyMemberId): number => state.balances[memberId] ?? 0,
    displayNameFor: (state) => (memberId: FamilyMemberId): string => {
      const member = state.members.find((item) => item.id === memberId);
      return member?.nickname?.trim() || member?.name || 'Familienmitglied';
    },
    balance(): number {
      return this.balanceFor(this.activeChildId);
    },
    totalVisibleSavedFor: (state) => (memberId: FamilyMemberId): number => state.goals
      .filter((goal) => goal.ownerId === memberId && goal.visibility !== 'private')
      .reduce((sum, goal) => sum + goal.saved, 0),
    averageTaskRatingFor: (state) => (memberId: FamilyMemberId): number => {
      const rated = state.contributions.filter(
        (contribution) => contribution.assigneeId === memberId && contribution.stars,
      );
      return rated.length === 0
        ? 0
        : rated.reduce((sum, contribution) => sum + (contribution.stars ?? 0), 0) / rated.length;
    },
    pendingCountFor: (state) => (memberId: FamilyMemberId): number => state.contributions.filter(
      (contribution) => contribution.assigneeId === memberId && contribution.status === 'pending',
    ).length,
    openCountFor: (state) => (memberId: FamilyMemberId): number => state.contributions.filter(
      (contribution) => contribution.assigneeId === memberId && contribution.status === 'available',
    ).length,
    dailyBaseContributions(state): Contribution[] {
      return state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === state.activeChildId,
      );
    },
    dailyEnergy(state): number {
      if (state.simulatedEnergy !== null) {return state.simulatedEnergy;}
      const total = this.dailyBaseContributions.reduce((sum, contribution) => sum + contribution.energy, 0);
      if (total === 0) {return 60;}
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
    contributionProgress: (state) => (memberId: FamilyMemberId): number => {
      return calculateContributionProgress(state.contributions, memberId);
    },
    familyEnergy(): number {
      if (this.simulatedEnergy !== null) {return this.simulatedEnergy;}
      const children = this.members.filter((member) => member.role === 'child');
      return calculateAverageEnergy(children.map((child) => this.contributionProgress(child.id)));
    },
    houseMeetsMinimumEnergy(): boolean {
      return this.familyEnergy >= MINIMUM_HOUSE_ENERGY_PERCENT;
    },
    currentDailyStreak(): number {
      return this.completedWeeklyStreak * this.currentWeekTarget + this.currentWeekDays;
    },
    rewardForContribution: (state) => (contributionId: ContributionId): number => {
      const contribution = state.contributions.find((item) => item.id === contributionId);
      const promotion = state.promotions.find(
        (item) => item.contributionId === contributionId && isPromotionAvailable(item),
      );
      return (contribution?.reward ?? 0) * (promotion?.multiplier ?? 1) + (promotion?.teamworkBonus ?? 0);
    },
    activeGoal(state): SavingGoal {
      const visibleGoals = state.goals.filter(
        (goal) => {
          const permissions = resolveFamilyPermissions(state.members.find(member => member.id === state.signedInMemberId));
          return (goal.visibility === 'family' && (goal.ownerId !== 'family' || permissions.canViewFamilyGoals)) ||
          (state.viewerRole === 'child' && goal.ownerId === state.activeChildId) ||
          (state.viewerRole === 'guardian' && permissions.canViewGuardianGoals && goal.visibility === 'guardians');
        },
      );
      return visibleGoals.find((goal) => goal.id === state.activeGoalId) ??
        visibleGoals.find((goal) => goal.ownerId === state.activeChildId) ??
        visibleGoals[0] ??
        state.goals[0] ??
        createSavingGoals()[0]!;
    },
    ownSavingGoals(state): SavingGoal[] {
      return state.goals.filter(
        (goal) => goal.ownerId === state.activeChildId && (
          state.viewerRole === 'child' ||
          goal.visibility === 'family' ||
          (goal.visibility === 'guardians' && resolveFamilyPermissions(state.members.find(member => member.id === state.signedInMemberId)).canViewGuardianGoals)
        ),
      );
    },
    totalSaved(): number {
      return this.ownSavingGoals.reduce((sum, goal) => sum + goal.saved, 0);
    },
    totalInterestEarned(): number {
      return this.ownSavingGoals.reduce((sum, goal) => sum + (goal.interestEarned ?? 0), 0);
    },
    withdrawableGoalBalance: () => (goal?: SavingGoal): number => {
      if (!goal) {return 0;}
      if (goal.saved >= goal.target) {return goal.saved;}
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
    savingsInterestRateFor: (state) => (memberId: FamilyMemberId): number => {
      const baseContributions = state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === memberId,
      );
      const streak = memberId === state.activeChildId
        ? state.completedWeeklyStreak * state.currentWeekTarget + state.currentWeekDays
        : state.members.find((member) => member.id === memberId)?.weeklyStreak ?? 0;
      const ratedContributions = baseContributions.filter((contribution) => contribution.stars);
      const rating = ratedContributions.length === 0
        ? 0
        : ratedContributions.reduce((sum, contribution) => sum + (contribution.stars ?? 0), 0)
          / ratedContributions.length;
      return familyParticipationInterestStrategy.calculateRate({
        baseRate: state.baseSavingsRatePercent,
        completionBonusRate: state.completionBonusRate,
        completionPercent: calculateContributionProgress(state.contributions, memberId),
        maxRate: state.maxSavingsRatePercent,
        rating,
        ratingBonusRate: state.ratingBonusRate,
        streakBonusRate: state.streakBonusRate,
        streakDays: streak,
      });
    },
    savingsInterestRate(): number {
      return this.savingsInterestRateFor(this.activeChildId);
    },
    activeWorldEffects(state): WorldEffect[] {
      return state.contributions
        .filter((contribution): contribution is Contribution & { worldEffect: WorldEffect } => contribution.status === 'approved' && Boolean(contribution.worldEffect))
        .map((contribution) => contribution.worldEffect);
    },
  },

  actions: {
    notify(message: string) {
      this.snackbar.message = message;
      this.snackbar.visible = true;
    },
    setFamilyCurrency(currency: FamilyCurrency) {
      if (!this.permissions.canManageFamily) {return;}
      if (!supportedFamilyCurrencies.includes(currency)) {return;}
      this.familyCurrencyCode = currency;
      persistence.writeText(FAMILY_WORLD_STORAGE_KEYS.familyCurrency, currency);
    },
    setLadirchenExchangeRate(rate: number) {
      if (!this.permissions.canManageFamily) {return;}
      if (!Number.isFinite(rate) || rate < 1) {return;}
      this.ladirchenPerCurrencyUnit = Math.round(rate);
      persistence.writeText(FAMILY_WORLD_STORAGE_KEYS.exchangeRate, String(this.ladirchenPerCurrencyUnit));
    },
    switchSession(memberId: FamilyMemberId) {
      const member = this.members.find((item) => item.id === memberId);
      if (!member) {return;}
      this.signedInMemberId = member.id;
      this.viewerRole = member.role;
      if (member.role === 'child') {
        this.activeChildId = member.id;
        this.revealNextGuardianGift();
      }
      this.notify(`Prototyp-Sitzung: Angemeldet als ${member.name}.`);
    },
    selectChildForGuardian(memberId: FamilyMemberId) {
      if (this.viewerRole !== 'guardian') {return;}
      const child = this.members.find((member) => member.id === memberId && member.role === 'child');
      if (!child) {return;}
      this.activeChildId = child.id;
      this.notify(`${child.name} ist für die Detailansichten ausgewählt.`);
    },
    saveOwnAppearance(appearance: AvatarAppearance) {
      const member = this.members.find(item => item.id === this.signedInMemberId && item.role === this.viewerRole);
      if (!member || (member.role === 'child' && member.id !== this.activeChildId)) {return;}
      member.appearance = { ...appearance };
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.familyMembers, this.members);
      this.notify('Dein neues Profil wurde gespeichert.');
    },
    setOwnNickname(nickname: string) {
      if (this.viewerRole !== 'child' || this.signedInMemberId !== this.activeChildId) {return;}
      const member = this.members.find((item) => item.id === this.activeChildId && item.role === 'child');
      if (!member) {return;}
      member.nickname = nickname.trim().slice(0, 18) || undefined;
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.familyMembers, this.members);
      this.notify(member.nickname ? `Dein Spitzname ist jetzt ${member.nickname}.` : 'Dein Spitzname wurde entfernt.');
    },
    submitContribution(id: ContributionId) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !contribution || contribution.assigneeId !== this.activeChildId || contribution.status !== 'available') {return;}
      contribution.status = 'pending';
      this.notify('Beitrag eingereicht. Die Welt reagiert nach der Bestätigung.');
    },
    claimContribution(id: ContributionId) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !contribution || contribution.assigneeId || contribution.status !== 'available') {return;}
      contribution.assigneeId = this.activeChildId;
      this.notify(`„${contribution.title}“ gehört jetzt zu deinen Beiträgen.`);
    },
    assignContribution(id: ContributionId, childId?: FamilyMemberId) {
      if (!this.permissions.canManageContent) {return;}
      const contribution = this.contributions.find((item) => item.id === id);
      if (!contribution || contribution.status !== 'available') {return;}
      if (childId && !this.members.some((member) => member.id === childId && member.role === 'child')) {return;}
      contribution.assigneeId = childId || undefined;
      contribution.invitedChildIds = [];
      const childName = childId ? this.members.find((member) => member.id === childId)?.name : undefined;
      this.notify(childName ? `„${contribution.title}“ wurde ${childName} zugewiesen.` : `„${contribution.title}“ ist jetzt für alle Kinder offen.`);
    },
    approveContribution(id: ContributionId, stars: number) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (!this.permissions.canManageContent || !contribution?.assigneeId || contribution.status !== 'pending') {return;}
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
    returnContribution(id: ContributionId) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (!this.permissions.canManageContent || !contribution || contribution.status !== 'pending') {return;}
      contribution.status = 'available';
      this.notify('Der Beitrag wurde mit der Bitte um Nachbesserung zurückgegeben.');
    },
    addContribution(input: NewContribution) {
      if (!this.permissions.canManageContent) {return;}
      this.contributions.push({
        id: createDomainId.contribution(`contribution-${Date.now()}`),
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
    setContributionPartners(id: ContributionId, childIds: FamilyMemberId[]) {
      const contribution = this.contributions.find((item) => item.id === id);
      if (!contribution || contribution.kind !== 'extra') {return;}
      const allowedIds = new Set(this.members
        .filter((member) => member.role === 'child' && member.id !== contribution.assigneeId)
        .map((member) => member.id));
      contribution.invitedChildIds = childIds.filter((childId) => allowedIds.has(childId));
      const names = this.members
        .filter((member) => contribution.invitedChildIds?.includes(member.id))
        .map((member) => member.name);
      this.notify(names.length > 0 ? `${names.join(' und ')} wurden zur Spezialaufgabe eingeladen.` : 'Die Einladungen wurden entfernt.');
    },
    toggleCheer(id: SavingGoalId) {
      const goal = this.goals.find((item) => item.id === id);
      if (!goal) {return;}
      if (this.viewerRole === 'guardian') {
        const canViewGoal = (goal.visibility === 'family' && (goal.ownerId !== 'family' || this.permissions.canViewFamilyGoals)) ||
          (goal.visibility === 'guardians' && this.permissions.canViewGuardianGoals);
        if (!this.permissions.canSupportChildGoals || !canViewGoal) {return;}
      }
      goal.cheered = !goal.cheered;
      this.notify(goal.cheered ? 'Deine Unterstützung ist für die Familie sichtbar.' : 'Unterstützung zurückgenommen.');
    },
    addGoal(input: NewGoal) {
      if (this.viewerRole === 'guardian' && !this.permissions.canManageGoals) {return;}
      const id = createDomainId.savingGoal(`goal-${Date.now()}`);
      this.goals.push({ id, ...input, ownerId: this.activeChildId, saved: 5, starterBonus: 5, shared: false, cheered: false });
      this.activeGoalId = id;
      this.notify('Neues Sparziel angelegt: 5 L geschützter Startbonus wurden gutgeschrieben.');
    },
    updateGoal(id: SavingGoalId, input: NewGoal) {
      if (this.viewerRole === 'guardian' && !this.permissions.canManageGoals) {return;}
      const goal = this.goals.find((item) => item.id === id && item.ownerId === this.activeChildId);
      if (!goal) {return;}
      goal.title = input.title;
      goal.icon = input.icon;
      goal.target = Math.max(goal.saved, input.target);
      goal.visibility = input.visibility;
      this.notify('Sparziel wurde aktualisiert.');
    },
    supportGoal(id: SavingGoalId, amount: number) {
      if (!this.permissions.canSupportChildGoals) {return;}
      const goal = this.goals.find((item) =>
        item.id === id &&
        item.ownerId !== 'family' &&
        item.visibility !== 'private' &&
        (this.permissions.canViewGuardianGoals || item.visibility === 'family'),
      );
      if (!goal || goal.ownerId === 'family') {return;}
      const safeAmount = Math.max(0, Math.min(Math.round(amount), goal.target - goal.saved));
      if (safeAmount === 0) {return;}
      goal.saved += safeAmount;
      const childName = this.members.find((member) => member.id === goal.ownerId)?.name ?? 'das Kind';
      const guardianName = this.signedInMember.name.replace(' (du)', '');
      this.pendingGuardianGifts.push({
        id: createDomainId.guardianGift(`gift-${Date.now()}`),
        childId: goal.ownerId,
        guardianName,
        goalTitle: goal.title,
        destination: 'goal',
        amount: safeAmount,
      });
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.pendingGifts, this.pendingGuardianGifts);
      this.notify(`${safeAmount} Ladirchen wurden ${childName}s Sparziel geschenkt.`);
    },
    giftLadirchenToGoal(id: SavingGoalId, amount: number) {
      if (this.viewerRole !== 'child') {return;}
      const goal = this.goals.find((item) =>
        item.id === id &&
        item.ownerId !== this.activeChildId &&
        item.ownerId !== 'family' &&
        item.visibility === 'family',
      );
      const recipient = goal
        ? this.members.find((member) => member.id === goal.ownerId && member.role === 'child')
        : undefined;
      if (!goal || !recipient) {return;}
      const safeAmount = Math.max(0, Math.min(
        Math.round(amount),
        this.availableBalance,
        goal.target - goal.saved,
      ));
      if (safeAmount === 0) {return;}
      this.balances[this.activeChildId] = this.balance - safeAmount;
      goal.saved += safeAmount;
      this.pendingGuardianGifts.push({
        id: createDomainId.guardianGift(`gift-${Date.now()}`),
        childId: recipient.id,
        guardianName: this.signedInMember.name,
        goalTitle: goal.title,
        destination: 'goal',
        amount: safeAmount,
      });
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.pendingGifts, this.pendingGuardianGifts);
      this.notify(`${safeAmount} Ladirchen wurden ${recipient.name}s Sparziel geschenkt.`);
    },
    giftLadirchenToChild(childId: FamilyMemberId, amount: number, reason: string) {
      if (!this.permissions.canManageContent) {return;}
      const child = this.members.find((member) => member.id === childId && member.role === 'child');
      const safeAmount = Math.max(1, Math.min(10_000, Math.round(amount)));
      const safeReason = reason.trim();
      if (!child || !safeReason) {return;}
      this.balances[child.id] = this.balanceFor(child.id) + safeAmount;
      this.pendingGuardianGifts.push({
        id: createDomainId.guardianGift(`gift-${Date.now()}`),
        childId: child.id,
        guardianName: this.signedInMember.name.replace(' (du)', ''),
        goalTitle: safeReason,
        destination: 'balance',
        amount: safeAmount,
      });
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.pendingGifts, this.pendingGuardianGifts);
      this.notify(`${safeAmount} Ladirchen wurden ${child.name} als besonderes Geschenk gutgeschrieben.`);
    },
    revealNextGuardianGift() {
      if (this.viewerRole !== 'child' || this.guardianGiftAnimation.visible) {return;}
      const giftIndex = this.pendingGuardianGifts.findIndex((gift) => gift.childId === this.activeChildId);
      if (giftIndex < 0) {return;}
      const [gift] = this.pendingGuardianGifts.splice(giftIndex, 1);
      if (!gift) {return;}
      this.guardianGiftAnimation = {
        visible: true,
        guardianName: gift.guardianName,
        goalTitle: gift.goalTitle,
        destination: gift.destination ?? 'goal',
        amount: gift.amount,
        version: this.guardianGiftAnimation.version + 1,
      };
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.pendingGifts, this.pendingGuardianGifts);
    },
    dismissGuardianGift() {
      this.guardianGiftAnimation.visible = false;
      setTimeout(() => this.revealNextGuardianGift(), 250);
    },
    inviteGuardian(name: string, email: string, guardianAccess: GuardianAccessLevel = 'supporter') {
      if (!this.permissions.canInviteMembers) {return;}
      const normalizedName = name.toLocaleLowerCase('de');
      const preset = normalizedName.includes('oma') ? 'grandma' : normalizedName.includes('opa') ? 'grandpa' : 'adult';
      this.members.push({
        id: createDomainId.familyMember(`guardian-${Date.now()}`),
        name,
        email,
        avatar: '🧑',
        color: '#7e8db8',
        role: 'guardian',
        guardianAccess,
        weeklyStreak: 0,
        invitationPending: true,
        appearance: createGuardianAvatarAppearance(preset),
      });
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.familyMembers, this.members);
      this.notify(`Einladung an ${name} wurde im Prototyp vorgemerkt.`);
    },
    setGuardianAccess(memberId: FamilyMemberId, guardianAccess: GuardianAccessLevel) {
      if (!this.permissions.canManageFamily || memberId === this.signedInMemberId) {return;}
      const member = this.members.find(item => item.id === memberId && item.role === 'guardian');
      if (!member) {return;}
      member.guardianAccess = guardianAccess;
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.familyMembers, this.members);
      this.notify(`${member.name} hat jetzt die Berechtigung „${guardianAccess === 'admin' ? 'Administration' : 'Zielbegleitung'}“.`);
    },
    addPromotion(input: NewPromotion) {
      if (!this.permissions.canManageContent) {return;}
      const contribution = this.contributions.find((item) => item.id === input.contributionId);
      if (!contribution) {return;}
      this.promotions.push({
        id: createDomainId.promotion(`promotion-${Date.now()}`),
        ...input,
        title: `Nur heute: ${input.multiplier}-fache Ladirchen`,
        active: true,
      });
      this.notify(`Bonusaktion für „${contribution.title}“ wurde aktiviert.`);
    },
    addShopReward(input: NewShopReward) {
      if (!this.permissions.canManageContent) {return;}
      this.shopRewards.unshift({ id: createDomainId.shopReward(`shop-reward-${Date.now()}`), ...input, status: 'available' });
      this.notify(`„${input.title}“ wurde in den Familien-Shop gestellt.`);
    },
    requestShopReward(id: ShopRewardId) {
      const reward = this.shopRewards.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !reward || reward.status !== 'available' || reward.quantity < 1 || !shopRewardIsAvailable(reward.availableUntil) || reward.price > this.availableBalance) {return;}
      if (!shopRedemptionIsOpen()) {
        this.notify('Einlösungen sind heute geschlossen. Morgen ist der Familien-Shop wieder bis 18:00 Uhr geöffnet.');
        return;
      }
      reward.status = 'requested';
      reward.requesterId = this.activeChildId;
      this.notify(`${reward.price} Ladirchen sind für „${reward.title}“ reserviert.`);
    },
    cancelShopRewardRequest(id: ShopRewardId) {
      const reward = this.shopRewards.find((item) => item.id === id && item.requesterId === this.activeChildId);
      if (!reward || reward.status !== 'requested') {return;}
      reward.status = 'available';
      reward.requesterId = undefined;
      this.notify('Die Reservierung wurde aufgehoben.');
    },
    decideShopReward(id: ShopRewardId, approved: boolean) {
      const reward = this.shopRewards.find((item) => item.id === id);
      if (!this.permissions.canManageContent || !reward || reward.status !== 'requested') {return;}
      if (approved) {
        if (!shopRedemptionIsOpen()) {
          this.notify('Nach 18:00 Uhr können Belohnungen nicht mehr freigegeben werden.');
          return;
        }
        const requesterId = reward.requesterId;
        if (!requesterId) {return;}
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
      if (this.onboardingCompleted && !this.permissions.canManageFamily) {return;}
      this.familySetupOpen = true;
    },
    completeFamilySetup(members: FamilyMember[], pets: FamilyPet[]) {
      const normalizedMembers = normalizeFamilyMembers(members);
      this.members = normalizedMembers;
      this.pets = pets;
      const activeChildStillExists = normalizedMembers.some(
        (member) => member.id === this.activeChildId && member.role === 'child',
      );
      if (!activeChildStillExists) {
        this.activeChildId = normalizedMembers.find((member) => member.role === 'child')?.id ?? this.activeChildId;
      }
      this.onboardingCompleted = true;
      this.familySetupOpen = false;
      persistence.writeText(FAMILY_WORLD_STORAGE_KEYS.familySetup, 'completed');
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.familyMembers, normalizedMembers);
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.familyPets, pets);
      this.notify('Eure Familie ist eingerichtet. Willkommen in eurer Familienwelt!');
    },
    saveToGoal(id: SavingGoalId, amount: number) {
      const goal = this.goals.find((item) => item.id === id);
      if (!goal) {return;}
      const remaining = Math.max(0, goal.target - goal.saved);
      const safeAmount = Math.max(0, Math.min(amount, this.availableBalance, remaining));
      if (safeAmount === 0) {return;}
      goal.saved += safeAmount;
      this.balances[this.activeChildId] = this.balance - safeAmount;
      this.notify(`${safeAmount} Ladirchen wurden dem Ziel zugeordnet.`);
    },
    withdrawFromGoal(id: SavingGoalId, amount: number) {
      const goal = this.goals.find((item) => item.id === id && item.ownerId === this.activeChildId);
      if (!goal) {return;}
      const safeAmount = Math.max(0, Math.min(amount, this.withdrawableGoalBalance(goal)));
      if (safeAmount === 0) {return;}
      goal.saved -= safeAmount;
      this.balances[this.activeChildId] = this.balance + safeAmount;
      this.notify(`${safeAmount} Ladirchen sind wieder frei für den Familien-Shop.`);
    },
    cancelGoal(id: SavingGoalId) {
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
        this.activeGoalId = this.goals.find((item) => item.ownerId === this.activeChildId)?.id ?? this.goals[0]?.id ?? SAVING_GOAL_IDS.bike;
      }
      this.notify(forfeitedBonus > 0
        ? `Sparziel aufgelöst: ${returned} L sind wieder frei, der 5-L-Startbonus ist verfallen.`
        : `Sparziel aufgelöst: ${returned} L sind wieder frei.`);
    },
    creditWeeklyInterestDemo() {
      let credited = 0;
      for (const goal of this.goals) {
        if (goal.ownerId === 'family' || goal.saved <= 0) {continue;}
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
      if (this.viewerRole !== 'child') {return 0;}
      const safeRate = Math.max(0, Math.min(this.maxSavingsRatePercent, rate));
      let credited = 0;
      for (const goal of this.goals) {
        if (goal.ownerId !== this.activeChildId || goal.saved <= 0) {continue;}
        const remaining = Math.max(0, goal.target - goal.saved);
        const interest = Math.min(remaining, Math.max(1, Math.round(goal.saved * (safeRate / 100))));
        if (interest <= 0) {continue;}
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
    purchaseAccessory(id: HouseAccessory['id']) {
      const accessory = this.accessories.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !accessory || accessory.owned || accessory.price > this.availableBalance) {return;}
      accessory.owned = true;
      accessory.equipped = true;
      this.balances[this.activeChildId] = this.balance - accessory.price;
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.houseAccessories, this.accessories);
      this.notify(`${accessory.title} wurde zur Familienwelt hinzugefügt.`);
    },
    purchaseFurnitureSet(id: FurnitureSetId) {
      const set = FURNITURE_SETS.find((item) => item.id === id);
      if (this.viewerRole !== 'child' || !set || set.minimumHouseLevel > this.houseLevel || this.ownedFurnitureSetIds.includes(id) || set.price > this.availableBalance) {return;}
      for (const accessory of this.accessories) {
        if (!set.accessoryIds.includes(accessory.id)) {continue;}
        accessory.owned = true;
        accessory.equipped = true;
      }
      this.balances[this.activeChildId] = this.balance - set.price;
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.houseAccessories, this.accessories);
      this.notify(`${set.name} wurde vollständig zur Familienwelt hinzugefügt.`);
    },
    toggleAccessory(id: HouseAccessory['id']) {
      const accessory = this.accessories.find((item) => item.id === id);
      if (!accessory?.owned) {return;}
      accessory.equipped = !accessory.equipped;
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.houseAccessories, this.accessories);
    },
    moveHouseEntity(placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number) {
      if (!this.canArrangeHouse) {return;}
      const room = HOUSE_ROOMS.find((item) => item.id === zoneId);
      if (room && room.minimumHouseLevel > this.houseLevel) {return;}
      const placement = this.houseLayout.find((item) => item.id === placementId);
      if (!placement) {return;}
      placement.zoneId = zoneId;
      placement.x = Math.max(4, Math.min(96, Number(x.toFixed(2))));
      placement.y = Math.max(8, Math.min(94, Number(y.toFixed(2))));
      persistence.writeList(FAMILY_WORLD_STORAGE_KEYS.houseLayout, this.houseLayout);
    },
    setSimulatedEnergy(value: number | null) {
      if (!this.permissions.canManageContent) {return;}
      this.simulatedEnergy = value === null ? null : Math.max(0, Math.min(100, Math.round(value)));
    },
    completeWeekDemo(): boolean {
      if (!this.permissions.canManageContent) {return false;}
      if (!this.houseMeetsMinimumEnergy) {
        this.notify('Die Hausentwicklung wartet: Die gemeinsame Hausenergie muss mindestens 60 % erreichen.');
        return false;
      }
      this.completedWeeklyStreak += 1;
      this.currentWeekDays = 0;
      this.houseLevel = HOUSE_STAGES[Math.min(HOUSE_STAGES.length - 1, this.houseLevel + 1)]!.level;
      this.revealVersion += 1;
      const member = this.members.find((item) => item.id === this.activeChildId);
      if (member) {member.weeklyStreak = this.completedWeeklyStreak;}
      this.notify('Wochenserie geschafft: Eine neue Hausstufe wurde enthüllt!');
      return true;
    },
    failWeekDemo() {
      if (!this.permissions.canManageContent) {return;}
      this.completedWeeklyStreak = 0;
      this.currentWeekDays = 0;
      this.houseLevel = HOUSE_STAGES[Math.max(0, this.houseLevel - 1)]!.level;
      this.revealVersion += 1;
      const member = this.members.find((item) => item.id === this.activeChildId);
      if (member) {member.weeklyStreak = 0;}
      this.notify('Das Haus ist eine Stufe zurückgegangen. Alle gekauften Dinge bleiben erhalten.');
    },
  },
});
