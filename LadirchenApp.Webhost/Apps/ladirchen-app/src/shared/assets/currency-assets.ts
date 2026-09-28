import { createRawAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";

const modules = import.meta.glob("../../assets/currency/*.webp", { import: "default" }) as AssetGlobModules;
const loadCurrencyAsset = createRawAssetLoader(modules);

export const getLadirchenCoinUrl = (): Promise<string> => loadCurrencyAsset("ladirchen-coin.webp");
