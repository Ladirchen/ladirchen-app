import auroraLadiUrl from '@/assets/ladi/aurora-ladi.webp';
import gardenLadiUrl from '@/assets/ladi/garden-ladi.webp';
import idleLadiUrl from '@/assets/ladi/idle-ladi.webp';
import perchedLadiUrl from '@/assets/ladi/perched-ladi.webp';
import smartLadiUrl from '@/assets/ladi/smart-ladi.webp';
import starterLadiUrl from '@/assets/ladi/starter-ladi.webp';
import sunLadiUrl from '@/assets/ladi/sun-ladi.webp';
import superLadiUrl from '@/assets/ladi/super-ladi.webp';
import type { LadiSpriteId } from '@/domain/ladi';

export const LADI_SPRITE_ASSET_URLS = {
  'aurora-ladi': auroraLadiUrl,
  'garden-ladi': gardenLadiUrl,
  'idle-ladi': idleLadiUrl,
  'perched-ladi': perchedLadiUrl,
  'smart-ladi': smartLadiUrl,
  'starter-ladi': starterLadiUrl,
  'sun-ladi': sunLadiUrl,
  'super-ladi': superLadiUrl,
} as const satisfies Record<LadiSpriteId, string>;
