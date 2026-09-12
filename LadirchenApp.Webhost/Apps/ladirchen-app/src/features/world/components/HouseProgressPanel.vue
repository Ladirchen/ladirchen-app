<template>
  <section aria-label="Hausentwicklung">
    <div class="house-stage-card mb-3">
      <div class="house-stage-visual" aria-hidden="true">
        <span class="house-halo" />
        <span class="house-spark house-spark--one">✦</span>
        <span class="house-spark house-spark--two">✦</span>
        <span class="house-stage-icon">{{ currentStage.icon }}</span>
      </div>
      <div class="house-stage-copy">
        <span class="section-kicker">AKTUELLE HAUSSTUFE</span>
        <div class="house-stage-title">
          <strong>{{ currentStage.name }}</strong>
          <span>Stufe {{ houseLevel + 1 }}</span>
        </div>
        <p>{{ completedWeeks }} erfolgreiche Hauswoche{{ completedWeeks === 1 ? '' : 'n' }} abgeschlossen</p>
        <div class="stage-track" :aria-label="`Hausstufe ${houseLevel + 1} von ${houseStages.length}`">
          <span
            v-for="stage in houseStages"
            :key="stage.level"
            :class="{ reached: stage.level <= houseLevel, current: stage.level === houseLevel }"
          />
        </div>
        <b>{{ houseLevel < houseStages.length - 1 ? 'Nächste Stufe: Überraschung' : 'Höchste Hausstufe erreicht' }}</b>
      </div>
    </div>

    <div class="house-evolution">
      <div class="evolution-heading">
        <div class="evolution-icon" aria-hidden="true"><v-icon size="22">mdi-home-switch</v-icon></div>
        <div><span class="section-kicker">WOCHENABSCHLUSS</span><strong>So entwickelt sich das Haus</strong></div>
      </div>
      <div class="evolution-steps mt-3">
        <div><span>60 %</span><p>Erreicht eure gemeinsame Hausenergie täglich mindestens 60 %, zählt der Tag für die Serie.</p></div>
        <div><span>7 Tage</span><p>Am Wochenabschluss wird die nächste Hausstufe enthüllt.</p></div>
        <div><span>Unter 60 %</span><p>Das Haus geht eine Stufe zurück. Ladirchen, Ziele, Möbel und Gartendinge bleiben erhalten.</p></div>
      </div>
      <p class="evolution-note mt-3">Die Hausenergie ist der Durchschnitt des Beitragsfortschritts aller Kinder. Bezugspersonen erstellen und bewerten Beiträge, erledigen sie aber nicht selbst.</p>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from '../data/house-catalog';
import type { HouseStageLevel } from '@/domain/house';

const props = defineProps<{
  completedWeeks: number;
  houseLevel: HouseStageLevel;
}>();

const houseStages = HOUSE_STAGES;
const currentStage = computed(() => houseStages[props.houseLevel] ?? DEFAULT_HOUSE_STAGE);
</script>

<style scoped>
.house-stage-card {
  min-height: 126px;
  padding: 14px;
  @apply d-flex align-center;
  gap: 13px;
  @apply overflow-hidden;
  border: 1px solid rgba(236, 179, 74, 0.2);
  border-radius: 20px;
  background: linear-gradient(135deg, #fff9ed, #fff1d4 58%, #edf9f2);
  box-shadow: 0 4px 0 rgba(179, 126, 45, 0.08);
}
.house-stage-visual {
  width: 88px;
  height: 88px;
  @apply position-relative d-grid place-center;
  flex: 0 0 88px;
}
.house-halo {
  width: 72px;
  height: 72px;
  @apply position-absolute;
  border-radius: 50%;
  background: repeating-conic-gradient(
    from 0deg,
    rgba(244, 179, 64, 0.28) 0 12deg,
    transparent 12deg 25deg
  );
  animation: house-halo-spin 12s linear infinite;
}
.house-stage-icon {
  @apply position-relative;
  z-index: 2;
  font-size: 52px;
  line-height: 1;
  filter: drop-shadow(0 6px 5px rgba(111, 76, 41, 0.18));
  animation: house-stage-bounce 3.2s ease-in-out infinite;
}
.house-spark {
  @apply position-absolute;
  z-index: 3;
  color: #e99a2c;
  font-size: 14px;
  animation: house-spark 1.8s ease-in-out infinite;
}
.house-spark--one {
  top: 7px;
  right: 7px;
}
.house-spark--two {
  bottom: 7px;
  left: 5px;
  animation-delay: -0.9s;
}
.house-stage-copy {
  @apply min-w-0;
  flex: 1;
}
.section-kicker {
  color: #278568;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.1em;
}
.house-stage-title {
  margin: 2px 0;
  @apply d-flex align-center flex-wrap;
  gap: 5px;
}
.house-stage-title strong {
  font-size: 14px;
}
.house-stage-title span {
  padding: 3px 7px;
  color: #805716;
  border-radius: 999px;
  background: #ffe8ad;
  font-size: 8px;
  @apply font-weight-black;
}
.house-stage-copy > p {
  @apply ma-0;
  color: var(--lad-muted);
  font-size: 9px;
  line-height: 1.35;
}
.house-stage-copy > b {
  @apply d-block;
  margin-top: 5px;
  color: #7b581d;
  font-size: 8px;
}
.stage-track {
  @apply mt-2 d-grid;
  grid-template-columns: repeat(5, 1fr);
  @apply ga-1;
}
.stage-track span {
  height: 6px;
  border-radius: 999px;
  background: rgba(93, 126, 107, 0.13);
}
.stage-track span.reached {
  background: #48bf91;
}
.stage-track span.current {
  box-shadow: 0 0 0 3px rgba(72, 191, 145, 0.16);
  animation: stage-pulse 1.8s ease-in-out infinite;
}
.house-evolution {
  padding: 14px;
  border: 1px solid rgba(80, 151, 120, 0.17);
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.78);
}
.evolution-heading {
  @apply d-flex align-center;
  gap: 10px;
}
.evolution-heading > div:last-child span,
.evolution-heading > div:last-child strong {
  @apply d-block;
}
.evolution-heading > div:last-child strong {
  margin-top: 1px;
  font-size: 13px;
}
.evolution-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center;
  flex: 0 0 40px;
  color: #278568;
  border-radius: 13px;
  background: #def4e8;
}
.evolution-steps {
  @apply d-grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 6px;
}
.evolution-steps > div {
  @apply pa-2;
  border-radius: 12px;
  background: #f5faf7;
}
.evolution-steps span {
  color: #238765;
  font-size: 9px;
  font-weight: 950;
}
.evolution-steps p {
  margin: 3px 0 0;
  color: #566960;
  font-size: 7.5px;
  line-height: 1.35;
}
.evolution-note {
  margin-bottom: 0;
  color: var(--lad-muted);
  font-size: 8px;
  line-height: 1.4;
}
@keyframes house-spark {
  0%,
  100% {
    opacity: 0.3;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(20deg);
  }
}
@keyframes house-stage-bounce {
  0%,
  100% {
    transform: translateY(2px) rotate(-2deg);
  }
  50% {
    transform: translateY(-6px) rotate(2deg);
  }
}
@keyframes house-halo-spin {
  to {
    transform: rotate(360deg);
  }
}
@keyframes stage-pulse {
  0%,
  100% {
    transform: scaleY(1);
  }
  50% {
    transform: scaleY(1.5);
  }
}
@media (max-width: 420px) {
  .house-stage-visual {
    width: 72px;
    flex-basis: 72px;
  }
  .evolution-steps {
    grid-template-columns: 1fr;
  }
}
@media (prefers-reduced-motion: reduce) {
  .house-halo,
  .house-stage-icon,
  .house-spark,
  .stage-track span.current {
    animation: none;
  }
}
</style>
