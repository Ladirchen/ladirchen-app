<template>
  <g class="propeller-cap">
    <path class="propeller-cap-body" d="M42 43c4-30 65-39 78-4-25 10-53 11-78 4Z" />
    <path class="propeller-cap-panel" d="M80 16c-14 0-25 6-31 15m31-15c14 0 25 6 31 15M80 16v25" />
    <path class="propeller-stem" d="M80 16V7" /><circle class="propeller-hub" cx="80" cy="7" r="4" />
    <path class="propeller-blade blade-left" d="M76 7C57-5 50 4 58 11c6 5 13 2 18-4Z" /><path class="propeller-blade blade-right" d="M84 7c19-12 26-3 18 4-6 5-13 2-18-4Z" />
  </g>
</template>

<style scoped>
.propeller-cap-body {
  fill: var(--lad-palette-teal-400);
  stroke: var(--lad-palette-muted-700);
  stroke-linejoin: round;
  stroke-width: 3;
}
.propeller-cap-panel {
  fill: none;
  stroke: var(--lad-palette-teal-600);
  stroke-width: 2;
}
.propeller-stem {
  fill: none;
  stroke: var(--lad-palette-muted-700);
  stroke-width: 4;
}
.propeller-hub {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 2;
}
.propeller-blade {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-600);
  stroke-linejoin: round;
  stroke-width: 2;
  transform-box: fill-box;
  transform-origin: center;
  animation: propeller-wiggle 4s var(--avatar-phase) ease-in-out infinite;
}
.blade-right {
  fill: var(--lad-palette-blue-250);
}
@keyframes propeller-wiggle {
  0%,
  80%,
  100% {
    transform: rotate(0);
  }
  88% {
    transform: rotate(18deg);
  }
  94% {
    transform: rotate(-15deg);
  }
}
</style>
