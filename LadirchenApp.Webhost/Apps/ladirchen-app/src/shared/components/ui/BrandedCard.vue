<template>
  <v-card
    class="branded-card"
    :class="[`branded-card--${tone}`, { 'branded-card--interactive': interactive }]"
    elevation="0"
    rounded="xl"
  >
    <slot />
  </v-card>
</template>

<script lang="ts" setup>
export type BrandedCardTone =
  | "balance"
  | "contributions"
  | "family"
  | "profile"
  | "shop"
  | "streak"
  | "wishes"
  | "world";

withDefaults(defineProps<{
  interactive?: boolean;
  tone: BrandedCardTone;
}>(), {
  interactive: false,
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.branded-card {
  @include branded-card;

  transition:
    border-color var(--lad-motion-normal) ease,
    box-shadow var(--lad-motion-normal) ease,
    transform var(--lad-motion-normal) ease;
}

@each $tone in world, contributions, wishes, shop, family, profile, streak,
  balance
{
  .branded-card--#{$tone} {
    --branded-card-accent: var(--lad-tone-#{$tone}-accent);
    --branded-card-tint: var(--lad-tone-#{$tone}-tint);
    --branded-card-companion: var(--lad-tone-#{$tone}-companion);
    --branded-card-lift: var(--lad-tone-#{$tone}-lift);
  }
}

.branded-card--interactive:hover {
  transform: translateY(rem(-2));
  box-shadow:
    0 rem(7) 0 color-mix(in srgb, var(--branded-card-lift) 18%, transparent),
    0 rem(15) rem(26)
      color-mix(in srgb, var(--branded-card-accent) 11%, transparent);
}
</style>
