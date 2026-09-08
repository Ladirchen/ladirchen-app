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
      <span class="kind-filter-label">Status</span>
      <div class="status-options" role="group" aria-label="Aufgabenstatus auswählen">
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
type StatusFilter = 'open' | 'completed';

defineProps<{ openCount: number }>();
const scope = defineModel<ContributionScope>('scope', { required: true });
const kind = defineModel<KindFilter>('kind', { required: true });
const status = defineModel<StatusFilter>('status', { required: true });

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

const statusOptions: Array<{ icon: string; title: string; value: StatusFilter }> = [
  { value: 'open', title: 'Noch offen', icon: 'mdi-progress-clock' },
  { value: 'completed', title: 'Abgeschlossen', icon: 'mdi-check-circle-outline' },
];

const selectStatus = (value: StatusFilter) => {
  status.value = value;
  if (value === 'completed' && scope.value === 'open') {scope.value = 'mine';}
};
</script>

<style scoped>
.contribution-filter {
  position: relative;
  isolation: isolate;
  padding: 15px;
  overflow: hidden;
  border: 2px solid rgba(62, 188, 140, 0.25);
  border-radius: 28px;
  background:
    radial-gradient(
      circle at 92% 7%,
      rgba(255, 211, 105, 0.2) 0 38px,
      transparent 39px
    ),
    linear-gradient(145deg, #effbf5, #fff8e4);
  box-shadow:
    0 7px 0 rgba(62, 188, 140, 0.13),
    0 14px 24px rgba(45, 104, 81, 0.07);
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
  width: 45px;
  height: 45px;
  @apply d-grid place-center;
  flex: 0 0 45px;
  border: 3px solid #fff;
  border-radius: 16px;
  background: linear-gradient(145deg, #dff7eb, #fff0bf);
  box-shadow: 0 4px 0 rgba(45, 137, 101, 0.22);
  transform: rotate(-4deg);
}
.filter-eyes {
  width: 32px;
  height: 25px;
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
  gap: 8px;
  padding-bottom: 7px;
  border-bottom: 8px solid rgba(225, 202, 158, 0.35);
  border-radius: 0 0 22px 22px;
}
.scope-options button {
  @apply min-w-0;
  min-height: 100px;
  padding: 10px 5px 9px;
  @apply position-relative d-flex flex-column align-center justify-center;
  gap: 3px;
  color: var(--lad-text);
  border: 2px solid #cfe5da;
  border-radius: 23px 23px 17px 17px;
  background: linear-gradient(155deg, #fff, #eaf8f0);
  box-shadow: 0 5px 0 #c5dfd1;
  @apply cursor-pointer;
  font: inherit;
  transition:
    transform 150ms ease,
    border-color 150ms ease,
    background 150ms ease,
    box-shadow 150ms ease;
}
.scope-options button:nth-child(2) {
  border-color: #f0d69f;
  background: linear-gradient(155deg, #fff, #fff1cf);
  box-shadow: 0 5px 0 #ead2a3;
}
.scope-options button:nth-child(3) {
  border-color: #d9d3ed;
  background: linear-gradient(155deg, #fff, #eeeafb);
  box-shadow: 0 5px 0 #d4cce8;
}
.scope-options button:hover {
  transform: translateY(-3px);
}
.scope-options button:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 rgba(72, 105, 91, 0.1);
}
.scope-options button.active {
  color: #16745a;
  transform: translateY(-5px) rotate(-1deg);
  border-color: #42ae82;
  background: linear-gradient(155deg, #fff, #dff6e9);
  box-shadow:
    0 8px 0 #a9d8bf,
    0 12px 16px rgba(45, 107, 82, 0.1);
}
.scope-options button:nth-child(2).active {
  color: #95600e;
  transform: translateY(-5px) rotate(1deg);
  border-color: #eba735;
  background: linear-gradient(155deg, #fff, #ffebbd);
  box-shadow:
    0 8px 0 #e5bd72,
    0 12px 16px rgba(145, 99, 30, 0.1);
}
.scope-options button:nth-child(3).active {
  color: #705296;
  border-color: #9d7ccc;
  background: linear-gradient(155deg, #fff, #eee4fb);
  box-shadow:
    0 8px 0 #c8b3e4,
    0 12px 16px rgba(92, 65, 128, 0.1);
}
.scope-options button :deep(.v-icon) {
  width: 40px;
  height: 40px;
  margin-bottom: 3px;
  border: 3px solid #fff;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.65);
  box-shadow: 0 4px 0 rgba(88, 127, 110, 0.12);
}
.scope-options button strong {
  font-size: 10px;
  line-height: 1.2;
}
.scope-options button span {
  color: var(--lad-muted);
  font-size: 7px;
  line-height: 1.2;
}
.scope-options button b {
  min-width: 21px;
  height: 21px;
  padding: 0 5px;
  @apply position-absolute;
  top: 6px;
  right: 6px;
  @apply d-grid place-center;
  color: #8a5908;
  border: 2px solid #fff;
  border-radius: 8px;
  background: #ffe4a8;
  box-shadow: 0 2px 0 #dab56a;
  font-size: 9px;
}
.kind-filter {
  margin-top: 13px;
  padding: 10px;
  @apply d-flex align-center;
  gap: 9px;
  border: 1px dashed rgba(62, 111, 91, 0.2);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.54);
}
.kind-filter--type {
  margin-top: 7px;
}
.kind-filter-label {
  flex: 0 0 auto;
  color: var(--lad-muted);
  font-size: 8px;
  font-weight: 850;
  letter-spacing: 0.06em;
  @apply text-uppercase;
}
.kind-options,
.status-options {
  @apply d-flex flex-wrap;
  gap: 6px;
}
.kind-options button,
.status-options button {
  padding: 7px 9px;
  @apply d-inline-flex align-center ga-1;
  color: #53645d;
  border: 1px solid rgba(73, 111, 96, 0.16);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.82);
  box-shadow: 0 2px 0 rgba(72, 105, 91, 0.09);
  @apply cursor-pointer;
  font: inherit;
  font-size: 8px;
  font-weight: 850;
}
.kind-options button.active,
.status-options button.active {
  color: #185f4d;
  transform: translateY(-1px);
  border-color: #79c9a7;
  background: white;
  box-shadow: 0 4px 0 #c3e7d5;
}
.status-options button:last-child.active {
  color: #356b9b;
  border-color: #8dc2ea;
  box-shadow: 0 4px 0 #c8e3f4;
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
