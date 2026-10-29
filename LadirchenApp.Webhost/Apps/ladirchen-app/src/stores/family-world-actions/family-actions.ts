import { createGuardianAvatarAppearance } from "@/domain/avatar";
import type { AvatarAppearance } from "@/domain/avatar";
import type { FamilyMember, FamilyPet, GuardianAccessLevel } from "@/domain/family/types";
import { FAMILY_PROFILE_RULES } from "@/domain/family/profile-rules";
import { createDomainId } from "@/domain/shared/identifiers";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import { normalizeFamilyMembers } from "@/stores/family-world-state";
import { AUTH_STATE_KEY, createUuid } from "./family-world-store-utils";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { FOLLOW_UP_REWARD_REVEAL_DELAY_MS } from "@/shared/runtime-timing";
import { familyMemberColorPalette } from "@/theme/color-palette";
import { executeBackendFamilyCommand } from "./backend-family-command";

export const familyActions = {
  refreshCurrentTime(this: FamilyWorldStoreContext) {
    this.currentTimeMilliseconds = Date.now();
  },
  switchSession(this: FamilyWorldStoreContext, memberId: FamilyMemberId, revealRewards = true) {
    const member = this.members.find((item) => item.id === memberId);

    if (!member) {
      return;
    }

    this.rewardAnimation.visible = false;
    this.signedInMemberId = member.id;
    this.viewerRole = member.role;

    if (member.role === "child") {
      this.activeChildId = member.id;
      this.revealNextGuardianGift();
    }

    if (revealRewards && this.isAuthenticated) {
      this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), FOLLOW_UP_REWARD_REVEAL_DELAY_MS);
    }

    this.notify("notifications.session.switched", { name: member.name });
  },
  signOut(this: FamilyWorldStoreContext) {
    this.isAuthenticated = false;
    this.familySetupOpen = false;
    this.piggyBankOpen = false;
    this.rewardAnimation.visible = false;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "signed-out");
  },
  signInCurrentFamily(this: FamilyWorldStoreContext, memberId: FamilyMemberId): boolean {
    const member = this.members.find((item) => item.id === memberId);

    if (!member) {
      return false;
    }

    this.switchSession(member.id, false);
    this.isAuthenticated = true;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "authenticated");
    this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), FOLLOW_UP_REWARD_REVEAL_DELAY_MS);

    return true;
  },
  async signInToFamily(this: FamilyWorldStoreContext, familyId: FamilyId, memberId: FamilyMemberId): Promise<boolean> {
    this.$familyWorld.familyContext.setActiveFamilyId(familyId);
    this.$reset();
    this.isAuthenticated = true;
    this.familyAggregatesHydrated = false;
    this.homeCustomizationHydrated = false;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "authenticated");
    await this.hydrateFamilyAggregates();
    const member = this.members.find((item) => item.id === memberId);

    if (!member) {
      this.isAuthenticated = false;
      this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "signed-out");

      return false;
    }

    this.signedInMemberId = member.id;
    this.viewerRole = member.role;

    if (member.role === "child") {
      this.activeChildId = member.id;
    }

    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "authenticated");
    this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), FOLLOW_UP_REWARD_REVEAL_DELAY_MS);

    return true;
  },
  createRegisteredFamily(
    this: FamilyWorldStoreContext,
    familyId: FamilyId,
    guardianId: FamilyMemberId,
    guardianName: string,
  ) {
    this.$familyWorld.familyContext.setActiveFamilyId(familyId);
    this.$reset();
    this.isAuthenticated = true;
    this.signedInMemberId = guardianId;
    this.viewerRole = "guardian";
    this.members = [
      {
        id: guardianId,
        name: guardianName.trim(),
        avatar: "🧑",
        color: familyMemberColorPalette.defaultGuardian,
        role: "guardian",
        guardianAccess: "admin",
        participatesInWeeklyGoal: false,
        weeklyStreak: 0,
        appearance: createGuardianAvatarAppearance("adult"),
      },
    ];
    this.pets = [];
    this.balances = {};
    this.contributions = [];
    this.goals = [];
    this.pendingGuardianGifts = [];
    this.onboardingCompleted = false;
    this.familySetupOpen = true;
    this.familyAggregatesHydrated = true;
    this.homeCustomizationHydrated = true;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "authenticated");
    this.persistFamilyProfile();
    this.persistContributions();
    this.persistSavings();
    this.persistRewardShop();
    this.persistFamilyProgression();
    this.persistHomeCustomization();
  },
  selectChildForGuardian(this: FamilyWorldStoreContext, memberId: FamilyMemberId) {
    if (this.viewerRole !== "guardian") {
      return;
    }

    const child = this.members.find((member) => member.id === memberId && member.role === "child");

    if (!child) {
      return;
    }

    this.activeChildId = child.id;
    this.notify("notifications.family.childSelected", { name: child.name });
  },
  async setWeeklyGoalParticipation(this: FamilyWorldStoreContext, memberId: FamilyMemberId, participates: boolean) {
    const memberName = this.members.find((item) => item.id === memberId)?.name ?? "";

    if (
      await executeBackendFamilyCommand(
        this,
        "profile.set-weekly-participation",
        { memberId, participates },
        "notifications.save.profile",
        () =>
          this.notify(
            participates ? "notifications.family.weeklyGoalJoined" : "notifications.family.weeklyGoalPaused",
            { name: memberName },
          ),
      )
    ) {
      return;
    }

    const member = this.members.find((item) => item.id === memberId && item.role === "guardian");
    const canEdit = this.permissions.canManageFamily || memberId === this.signedInMemberId;

    if (!member || !canEdit) {
      return;
    }

    member.participatesInWeeklyGoal = participates;
    this.persistFamilyProfile();
    this.notify(participates ? "notifications.family.weeklyGoalJoined" : "notifications.family.weeklyGoalPaused", {
      name: member.name,
    });
  },
  async saveOwnAppearance(this: FamilyWorldStoreContext, appearance: AvatarAppearance) {
    if (
      await executeBackendFamilyCommand(
        this,
        "profile.save-own-appearance",
        { appearance },
        "notifications.save.profile",
        () => this.notify("notifications.profile.saved"),
      )
    ) {
      return;
    }

    const member = this.members.find((item) => item.id === this.signedInMemberId && item.role === this.viewerRole);

    if (!member || (member.role === "child" && member.id !== this.activeChildId)) {
      return;
    }

    member.appearance = { ...appearance };
    this.persistFamilyProfile();
    this.notify("notifications.profile.saved");
  },
  async setOwnNickname(this: FamilyWorldStoreContext, nickname: string) {
    const normalizedNickname = nickname.trim().slice(0, FAMILY_PROFILE_RULES.nicknameMaximumLength);

    if (
      await executeBackendFamilyCommand(
        this,
        "profile.set-own-nickname",
        { nickname: normalizedNickname },
        "notifications.save.profile",
        () =>
          this.notify(
            normalizedNickname ? "notifications.profile.nicknameSaved" : "notifications.profile.nicknameRemoved",
            normalizedNickname ? { nickname: normalizedNickname } : {},
          ),
      )
    ) {
      return;
    }

    if (this.viewerRole !== "child" || this.signedInMemberId !== this.activeChildId) {
      return;
    }

    const member = this.members.find((item) => item.id === this.activeChildId && item.role === "child");

    if (!member) {
      return;
    }

    member.nickname = nickname.trim().slice(0, FAMILY_PROFILE_RULES.nicknameMaximumLength) || undefined;
    this.persistFamilyProfile();
    this.notify(
      member.nickname ? "notifications.profile.nicknameSaved" : "notifications.profile.nicknameRemoved",
      member.nickname ? { nickname: member.nickname } : {},
    );
  },
  async inviteGuardian(
    this: FamilyWorldStoreContext,
    name: string,
    email: string,
    guardianAccess: GuardianAccessLevel = "supporter",
  ) {
    const id = createDomainId.familyMember(createUuid());

    if (
      await executeBackendFamilyCommand(
        this,
        "profile.invite-guardian",
        { email, guardianAccess, id, name },
        "notifications.save.profile",
        () => this.notify("notifications.family.invitationCreated", { name }),
      )
    ) {
      return;
    }

    if (!this.permissions.canInviteMembers) {
      return;
    }

    const normalizedName = name.toLocaleLowerCase("de");
    let preset: "adult" | "grandma" | "grandpa" = "adult";

    if (normalizedName.includes("oma")) {
      preset = "grandma";
    } else if (normalizedName.includes("opa")) {
      preset = "grandpa";
    }

    this.members.push({
      id,
      name,
      email,
      avatar: "🧑",
      color: familyMemberColorPalette.defaultPet,
      role: "guardian",
      guardianAccess,
      participatesInWeeklyGoal: false,
      weeklyStreak: 0,
      invitationPending: true,
      appearance: createGuardianAvatarAppearance(preset),
    });
    this.persistFamilyProfile();
    this.notify("notifications.family.invitationCreated", { name });
  },
  async setGuardianAccess(
    this: FamilyWorldStoreContext,
    memberId: FamilyMemberId,
    guardianAccess: GuardianAccessLevel,
  ) {
    const memberName = this.members.find((item) => item.id === memberId)?.name ?? "";

    if (
      await executeBackendFamilyCommand(
        this,
        "profile.set-guardian-access",
        { guardianAccess, memberId },
        "notifications.save.profile",
        () =>
          this.notify(
            guardianAccess === "admin" ? "notifications.family.accessAdmin" : "notifications.family.accessSupporter",
            { name: memberName },
          ),
      )
    ) {
      return;
    }

    if (!this.permissions.canManageFamily || memberId === this.signedInMemberId) {
      return;
    }

    const member = this.members.find((item) => item.id === memberId && item.role === "guardian");

    if (!member) {
      return;
    }

    member.guardianAccess = guardianAccess;
    this.persistFamilyProfile();
    this.notify(
      guardianAccess === "admin" ? "notifications.family.accessAdmin" : "notifications.family.accessSupporter",
      { name: member.name },
    );
  },
  openFamilySetup(this: FamilyWorldStoreContext) {
    if (this.onboardingCompleted && !this.permissions.canManageFamily) {
      return;
    }

    this.familySetupOpen = true;
  },
  async completeFamilySetup(this: FamilyWorldStoreContext, members: FamilyMember[], pets: FamilyPet[]) {
    if (
      await executeBackendFamilyCommand(
        this,
        "profile.complete-setup",
        { members, pets },
        "notifications.save.profile",
        () => {
          const activeChildStillExists = this.members.some(
            (member) => member.id === this.activeChildId && member.role === "child",
          );

          if (!activeChildStillExists) {
            this.activeChildId = this.members.find((member) => member.role === "child")?.id ?? this.activeChildId;
          }

          this.familySetupOpen = false;
          this.notify("notifications.family.setupComplete");
        },
      )
    ) {
      return;
    }

    const normalizedMembers = normalizeFamilyMembers(members);
    this.members = normalizedMembers;
    this.pets = pets;
    const activeChildStillExists = normalizedMembers.some(
      (member) => member.id === this.activeChildId && member.role === "child",
    );

    if (!activeChildStillExists) {
      this.activeChildId = normalizedMembers.find((member) => member.role === "child")?.id ?? this.activeChildId;
    }

    this.onboardingCompleted = true;
    this.familySetupOpen = false;
    this.persistFamilyProfile();
    this.notify("notifications.family.setupComplete");
  },
} satisfies FamilyWorldActionGroup;
