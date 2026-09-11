<template>
  <span class="header-decoration" :class="`header-decoration--${tone}`" aria-hidden="true">
    <i />
    <i />
    <i />
  </span>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{
  tone?: 'world' | 'contributions' | 'wishes' | 'shop' | 'family' | 'profile' | 'streak' | 'balance';
}>(), {
  tone: 'world',
});
</script>

<style lang="scss" scoped>
.header-decoration {
  --decoration-accent: var(--lad-tone-world-accent);
  --decoration-tint: var(--lad-tone-world-tint);
  --decoration-companion: var(--lad-tone-world-companion);
  position: absolute;
  inset: 0;
  overflow: hidden;
  border-radius: inherit;
  pointer-events: none;
}

.header-decoration i {
  position: absolute;
  display: block;
  border-radius: 50%;
  background: color-mix(in srgb, var(--decoration-tint) 72%, transparent);
}

.header-decoration i:first-child {
  width: 8rem;
  height: 8rem;
  top: -4.75rem;
  right: -1.75rem;
  box-shadow: 0 0 0 1.125rem
    color-mix(in srgb, var(--decoration-companion) 42%, transparent);
}

.header-decoration i:nth-child(2) {
  width: 5.5rem;
  height: 2rem;
  right: 3rem;
  bottom: -1.125rem;
  background: color-mix(in srgb, var(--decoration-companion) 62%, transparent);
}

.header-decoration i:last-child {
  width: 2.375rem;
  height: 2.375rem;
  top: 1rem;
  right: 6.75rem;
  border: 0.5rem solid
    color-mix(in srgb, var(--decoration-accent) 9%, transparent);
  background: transparent;
}

.header-decoration--contributions {
  --decoration-accent: var(--lad-tone-contributions-accent);
  --decoration-tint: var(--lad-tone-contributions-tint);
  --decoration-companion: var(--lad-tone-contributions-companion);
}
.header-decoration--wishes {
  --decoration-accent: var(--lad-tone-wishes-accent);
  --decoration-tint: var(--lad-tone-wishes-tint);
  --decoration-companion: var(--lad-tone-wishes-companion);
}
.header-decoration--shop {
  --decoration-accent: var(--lad-tone-shop-accent);
  --decoration-tint: var(--lad-tone-shop-tint);
  --decoration-companion: var(--lad-tone-shop-companion);
}
.header-decoration--family {
  --decoration-accent: var(--lad-tone-family-accent);
  --decoration-tint: var(--lad-tone-family-tint);
  --decoration-companion: var(--lad-tone-family-companion);
}
.header-decoration--profile {
  --decoration-accent: var(--lad-tone-profile-accent);
  --decoration-tint: var(--lad-tone-profile-tint);
  --decoration-companion: var(--lad-tone-profile-companion);
}
.header-decoration--streak {
  --decoration-accent: var(--lad-tone-streak-accent);
  --decoration-tint: var(--lad-tone-streak-tint);
  --decoration-companion: var(--lad-tone-streak-companion);
}
.header-decoration--balance {
  --decoration-accent: var(--lad-tone-balance-accent);
  --decoration-tint: var(--lad-tone-balance-tint);
  --decoration-companion: var(--lad-tone-balance-companion);
}
</style>
