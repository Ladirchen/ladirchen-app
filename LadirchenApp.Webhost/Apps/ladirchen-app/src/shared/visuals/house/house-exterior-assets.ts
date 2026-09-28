import { createLazyAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";
import type {
  HouseExteriorAssetId,
  HouseExteriorBackgroundAssetId,
  HouseExteriorHouseAssetId,
  HouseStageId,
  HouseThemeId,
} from "@/domain/house";
import { resolveHouseEnergyVisualLevel } from "@/domain/house";

const modules = import.meta.glob("../../../assets/room-designs/*.webp", { import: "default" }) as AssetGlobModules;

// Legacy/renamed art files: these ids don't follow the "<id>.webp" naming convention.
const HOUSE_EXTERIOR_ASSET_FILE_NAMES = {
  "sunny-dollhouse-energy-1-exterior-background": "sunny-dollhouse-energy-1-exterior-background.webp",
  "sunny-dollhouse-energy-2-exterior-background": "sunny-dollhouse-energy-2-exterior-background.webp",
  "sunny-dollhouse-energy-3-exterior-background": "sunny-dollhouse-energy-3-exterior-background.webp",
  "sunny-dollhouse-energy-4-exterior-background": "sunny-dollhouse-energy-4-exterior-background.webp",
  "sunny-dollhouse-energy-5-exterior-background": "starter-home-ladi-background.webp",
  "halloween-night-energy-1-exterior-background": "halloween-night-energy-1-exterior-background.webp",
  "halloween-night-energy-2-exterior-background": "halloween-night-energy-2-exterior-background.webp",
  "halloween-night-energy-3-exterior-background": "halloween-night-energy-3-exterior-background.webp",
  "halloween-night-energy-4-exterior-background": "halloween-night-energy-4-exterior-background.webp",
  "halloween-night-energy-5-exterior-background": "halloween-night-exterior-background.webp",
  "christmas-wonderland-energy-1-exterior-background": "christmas-wonderland-energy-1-exterior-background.webp",
  "christmas-wonderland-energy-2-exterior-background": "christmas-wonderland-energy-2-exterior-background.webp",
  "christmas-wonderland-energy-3-exterior-background": "christmas-wonderland-energy-3-exterior-background.webp",
  "christmas-wonderland-energy-4-exterior-background": "christmas-wonderland-energy-4-exterior-background.webp",
  "christmas-wonderland-energy-5-exterior-background": "christmas-wonderland-exterior-background.webp",
  "cotton-candy-dream-energy-1-exterior-background": "cotton-candy-dream-energy-1-exterior-background.webp",
  "cotton-candy-dream-energy-2-exterior-background": "cotton-candy-dream-energy-2-exterior-background.webp",
  "cotton-candy-dream-energy-3-exterior-background": "cotton-candy-dream-energy-3-exterior-background.webp",
  "cotton-candy-dream-energy-4-exterior-background": "cotton-candy-dream-energy-4-exterior-background.webp",
  "cotton-candy-dream-energy-5-exterior-background": "cotton-candy-dream-exterior-background.webp",
  "starlight-palace-energy-1-exterior-background": "starlight-palace-energy-1-exterior-background.webp",
  "starlight-palace-energy-2-exterior-background": "starlight-palace-energy-2-exterior-background.webp",
  "starlight-palace-energy-3-exterior-background": "starlight-palace-energy-3-exterior-background.webp",
  "starlight-palace-energy-4-exterior-background": "starlight-palace-energy-4-exterior-background.webp",
  "starlight-palace-energy-5-exterior-background": "starlight-palace-exterior-background.webp",

  "starter-home-sunny-dollhouse-house": "starter-home-ladi-house.webp",
  "starter-home-halloween-night-house": "starter-home-halloween-night-house.webp",
  "starter-home-christmas-wonderland-house": "starter-home-christmas-wonderland-house.webp",
  "starter-home-cotton-candy-dream-house": "starter-home-cotton-candy-dream-house.webp",
  "starter-home-starlight-palace-house": "starter-home-starlight-palace-house.webp",

  "family-home-sunny-dollhouse-house": "family-home-ladi-house.webp",
  "family-home-halloween-night-house": "family-home-halloween-night-house.webp",
  "family-home-christmas-wonderland-house": "family-home-christmas-wonderland-house.webp",
  "family-home-cotton-candy-dream-house": "family-home-cotton-candy-dream-house.webp",
  "family-home-starlight-palace-house": "family-home-starlight-palace-house.webp",

  "garden-home-sunny-dollhouse-house": "modern-home-ladi-house.webp",
  "garden-home-halloween-night-house": "garden-home-halloween-night-house.webp",
  "garden-home-christmas-wonderland-house": "garden-home-christmas-wonderland-house.webp",
  "garden-home-cotton-candy-dream-house": "garden-home-cotton-candy-dream-house.webp",
  "garden-home-starlight-palace-house": "garden-home-starlight-palace-house.webp",

  "tower-home-sunny-dollhouse-house": "castle-home-ladi-house.webp",
  "tower-home-halloween-night-house": "tower-home-halloween-night-house.webp",
  "tower-home-christmas-wonderland-house": "tower-home-christmas-wonderland-house.webp",
  "tower-home-cotton-candy-dream-house": "tower-home-cotton-candy-dream-house.webp",
  "tower-home-starlight-palace-house": "tower-home-starlight-palace-house.webp",

  "dream-home-sunny-dollhouse-house": "palace-home-ladi-house.webp",
  "dream-home-halloween-night-house": "dream-home-halloween-night-house.webp",
  "dream-home-christmas-wonderland-house": "dream-home-christmas-wonderland-house.webp",
  "dream-home-cotton-candy-dream-house": "dream-home-cotton-candy-dream-house.webp",
  "dream-home-starlight-palace-house": "dream-home-starlight-palace-house.webp",
} as const satisfies Record<HouseExteriorAssetId, string>;

export const getHouseExteriorAssetUrl = createLazyAssetLoader<HouseExteriorAssetId>(
  modules,
  (id) => HOUSE_EXTERIOR_ASSET_FILE_NAMES[id],
);

export const houseExteriorBackgroundAssetId = (themeId: HouseThemeId, energy: number): HouseExteriorBackgroundAssetId =>
  `${themeId}-energy-${resolveHouseEnergyVisualLevel(energy)}-exterior-background`;

export const houseExteriorHouseAssetId = (stageId: HouseStageId, themeId: HouseThemeId): HouseExteriorHouseAssetId =>
  `${stageId}-${themeId}-house`;
