import { describe, expect, it } from "vitest";

import { HOUSE_LAYOUT_CONSTRAINTS } from "@/domain/house";
import type { HouseLayoutPlacement } from "@/domain/house/entities";
import { createFamilyWorldInitialData, FAMILY_MEMBER_IDS } from "@/infrastructure/fixtures/family-world-fixtures";
import { mergeHouseLayout } from "@/stores/family-world-state";

function findPlacement(placements: ReadonlyArray<HouseLayoutPlacement>, entityId: string): HouseLayoutPlacement {
  const placement = placements.find((item) => item.entityId === entityId);

  if (!placement) {
    throw new Error(`Missing house layout placement for ${entityId}`);
  }

  return placement;
}

describe("mergeHouseLayout", () => {
  it("keeps default placements when no saved layout exists", () => {
    const initialData = createFamilyWorldInitialData();

    expect(mergeHouseLayout([], initialData)).toEqual(initialData.houseLayout);
  });

  it("clamps saved character coordinates and scale", () => {
    const initialData = createFamilyWorldInitialData();
    const member = findPlacement(initialData.houseLayout, FAMILY_MEMBER_IDS.laura);

    const merged = mergeHouseLayout([{ ...member, x: -10, y: 101, scale: 2 }], initialData);

    expect(findPlacement(merged, FAMILY_MEMBER_IDS.laura)).toMatchObject({
      x: HOUSE_LAYOUT_CONSTRAINTS.minimumX,
      y: HOUSE_LAYOUT_CONSTRAINTS.maximumY,
      scale: HOUSE_LAYOUT_CONSTRAINTS.maximumScale,
    });
  });

  it("falls back to the default y when a character is saved below the floor", () => {
    const initialData = createFamilyWorldInitialData();
    const member = findPlacement(initialData.houseLayout, FAMILY_MEMBER_IDS.laura);

    const merged = mergeHouseLayout([{ ...member, y: HOUSE_LAYOUT_CONSTRAINTS.floorMinimumY - 1 }], initialData);

    expect(findPlacement(merged, FAMILY_MEMBER_IDS.laura).y).toBe(member.y);
  });

  it("preserves the default x for a perched Ladi", () => {
    const initialData = createFamilyWorldInitialData();
    const ladi = findPlacement(initialData.houseLayout, "family-ladi");

    const merged = mergeHouseLayout([{ ...ladi, x: 90, y: 30 }], initialData);

    expect(findPlacement(merged, "family-ladi")).toMatchObject({ x: ladi.x, y: 30 });
  });

  it("ignores saved positions for fixed furniture", () => {
    const initialData = createFamilyWorldInitialData();
    const counter = findPlacement(initialData.houseLayout, "kitchen-counter");

    const merged = mergeHouseLayout([{ ...counter, x: 90, y: 90 }], initialData);

    expect(findPlacement(merged, "kitchen-counter")).toEqual(counter);
  });

  it("uses visual y bounds and preserves the scale of locked furniture", () => {
    const initialData = createFamilyWorldInitialData();
    const wallArt = findPlacement(initialData.houseLayout, "wall-art");
    const diningTable = findPlacement(initialData.houseLayout, "family-dining-table");

    const merged = mergeHouseLayout(
      [
        { ...wallArt, y: 47 },
        { ...diningTable, scale: HOUSE_LAYOUT_CONSTRAINTS.maximumScale },
      ],
      initialData,
    );

    expect(findPlacement(merged, "wall-art").y).toBe(wallArt.y);
    expect(findPlacement(merged, "family-dining-table").scale).toBe(diningTable.scale);
  });

  it("restores a character's default placement when saved coordinates collide with furniture", () => {
    const initialData = createFamilyWorldInitialData();
    const member = findPlacement(initialData.houseLayout, FAMILY_MEMBER_IDS.laura);

    const merged = mergeHouseLayout([{ ...member, x: 70, y: 63 }], initialData);

    expect(findPlacement(merged, FAMILY_MEMBER_IDS.laura)).toEqual(member);
  });
});
