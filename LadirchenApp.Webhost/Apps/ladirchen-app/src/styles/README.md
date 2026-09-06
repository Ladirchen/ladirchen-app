# Frontend styling conventions

The frontend uses three complementary styling layers:

- **UnoCSS utilities** for layout, spacing, positioning, and simple state-independent rules.
- **SCSS tokens and mixins** for repeated visual rules that need parameters, such as typography, raised surfaces, icon tiles, dialog chrome, and responsive breakpoints.
- **Scoped component styles** for feature-specific composition, gradients, and illustrations.

## Shared tokens

Runtime design tokens are defined as CSS custom properties in `main.scss`. Theme colors that belong to the family-world shell remain in `family-world.scss`.

Use the semantic `--lad-color-*`, `--lad-border-*`, `--lad-shadow-*`, and `--lad-gradient-*` tokens for ordinary cards, status feedback, rewards, bonuses, and raised surfaces. Do not copy their literal colour values into feature components. SVG artwork and CSS illustrations may keep a local palette because those colours describe the drawing rather than an application state.

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
