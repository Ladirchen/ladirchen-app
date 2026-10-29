import type { AvatarAppearance } from "@/domain/avatar";
import type { FamilyMember, FamilyPet, GuardianAccessLevel } from "@/domain/family/types";
import { calendarDateInTimeZone } from "@/domain/shared/zoned-calendar";
import { createDomainId } from "@/domain/shared/identifiers";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import { FOLLOW_UP_REWARD_REVEAL_DELAY_MS } from "@/shared/runtime-timing";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";
import { AUTH_STATE_KEY, createUuid } from "./family-world-store-utils";

export const familyActions = {
  refreshCurrentTime(this: FamilyWorldStoreContext) {
    const previousDate = calendarDateInTimeZone(new Date(this.currentTimeMilliseconds), this.familyTimeZone);
    this.currentTimeMilliseconds = Date.now();
    const currentDate = calendarDateInTimeZone(new Date(this.currentTimeMilliseconds), this.familyTimeZone);

    if (previousDate !== currentDate && this.familyReadModelsHydrated) {
      void this.refreshFamilyReadModel("contributions");
    }
  },
  switchSession(this: FamilyWorldStoreContext, memberId: FamilyMemberId, revealRewards = true) {
    const member = this.members.find((item) => item.id === memberId);

    if (!member) {
      return;
    }

    this.rewardAnimation.visible = false;
    this.signedInMemberId = member.id;
    this.viewerRole = member.role;
    this.balances = {};
    this.availableBalances = {};
    this.goals = [];
    this.pendingGuardianGifts = [];
    this.savingsVouchers = [];
    this.sharedSavedBalances = {};
    this.shopRewards = [];
    this.members = this.members.filter((item) => !item.invitationPending).map(({ email: _email, ...item }) => item);

    if (member.role === "child") {
      this.activeChildId = member.id;
    }

    if (this.isAuthenticated) {
      void this.refreshAllFamilyReadModels().then(() => {
        if (member.role === "child") {
          this.revealNextGuardianGift();
        }

        if (revealRewards) {
          this.$familyWorld.scheduler.schedule(
            () => this.revealNextContributionReward(),
            FOLLOW_UP_REWARD_REVEAL_DELAY_MS,
          );
        }
      });
    }

    this.notify("notifications.session.switched", { name: member.name });
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
      this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "signed-out");
    }
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
    this.signedInMemberId = memberId;
    this.isAuthenticated = false;
    this.familyReadModelsHydrated = false;
    this.homeCustomizationHydrated = false;
    this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "authenticated");
    await this.hydrateFamilyReadModels();
    const member = this.members.find((item) => item.id === memberId);

    if (!this.familyReadModelsHydrated || !member) {
      this.isAuthenticated = false;
      this.$familyWorld.clientStorage.setItem(AUTH_STATE_KEY, "signed-out");

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

    const child = this.members.find((member) => member.id === memberId && member.role === "child");

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
  async inviteGuardian(
    this: FamilyWorldStoreContext,
    name: string,
    email: string,
    guardianAccess: GuardianAccessLevel = "supporter",
  ) {
    const id = createDomainId.familyMember(createUuid());
    await executeBackendFamilyCommand(
      this,
      "profile.invite-guardian",
      { email, guardianAccess, id, name },
      "notifications.save.profile",
      () => this.notify("notifications.family.invitationCreated", { name }),
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
    if (!this.onboardingCompleted || this.permissions.canManageFamily) {
      this.familySetupOpen = true;
    }
  },
  async completeFamilySetup(this: FamilyWorldStoreContext, members: FamilyMember[], pets: FamilyPet[]) {
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
    );
  },
} satisfies FamilyWorldActionGroup;
