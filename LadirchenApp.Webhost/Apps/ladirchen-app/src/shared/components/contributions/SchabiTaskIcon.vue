<template>
  <span class="schabi-task-icon" :aria-label="t('contributions.schabiTask')">S</span>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();
</script>

<style lang="scss" scoped>
.schabi-task-icon {
  width: 1.55em;
  aspect-ratio: 1;
  display: inline-grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: 0.38em;
  background: var(--lad-color-info);
  color: var(--lad-surface-raised);
  font-family: Arial, sans-serif;
  font-size: 0.82em;
  font-weight: 800;
  line-height: 1;
  box-shadow: inset 0 -0.12em 0 rgb(0 0 0 / 14%);
}
</style>
