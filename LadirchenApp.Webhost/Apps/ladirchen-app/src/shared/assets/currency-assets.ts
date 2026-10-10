import { createRawAssetLoader } from "@/shared/assets/lazy-asset-loader";
import type { AssetGlobModules } from "@/shared/assets/lazy-asset-loader";
const modules = import.meta.glob("../../assets/currency/*.webp", { import: "default" }) as AssetGlobModules;
const loadCurrencyAsset = createRawAssetLoader(modules);
export function getLadirchenCoinUrl(): Promise<string> {

  return loadCurrencyAsset("ladirchen-coin.webp");
}
