import sunnyDollhouseBackgroundUrl from "@/assets/room-designs/starter-home-ladi-background.webp";
import halloweenNightBackgroundUrl from "@/assets/room-designs/halloween-night-exterior-background.webp";
import christmasWonderlandBackgroundUrl from "@/assets/room-designs/christmas-wonderland-exterior-background.webp";
import cottonCandyDreamBackgroundUrl from "@/assets/room-designs/cotton-candy-dream-exterior-background.webp";
import starlightPalaceBackgroundUrl from "@/assets/room-designs/starlight-palace-exterior-background.webp";
import sunnyDollhouseEnergy1BackgroundUrl from "@/assets/room-designs/sunny-dollhouse-energy-1-exterior-background.webp";
import sunnyDollhouseEnergy2BackgroundUrl from "@/assets/room-designs/sunny-dollhouse-energy-2-exterior-background.webp";
import sunnyDollhouseEnergy3BackgroundUrl from "@/assets/room-designs/sunny-dollhouse-energy-3-exterior-background.webp";
import sunnyDollhouseEnergy4BackgroundUrl from "@/assets/room-designs/sunny-dollhouse-energy-4-exterior-background.webp";
import halloweenNightEnergy1BackgroundUrl from "@/assets/room-designs/halloween-night-energy-1-exterior-background.webp";
import halloweenNightEnergy2BackgroundUrl from "@/assets/room-designs/halloween-night-energy-2-exterior-background.webp";
import halloweenNightEnergy3BackgroundUrl from "@/assets/room-designs/halloween-night-energy-3-exterior-background.webp";
import halloweenNightEnergy4BackgroundUrl from "@/assets/room-designs/halloween-night-energy-4-exterior-background.webp";
import christmasWonderlandEnergy1BackgroundUrl from "@/assets/room-designs/christmas-wonderland-energy-1-exterior-background.webp";
import christmasWonderlandEnergy2BackgroundUrl from "@/assets/room-designs/christmas-wonderland-energy-2-exterior-background.webp";
import christmasWonderlandEnergy3BackgroundUrl from "@/assets/room-designs/christmas-wonderland-energy-3-exterior-background.webp";
import christmasWonderlandEnergy4BackgroundUrl from "@/assets/room-designs/christmas-wonderland-energy-4-exterior-background.webp";
import cottonCandyDreamEnergy1BackgroundUrl from "@/assets/room-designs/cotton-candy-dream-energy-1-exterior-background.webp";
import cottonCandyDreamEnergy2BackgroundUrl from "@/assets/room-designs/cotton-candy-dream-energy-2-exterior-background.webp";
import cottonCandyDreamEnergy3BackgroundUrl from "@/assets/room-designs/cotton-candy-dream-energy-3-exterior-background.webp";
import cottonCandyDreamEnergy4BackgroundUrl from "@/assets/room-designs/cotton-candy-dream-energy-4-exterior-background.webp";
import starlightPalaceEnergy1BackgroundUrl from "@/assets/room-designs/starlight-palace-energy-1-exterior-background.webp";
import starlightPalaceEnergy2BackgroundUrl from "@/assets/room-designs/starlight-palace-energy-2-exterior-background.webp";
import starlightPalaceEnergy3BackgroundUrl from "@/assets/room-designs/starlight-palace-energy-3-exterior-background.webp";
import starlightPalaceEnergy4BackgroundUrl from "@/assets/room-designs/starlight-palace-energy-4-exterior-background.webp";

import starterHomeSunnyDollhouseUrl from "@/assets/room-designs/starter-home-ladi-house.webp";
import starterHomeHalloweenNightUrl from "@/assets/room-designs/starter-home-halloween-night-house.webp";
import starterHomeChristmasWonderlandUrl from "@/assets/room-designs/starter-home-christmas-wonderland-house.webp";
import starterHomeCottonCandyDreamUrl from "@/assets/room-designs/starter-home-cotton-candy-dream-house.webp";
import starterHomeStarlightPalaceUrl from "@/assets/room-designs/starter-home-starlight-palace-house.webp";

import familyHomeSunnyDollhouseUrl from "@/assets/room-designs/family-home-ladi-house.webp";
import familyHomeHalloweenNightUrl from "@/assets/room-designs/family-home-halloween-night-house.webp";
import familyHomeChristmasWonderlandUrl from "@/assets/room-designs/family-home-christmas-wonderland-house.webp";
import familyHomeCottonCandyDreamUrl from "@/assets/room-designs/family-home-cotton-candy-dream-house.webp";
import familyHomeStarlightPalaceUrl from "@/assets/room-designs/family-home-starlight-palace-house.webp";

import gardenHomeSunnyDollhouseUrl from "@/assets/room-designs/modern-home-ladi-house.webp";
import gardenHomeHalloweenNightUrl from "@/assets/room-designs/garden-home-halloween-night-house.webp";
import gardenHomeChristmasWonderlandUrl from "@/assets/room-designs/garden-home-christmas-wonderland-house.webp";
import gardenHomeCottonCandyDreamUrl from "@/assets/room-designs/garden-home-cotton-candy-dream-house.webp";
import gardenHomeStarlightPalaceUrl from "@/assets/room-designs/garden-home-starlight-palace-house.webp";

import towerHomeSunnyDollhouseUrl from "@/assets/room-designs/castle-home-ladi-house.webp";
import towerHomeHalloweenNightUrl from "@/assets/room-designs/tower-home-halloween-night-house.webp";
import towerHomeChristmasWonderlandUrl from "@/assets/room-designs/tower-home-christmas-wonderland-house.webp";
import towerHomeCottonCandyDreamUrl from "@/assets/room-designs/tower-home-cotton-candy-dream-house.webp";
import towerHomeStarlightPalaceUrl from "@/assets/room-designs/tower-home-starlight-palace-house.webp";

import dreamHomeSunnyDollhouseUrl from "@/assets/room-designs/palace-home-ladi-house.webp";
import dreamHomeHalloweenNightUrl from "@/assets/room-designs/dream-home-halloween-night-house.webp";
import dreamHomeChristmasWonderlandUrl from "@/assets/room-designs/dream-home-christmas-wonderland-house.webp";
import dreamHomeCottonCandyDreamUrl from "@/assets/room-designs/dream-home-cotton-candy-dream-house.webp";
import dreamHomeStarlightPalaceUrl from "@/assets/room-designs/dream-home-starlight-palace-house.webp";

import type {
  HouseExteriorAssetId,
  HouseExteriorBackgroundAssetId,
  HouseExteriorHouseAssetId,
  HouseStageId,
  HouseThemeId,
} from "@/domain/house";
import { resolveHouseEnergyVisualLevel } from "@/domain/house";

export const HOUSE_EXTERIOR_ASSET_URLS = {
  "sunny-dollhouse-energy-1-exterior-background": sunnyDollhouseEnergy1BackgroundUrl,
  "sunny-dollhouse-energy-2-exterior-background": sunnyDollhouseEnergy2BackgroundUrl,
  "sunny-dollhouse-energy-3-exterior-background": sunnyDollhouseEnergy3BackgroundUrl,
  "sunny-dollhouse-energy-4-exterior-background": sunnyDollhouseEnergy4BackgroundUrl,
  "sunny-dollhouse-energy-5-exterior-background": sunnyDollhouseBackgroundUrl,
  "halloween-night-energy-1-exterior-background": halloweenNightEnergy1BackgroundUrl,
  "halloween-night-energy-2-exterior-background": halloweenNightEnergy2BackgroundUrl,
  "halloween-night-energy-3-exterior-background": halloweenNightEnergy3BackgroundUrl,
  "halloween-night-energy-4-exterior-background": halloweenNightEnergy4BackgroundUrl,
  "halloween-night-energy-5-exterior-background": halloweenNightBackgroundUrl,
  "christmas-wonderland-energy-1-exterior-background": christmasWonderlandEnergy1BackgroundUrl,
  "christmas-wonderland-energy-2-exterior-background": christmasWonderlandEnergy2BackgroundUrl,
  "christmas-wonderland-energy-3-exterior-background": christmasWonderlandEnergy3BackgroundUrl,
  "christmas-wonderland-energy-4-exterior-background": christmasWonderlandEnergy4BackgroundUrl,
  "christmas-wonderland-energy-5-exterior-background": christmasWonderlandBackgroundUrl,
  "cotton-candy-dream-energy-1-exterior-background": cottonCandyDreamEnergy1BackgroundUrl,
  "cotton-candy-dream-energy-2-exterior-background": cottonCandyDreamEnergy2BackgroundUrl,
  "cotton-candy-dream-energy-3-exterior-background": cottonCandyDreamEnergy3BackgroundUrl,
  "cotton-candy-dream-energy-4-exterior-background": cottonCandyDreamEnergy4BackgroundUrl,
  "cotton-candy-dream-energy-5-exterior-background": cottonCandyDreamBackgroundUrl,
  "starlight-palace-energy-1-exterior-background": starlightPalaceEnergy1BackgroundUrl,
  "starlight-palace-energy-2-exterior-background": starlightPalaceEnergy2BackgroundUrl,
  "starlight-palace-energy-3-exterior-background": starlightPalaceEnergy3BackgroundUrl,
  "starlight-palace-energy-4-exterior-background": starlightPalaceEnergy4BackgroundUrl,
  "starlight-palace-energy-5-exterior-background": starlightPalaceBackgroundUrl,

  "starter-home-sunny-dollhouse-house": starterHomeSunnyDollhouseUrl,
  "starter-home-halloween-night-house": starterHomeHalloweenNightUrl,
  "starter-home-christmas-wonderland-house": starterHomeChristmasWonderlandUrl,
  "starter-home-cotton-candy-dream-house": starterHomeCottonCandyDreamUrl,
  "starter-home-starlight-palace-house": starterHomeStarlightPalaceUrl,

  "family-home-sunny-dollhouse-house": familyHomeSunnyDollhouseUrl,
  "family-home-halloween-night-house": familyHomeHalloweenNightUrl,
  "family-home-christmas-wonderland-house": familyHomeChristmasWonderlandUrl,
  "family-home-cotton-candy-dream-house": familyHomeCottonCandyDreamUrl,
  "family-home-starlight-palace-house": familyHomeStarlightPalaceUrl,

  "garden-home-sunny-dollhouse-house": gardenHomeSunnyDollhouseUrl,
  "garden-home-halloween-night-house": gardenHomeHalloweenNightUrl,
  "garden-home-christmas-wonderland-house": gardenHomeChristmasWonderlandUrl,
  "garden-home-cotton-candy-dream-house": gardenHomeCottonCandyDreamUrl,
  "garden-home-starlight-palace-house": gardenHomeStarlightPalaceUrl,

  "tower-home-sunny-dollhouse-house": towerHomeSunnyDollhouseUrl,
  "tower-home-halloween-night-house": towerHomeHalloweenNightUrl,
  "tower-home-christmas-wonderland-house": towerHomeChristmasWonderlandUrl,
  "tower-home-cotton-candy-dream-house": towerHomeCottonCandyDreamUrl,
  "tower-home-starlight-palace-house": towerHomeStarlightPalaceUrl,

  "dream-home-sunny-dollhouse-house": dreamHomeSunnyDollhouseUrl,
  "dream-home-halloween-night-house": dreamHomeHalloweenNightUrl,
  "dream-home-christmas-wonderland-house": dreamHomeChristmasWonderlandUrl,
  "dream-home-cotton-candy-dream-house": dreamHomeCottonCandyDreamUrl,
  "dream-home-starlight-palace-house": dreamHomeStarlightPalaceUrl,
} as const satisfies Record<HouseExteriorAssetId, string>;

export const houseExteriorBackgroundAssetId = (
  themeId: HouseThemeId,
  energy: number,
): HouseExteriorBackgroundAssetId => `${themeId}-energy-${resolveHouseEnergyVisualLevel(energy)}-exterior-background`;

export const houseExteriorHouseAssetId = (
  stageId: HouseStageId,
  themeId: HouseThemeId,
): HouseExteriorHouseAssetId => `${stageId}-${themeId}-house`;
