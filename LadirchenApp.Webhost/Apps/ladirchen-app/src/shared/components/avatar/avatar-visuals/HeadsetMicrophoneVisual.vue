<template>
  <g class="headset-microphone" aria-hidden="true">
    <path class="band" d="M42 61C45 29 115 29 118 61" />
    <path class="earpiece" d="M113 55q14 5 9 22l-7 8-7-9Z" />
    <path class="boom" d="M117 78q-4 17-22 18" />
    <circle class="microphone" cx="92" cy="96" r="5" />
    <path class="shine" d="M114 61q4 5 2 11" />
  </g>
</template>

<style scoped>
.headset-microphone .band,
.headset-microphone .boom {
  fill: none;
  stroke: var(--lad-palette-violet-650);
  stroke-linecap: round;
  stroke-width: 4;
}
.headset-microphone .earpiece {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-violet-650);
  stroke-linejoin: round;
  stroke-width: 2;
}
.headset-microphone .microphone {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-700);
  stroke-width: 1.5;
}
.headset-microphone .shine {
  fill: none;
  stroke: var(--lad-palette-white);
  stroke-linecap: round;
  stroke-width: 2;
}
</style>
