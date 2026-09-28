import artDeskUrl from "@/assets/furniture/art-desk.webp";
import batGarlandUrl from "@/assets/furniture/bat-garland.webp";
import bedsideTableUrl from "@/assets/furniture/bedside-table.webp";
import bookshelfUrl from "@/assets/furniture/bookshelf.webp";
import bunkBedUrl from "@/assets/furniture/bunk-bed.webp";
import candyBushUrl from "@/assets/furniture/candy-bush.webp";
import candyFenceUrl from "@/assets/furniture/candy-fence.webp";
import catTreeUrl from "@/assets/furniture/cat-tree.webp";
import diningTableUrl from "@/assets/furniture/dining-table.webp";
import dogBlanketUrl from "@/assets/furniture/dog-blanket.webp";
import doubleBedUrl from "@/assets/furniture/double-bed.webp";
import flowerBoxUrl from "@/assets/furniture/flower-box.webp";
import foodBowlUrl from "@/assets/furniture/food-bowl.webp";
import familyFlagUrl from "@/assets/furniture/family-flag.webp";
import gardenChairUrl from "@/assets/furniture/garden-chair.webp";
import gardenTableUrl from "@/assets/furniture/garden-table.webp";
import hammockUrl from "@/assets/furniture/hammock.webp";
import kitchenCounterUrl from "@/assets/furniture/kitchen-counter.webp";
import lampUrl from "@/assets/furniture/lamp.webp";
import petBedUrl from "@/assets/furniture/pet-bed.webp";
import plantUrl from "@/assets/furniture/plant.webp";
import poolUrl from "@/assets/furniture/pool.webp";
import pumpkinArchUrl from "@/assets/furniture/pumpkin-arch.webp";
import retroFridgeFrontUrl from "@/assets/furniture/retro-fridge-front.webp";
import retroFridgeUrl from "@/assets/furniture/retro-fridge.webp";
import retroFridgeOpenFrontUrl from "@/assets/furniture/retro-fridge-open-front.webp";
import rugUrl from "@/assets/furniture/rug.webp";
import sofaUrl from "@/assets/furniture/sofa.webp";
import storageCabinetUrl from "@/assets/furniture/storage-cabinet.webp";
import stringLightsUrl from "@/assets/furniture/string-lights.webp";
import sunshadeUrl from "@/assets/furniture/sunshade.webp";
import tableUrl from "@/assets/furniture/table.webp";
import telescopeUrl from "@/assets/furniture/telescope.webp";
import trampolineUrl from "@/assets/furniture/trampoline.webp";
import wallArtUrl from "@/assets/furniture/wall-art.webp";
import wardrobeUrl from "@/assets/furniture/wardrobe.webp";
import type { FurnitureVisualId } from "@/domain/house";

export const FURNITURE_SPRITE_ASSET_URLS = {
  "art-desk": artDeskUrl,
  "bat-garland": batGarlandUrl,
  "bedside-table": bedsideTableUrl,
  "bookshelf": bookshelfUrl,
  "bunk-bed": bunkBedUrl,
  "candy-bush": candyBushUrl,
  "candy-fence": candyFenceUrl,
  "cat-tree": catTreeUrl,
  "dining-table": diningTableUrl,
  "dog-blanket": dogBlanketUrl,
  "double-bed": doubleBedUrl,
  "flower-box": flowerBoxUrl,
  "food-bowl": foodBowlUrl,
  "family-flag": familyFlagUrl,
  "garden-chair": gardenChairUrl,
  "garden-table": gardenTableUrl,
  "hammock": hammockUrl,
  "kitchen-counter": kitchenCounterUrl,
  "lamp": lampUrl,
  "pet-bed": petBedUrl,
  "plant": plantUrl,
  "pool": poolUrl,
  "pumpkin-arch": pumpkinArchUrl,
  "retro-fridge": retroFridgeUrl,
  "rug": rugUrl,
  "sofa": sofaUrl,
  "storage-cabinet": storageCabinetUrl,
  "string-lights": stringLightsUrl,
  "sunshade": sunshadeUrl,
  "table": tableUrl,
  "telescope": telescopeUrl,
  "trampoline": trampolineUrl,
  "wall-art": wallArtUrl,
  "wardrobe": wardrobeUrl,
} as const satisfies Record<FurnitureVisualId, string>;

export interface FurnitureSpriteDefinition {
  readonly closedUrl: string;
  readonly openUrl?: string;
}

const baseSpriteDefinitions = Object.fromEntries(Object.entries(FURNITURE_SPRITE_ASSET_URLS).map(([id, closedUrl]) => [id, { closedUrl }])) as Record<FurnitureVisualId, FurnitureSpriteDefinition>;

export const FURNITURE_SPRITE_DEFINITIONS: Record<FurnitureVisualId, FurnitureSpriteDefinition> = {
  ...baseSpriteDefinitions,
  "retro-fridge": { closedUrl: retroFridgeFrontUrl, openUrl: retroFridgeOpenFrontUrl },
};
