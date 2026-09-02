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
        <h2 id="contribution-filter-title">Was möchtest du sehen?</h2>
        <p>Wähle zuerst, für wen die Aufgaben sind.</p>
      </div>
    </div>

    <div class="scope-options" role="group" aria-label="Aufgaben auswählen">
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
      <span class="kind-filter-label">Welche Art?</span>
      <div class="kind-options" role="group" aria-label="Aufgabenart auswählen">
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
import type { ContributionKind } from '@/domain/types';

type ContributionScope = 'all' | 'mine' | 'open';
type KindFilter = 'all' | ContributionKind;

defineProps<{ openCount: number }>();
const scope = defineModel<ContributionScope>('scope', { required: true });
const kind = defineModel<KindFilter>('kind', { required: true });

const scopeOptions: Array<{
  description: string;
  icon: string;
  title: string;
  value: ContributionScope;
}> = [
  { value: 'mine', title: 'Meine Aufgaben', description: 'Schon für dich', icon: 'mdi-account-star-outline' },
  { value: 'open', title: 'Freie Aufgaben', description: 'Such dir eine aus', icon: 'mdi-hand-wave-outline' },
  { value: 'all', title: 'Alle Aufgaben', description: 'Alles ansehen', icon: 'mdi-view-grid-outline' },
];

const kindOptions: Array<{ icon: string; title: string; value: KindFilter }> = [
  { value: 'all', title: 'Alle', icon: '✨' },
  { value: 'basic', title: 'Hausenergie', icon: '⚡' },
  { value: 'extra', title: 'Extra-Ladirchen', icon: '🪙' },
];
</script>

<style scoped>
.contribution-filter {
  padding: 15px;
  border: 1px solid rgba(62, 188, 140, 0.25);
  border-radius: 24px;
  background: linear-gradient(145deg, #f0fbf6 0%, #fffaf0 100%);
  box-shadow: 0 4px 0 rgba(62, 188, 140, 0.1);
}
.filter-heading {
  margin-bottom: 13px;
  @apply d-flex align-center;
  gap: 10px;
}
.filter-heading h2 {
  @apply ma-0;
  font-size: 16px;
  letter-spacing: -0.02em;
}
.filter-heading p {
  margin: 1px 0 0;
  color: var(--lad-muted);
  font-size: 10px;
}
.filter-mascot {
  width: 39px;
  height: 39px;
  @apply d-grid place-center;
  flex: 0 0 39px;
  border-radius: 14px;
  background: white;
  box-shadow: 0 3px 0 rgba(62, 188, 140, 0.14);
}
.filter-eyes {
  width: 31px;
  height: 24px;
  @apply overflow-visible;
}
.filter-eye {
  transform-box: fill-box;
  transform-origin: center;
}
.filter-eye > ellipse {
  fill: #fff;
  stroke: #91a6a0;
  stroke-width: 1.2;
}
.filter-pupil {
  fill: #263a42;
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
  @apply ga-2;
}
.scope-options button {
  @apply min-w-0;
  min-height: 86px;
  padding: 10px 6px 8px;
  @apply position-relative d-flex flex-column align-center justify-center;
  gap: 2px;
  color: var(--lad-text);
  border: 2px solid transparent;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.8);
  box-shadow: 0 3px 0 rgba(72, 105, 91, 0.1);
  @apply cursor-pointer;
  font: inherit;
  transition:
    transform 150ms ease,
    border-color 150ms ease,
    background 150ms ease;
}
.scope-options button:hover {
  transform: translateY(-1px);
}
.scope-options button:active {
  transform: translateY(2px);
  box-shadow: 0 1px 0 rgba(72, 105, 91, 0.1);
}
.scope-options button.active {
  color: #16745a;
  border-color: var(--lad-mint);
  background: #e2f7ed;
  box-shadow: 0 4px 0 #b7e5cf;
}
.scope-options button strong {
  font-size: 11px;
  line-height: 1.2;
}
.scope-options button span {
  color: var(--lad-muted);
  font-size: 8px;
  line-height: 1.2;
}
.scope-options button b {
  min-width: 20px;
  height: 20px;
  padding: 0 5px;
  @apply position-absolute;
  top: 6px;
  right: 6px;
  @apply d-grid place-center;
  color: #8a5908;
  border-radius: 10px;
  background: #ffe4a8;
  font-size: 10px;
}
.kind-filter {
  margin-top: 14px;
  padding-top: 12px;
  @apply d-flex align-center;
  gap: 10px;
  border-top: 1px dashed rgba(62, 111, 91, 0.18);
}
.kind-filter-label {
  flex: 0 0 auto;
  color: var(--lad-muted);
  font-size: 9px;
  font-weight: 850;
  letter-spacing: 0.06em;
  @apply text-uppercase;
}
.kind-options {
  @apply d-flex flex-wrap;
  gap: 6px;
}
.kind-options button {
  padding: 7px 10px;
  @apply d-inline-flex align-center ga-1;
  color: #53645d;
  border: 1px solid rgba(73, 111, 96, 0.16);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.76);
  @apply cursor-pointer;
  font: inherit;
  font-size: 9px;
  font-weight: 850;
}
.kind-options button.active {
  color: #185f4d;
  border-color: #9ed9bf;
  background: white;
  box-shadow: 0 2px 0 #cceadb;
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
    transform: translateX(-2px);
  }
  58%,
  75% {
    transform: translateX(2px);
  }
}
@media (max-width: 380px) {
  .scope-options button {
    min-height: 80px;
    padding-inline: 3px;
  }
  .scope-options button strong {
    font-size: 10px;
  }
  .kind-filter {
    @apply align-start flex-column;
    gap: 7px;
  }
}
@media (prefers-reduced-motion: reduce) {
  .filter-eye,
  .filter-pupil {
    animation: none;
  }
}
</style>
