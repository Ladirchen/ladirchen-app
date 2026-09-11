import { HOUSE_THEMES } from './catalog';
import type { HouseThemeDefinition, HouseThemeId } from './types';
import type { RoomDesignDefinition } from './room-designs';

type ThemedHouseThemeId = Exclude<HouseThemeId, 'sunny-dollhouse'>;
type ThemedHouseThemeDefinition = HouseThemeDefinition & { readonly id: ThemedHouseThemeId };

const isThemedHouseTheme = (theme: HouseThemeDefinition): theme is ThemedHouseThemeDefinition =>
  theme.id !== 'sunny-dollhouse';

const createThemedRoomDesigns = (
  houseThemeId: ThemedHouseThemeId,
  icon: string,
): ReadonlyArray<RoomDesignDefinition> => [
  {
    id: `garden-${houseThemeId}`,
    houseThemeId,
    zoneId: 'garden',
    nameKey: `catalog.roomDesigns.garden-${houseThemeId}.name`,
    descriptionKey: `catalog.roomDesigns.garden-${houseThemeId}.description`,
    icon,
    price: 0,
    minimumHouseLevel: 0,
    ownedByDefault: false,
    backgroundAssetId: `garden-${houseThemeId}-background`,
    canvasWidth: 1672,
    canvasHeight: 992,
    overlays: [],
  },
  {
    id: `living-${houseThemeId}`,
    houseThemeId,
    zoneId: 'living-room',
    nameKey: `catalog.roomDesigns.living-${houseThemeId}.name`,
    descriptionKey: `catalog.roomDesigns.living-${houseThemeId}.description`,
    icon,
    price: 0,
    minimumHouseLevel: 0,
    ownedByDefault: false,
    backgroundAssetId: `living-${houseThemeId}-background`,
    canvasWidth: 1672,
    canvasHeight: 941,
    overlays: [],
  },
  {
    id: `kitchen-${houseThemeId}`,
    houseThemeId,
    zoneId: 'kitchen',
    nameKey: `catalog.roomDesigns.kitchen-${houseThemeId}.name`,
    descriptionKey: `catalog.roomDesigns.kitchen-${houseThemeId}.description`,
    icon,
    price: 0,
    minimumHouseLevel: 0,
    ownedByDefault: false,
    backgroundAssetId: `kitchen-${houseThemeId}-background`,
    canvasWidth: 1672,
    canvasHeight: 941,
    overlays: [],
  },
];

export const ROOM_DESIGNS = [
  {
    id: 'garden-ladi-hills',
    houseThemeId: 'sunny-dollhouse',
    zoneId: 'garden',
    nameKey: 'catalog.roomDesigns.garden-ladi-hills.name',
    descriptionKey: 'catalog.roomDesigns.garden-ladi-hills.description',
    icon: '🌿',
    price: 0,
    minimumHouseLevel: 0,
    ownedByDefault: true,
    backgroundAssetId: 'garden-ladi-hills-background',
    canvasWidth: 1586,
    canvasHeight: 992,
    overlays: [
      { id: 'garden-hanging-leaves', motion: 'sway', xPercent: 91, yPercent: 6, scale: 1 },
      { id: 'garden-wind-spinner', motion: 'spin', xPercent: 77, yPercent: 15, scale: 1 },
    ],
  },
  {
    id: 'living-ladi-classic',
    houseThemeId: 'sunny-dollhouse',
    zoneId: 'living-room',
    nameKey: 'catalog.roomDesigns.living-ladi-classic.name',
    descriptionKey: 'catalog.roomDesigns.living-ladi-classic.description',
    icon: '🛋️',
    price: 0,
    minimumHouseLevel: 0,
    ownedByDefault: true,
    backgroundAssetId: 'living-ladi-classic-background',
    canvasWidth: 1672,
    canvasHeight: 941,
    overlays: [],
  },
  {
    id: 'kitchen-ladi-classic',
    houseThemeId: 'sunny-dollhouse',
    zoneId: 'kitchen',
    nameKey: 'catalog.roomDesigns.kitchen-ladi-classic.name',
    descriptionKey: 'catalog.roomDesigns.kitchen-ladi-classic.description',
    icon: '🍳',
    price: 0,
    minimumHouseLevel: 0,
    ownedByDefault: true,
    backgroundAssetId: 'kitchen-ladi-classic-background',
    canvasWidth: 1672,
    canvasHeight: 941,
    overlays: [],
  },
  ...HOUSE_THEMES
    .filter(isThemedHouseTheme)
    .flatMap(theme => createThemedRoomDesigns(theme.id, theme.icon)),
] as const satisfies ReadonlyArray<RoomDesignDefinition>;

export const DEFAULT_ROOM_DESIGNS = ROOM_DESIGNS.filter(design => design.ownedByDefault);

export const roomDesignsForTheme = (themeId: HouseThemeId): ReadonlyArray<RoomDesignDefinition> =>
  ROOM_DESIGNS.filter(design => design.houseThemeId === themeId);
