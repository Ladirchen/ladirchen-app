<template>
  <g
    v-if="item.visual && item.scene"
    class="furniture-piece"
    :class="item.visual"
    :transform="`translate(${item.scene.x} ${item.scene.y}) scale(${item.scene.scale ?? 1})`"
  >
    <template v-if="item.visual === 'rug'">
      <ellipse fill="#f09a50" rx="48" ry="20" />
      <ellipse fill="none" stroke="#ffd98b" stroke-width="5" rx="34" ry="13" />
      <path d="m-12-1 8 7 18-15" fill="none" stroke="#fff0bf" stroke-linecap="round" stroke-linejoin="round" stroke-width="4" />
    </template>

    <template v-else-if="item.visual === 'sofa'">
      <rect x="-36" y="-18" width="72" height="42" rx="14" fill="#6e9fe0" />
      <rect x="-29" y="-29" width="58" height="38" rx="15" fill="#93bdf0" />
      <rect x="-45" y="-8" width="19" height="31" rx="9" fill="#5f8ccd" />
      <rect x="26" y="-8" width="19" height="31" rx="9" fill="#5f8ccd" />
      <path d="M-13-24V5m26-29V5" stroke="#d6e9ff" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'bookshelf'">
      <rect x="-24" y="-42" width="48" height="83" rx="5" fill="#a66d4f" />
      <path d="M-19-16h38M-19 9h38M-19 34h38" stroke="#6f4939" stroke-width="4" />
      <path class="book-spines" d="M-15-36v16m9-13v13M4-37v17m9-11v11M-15-10V5M-4-7V5M7-12V5m8-10V5m-30 20v14m10-17v17M6 25v14m9-17v17" />
    </template>

    <template v-else-if="item.visual === 'lamp'">
      <path d="M0-34v63" stroke="#6c5b55" stroke-width="5" />
      <ellipse cy="32" fill="#6c5b55" rx="17" ry="5" />
      <circle cy="-33" fill="#fff1a8" opacity=".5" r="27" />
      <path d="m-20-33 8-26h24l8 26Z" fill="#ffe06e" stroke="#bd7a39" stroke-linejoin="round" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'bunk-bed'">
      <rect x="-41" y="-35" width="82" height="67" rx="7" fill="#76a175" />
      <rect x="-35" y="-26" width="67" height="20" rx="7" fill="#f4a8b9" />
      <rect x="-35" y="4" width="67" height="20" rx="7" fill="#efca63" />
      <path d="M-31-31v66m66-66v66M34 1h13m-7 0v35" stroke="#496e54" stroke-linecap="round" stroke-width="5" />
      <path d="M-29-21h18m-18 30h18" stroke="#fff8dc" stroke-width="9" />
    </template>

    <template v-else-if="item.visual === 'table'">
      <ellipse fill="#f3c866" rx="31" ry="10" />
      <path d="m-18 1-4 28M22 1l4 28" stroke="#8e6248" stroke-linecap="round" stroke-width="5" />
      <rect x="-12" y="-10" width="12" height="8" rx="2" fill="#eb6f67" /><circle cx="11" cy="-6" fill="#6f8fe5" r="7" />
    </template>

    <template v-else-if="item.visual === 'plant'">
      <path d="M-2 1c-16-8-16-23-4-25 10 6 11 16 4 25Zm2 0c1-18 11-27 22-22 2 12-7 20-22 22Zm-1-1c-7-14-1-26 9-27 7 10 3 20-9 27Z" fill="#62b874" stroke="#347c50" stroke-linejoin="round" stroke-width="2" />
      <path d="M-15 1h29L9 28h-19Z" fill="#e98169" stroke="#9e5548" stroke-linejoin="round" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'wall-art'">
      <rect x="-21" y="-15" width="42" height="30" rx="6" fill="#fff" stroke="#e5a35f" stroke-width="5" />
      <path d="m-12 6 9-10 7 7 7-10" fill="none" stroke="#62ae84" stroke-linecap="round" stroke-linejoin="round" stroke-width="4" />
      <circle cx="12" cy="-6" fill="#f4bd4f" r="4" />
    </template>

    <template v-else-if="item.visual === 'cat-tree'">
      <path d="M-18 34h36M-10 31-4-18m14 49L4-3" stroke="#8e6248" stroke-linecap="round" stroke-width="5" />
      <path d="M-18-18h28l-7 12h-28Zm-5-3h28" fill="#d98e78" stroke="#9b5f52" stroke-linejoin="round" stroke-width="3" />
      <circle cx="8" cy="-26" fill="#f3bd62" r="11" /><path d="m2-32 3-7 5 6 6-5 1 9" fill="#f3bd62" stroke="#9b5f52" stroke-linejoin="round" stroke-width="2" />
    </template>

    <template v-else-if="item.visual === 'pet-bed'">
      <ellipse fill="#9cb9e8" stroke="#617da9" stroke-width="3" rx="30" ry="15" />
      <ellipse fill="#d9e7ff" stroke="#7896c6" stroke-width="2" rx="20" ry="9" />
      <path d="M-21-4q21 10 42 0" fill="none" stroke="#fff" stroke-linecap="round" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'dog-blanket'">
      <path d="M-30-16q30-12 60 0v31q-30 12-60 0Z" fill="#e88b87" stroke="#9e5d62" stroke-width="3" />
      <path d="M-20-11v22m13-25v28m14-28v28m13-25v22" stroke="#ffd6a4" stroke-width="3" />
      <circle cx="0" cy="-2" fill="#fff0c4" r="6" /><path d="m-3-2 3 3 5-6" fill="none" stroke="#bd6c66" stroke-linecap="round" stroke-width="2" />
    </template>

    <template v-else-if="item.visual === 'food-bowl'">
      <path d="M-21-7h42l-5 21q-16 10-32 0Z" fill="#70bfe0" stroke="#4b7f9e" stroke-width="3" />
      <ellipse fill="#fff1be" stroke="#4b7f9e" stroke-width="3" rx="22" ry="7" />
      <circle cx="-8" cy="-2" fill="#db8e55" r="3" /><circle cx="1" cy="-1" fill="#8f674f" r="3" /><circle cx="9" cy="-2" fill="#db8e55" r="3" />
    </template>
  </g>
</template>

<script lang="ts" setup>
import type { HouseAccessory } from '@/domain/types';

defineProps<{ item: HouseAccessory }>();
</script>

<style scoped>
.furniture-piece {
  animation: furniture-arrive 480ms cubic-bezier(0.18, 0.89, 0.32, 1.28);
  filter: drop-shadow(0 4px 2px rgba(72, 55, 43, 0.16));
}
.book-spines {
  fill: none;
  stroke: #f1bf63;
  stroke-linecap: round;
  stroke-width: 5;
}
@keyframes furniture-arrive {
  from {
    opacity: 0;
    transform: scale(0.3) translateY(18px);
  }
  to {
    opacity: 1;
  }
}
@media (prefers-reduced-motion: reduce) {
  .furniture-piece {
    animation: none;
  }
}
</style>
