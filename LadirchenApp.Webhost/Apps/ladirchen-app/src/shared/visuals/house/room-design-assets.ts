import { createLazyAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";
import type { RoomBackgroundAssetId, RoomDesignDefinition } from "@/domain/house";
import { resolveHouseEnergyVisualLevel } from "@/domain/house";

const modules = import.meta.glob("../../../assets/room-designs/*.webp", { import: "default" }) as AssetGlobModules;

// ids carry a "-background" suffix the source files don't have (e.g. "kitchen-ladi-classic-background" -> "kitchen-ladi-classic.webp")
export const getRoomDesignBackgroundUrl = createLazyAssetLoader<RoomBackgroundAssetId>(
  modules,
  (id) => `${id.replace(/-background$/, "")}.webp`,
);

export const roomDesignBackgroundAssetId = (design: RoomDesignDefinition, energy: number): RoomBackgroundAssetId => {
  if (design.id !== "garden-ladi-hills") {
    return design.backgroundAssetId;
  }
  const level = resolveHouseEnergyVisualLevel(energy);
  return level === 5 ? design.backgroundAssetId : `garden-ladi-hills-energy-${level}-background`;
};
