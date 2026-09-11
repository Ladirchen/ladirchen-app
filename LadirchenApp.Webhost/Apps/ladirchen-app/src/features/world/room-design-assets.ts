import gardenLadiHillsUrl from '@/assets/room-designs/garden-ladi-hills.webp';
import gardenLadiHillsEnergy1Url from '@/assets/room-designs/garden-ladi-hills-energy-1.webp';
import gardenLadiHillsEnergy2Url from '@/assets/room-designs/garden-ladi-hills-energy-2.webp';
import gardenLadiHillsEnergy3Url from '@/assets/room-designs/garden-ladi-hills-energy-3.webp';
import gardenLadiHillsEnergy4Url from '@/assets/room-designs/garden-ladi-hills-energy-4.webp';
import gardenChristmasWonderlandUrl from '@/assets/room-designs/garden-christmas-wonderland.webp';
import gardenCottonCandyDreamUrl from '@/assets/room-designs/garden-cotton-candy-dream.webp';
import gardenHalloweenNightUrl from '@/assets/room-designs/garden-halloween-night.webp';
import gardenStarlightPalaceUrl from '@/assets/room-designs/garden-starlight-palace.webp';
import kitchenChristmasWonderlandUrl from '@/assets/room-designs/kitchen-christmas-wonderland.webp';
import kitchenCottonCandyDreamUrl from '@/assets/room-designs/kitchen-cotton-candy-dream.webp';
import kitchenHalloweenNightUrl from '@/assets/room-designs/kitchen-halloween-night.webp';
import kitchenLadiClassicUrl from '@/assets/room-designs/kitchen-ladi-classic.webp';
import kitchenStarlightPalaceUrl from '@/assets/room-designs/kitchen-starlight-palace.webp';
import livingChristmasWonderlandUrl from '@/assets/room-designs/living-christmas-wonderland.webp';
import livingCottonCandyDreamUrl from '@/assets/room-designs/living-cotton-candy-dream.webp';
import livingHalloweenNightUrl from '@/assets/room-designs/living-halloween-night.webp';
import livingLadiClassicUrl from '@/assets/room-designs/living-ladi-classic.webp';
import livingStarlightPalaceUrl from '@/assets/room-designs/living-starlight-palace.webp';
import type { RoomBackgroundAssetId, RoomDesignDefinition } from '@/domain/house';
import { resolveHouseEnergyVisualLevel } from '@/domain/house';

export const ROOM_DESIGN_ASSET_URLS = {
  'garden-christmas-wonderland-background': gardenChristmasWonderlandUrl,
  'garden-cotton-candy-dream-background': gardenCottonCandyDreamUrl,
  'garden-halloween-night-background': gardenHalloweenNightUrl,
  'garden-ladi-hills-background': gardenLadiHillsUrl,
  'garden-ladi-hills-energy-1-background': gardenLadiHillsEnergy1Url,
  'garden-ladi-hills-energy-2-background': gardenLadiHillsEnergy2Url,
  'garden-ladi-hills-energy-3-background': gardenLadiHillsEnergy3Url,
  'garden-ladi-hills-energy-4-background': gardenLadiHillsEnergy4Url,
  'garden-starlight-palace-background': gardenStarlightPalaceUrl,
  'kitchen-christmas-wonderland-background': kitchenChristmasWonderlandUrl,
  'kitchen-cotton-candy-dream-background': kitchenCottonCandyDreamUrl,
  'kitchen-halloween-night-background': kitchenHalloweenNightUrl,
  'kitchen-ladi-classic-background': kitchenLadiClassicUrl,
  'kitchen-starlight-palace-background': kitchenStarlightPalaceUrl,
  'living-christmas-wonderland-background': livingChristmasWonderlandUrl,
  'living-cotton-candy-dream-background': livingCottonCandyDreamUrl,
  'living-halloween-night-background': livingHalloweenNightUrl,
  'living-ladi-classic-background': livingLadiClassicUrl,
  'living-starlight-palace-background': livingStarlightPalaceUrl,
} as const satisfies Record<RoomBackgroundAssetId, string>;

export const roomDesignBackgroundAssetId = (
  design: RoomDesignDefinition,
  energy: number,
): RoomBackgroundAssetId => {
  if (design.id !== 'garden-ladi-hills') {return design.backgroundAssetId;}
  const level = resolveHouseEnergyVisualLevel(energy);
  return level === 5 ? design.backgroundAssetId : `garden-ladi-hills-energy-${level}-background`;
};
