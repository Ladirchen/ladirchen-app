import { createLazyAssetLoader, createRawAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";
import type { FurnitureVisualId } from "@/domain/house";

const modules = import.meta.glob("../../../assets/furniture/*.webp", { import: "default" }) as AssetGlobModules;
const getFurnitureSpriteUrl = createLazyAssetLoader<FurnitureVisualId>(modules);
const loadFurnitureAsset = createRawAssetLoader(modules);

// retro-fridge has distinct closed/open artwork that doesn't follow the "<id>.webp" convention
const RETRO_FRIDGE_FILE_NAMES = { closed: "retro-fridge-front.webp", open: "retro-fridge-open-front.webp" } as const;

export interface FurnitureSpriteUrls {
  readonly closedUrl: Promise<string>;
  readonly openUrl?: Promise<string>;
}

export const getFurnitureSpriteUrls = (visual: FurnitureVisualId): FurnitureSpriteUrls => {
  if (visual === "retro-fridge") {
    return {
      closedUrl: loadFurnitureAsset(RETRO_FRIDGE_FILE_NAMES.closed),
      openUrl: loadFurnitureAsset(RETRO_FRIDGE_FILE_NAMES.open),
    };
  }
  return { closedUrl: getFurnitureSpriteUrl(visual) };
};
