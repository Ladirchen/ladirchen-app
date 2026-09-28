import { describe, expect, it } from "vitest";

import { FURNITURE_VISUAL_IDS } from "./types";
import { FURNITURE_VISUAL_DEFINITIONS, furnitureVisualDefinitionFor } from "./furniture-visuals";

describe("furniture visual definitions", () => {
  it("defines behavior for every supported furniture visual", () => {
    expect(Object.keys(FURNITURE_VISUAL_DEFINITIONS).sort()).toEqual([...FURNITURE_VISUAL_IDS].sort());
  });

  it("keeps wall decorations inside their wall placement range", () => {
    expect(furnitureVisualDefinitionFor("wall-art")).toMatchObject({ maximumY: 46, minimumY: 12, placementY: 28 });
    expect(furnitureVisualDefinitionFor("bat-garland")).toMatchObject({ maximumY: 46, minimumY: 12 });
  });

  it("keeps non-solid decorations out of collision checks", () => {
    expect(furnitureVisualDefinitionFor("rug").collision).toBeUndefined();
    expect(furnitureVisualDefinitionFor("sofa").collision).toBeDefined();
  });
});
