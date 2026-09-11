import guideBranchUrl from '@/assets/world/guide-branch.webp';
import ladiPerchUrl from '@/assets/world/ladi-perch.webp';

export type WorldDecorationSpriteId = 'guide-branch' | 'ladi-perch';

export const WORLD_DECORATION_SPRITE_URLS = {
  'guide-branch': guideBranchUrl,
  'ladi-perch': ladiPerchUrl,
} as const satisfies Record<WorldDecorationSpriteId, string>;
