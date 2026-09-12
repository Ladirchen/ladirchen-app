# Frontend styling conventions

The frontend uses three complementary styling layers:

- **UnoCSS utilities** for layout, spacing, positioning, and simple state-independent rules.
- **SCSS tokens and mixins** for repeated visual rules that need parameters, such as typography, raised surfaces, icon tiles, dialog chrome, and responsive breakpoints.
- **Scoped component styles** for feature-specific composition, gradients, and illustrations.

## Product-area tones

The eight persistent areas use semantic tones rather than page-local color choices: `world`, `contributions`, `wishes`, `shop`, `family`, `profile`, `streak`, and `balance`. Their accent, tint, and lift tokens are defined once in `main.scss` as `--lad-tone-<area>-*` properties.

Use `BrandedCard` for repeated application cards and select the area with its required `tone` prop. Keep card padding and layout in UnoCSS classes; do not recreate the border, gradient, radius, or lift shadow in page SCSS. Navigation and compact header surfaces consume the same area-tone tokens directly.

## Shared tokens

`src/theme/color-palette.ts` is the single source of truth for every application and illustration colour. It contains the consolidated visual palette, semantic application colours, and typed domain palettes. Application startup installs the visual colours as `--lad-palette-*` CSS custom properties, so SCSS and Vue styles consume the same values without maintaining a second palette.

Use the semantic `--lad-color-*`, `--lad-border-*`, `--lad-shadow-*`, and `--lad-gradient-*` tokens for ordinary cards, status feedback, rewards, bonuses, and raised surfaces. Use `--lad-palette-*` only for feature artwork and CSS illustrations. Reuse the closest existing tone instead of adding a nearly identical colour.

Direct hexadecimal and `rgb()`/`rgba()` literals are prohibited outside `src/theme/color-palette.ts`. `pnpm colors:check` enforces this rule and is also part of `pnpm lint`. When a genuinely new colour is required, add it to the appropriate typed palette first.

SCSS-only values, currently the named responsive breakpoints, live in `_tokens.scss`.

## Units

- Use `rem` for text, spacing, control sizes, radii, and responsive UI dimensions so browser font preferences can scale the interface.
- Use `em` for media-query breakpoints. Access them through `respond-down` or `respond-up`; do not add raw component-level media queries.
- Keep `px` only for genuine drawing geometry: SVG view-box details, strokes, hairlines, and pixel-precise CSS illustrations. A visual element is not automatically an illustration just because it contains an icon.
- Unitless values remain appropriate for line height, opacity, scale, flex factors, and SVG ratios.

## Shared mixins

Import `_mixins.scss` explicitly in a scoped style block:

```scss
<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.feature-title {
  @include heading(var(--lad-font-size-section));
}

@include respond-down(phone) {
  // Component-specific responsive composition.
}
</style>
```

Use an existing mixin when a rule represents the same UI concept in multiple components. Pass colors, size, radius, and elevation as parameters instead of copying a nearly identical block.

The contribution-label and contribution-metadata mixins are shared by the home and contribution views. Changes to that task presentation belong in the shared mixins rather than in one of the two pages.

Do not create a mixin for a one-off selector or move illustration-specific colors into the shared theme. Those details should stay close to the component that owns them.
