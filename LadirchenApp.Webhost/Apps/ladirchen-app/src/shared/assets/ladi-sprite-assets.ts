import { createLazyAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";
import type { LadiSpriteId } from "@/domain/ladi";

const modules = import.meta.glob("../../assets/ladi/*.webp", { import: "default" }) as AssetGlobModules;

export const getLadiSpriteUrl = createLazyAssetLoader<LadiSpriteId>(modules);
