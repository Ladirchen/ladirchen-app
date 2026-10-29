import { describe, expect, it, vi } from "vite-plus/test";

import { createEmptyFamilyWorldInitialData } from "@/infrastructure/empty-family-world-initial-data";
import { createFamilyMembers, FAMILY_MEMBER_IDS } from "@/infrastructure/fixtures/family-world-fixtures";
import { createFamilyWorldState } from "@/stores/family-world-state";
import type { FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { createDomainId } from "@/domain/shared/identifiers";
import { familyActions } from "./family-actions";

describe("signInToFamily", () => {
  it("sets the authenticated member before hydration and renders the family only after a complete load", async () => {
    const initialState = createFamilyWorldState(createEmptyFamilyWorldInitialData());
    const observedDuringHydration: Array<{ authenticated: boolean; memberId: string }> = [];
    const context = Object.assign(initialState, {
      $familyWorld: {
        clientStorage: { getItem: vi.fn(), removeItem: vi.fn(), setItem: vi.fn() },
        familyContext: { activeFamilyId: "", setActiveFamilyId: vi.fn() },
        scheduler: { schedule: vi.fn() },
      },
      $reset: vi.fn(),
      hydrateFamilyReadModels: vi.fn(async () => {
        observedDuringHydration.push({
          authenticated: context.isAuthenticated,
          memberId: context.signedInMemberId,
        });
        context.members = createFamilyMembers();
        context.familyReadModelsHydrated = true;
      }),
    }) as unknown as FamilyWorldStoreContext;
    const familyId = createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478");

    const signedIn = await familyActions.signInToFamily.call(context, familyId, FAMILY_MEMBER_IDS.mama);

    expect(observedDuringHydration).toEqual([{ authenticated: false, memberId: FAMILY_MEMBER_IDS.mama }]);
    expect(signedIn).toBe(true);
    expect(context.isAuthenticated).toBe(true);
    expect(context.viewerRole).toBe("guardian");
  });
});
