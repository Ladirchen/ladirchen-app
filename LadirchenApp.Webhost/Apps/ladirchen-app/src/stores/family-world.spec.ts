import { createPinia, setActivePinia } from "pinia";
import { beforeEach, describe, expect, it } from "vite-plus/test";

import { createEmptyFamilyWorldInitialData } from "@/infrastructure/empty-family-world-initial-data";
import type { SavingGoal } from "@/domain/savings/types";
import { createDomainId } from "@/domain/shared/identifiers";
import { NO_FAMILY_PERMISSIONS } from "@/domain/family/permissions";
import { familyMemberColorPalette } from "@/theme/color-palette";
import { createFamilyWorldStoreDefinition } from "./family-world";

describe("family world empty state", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it("does not invent active entities before read-model hydration", () => {
    const useStore = createFamilyWorldStoreDefinition({ create: createEmptyFamilyWorldInitialData });
    const store = useStore();

    expect(store.activeChild).toBeUndefined();
    expect(store.signedInMember).toBeUndefined();
    expect(store.activeGoal).toBeUndefined();
    expect(store.permissions.canManageFamily).toBe(false);
  });

  it("selects a pending family-goal decision only from backend-projected capabilities", () => {
    const memberId = createDomainId.familyMember("member-1");
    const useStore = createFamilyWorldStoreDefinition({ create: createEmptyFamilyWorldInitialData });
    const store = useStore();
    const goal: SavingGoal = {
      cancellationRequest: {
        requestedByMemberId: createDomainId.familyMember("member-2"),
        consentedMemberIds: [createDomainId.familyMember("member-2")],
        pendingMemberIds: [memberId],
      },
      cheered: false,
      commandLimits: {
        [memberId]: {
          canApproveCancellation: false,
          canCancel: false,
          canRejectCancellation: false,
          canRequestCancellation: false,
          canWithdraw: false,
          maximumDepositAmount: 0,
          maximumSupportAmount: 0,
        },
      },
      icon: "🎯",
      id: createDomainId.savingGoal("family-goal"),
      ownerId: "family",
      saved: 25,
      shared: true,
      target: 100,
      title: "Family goal",
      visibility: "family",
    };
    store.signedInMemberId = memberId;
    store.goals = [goal];

    expect(store.pendingGoalCancellation).toBeUndefined();

    store.goals = [
      {
        ...goal,
        commandLimits: {
          [memberId]: {
            ...goal.commandLimits![memberId]!,
            canApproveCancellation: true,
          },
        },
      },
    ];

    expect(store.pendingGoalCancellation?.id).toBe(goal.id);

    store.goals = [
      {
        ...goal,
        cancellationRequest: {
          ...goal.cancellationRequest!,
          pendingMemberIds: [],
        },
        commandLimits: {
          [memberId]: {
            ...goal.commandLimits![memberId]!,
            canRejectCancellation: true,
          },
        },
      },
    ];

    expect(store.pendingGoalCancellation).toBeUndefined();
  });

  it("exposes member invitations only for an active member with the backend capability", () => {
    const memberId = createDomainId.familyMember("administrator");
    const useStore = createFamilyWorldStoreDefinition({ create: createEmptyFamilyWorldInitialData });
    const store = useStore();
    store.signedInMemberId = memberId;
    store.members = [
      {
        avatar: "🧑",
        color: familyMemberColorPalette.mama,
        guardianAccess: "admin",
        id: memberId,
        name: "Administrator",
        permissions: { ...NO_FAMILY_PERMISSIONS, canInviteMembers: true },
        role: "guardian",
        weeklyStreak: 0,
      },
    ];

    expect(store.canInviteFamilyMembers).toBe(true);

    store.members[0]!.invitationPending = true;

    expect(store.canInviteFamilyMembers).toBe(false);
  });
});
