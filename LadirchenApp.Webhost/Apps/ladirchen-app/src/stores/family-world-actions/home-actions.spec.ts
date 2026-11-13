import { describe, expect, it, vi } from "vite-plus/test";
import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";

describe("house level celebration actions", () => {
  it("shows a new house level once for each signed-in family member", () => {
    const values = new Map<string, string>();
    const setItem = vi.fn((key: string, value: string) => values.set(key, value));
    const dependencies = createFamilyWorldTestDependencies({
      clientStorage: {
        getItem: (key) => values.get(key) ?? null,
        removeItem: (key) => values.delete(key),
        setItem,
      },
    });
    const store = createFamilyWorldTestStore(dependencies);
    store.signedInMemberId = createDomainId.familyMember("child-1");
    store.houseLevel = 2;

    store.revealPendingHouseLevel();

    expect(store.houseLevelAnimation.visible).toBe(true);
    expect(store.houseLevelAnimation.previousLevel).toBe(0);
    expect(store.houseLevelAnimation.level).toBe(2);

    store.dismissHouseLevelAnimation();
    store.revealPendingHouseLevel();

    expect(setItem).toHaveBeenCalledWith("ladirchen:house-level:child-1", "2");
    expect(store.houseLevelAnimation.visible).toBe(false);
  });

  it("tracks a lower level so a later house upgrade is celebrated again", () => {
    const values = new Map([["ladirchen:house-level:child-1", "3"]]);
    const dependencies = createFamilyWorldTestDependencies({
      clientStorage: {
        getItem: (key) => values.get(key) ?? null,
        removeItem: (key) => values.delete(key),
        setItem: (key, value) => values.set(key, value),
      },
    });
    const store = createFamilyWorldTestStore(dependencies);
    store.signedInMemberId = createDomainId.familyMember("child-1");
    store.houseLevel = 2;
    store.revealPendingHouseLevel();
    store.houseLevel = 3;

    store.revealPendingHouseLevel();

    expect(store.houseLevelAnimation.visible).toBe(true);
    expect(store.houseLevelAnimation.previousLevel).toBe(2);
    expect(store.houseLevelAnimation.level).toBe(3);
  });
});
