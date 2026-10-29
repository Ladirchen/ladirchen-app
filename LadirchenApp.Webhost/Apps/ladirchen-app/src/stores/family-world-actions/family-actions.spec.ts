import { describe, expect, it, vi } from "vite-plus/test";

import {
  createFamilyMembers,
  createFamilyWorldInitialData,
  FAMILY_MEMBER_IDS,
} from "@/infrastructure/fixtures/family-world-fixtures";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import { NO_FAMILY_PERMISSIONS } from "@/domain/family/permissions";
import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";

describe("signInToFamily", () => {
  it("sets the authenticated member before hydration and renders the family only after a complete load", async () => {
    const observedDuringHydration: Array<{ authenticated: boolean; memberId: string }> = [];
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies());
    vi.spyOn(context, "hydrateFamilyReadModels").mockImplementation(async () => {
      observedDuringHydration.push({
        authenticated: context.isAuthenticated,
        memberId: context.signedInMemberId,
      });
      context.members = createFamilyMembers();
      context.familyReadModelsHydrated = true;
    });
    const familyId = createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478");

    const signedIn = await context.signInToFamily(familyId, FAMILY_MEMBER_IDS.mama);

    expect(observedDuringHydration).toEqual([{ authenticated: false, memberId: FAMILY_MEMBER_IDS.mama }]);
    expect(signedIn).toBe(true);
    expect(context.isAuthenticated).toBe(true);
    expect(context.viewerRole).toBe("guardian");
  });
});

describe("restoreAuthenticatedSession", () => {
  it("restores the persisted member identity before loading the family read models", async () => {
    const familyId = createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478");
    const session = { familyId, familyName: "Test family", memberId: FAMILY_MEMBER_IDS.mama };
    const baseDependencies = createFamilyWorldTestDependencies();
    const getCurrentSession = vi.fn().mockResolvedValue(session);
    const context = createFamilyWorldTestStore(
      createFamilyWorldTestDependencies({
        authentication: { ...baseDependencies.authentication, getCurrentSession },
      }),
    );
    vi.spyOn(context, "hydrateFamilyReadModels").mockImplementation(async () => {
      context.members = createFamilyMembers();
      context.familyReadModelsHydrated = true;
    });

    await expect(context.restoreAuthenticatedSession()).resolves.toBe(true);

    expect(getCurrentSession).toHaveBeenCalledOnce();
    expect(context.$familyWorld.familyContext.activeFamilyId).toBe(familyId);
    expect(context.signedInMemberId).toBe(FAMILY_MEMBER_IDS.mama);
    expect(context.viewerRole).toBe("guardian");
    expect(context.isAuthenticated).toBe(true);
  });

  it("keeps the login screen active when no server session exists", async () => {
    const baseDependencies = createFamilyWorldTestDependencies();
    const context = createFamilyWorldTestStore(
      createFamilyWorldTestDependencies({
        authentication: {
          ...baseDependencies.authentication,
          getCurrentSession: vi.fn().mockResolvedValue(null),
        },
      }),
    );
    context.isAuthenticated = true;
    context.members = createFamilyMembers();

    await expect(context.restoreAuthenticatedSession()).resolves.toBe(false);

    expect(context.isAuthenticated).toBe(false);
    expect(context.members).toEqual([]);
    expect(context.sessionRestorationFailed).toBe(false);
  });

  it("preserves the current UI state and exposes a retryable failure when the session endpoint is unavailable", async () => {
    const baseDependencies = createFamilyWorldTestDependencies();
    const getCurrentSession = vi
      .fn()
      .mockRejectedValueOnce(new Error("Session endpoint unavailable"))
      .mockResolvedValueOnce(null);
    const context = createFamilyWorldTestStore(
      createFamilyWorldTestDependencies({
        authentication: {
          ...baseDependencies.authentication,
          getCurrentSession,
        },
      }),
    );
    const members = createFamilyMembers();
    context.isAuthenticated = true;
    context.members = members;

    await expect(context.restoreAuthenticatedSession()).resolves.toBe(false);

    expect(context.isAuthenticated).toBe(true);
    expect(context.members).toEqual(members);
    expect(context.sessionRestorationFailed).toBe(true);

    await expect(context.restoreAuthenticatedSession()).resolves.toBe(false);

    expect(context.isAuthenticated).toBe(false);
    expect(context.members).toEqual([]);
    expect(context.sessionRestorationFailed).toBe(false);
  });
});

describe("openFamilySetup", () => {
  it("allows an active administrator to open account setup before onboarding is complete", () => {
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies());
    const members = createFamilyMembers();
    context.signedInMemberId = FAMILY_MEMBER_IDS.mama;
    context.members = members.map((member) =>
      member.id === FAMILY_MEMBER_IDS.mama
        ? {
            ...member,
            permissions: { ...NO_FAMILY_PERMISSIONS, canInviteMembers: true, canManageFamily: true },
          }
        : member,
    );

    context.openFamilySetup();

    expect(context.familySetupOpen).toBe(true);
    expect(context.canInviteFamilyMembers).toBe(true);
  });

  it("does not expose account setup to a supporter while onboarding is incomplete", () => {
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies());
    const members = createFamilyMembers();
    context.signedInMemberId = FAMILY_MEMBER_IDS.hannes;
    context.members = members.map((member) =>
      member.id === FAMILY_MEMBER_IDS.hannes
        ? { ...member, permissions: { ...NO_FAMILY_PERMISSIONS, canViewFamilyGoals: true } }
        : member,
    );

    context.openFamilySetup();

    expect(context.familySetupOpen).toBe(false);
    expect(context.canInviteFamilyMembers).toBe(false);
  });
});

describe("selectChildForGuardian", () => {
  it("does not select a child whose personal account is still pending", () => {
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies());
    const members = createFamilyMembers();
    const activeChild = members.find((member) => member.role === "child")!;
    const pendingChildId = createDomainId.familyMember("pending-child");
    context.viewerRole = "guardian";
    context.members = [
      ...members,
      {
        ...activeChild,
        id: pendingChildId,
        invitationPending: true,
        name: "Pending child",
      },
    ];
    context.activeChildId = activeChild.id;

    context.selectChildForGuardian(pendingChildId);

    expect(context.activeChildId).toBe(activeChild.id);
  });
});

describe("completeFamilySetup", () => {
  it("sends the stable member identities and editable family details to the backend", async () => {
    const execute = vi.fn<FamilyWorldDependencies["familyCommands"]["execute"]>().mockResolvedValue(["family-profile"]);
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies({ familyCommands: { execute } }), {
      create: createFamilyWorldInitialData,
    });
    vi.spyOn(context, "refreshFamilyReadModel").mockResolvedValue(undefined);
    vi.spyOn(context, "notify").mockImplementation(() => undefined);
    context.familySetupOpen = true;
    const members = context.members.map((member) => ({ ...member, name: `${member.name} updated` }));
    const pets = context.pets.map((pet) => ({ ...pet, name: `${pet.name} updated` }));

    await context.completeFamilySetup(members, pets);

    const payload = execute.mock.calls[0]?.[3];
    expect(execute).toHaveBeenCalledWith(
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "profile.complete-setup",
      expect.objectContaining({ pets }),
    );
    expect(payload).toMatchObject({
      members: members.map(({ permissions: _permissions, weeklyStreak: _weeklyStreak, ...member }) => member),
      pets,
    });
    expect(context.familySetupOpen).toBe(false);
  });
});
