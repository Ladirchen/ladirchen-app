import type { AvatarAppearance } from "@/domain/avatar";
import type { FamilyMember, FamilyPet, GuardianAccessLevel } from "@/domain/family/types";
import { calendarDateInTimeZone } from "@/domain/shared/zoned-calendar";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import { FOLLOW_UP_REWARD_REVEAL_DELAY_MS } from "@/shared/runtime-timing";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";

export const familyActions = {
  refreshCurrentTime(this: FamilyWorldStoreContext) {
    const previousDate = calendarDateInTimeZone(new Date(this.currentTimeMilliseconds), this.familyTimeZone);
    this.currentTimeMilliseconds = Date.now();
    const currentDate = calendarDateInTimeZone(new Date(this.currentTimeMilliseconds), this.familyTimeZone);

    if (previousDate !== currentDate && this.familyReadModelsHydrated) {
      void this.refreshFamilyReadModel("contributions");
    }
  },
  async signOut(this: FamilyWorldStoreContext) {
    try {
      await this.$familyWorld.authentication.signOut();
    } finally {
      await this.$familyWorld.familyAggregateUpdates.disconnect();
      this.isAuthenticated = false;
      this.familySetupOpen = false;
      this.piggyBankOpen = false;
      this.rewardAnimation.visible = false;
    }
  },
  async restoreAuthenticatedSession(this: FamilyWorldStoreContext): Promise<boolean> {
    this.sessionRestorationFailed = false;

    try {
      const session = await this.$familyWorld.authentication.getCurrentSession();

      if (session) {
        const restored = await this.signInToFamily(session.familyId, session.memberId);
        this.sessionRestorationFailed = !restored;

        return restored;
      }
    } catch {
      if (this.isAuthenticated) {
        this.sessionRestorationFailed = true;
      } else {
        this.$reset();
      }

      return false;
    }

    this.$reset();

    return false;
  },
  async signInToFamily(this: FamilyWorldStoreContext, familyId: FamilyId, memberId: FamilyMemberId): Promise<boolean> {
    this.$familyWorld.familyContext.setActiveFamilyId(familyId);
    this.$reset();
    this.signedInMemberId = memberId;
    this.isAuthenticated = false;
    this.familyReadModelsHydrated = false;
    this.homeCustomizationHydrated = false;
    await this.hydrateFamilyReadModels();
    const member = this.members.find((item) => item.id === memberId);

    if (!this.familyReadModelsHydrated || !member) {
      this.isAuthenticated = false;

      return false;
    }

    this.signedInMemberId = member.id;
    this.viewerRole = member.role;

    if (member.role === "child") {
      this.activeChildId = member.id;
    }

    this.isAuthenticated = true;
    this.$familyWorld.scheduler.schedule(() => this.revealNextContributionReward(), FOLLOW_UP_REWARD_REVEAL_DELAY_MS);

    return true;
  },
  selectChildForGuardian(this: FamilyWorldStoreContext, memberId: FamilyMemberId) {
    if (this.viewerRole !== "guardian") {
      return;
    }

    const child = this.members.find(
      (member) => member.id === memberId && member.role === "child" && !member.invitationPending,
    );

    if (child) {
      this.activeChildId = child.id;
      this.notify("notifications.family.childSelected", { name: child.name });
    }
  },
  async setWeeklyGoalParticipation(this: FamilyWorldStoreContext, memberId: FamilyMemberId, participates: boolean) {
    const name = this.members.find((item) => item.id === memberId)?.name ?? "";
    await executeBackendFamilyCommand(
      this,
      "profile.set-weekly-participation",
      { memberId, participates },
      "notifications.save.profile",
      () =>
        this.notify(participates ? "notifications.family.weeklyGoalJoined" : "notifications.family.weeklyGoalPaused", {
          name,
        }),
    );
  },
  async saveOwnAppearance(this: FamilyWorldStoreContext, appearance: AvatarAppearance) {
    await executeBackendFamilyCommand(
      this,
      "profile.save-own-appearance",
      { appearance },
      "notifications.save.profile",
      () => this.notify("notifications.profile.saved"),
    );
  },
  async setOwnNickname(this: FamilyWorldStoreContext, nickname: string) {
    await executeBackendFamilyCommand(
      this,
      "profile.set-own-nickname",
      { nickname },
      "notifications.save.profile",
      () => {
        const savedNickname = this.signedInMember?.nickname ?? "";

        this.notify(
          savedNickname ? "notifications.profile.nicknameSaved" : "notifications.profile.nicknameRemoved",
          savedNickname ? { nickname: savedNickname } : {},
        );
      },
    );
  },
  async setGuardianAccess(
    this: FamilyWorldStoreContext,
    memberId: FamilyMemberId,
    guardianAccess: GuardianAccessLevel,
  ) {
    const name = this.members.find((item) => item.id === memberId)?.name ?? "";
    await executeBackendFamilyCommand(
      this,
      "profile.set-guardian-access",
      { guardianAccess, memberId },
      "notifications.save.profile",
      () =>
        this.notify(
          guardianAccess === "admin" ? "notifications.family.accessAdmin" : "notifications.family.accessSupporter",
          { name },
        ),
    );
  },
  openFamilySetup(this: FamilyWorldStoreContext) {
    const canOpenSetup = this.onboardingCompleted ? this.permissions.canManageFamily : this.canInviteFamilyMembers;

    if (canOpenSetup) {
      this.familySetupOpen = true;
    }
  },
  async completeFamilySetup(this: FamilyWorldStoreContext, members: FamilyMember[], pets: FamilyPet[]) {
    const memberPayload = members.map(
      ({ permissions: _permissions, weeklyStreak: _weeklyStreak, ...member }) => member,
    );
    await executeBackendFamilyCommand(
      this,
      "profile.complete-setup",
      { members: memberPayload, pets },
      "notifications.save.profile",
      () => {
        this.familySetupOpen = false;
        this.notify("notifications.family.setupComplete");
      },
    );
  },
} satisfies FamilyWorldActionGroup;
