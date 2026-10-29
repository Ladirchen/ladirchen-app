import { describe, expect, it, vi } from "vite-plus/test";

import { createFamilyMembers, FAMILY_MEMBER_IDS } from "@/infrastructure/fixtures/family-world-fixtures";
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
