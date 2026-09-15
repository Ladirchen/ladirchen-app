<template>
  <g class="headphones">
    <path class="headphone-band" d="M38 68C38 20 122 20 122 68" />
    <path class="headphone-band-highlight" d="M44 63C45 29 115 29 116 63" />
    <path class="headphone-connector" d="M38 60v13m84-13v13" />
    <rect class="headphone-cup" x="29" y="63" width="20" height="38" rx="9" /><rect class="headphone-cup" x="111" y="63" width="20" height="38" rx="9" />
    <rect class="headphone-pad" x="38" y="70" width="8" height="24" rx="4" /><rect class="headphone-pad" x="114" y="70" width="8" height="24" rx="4" />
    <path class="headphone-star" d="m39 74 2 4 4 1-3 3 1 4-4-2-4 2 1-4-3-3 4-1Zm82 0 2 4 4 1-3 3 1 4-4-2-4 2 1-4-3-3 4-1Z" />
  </g>
</template>

<style scoped>
.headphone-band {
  fill: none;
  stroke: var(--lad-palette-violet-650);
  stroke-linecap: round;
  stroke-width: 8;
}
.headphone-band-highlight {
  fill: none;
  stroke: var(--lad-palette-violet-400);
  stroke-linecap: round;
  stroke-width: 2.5;
}
.headphone-connector {
  fill: none;
  stroke: var(--lad-palette-violet-650);
  stroke-linecap: round;
  stroke-width: 6;
}
.headphone-cup {
  fill: var(--lad-palette-violet-400);
  stroke: var(--lad-palette-violet-650);
  stroke-width: 3;
}
.headphone-pad {
  fill: var(--lad-palette-purple-200);
  stroke: none;
}
.headphone-star {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-700);
  stroke-linejoin: round;
  stroke-width: 1;
}
</style>
