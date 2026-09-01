<template>
  <header class="family-world-page-header" :class="`family-world-page-header--${tone}`">
    <div class="page-header-copy">
      <p class="page-header-kicker">{{ eyebrow }}</p>
      <h1>{{ title }}</h1>
      <p>{{ description }}</p>
    </div>
    <div class="page-header-side">
      <div v-if="icon" class="page-header-icon" aria-hidden="true">
        <v-icon :icon="icon" size="30" />
      </div>
      <slot name="action" />
    </div>
  </header>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{
  description: string;
  eyebrow: string;
  icon?: string;
  title: string;
  tone?: 'mint' | 'blue' | 'amber' | 'coral';
}>(), {
  icon: undefined,
  tone: 'mint',
});
</script>

<style scoped>
.family-world-page-header {
  --header-accent: #2e9d75;
  --header-accent-soft: #dff5e9;
  min-height: 138px;
  margin-bottom: 20px;
  padding: 20px;
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  border: 1px solid color-mix(in srgb, var(--header-accent) 18%, white);
  border-radius: 24px;
  background: linear-gradient(
    145deg,
    #fffdf8 0%,
    var(--header-accent-soft) 145%
  );
  box-shadow: 0 5px 0 color-mix(in srgb, var(--header-accent) 14%, transparent);
}
.family-world-page-header::before,
.family-world-page-header::after {
  content: "";
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
}
.family-world-page-header::before {
  width: 130px;
  height: 130px;
  top: -77px;
  right: -32px;
  background: color-mix(in srgb, var(--header-accent-soft) 68%, transparent);
  box-shadow: 0 0 0 18px
    color-mix(in srgb, var(--header-accent-soft) 35%, transparent);
}
.family-world-page-header::after {
  width: 90px;
  height: 28px;
  right: 48px;
  bottom: -17px;
  background: color-mix(in srgb, var(--header-accent-soft) 55%, transparent);
}
.family-world-page-header--blue {
  --header-accent: #4e8fdd;
  --header-accent-soft: #e7f3ff;
}
.family-world-page-header--amber {
  --header-accent: #dfa03d;
  --header-accent-soft: #fff0c9;
}
.family-world-page-header--coral {
  --header-accent: #e87867;
  --header-accent-soft: #ffe8e2;
}
.page-header-copy {
  max-width: 310px;
  position: relative;
  z-index: 1;
}
.page-header-kicker {
  margin: 0 0 5px;
  color: var(--header-accent);
  font-size: 10px;
  font-weight: 950;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
h1 {
  margin: 0;
  color: var(--lad-text);
  font-size: 27px;
  line-height: 1.08;
  letter-spacing: -0.045em;
}
.page-header-copy > p:last-child {
  margin: 7px 0 0;
  color: var(--lad-muted);
  font-size: 12px;
  line-height: 1.45;
}
.page-header-side {
  min-width: 54px;
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: 8px;
}
.page-header-icon {
  width: 54px;
  height: 54px;
  display: grid;
  place-items: center;
  color: var(--header-accent);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.74);
  box-shadow: 0 4px 0 color-mix(in srgb, var(--header-accent) 13%, transparent);
}
@media (max-width: 390px) {
  .family-world-page-header {
    min-height: 126px;
    padding: 17px;
  }
  .page-header-copy {
    max-width: 245px;
  }
  .page-header-copy > p:last-child {
    font-size: 11px;
  }
  .page-header-icon {
    width: 46px;
    height: 46px;
    border-radius: 15px;
  }
}
</style>
