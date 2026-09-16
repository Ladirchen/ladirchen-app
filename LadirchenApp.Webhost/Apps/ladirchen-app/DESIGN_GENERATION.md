# Design generation

New avatar parts, furniture, outdoor accessories, and house themes are scaffolded with one command. The generator registers IDs, visuals, catalogue metadata, and English and German translations together, so a new design cannot be forgotten in one of the registration layers.

## Avatar designs

Supported categories are `face`, `face-shape`, `hair`, `outfit`, `accessory`, `fun-accessory`, and `seasonal-accessory`.

```bash
pnpm generate:design avatar hair side-braid \
  --label-en="Side braid" \
  --label-de="Seitenzopf"

pnpm generate:design avatar outfit raincoat
pnpm generate:design avatar face-shape heart-shaped
pnpm generate:design avatar accessory tiara --head-clearance=true
pnpm generate:design avatar outfit evening-dress --profile=guardian
```

The generated Vue component is placed in `src/features/avatar/components/avatar-visuals/`. Hair, outfit, and fun-accessory components receive a `layer` property because those designs may render behind and in front of the avatar. Outfit components support the `back`, `head`, and `front` layers.

Each avatar category owns a typed catalogue in `src/domain/avatar/`. The generator adds the catalogue definition, derived ID type, visual registration, and translations together. Use `--profile=child` or `--profile=guardian` to restrict a part; the default is `all`. Availability is therefore configured on the catalogue entry instead of in parallel child/adult exclusion lists. After generation, only the visual component needs its final artwork.

Regular accessories use `--head-clearance=true` by default. Set it to `false` for face accessories such as glasses that remain visible when the avatar wears headwear.

## Furniture and outdoor accessories

Indoor furniture defaults to the living room. Catalogue state, translation keys, category, placement kind, and motion defaults come from the shared definition helpers.

```bash
pnpm generate:design furniture indoor reading-chair \
  --room=living-room \
  --price=90 \
  --icon=🪑

pnpm generate:design furniture outdoor bird-bath \
  --price=120 \
  --icon=🐦

pnpm generate:design furniture outdoor magic-gate \
  --category=special

pnpm generate:design furniture outdoor family-banner \
  --category=special \
  --motion=wave \
  --icon=🚩
```

The explicit `indoor` and `outdoor` forms select the correct catalogue placement and default zone. The older `--placement=inside|outside` option remains available when the area is omitted. An outdoor item automatically uses the same generated visual in the shop, the furniture-storage tray, and the garden; no separate storage design is required.

Special animated items support `--motion=wave`, `--motion=glow`, and `--motion=flutter`. The generated SVG contains an editable starter animation and a reduced-motion fallback. Use `--motion=none` for a static item.

## Scheduled NEW / NEU badges

Furniture, outdoor or special accessories, and house themes can receive a time-limited badge. `--new` starts the badge on the generation date and keeps it visible for 14 calendar days:

```bash
pnpm generate:design furniture indoor reading-chair --new
pnpm generate:design house-theme ocean-home --new
```

Schedule a future launch or choose another duration with `--new-from` and `--new-days`:

```bash
pnpm generate:design furniture outdoor winter-flag \
  --category=special \
  --motion=wave \
  --new-from=2026-12-01 \
  --new-days=21
```

Alternatively, use an inclusive end date with `--new-until=YYYY-MM-DD`. The generator stores the result as `newBadge: { from, until }` in the catalogue definition. The shop checks the dates automatically and displays `NEW` in English or `NEU` in German only while the schedule is active.

Useful options are `--level`, `--x`, `--y`, `--scale`, and `--set`. Default layout coordinates are stored with the catalogue definition and automatically become a house-layout placement.

## House themes

```bash
pnpm generate:design house-theme ocean-home \
  --kind=fantasy \
  --price=350 \
  --icon=🌊
```

The generator creates a theme palette, catalogue definition, optional SVG decoration component, registry entry, and both translations. Generated colours initially match the default house and should be adjusted in `src/theme/color-palette.ts`. Edit or remove the placeholder shapes in the generated theme visual as required.

## Verification and safety

Preview planned changes without writing files:

```bash
pnpm generate:design avatar hair side-braid --dry-run
```

After editing the generated SVG design, verify the application:

```bash
pnpm lint
pnpm type-check
pnpm build
```

The registries are typed against the domain IDs. Missing generated avatar visuals and missing furniture visual registrations therefore fail the TypeScript check.
