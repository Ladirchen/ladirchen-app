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
        <span class="scope-option-icon" aria-hidden="true">
          <v-icon :icon="option.icon" size="25" />
        </span>
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
import { computed } from "vue";
import { useI18n } from "vue-i18n";

import type { ContributionKind } from "@/domain/contributions/types";
import { UI_ICONS } from "@/shared/ui-icons";

type ContributionScope = "all" | "mine" | "open";
type KindFilter = "all" | ContributionKind;
type StatusFilter = "open" | "completed";

defineProps<{ openCount: number }>();
const scope = defineModel<ContributionScope>("scope", { required: true });
const kind = defineModel<KindFilter>("kind", { required: true });
const status = defineModel<StatusFilter>("status", { required: true });
const { t } = useI18n();

const scopeOptions = computed<Array<{
  description: string;
  icon: string;
  title: string;
  value: ContributionScope;
}>>(() => [
  { value: "mine", title: t("contributions.filter.scope.mine.title"), description: t("contributions.filter.scope.mine.description"), icon: UI_ICONS.contributionScope.mine },
  { value: "open", title: t("contributions.filter.scope.open.title"), description: t("contributions.filter.scope.open.description"), icon: UI_ICONS.contributionScope.open },
  { value: "all", title: t("contributions.filter.scope.all.title"), description: t("contributions.filter.scope.all.description"), icon: UI_ICONS.contributionScope.all },
]);

const kindOptions = computed<Array<{ icon: string; title: string; value: KindFilter }>>(() => [
  { value: "all", title: t("contributions.filter.kinds.all"), icon: "✨" },
  { value: "basic", title: t("contributions.filter.kinds.energy"), icon: "⚡" },
  { value: "extra", title: t("contributions.filter.kinds.extra"), icon: "🪙" },
]);

const statusOptions = computed<Array<{ icon: string; title: string; value: StatusFilter }>>(() => [
  { value: "open", title: t("contributions.filter.statuses.open"), icon: UI_ICONS.contributionStatus.open },
  { value: "completed", title: t("contributions.filter.statuses.completed"), icon: UI_ICONS.contributionStatus.completed },
]);

const selectStatus = (value: StatusFilter) => {
  status.value = value;
  if (value === "completed" && scope.value === "open") {scope.value = "mine";}
};
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.contribution-filter {
  --uno: position-relative overflow-hidden;
  isolation: isolate;
  padding: rem(15);

  @include raised-surface(
    color-mix(in srgb, var(--lad-color-primary) 25%, transparent),
    color-mix(in srgb, var(--lad-color-primary) 12%, transparent),
    1.75rem,
    rem(7)
  );
  background:
    radial-gradient(
      circle at 92% 7%,
      color-mix(in srgb, var(--lad-color-reward) 20%, transparent) 0 rem(38),
      transparent rem(39)
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-soft)
    );
  box-shadow:
    0 rem(7) 0 color-mix(in srgb, var(--lad-color-primary) 12%, transparent),
    0 rem(14) 1.5rem
      color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.filter-heading {
  margin-bottom: rem(13);
  --uno: d-flex align-center;
  gap: rem(10);
}
.filter-heading h2 {
  --uno: ma-0;
  font-size: 1rem;
  letter-spacing: -0.02em;
}
.filter-heading p {
  margin: rem(1) 0 0;
  color: var(--lad-muted);
  font-size: rem(10);
}
.filter-mascot {
  @include icon-tile(
    rem(45),
    1rem,
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-pale)
    ),
    color-mix(in srgb, var(--lad-color-primary-strong) 20%, transparent),
    -4deg,
    rem(3) solid var(--lad-surface-raised)
  );
  flex-basis: rem(45);
}
.filter-eyes {
  width: 2rem;
  height: rem(25);
  --uno: overflow-visible;
}
.filter-eye {
  transform-box: fill-box;
  transform-origin: center;
}
.filter-eye > ellipse {
  fill: var(--lad-surface-raised);
  stroke: var(--lad-text-subtle);
  stroke-width: 1.2;
}
.filter-pupil {
  fill: var(--lad-text);
  transform-box: fill-box;
  transform-origin: center;
  animation: filter-look 8.6s ease-in-out infinite;
}
.filter-glint {
  fill: var(--lad-surface-raised);
}
.filter-eye--left {
  animation: filter-wink 7.4s 1.1s ease-in-out infinite;
}
.filter-eye--right {
  animation: filter-blink 11.3s 3.2s ease-in-out infinite;
}
.scope-options {
  --uno: d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
  padding-bottom: rem(7);
  border-bottom: 0.5rem solid
    color-mix(in srgb, var(--lad-color-reward-muted) 35%, transparent);
  border-radius: 0 0 rem(22) rem(22);
}
.scope-options button {
  --uno: min-w-0 position-relative d-flex flex-column align-center justify-center cursor-pointer;
  min-height: 6.25rem;
  padding: rem(10) rem(5) rem(9);

  gap: rem(3);
  color: var(--lad-text);
  border: rem(2) solid var(--lad-color-primary-soft);
  border-radius: rem(23) rem(23) rem(17) rem(17);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 rem(5) 0 var(--lad-color-primary-soft);

  font: inherit;
  transition:
    transform 150ms ease,
    border-color 150ms ease,
    background 150ms ease,
    box-shadow 150ms ease;
}
.scope-options button:nth-child(2) {
  border-color: var(--lad-color-reward-muted);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 rem(5) 0 var(--lad-color-reward-muted);
}
.scope-options button:nth-child(3) {
  border-color: var(--lad-color-info-soft);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 rem(5) 0 var(--lad-color-bonus-soft);
}
.scope-options button:hover {
  transform: translateY(rem(-3));
}
.scope-options button:active {
  transform: translateY(rem(2));
  box-shadow: 0 rem(2) 0
    color-mix(in srgb, var(--lad-text-strong) 10%, transparent);
}
.scope-options button.active {
  color: var(--lad-color-primary-deep);
  transform: translateY(rem(-5)) rotate(-1deg);
  border-color: var(--lad-color-primary);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow:
    0 0.5rem 0 var(--lad-neutral-soft),
    0 0.75rem 1rem
      color-mix(in srgb, var(--lad-color-primary-deep) 10%, transparent);
}
.scope-options button:nth-child(2).active {
  color: var(--lad-color-reward-ink);
  transform: translateY(rem(-5)) rotate(1deg);
  border-color: var(--lad-color-reward-border);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-color-reward-pale)
  );
  box-shadow:
    0 0.5rem 0 var(--lad-color-accent-warm-soft),
    0 0.75rem 1rem
      color-mix(in srgb, var(--lad-color-reward-ink) 10%, transparent);
}
.scope-options button:nth-child(3).active {
  color: var(--lad-color-bonus-muted);
  border-color: var(--lad-color-bonus-highlight);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow:
    0 0.5rem 0 var(--lad-color-bonus-soft),
    0 0.75rem 1rem
      color-mix(in srgb, var(--lad-color-bonus-strong) 10%, transparent);
}
.scope-options button :deep(.v-icon) {
  width: 2.5rem;
  height: 2.5rem;
  margin-bottom: rem(3);
  border: rem(3) solid var(--lad-border-on-accent);
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-surface-raised) 65%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 12%, transparent);
}
.scope-options button strong {
  font-size: rem(10);
  line-height: 1.2;
}
.scope-options button span {
  color: var(--lad-muted);
  font-size: rem(7);
  line-height: 1.2;
}
.scope-options button b {
  min-width: rem(21);
  height: rem(21);
  padding: 0 rem(5);
  --uno: position-absolute d-grid place-center;
  top: rem(6);
  right: rem(6);

  color: var(--lad-color-reward-strong);
  border: rem(2) solid var(--lad-border-on-accent);
  border-radius: 0.5rem;
  background: var(--lad-color-reward-pale);
  box-shadow: 0 rem(2) 0 var(--lad-color-accent-warm-soft);
  font-size: rem(9);
}
.scope-option-icon {
  margin-bottom: rem(3);
  color: var(--lad-text-inverse);
  @include icon-tile(
    rem(42),
    rem(15),
    linear-gradient(
      145deg,
      var(--lad-color-primary-highlight),
      var(--lad-color-primary-strong)
    ),
    var(--lad-color-primary-deep),
    -4deg,
    rem(3) solid var(--lad-border-on-accent),
    0.25rem,
    0 rem(7) 0.75rem
      color-mix(in srgb, var(--lad-color-primary-deep) 20%, transparent)
  );
}
.scope-options .scope-option-icon :deep(.v-icon) {
  width: 1.5rem;
  height: 1.5rem;
  --uno: ma-0;
  color: inherit;
  background-color: currentColor;
  opacity: 1;
  filter: drop-shadow(
    0 rem(1) rem(1) color-mix(in srgb, var(--lad-text) 22%, transparent)
  );
}
.scope-options button .scope-option-icon {
  color: var(--lad-text-inverse);
  font-size: 1rem;
  line-height: 1;
}
.scope-options button:nth-child(2) .scope-option-icon {
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-accent),
    var(--lad-color-reward-strong)
  );
  box-shadow:
    0 0.25rem 0 var(--lad-color-reward-deep),
    0 rem(7) 0.75rem
      color-mix(in srgb, var(--lad-color-reward-deep) 20%, transparent);
  transform: rotate(4deg);
}
.scope-options button:nth-child(3) .scope-option-icon {
  background: linear-gradient(
    145deg,
    var(--lad-color-info-highlight),
    var(--lad-color-info-strong)
  );
  box-shadow:
    0 0.25rem 0 var(--lad-color-info-deep),
    0 rem(7) 0.75rem
      color-mix(in srgb, var(--lad-color-info-deep) 20%, transparent);
  transform: rotate(-3deg);
}
.kind-filter {
  margin-top: rem(13);
  padding: rem(10);
  --uno: d-flex align-center;
  gap: rem(9);
  border: rem(1) dashed
    color-mix(in srgb, var(--lad-text-strong) 20%, transparent);
  border-radius: 1rem;
  background: color-mix(in srgb, var(--lad-surface-raised) 50%, transparent);
}
.kind-filter--type {
  margin-top: rem(7);
}
.kind-filter-label {
  flex: 0 0 auto;
  @include overline(var(--lad-muted), var(--lad-font-size-micro), 0.06em);
  font-weight: var(--lad-font-weight-strong);
}
.kind-options,
.status-options {
  --uno: d-flex flex-wrap;
  gap: rem(6);
}
.kind-options button,
.status-options button {
  padding: rem(7) rem(9);
  --uno: d-inline-flex align-center ga-1 cursor-pointer;
  color: var(--lad-text-strong);
  border: rem(1) solid
    color-mix(in srgb, var(--lad-color-primary-supporting) 15%, transparent);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
  box-shadow: 0 rem(2) 0
    color-mix(in srgb, var(--lad-text-strong) 8%, transparent);

  font: inherit;
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-strong);
}
.kind-options button.active,
.status-options button.active {
  color: var(--lad-color-primary-deep);
  transform: translateY(rem(-1));
  border-color: var(--lad-color-primary-highlight);
  background: var(--lad-surface-raised);
  box-shadow: 0 0.25rem 0 var(--lad-color-primary-soft);
}
.status-options button:last-child.active {
  color: var(--lad-color-info-deep);
  border-color: var(--lad-color-info-highlight);
  box-shadow: 0 0.25rem 0 var(--lad-color-info-soft);
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
    transform: translateX(rem(-2));
  }
  58%,
  75% {
    transform: translateX(rem(2));
  }
}
@include respond-down(compact) {
  .scope-options button {
    min-height: 5rem;
    padding-inline: rem(3);
  }
  .scope-options button strong {
    font-size: rem(10);
  }
  .kind-filter {
    --uno: align-start flex-column;
    gap: rem(7);
  }
}
@include reduced-motion {
  .filter-eye,
  .filter-pupil {
    animation: none;
  }
}
</style>
