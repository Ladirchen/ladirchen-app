import { createLazyAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";

export type WorldDecorationSpriteId = "guide-branch" | "ladi-perch";

const modules = import.meta.glob("../../assets/world/*.webp", { import: "default" }) as AssetGlobModules;

export const getWorldDecorationSpriteUrl = createLazyAssetLoader<WorldDecorationSpriteId>(modules);
