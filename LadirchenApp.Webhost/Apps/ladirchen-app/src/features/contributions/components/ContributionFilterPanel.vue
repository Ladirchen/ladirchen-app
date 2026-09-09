<template>
  <section class="contribution-filter" aria-labelledby="contribution-filter-title">
    <div class="filter-heading">
      <div class="filter-mascot" aria-hidden="true">
        <svg class="filter-eyes" viewBox="0 0 42 28">
          <g class="filter-eye filter-eye--left">
            <ellipse cx="12" cy="14" rx="9" ry="11" />
            <circle class="filter-pupil" cx="14" cy="15" r="3" />
            <circle class="filter-glint" cx="15" cy="14" r="1" />
          </g>
          <g class="filter-eye filter-eye--right">
            <ellipse cx="30" cy="14" rx="9" ry="11" />
            <circle class="filter-pupil" cx="32" cy="15" r="3" />
            <circle class="filter-glint" cx="33" cy="14" r="1" />
          </g>
        </svg>
      </div>
      <div>
        <h2 id="contribution-filter-title">{{ t('contributions.filter.title') }}</h2>
        <p>{{ t('contributions.filter.description') }}</p>
      </div>
    </div>

    <div class="scope-options" role="group" :aria-label="t('contributions.filter.scopeAria')">
      <button
        v-for="option in scopeOptions"
        :key="option.value"
        :aria-pressed="scope === option.value"
        :class="{ active: scope === option.value }"
        type="button"
        @click="scope = option.value"
      >
        <v-icon :icon="option.icon" size="25" />
        <strong>{{ option.title }}</strong>
        <span>{{ option.description }}</span>
        <b v-if="option.value === 'open'">{{ openCount }}</b>
      </button>
    </div>

    <div class="kind-filter">
      <span class="kind-filter-label">{{ t('contributions.filter.status') }}</span>
      <div class="status-options" role="group" :aria-label="t('contributions.filter.statusAria')">
        <button
          v-for="option in statusOptions"
          :key="option.value"
          :aria-pressed="status === option.value"
          :class="{ active: status === option.value }"
          type="button"
          @click="selectStatus(option.value)"
        >
          <v-icon :icon="option.icon" size="15" />{{ option.title }}
        </button>
      </div>
    </div>

    <div class="kind-filter kind-filter--type">
      <span class="kind-filter-label">{{ t('contributions.filter.kind') }}</span>
      <div class="kind-options" role="group" :aria-label="t('contributions.filter.kindAria')">
        <button
          v-for="option in kindOptions"
          :key="option.value"
          :aria-pressed="kind === option.value"
          :class="{ active: kind === option.value }"
          type="button"
          @click="kind = option.value"
        >
          <span aria-hidden="true">{{ option.icon }}</span>{{ option.title }}
        </button>
      </div>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

import type { ContributionKind } from '@/domain/types';

type ContributionScope = 'all' | 'mine' | 'open';
type KindFilter = 'all' | ContributionKind;
type StatusFilter = 'open' | 'completed';

defineProps<{ openCount: number }>();
const scope = defineModel<ContributionScope>('scope', { required: true });
const kind = defineModel<KindFilter>('kind', { required: true });
const status = defineModel<StatusFilter>('status', { required: true });
const { t } = useI18n();

const scopeOptions = computed<Array<{
  description: string;
  icon: string;
  title: string;
  value: ContributionScope;
}>>(() => [
  { value: 'mine', title: t('contributions.filter.scope.mine.title'), description: t('contributions.filter.scope.mine.description'), icon: 'mdi-account-star-outline' },
  { value: 'open', title: t('contributions.filter.scope.open.title'), description: t('contributions.filter.scope.open.description'), icon: 'mdi-hand-wave-outline' },
  { value: 'all', title: t('contributions.filter.scope.all.title'), description: t('contributions.filter.scope.all.description'), icon: 'mdi-view-grid-outline' },
]);

const kindOptions = computed<Array<{ icon: string; title: string; value: KindFilter }>>(() => [
  { value: 'all', title: t('contributions.filter.kinds.all'), icon: '✨' },
  { value: 'basic', title: t('contributions.filter.kinds.energy'), icon: '⚡' },
  { value: 'extra', title: t('contributions.filter.kinds.extra'), icon: '🪙' },
]);

const statusOptions = computed<Array<{ icon: string; title: string; value: StatusFilter }>>(() => [
  { value: 'open', title: t('contributions.filter.statuses.open'), icon: 'mdi-progress-clock' },
  { value: 'completed', title: t('contributions.filter.statuses.completed'), icon: 'mdi-check-circle-outline' },
]);

const selectStatus = (value: StatusFilter) => {
  status.value = value;
  if (value === 'completed' && scope.value === 'open') {scope.value = 'mine';}
};
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.contribution-filter {
  position: relative;
  isolation: isolate;
  padding: 0.9375rem;
  overflow: hidden;
  @include raised-surface(
    color-mix(in srgb, var(--lad-palette-mint) 25%, transparent),
    color-mix(in srgb, var(--lad-palette-mint) 12%, transparent),
    1.75rem,
    0.4375rem
  );
  background:
    radial-gradient(
      circle at 92% 7%,
      color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent) 0 2.375rem,
      transparent 2.4375rem
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-100)
    );
  box-shadow:
    0 0.4375rem 0 color-mix(in srgb, var(--lad-palette-mint) 12%, transparent),
    0 0.875rem 1.5rem
      color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent);
}
.filter-heading {
  margin-bottom: 0.8125rem;
  @apply d-flex align-center;
  gap: 0.625rem;
}
.filter-heading h2 {
  @apply ma-0;
  font-size: 1rem;
  letter-spacing: -0.02em;
}
.filter-heading p {
  margin: 0.0625rem 0 0;
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.filter-mascot {
  @include icon-tile(
    2.8125rem,
    1rem,
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-150)
    ),
    color-mix(in srgb, var(--lad-palette-mint-strong) 20%, transparent),
    -4deg,
    0.1875rem solid var(--lad-palette-white)
  );
  flex-basis: 2.8125rem;
}
.filter-eyes {
  width: 2rem;
  height: 1.5625rem;
  @apply overflow-visible;
}
.filter-eye {
  transform-box: fill-box;
  transform-origin: center;
}
.filter-eye > ellipse {
  fill: var(--lad-palette-white);
  stroke: var(--lad-palette-muted-350);
  stroke-width: 1.2;
}
.filter-pupil {
  fill: var(--lad-palette-text);
  transform-box: fill-box;
  transform-origin: center;
  animation: filter-look 8.6s ease-in-out infinite;
}
.filter-glint {
  fill: white;
}
.filter-eye--left {
  animation: filter-wink 7.4s 1.1s ease-in-out infinite;
}
.filter-eye--right {
  animation: filter-blink 11.3s 3.2s ease-in-out infinite;
}
.scope-options {
  @apply d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
  padding-bottom: 0.4375rem;
  border-bottom: 0.5rem solid
    color-mix(in srgb, var(--lad-palette-amber-200) 35%, transparent);
  border-radius: 0 0 1.375rem 1.375rem;
}
.scope-options button {
  @apply min-w-0;
  min-height: 6.25rem;
  padding: 0.625rem 0.3125rem 0.5625rem;
  @apply position-relative d-flex flex-column align-center justify-center;
  gap: 0.1875rem;
  color: var(--lad-text);
  border: 0.125rem solid var(--lad-palette-teal-150);
  border-radius: 1.4375rem 1.4375rem 1.0625rem 1.0625rem;
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow: 0 0.3125rem 0 var(--lad-palette-teal-150);
  @apply cursor-pointer;
  font: inherit;
  transition:
    transform 150ms ease,
    border-color 150ms ease,
    background 150ms ease,
    box-shadow 150ms ease;
}
.scope-options button:nth-child(2) {
  border-color: var(--lad-palette-amber-200);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 0.3125rem 0 var(--lad-palette-amber-200);
}
.scope-options button:nth-child(3) {
  border-color: var(--lad-palette-blue-150);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow: 0 0.3125rem 0 var(--lad-palette-purple-200);
}
.scope-options button:hover {
  transform: translateY(-0.1875rem);
}
.scope-options button:active {
  transform: translateY(0.125rem);
  box-shadow: 0 0.125rem 0
    color-mix(in srgb, var(--lad-palette-muted-700) 10%, transparent);
}
.scope-options button.active {
  color: var(--lad-palette-teal-700);
  transform: translateY(-0.3125rem) rotate(-1deg);
  border-color: var(--lad-palette-mint);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 0.5rem 0 var(--lad-palette-muted-250),
    0 0.75rem 1rem
      color-mix(in srgb, var(--lad-palette-teal-700) 10%, transparent);
}
.scope-options button:nth-child(2).active {
  color: var(--lad-palette-amber-650);
  transform: translateY(-0.3125rem) rotate(1deg);
  border-color: var(--lad-palette-amber-450);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-amber-150)
  );
  box-shadow:
    0 0.5rem 0 var(--lad-palette-orange-350),
    0 0.75rem 1rem
      color-mix(in srgb, var(--lad-palette-amber-650) 10%, transparent);
}
.scope-options button:nth-child(3).active {
  color: var(--lad-palette-violet-500);
  border-color: var(--lad-palette-purple-350);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 0.5rem 0 var(--lad-palette-purple-200),
    0 0.75rem 1rem
      color-mix(in srgb, var(--lad-palette-violet-650) 10%, transparent);
}
.scope-options button :deep(.v-icon) {
  width: 2.5rem;
  height: 2.5rem;
  margin-bottom: 0.1875rem;
  border: 0.1875rem solid var(--lad-palette-white);
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-white) 65%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent);
}
.scope-options button strong {
  font-size: 0.625rem;
  line-height: 1.2;
}
.scope-options button span {
  color: var(--lad-muted);
  font-size: 0.4375rem;
  line-height: 1.2;
}
.scope-options button b {
  min-width: 1.3125rem;
  height: 1.3125rem;
  padding: 0 0.3125rem;
  @apply position-absolute;
  top: 0.375rem;
  right: 0.375rem;
  @apply d-grid place-center;
  color: var(--lad-palette-amber-700);
  border: 0.125rem solid var(--lad-palette-white);
  border-radius: 0.5rem;
  background: var(--lad-palette-amber-150);
  box-shadow: 0 0.125rem 0 var(--lad-palette-orange-350);
  font-size: 0.5625rem;
}
.kind-filter {
  margin-top: 0.8125rem;
  padding: 0.625rem;
  @apply d-flex align-center;
  gap: 0.5625rem;
  border: 0.0625rem dashed
    color-mix(in srgb, var(--lad-palette-muted-700) 20%, transparent);
  border-radius: 1rem;
  background: color-mix(in srgb, var(--lad-palette-white) 50%, transparent);
}
.kind-filter--type {
  margin-top: 0.4375rem;
}
.kind-filter-label {
  flex: 0 0 auto;
  @include overline(var(--lad-muted), var(--lad-font-size-micro), 0.06em);
  font-weight: var(--lad-font-weight-strong);
}
.kind-options,
.status-options {
  @apply d-flex flex-wrap;
  gap: 0.375rem;
}
.kind-options button,
.status-options button {
  padding: 0.4375rem 0.5625rem;
  @apply d-inline-flex align-center ga-1;
  color: var(--lad-palette-muted-700);
  border: 0.0625rem solid
    color-mix(in srgb, var(--lad-palette-teal-600) 15%, transparent);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  box-shadow: 0 0.125rem 0
    color-mix(in srgb, var(--lad-palette-muted-700) 8%, transparent);
  @apply cursor-pointer;
  font: inherit;
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-strong);
}
.kind-options button.active,
.status-options button.active {
  color: var(--lad-palette-teal-700);
  transform: translateY(-0.0625rem);
  border-color: var(--lad-palette-teal-400);
  background: white;
  box-shadow: 0 0.25rem 0 var(--lad-palette-teal-150);
}
.status-options button:last-child.active {
  color: var(--lad-palette-blue-600);
  border-color: var(--lad-palette-blue-250);
  box-shadow: 0 0.25rem 0 var(--lad-palette-blue-150);
}
@keyframes filter-wink {
  0%,
  43%,
  47%,
  100% {
    transform: scaleY(1);
  }
  44.5%,
  46% {
    transform: scaleY(0.08);
  }
}
@keyframes filter-blink {
  0%,
  68%,
  71%,
  100% {
    transform: scaleY(1);
  }
  69%,
  70% {
    transform: scaleY(0.08);
  }
}
@keyframes filter-look {
  0%,
  18%,
  100% {
    transform: translateX(0);
  }
  28%,
  45% {
    transform: translateX(-0.125rem);
  }
  58%,
  75% {
    transform: translateX(0.125rem);
  }
}
@include respond-down(compact) {
  .scope-options button {
    min-height: 5rem;
    padding-inline: 0.1875rem;
  }
  .scope-options button strong {
    font-size: 0.625rem;
  }
  .kind-filter {
    @apply align-start flex-column;
    gap: 0.4375rem;
  }
}
@include reduced-motion {
  .filter-eye,
  .filter-pupil {
    animation: none;
  }
}
</style>
