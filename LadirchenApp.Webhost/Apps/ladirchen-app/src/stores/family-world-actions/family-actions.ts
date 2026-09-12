import { createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyPet, GuardianAccessLevel } from '@/domain/family/types';
import { createDomainId } from '@/domain/shared/identifiers';
import type { FamilyId, FamilyMemberId } from '@/domain/shared/identifiers';
import { normalizeFamilyMembers } from '@/stores/family-world-state';
import { AUTH_STATE_KEY, createUuid } from './family-world-store-utils';
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from '../family-world-store-context';
import { familyMemberColorPalette } from '@/theme/color-palette';

export const familyActions = {
  refreshCurrentTime(this: FamilyWorldStoreContext) {
    this.currentTimeMilliseconds = Date.now();
  },
  switchSession(this: FamilyWorldStoreContext, memberId: FamilyMemberId, revealRewards = true) {
    const member = this.members.find((item) => item.id === memberId);
    if (!member) {return;}
    this.rewardAnimation.visible = false;
    this.signedInMemberId = member.id;
    this.viewerRole = member.role;
    if (member.role === 'child') {
      this.activeChildId = member.id;
      this.revealNextGuardianGift();
    }
    if (revealRewards && this.isAuthenticated) {
      this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), 50);
    }
    this.notify('notifications.session.switched', { name: member.name });
  },
  signOut(this: FamilyWorldStoreContext) {
    this.isAuthenticated = false;
    this.familySetupOpen = false;
    this.piggyBankOpen = false;
    this.rewardAnimation.visible = false;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, 'signed-out');
  },
  signInCurrentFamily(this: FamilyWorldStoreContext, memberId: FamilyMemberId): boolean {
    const member = this.members.find(item => item.id === memberId);
    if (!member) {return false;}
    this.switchSession(member.id, false);
    this.isAuthenticated = true;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, 'authenticated');
    this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), 50);
    return true;
  },
  async signInToFamily(this: FamilyWorldStoreContext, familyId: FamilyId, memberId: FamilyMemberId): Promise<boolean> {
    this.$familyWorld.familyContext.setActiveFamilyId(familyId);
    this.$reset();
    this.isAuthenticated = true;
    this.familyAggregatesHydrated = false;
    this.homeCustomizationHydrated = false;
    await this.hydrateFamilyAggregates();
    const member = this.members.find(item => item.id === memberId);
    if (!member) {
      this.isAuthenticated = false;
      this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, 'signed-out');
      return false;
    }
    this.signedInMemberId = member.id;
    this.viewerRole = member.role;
    if (member.role === 'child') {this.activeChildId = member.id;}
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, 'authenticated');
    this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), 50);
    return true;
  },
  createRegisteredFamily(this: FamilyWorldStoreContext, familyId: FamilyId, guardianId: FamilyMemberId, guardianName: string) {
    this.$familyWorld.familyContext.setActiveFamilyId(familyId);
    this.$reset();
    this.isAuthenticated = true;
    this.signedInMemberId = guardianId;
    this.viewerRole = 'guardian';
    this.members = [{
      id: guardianId,
      name: guardianName.trim(),
      avatar: '🧑',
      color: familyMemberColorPalette.defaultGuardian,
      role: 'guardian',
      guardianAccess: 'admin',
      participatesInWeeklyGoal: false,
      weeklyStreak: 0,
      appearance: createGuardianAvatarAppearance('adult'),
    }];
    this.pets = [];
    this.balances = {};
    this.contributions = [];
    this.goals = [];
    this.pendingGuardianGifts = [];
    this.onboardingCompleted = false;
    this.familySetupOpen = true;
    this.familyAggregatesHydrated = true;
    this.homeCustomizationHydrated = true;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, 'authenticated');
    this.persistFamilyProfile();
    this.persistContributions();
    this.persistSavings();
    this.persistRewardShop();
    this.persistFamilyProgression();
    this.persistHomeCustomization();
  },
  selectChildForGuardian(this: FamilyWorldStoreContext, memberId: FamilyMemberId) {
    if (this.viewerRole !== 'guardian') {return;}
    const child = this.members.find((member) => member.id === memberId && member.role === 'child');
    if (!child) {return;}
    this.activeChildId = child.id;
    this.notify('notifications.family.childSelected', { name: child.name });
  },
  setWeeklyGoalParticipation(this: FamilyWorldStoreContext, memberId: FamilyMemberId, participates: boolean) {
    const member = this.members.find((item) => item.id === memberId && item.role === 'guardian');
    const canEdit = this.permissions.canManageFamily || memberId === this.signedInMemberId;
    if (!member || !canEdit) {return;}
    member.participatesInWeeklyGoal = participates;
    this.persistFamilyProfile();
    this.notify(participates ? 'notifications.family.weeklyGoalJoined' : 'notifications.family.weeklyGoalPaused', { name: member.name });
  },
  saveOwnAppearance(this: FamilyWorldStoreContext, appearance: AvatarAppearance) {
    const member = this.members.find(item => item.id === this.signedInMemberId && item.role === this.viewerRole);
    if (!member || (member.role === 'child' && member.id !== this.activeChildId)) {return;}
    member.appearance = { ...appearance };
    this.persistFamilyProfile();
    this.notify('notifications.profile.saved');
  },
  setOwnNickname(this: FamilyWorldStoreContext, nickname: string) {
    if (this.viewerRole !== 'child' || this.signedInMemberId !== this.activeChildId) {return;}
    const member = this.members.find((item) => item.id === this.activeChildId && item.role === 'child');
    if (!member) {return;}
    member.nickname = nickname.trim().slice(0, 18) || undefined;
    this.persistFamilyProfile();
    this.notify(member.nickname ? 'notifications.profile.nicknameSaved' : 'notifications.profile.nicknameRemoved', member.nickname ? { nickname: member.nickname } : {});
  },
  inviteGuardian(this: FamilyWorldStoreContext, name: string, email: string, guardianAccess: GuardianAccessLevel = 'supporter') {
    if (!this.permissions.canInviteMembers) {return;}
    const normalizedName = name.toLocaleLowerCase('de');
    const preset = normalizedName.includes('oma') ? 'grandma' : normalizedName.includes('opa') ? 'grandpa' : 'adult';
    this.members.push({
      id: createDomainId.familyMember(createUuid()),
      name,
      email,
      avatar: '🧑',
      color: familyMemberColorPalette.defaultPet,
      role: 'guardian',
      guardianAccess,
      participatesInWeeklyGoal: false,
      weeklyStreak: 0,
      invitationPending: true,
      appearance: createGuardianAvatarAppearance(preset),
    });
    this.persistFamilyProfile();
    this.notify('notifications.family.invitationCreated', { name });
  },
  setGuardianAccess(this: FamilyWorldStoreContext, memberId: FamilyMemberId, guardianAccess: GuardianAccessLevel) {
    if (!this.permissions.canManageFamily || memberId === this.signedInMemberId) {return;}
    const member = this.members.find(item => item.id === memberId && item.role === 'guardian');
    if (!member) {return;}
    member.guardianAccess = guardianAccess;
    this.persistFamilyProfile();
    this.notify(guardianAccess === 'admin' ? 'notifications.family.accessAdmin' : 'notifications.family.accessSupporter', { name: member.name });
  },
  openFamilySetup(this: FamilyWorldStoreContext) {
    if (this.onboardingCompleted && !this.permissions.canManageFamily) {return;}
    this.familySetupOpen = true;
  },
  completeFamilySetup(this: FamilyWorldStoreContext, members: FamilyMember[], pets: FamilyPet[]) {
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
    this.persistFamilyProfile();
    this.notify('notifications.family.setupComplete');
  },
} satisfies FamilyWorldActionGroup;
