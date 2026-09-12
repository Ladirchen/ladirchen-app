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

    <template v-else-if="item.visual === 'kitchen-counter'">
      <rect x="-48" y="-26" width="96" height="58" rx="5" fill="#eadfd0" stroke="#745849" stroke-width="3" />
      <rect x="-51" y="-31" width="102" height="9" rx="4" fill="#a87354" stroke="#694b3d" stroke-width="3" />
      <path d="M-15-21v51m31-51v51" stroke="#b99a83" stroke-width="3" />
      <path d="M-39-13h17v29h-17Zm62 0h17v29H23Z" fill="#f8f3eb" stroke="#9b7e6b" stroke-width="2" />
      <ellipse cx="-15" cy="-30" fill="#b9dce0" stroke="#536e74" stroke-width="2" rx="15" ry="4" />
      <path d="M-15-34v-12q0-6 7-6t7 6" fill="none" stroke="#536e74" stroke-linecap="round" stroke-width="4" />
      <circle cx="28" cy="-29" fill="#34464d" r="5" /><circle cx="42" cy="-29" fill="#34464d" r="5" />
    </template>

    <template v-else-if="item.visual === 'retro-fridge'">
      <rect x="-25" y="-49" width="50" height="88" rx="9" fill="#9ccdc4" stroke="#496d68" stroke-width="4" />
      <path d="M-23-13h46" stroke="#496d68" stroke-width="4" /><path d="M14-37v14M14-4v20" stroke="#f7f0dc" stroke-linecap="round" stroke-width="4" />
      <circle cx="-10" cy="-31" fill="#ef7a72" r="4" /><path d="m-15 5 8-8 7 7" fill="none" stroke="#f4c759" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'dining-table'">
      <ellipse cy="-6" fill="#b87c55" stroke="#714d3c" stroke-width="3" rx="42" ry="14" /><path d="M-30 1-35 33M30 1l5 32" stroke="#714d3c" stroke-linecap="round" stroke-width="6" />
      <path d="M-18-10h12m11 0h12" stroke="#f8e8ce" stroke-linecap="round" stroke-width="6" /><path d="M-50 8v21m100-21v21" stroke="#6e5548" stroke-width="5" />
    </template>

    <template v-else-if="item.visual === 'double-bed'">
      <rect x="-47" y="-32" width="94" height="65" rx="9" fill="#806b68" stroke="#514342" stroke-width="4" /><rect x="-42" y="-24" width="84" height="55" rx="8" fill="#d9a9b6" />
      <path d="M-42 1h84v30h-84Z" fill="#a983b5" /><rect x="-35" y="-19" width="30" height="17" rx="7" fill="#fff2df" /><rect x="5" y="-19" width="30" height="17" rx="7" fill="#fff2df" />
      <path d="M-31 33v7m62-7v7" stroke="#514342" stroke-width="6" />
    </template>

    <template v-else-if="item.visual === 'wardrobe'">
      <rect x="-32" y="-48" width="64" height="91" rx="5" fill="#a97756" stroke="#654838" stroke-width="4" /><path d="M0-45v85" stroke="#654838" stroke-width="3" />
      <circle cx="-6" cy="-3" fill="#e9c56c" r="3" /><circle cx="6" cy="-3" fill="#e9c56c" r="3" /><path d="M-24-38h19m10 0h19" stroke="#d2a27c" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'bedside-table'">
      <rect x="-21" y="-2" width="42" height="34" rx="4" fill="#b17a57" stroke="#6c4d3c" stroke-width="3" /><path d="M-17 8h34" stroke="#6c4d3c" stroke-width="2" /><circle cy="15" fill="#ecc86d" r="2.5" />
      <path d="M0-4v-18" stroke="#5e514a" stroke-width="3" /><path d="m-13-20 6-15H7l6 15Z" fill="#f2c967" stroke="#9b6a31" stroke-width="2" />
    </template>

    <template v-else-if="item.visual === 'art-desk'">
      <path d="m-37-30 68-9 8 42-68 9Z" fill="#d7a46c" stroke="#74513d" stroke-width="4" /><path d="m-26 10-8 27M34 3l8 27" stroke="#74513d" stroke-linecap="round" stroke-width="5" />
      <rect x="-18" y="-24" width="32" height="22" rx="2" fill="#fff5dc" transform="rotate(-8)" /><path d="m-10-16 8 7 10-11" fill="none" stroke="#64aa8b" stroke-width="3" /><circle cx="24" cy="-25" fill="#ed746f" r="7" />
    </template>

    <template v-else-if="item.visual === 'storage-cabinet'">
      <rect x="-33" y="-43" width="66" height="82" rx="5" fill="#7da797" stroke="#48665d" stroke-width="4" /><path d="M-27-17h54M-27 10h54M0-40v76" stroke="#48665d" stroke-width="3" />
      <rect x="-20" y="-32" width="13" height="6" rx="2" fill="#f6df90" /><rect x="7" y="-5" width="13" height="6" rx="2" fill="#f2a5af" /><rect x="-20" y="22" width="13" height="6" rx="2" fill="#a9d8ec" />
    </template>

    <template v-else-if="item.visual === 'pool'">
      <ellipse cy="8" fill="#f7f2df" stroke="#c26c42" stroke-width="5" rx="50" ry="26" /><ellipse cy="6" fill="#83d7e8" stroke="#fff" stroke-width="3" rx="43" ry="20" />
      <path d="M-12-9q12 9 24 0M-28 9q14 9 28 0m12 5q12 8 24 0" fill="none" stroke="#d9f8ff" stroke-linecap="round" stroke-width="3" /><path d="M-43-2v31m10-37v37m-10-8h10m-10-10h10" stroke="#77878a" stroke-linecap="round" stroke-width="4" />
    </template>

    <template v-else-if="item.visual === 'garden-chair'">
      <path d="m-25-36 39 8-10 37-39-8Z" fill="#f0b79c" stroke="#744f3c" stroke-width="4" /><path d="m-30-39 49 10M-35 2 14 12M-30 3l-7 34m48-27 13 28" fill="none" stroke="#744f3c" stroke-linecap="round" stroke-width="5" />
      <path d="m-20-26 26 5M-24-14 3-9" stroke="#fff0df" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'garden-table'">
      <ellipse cy="-6" fill="#b87950" stroke="#6f4a37" stroke-width="4" rx="35" ry="12" /><path d="M-22 1-27 31M22 1l27 30" stroke="#6f4a37" stroke-linecap="round" stroke-width="5" />
      <path d="M-8-16h16L5-3H-5Z" fill="#f7e3d2" stroke="#856a5b" stroke-width="2" /><path d="M0-17v-13" stroke="#856a5b" stroke-width="2" /><circle cy="-33" fill="#ef8a7b" r="5" />
    </template>

    <template v-else-if="item.visual === 'trampoline'">
      <ellipse fill="#435d60" stroke="#253d42" stroke-width="5" rx="48" ry="19" /><ellipse fill="#79c9b0" stroke="#c6f5e5" stroke-width="3" rx="39" ry="13" />
      <path d="M-43 7-36 34M43 7l36 27M-47-2v-42m94 42v-42M-47-42q47-15 94 0" fill="none" stroke="#667779" stroke-linecap="round" stroke-width="4" /><path d="M-47-30q47-12 94 0M-47-15q47-10 94 0" fill="none" stroke="#a9c6c6" stroke-width="2" />
    </template>

    <template v-else-if="item.visual === 'sunshade'">
      <path d="M0-10v45" stroke="#6b5549" stroke-width="5" /><path d="M-43-10Q0-62 43-10Z" fill="#ef7c74" stroke="#994c4d" stroke-linejoin="round" stroke-width="4" />
      <path d="M-22-10Q0-58 0-10m22 0Q0-58 0-10" fill="#ffd596" stroke="#994c4d" stroke-width="2" /><ellipse cy="36" fill="#6b5549" rx="17" ry="5" />
    </template>

    <template v-else-if="item.visual === 'flower-box'">
      <path d="M-39 4h78l-6 25h-66Z" fill="#a56b49" stroke="#6d4735" stroke-width="3" /><path d="M-29 4v-17m17 17v-24M2 4v-19M18 4v-25M31 4v-16" stroke="#438c58" stroke-width="3" />
      <circle cx="-29" cy="-15" fill="#ef7774" r="8" /><circle cx="-12" cy="-22" fill="#f0c650" r="8" /><circle cx="3" cy="-17" fill="#e68dac" r="8" /><circle cx="18" cy="-23" fill="#7dc89c" r="8" /><circle cx="31" cy="-18" fill="#ef9674" r="8" />
    </template>

    <template v-else-if="item.visual === 'string-lights'">
      <path d="M-50-22Q0 12 50-22" fill="none" stroke="#665648" stroke-width="3" /><path d="M-38-13v12M-18-4V8M4 1v12m21-17V8m17-20V0" stroke="#665648" stroke-width="2" />
      <circle cx="-38" fill="#ffd55f" r="6" /><circle cx="-18" cy="9" fill="#ef8b84" r="6" /><circle cx="4" cy="14" fill="#ffd55f" r="6" /><circle cx="25" cy="9" fill="#77c9ae" r="6" /><circle cx="42" cy="1" fill="#ffd55f" r="6" />
    </template>

    <template v-else-if="item.visual === 'hammock'">
      <path d="M-46-26v62M46-26v62" stroke="#6f4b37" stroke-linecap="round" stroke-width="6" /><path d="M-42-13Q0 50 42-13" fill="#ef897c" stroke="#9e514e" stroke-width="5" /><path d="M-34-8Q0 33 34-8M-24 1Q0 25 24 1" fill="none" stroke="#ffe1ad" stroke-width="3" />
    </template>

    <template v-else-if="item.visual === 'telescope'">
      <path d="m-29-30 50-17 7 17-51 17Z" fill="#638092" stroke="#344e5d" stroke-linejoin="round" stroke-width="4" /><ellipse cx="25" cy="-39" fill="#a9d8e5" stroke="#344e5d" stroke-width="3" rx="7" ry="10" transform="rotate(-19 25 -39)" />
      <path d="M0-18-16 35M0-18l18 53M0-18v55" fill="none" stroke="#53646b" stroke-linecap="round" stroke-width="5" />
    </template>

    <template v-else-if="item.visual === 'pumpkin-arch'">
      <path d="M-42 36V8q0-48 42-48T42 8v28" fill="none" stroke="#55774b" stroke-linecap="round" stroke-width="7" />
      <g class="pumpkin-lights"><circle cx="-39" cy="18" r="10" /><circle cx="-32" cy="-10" r="9" /><circle cx="-13" cy="-34" r="10" /><circle cx="13" cy="-34" r="10" /><circle cx="32" cy="-10" r="9" /><circle cx="39" cy="18" r="10" /></g>
      <path d="m-43 16 4 4 4-4m-2 8q4 3 8 0m55-10 4 4 4-4m-2 8q4 3 8 0" fill="none" stroke="#57362c" stroke-linecap="round" stroke-width="2" />
    </template>

    <template v-else-if="item.visual === 'bat-garland'">
      <path d="M-51-22Q0 18 51-22" fill="none" stroke="#4d4056" stroke-width="3" />
      <g class="garland-bats" fill="#493752"><path d="m-38-7 8-7 8 7-8 11Z" /><path d="m-10 5 9-8 9 8-9 12Z" /><path d="m20-2 8-7 8 7-8 11Z" /></g>
    </template>

    <template v-else-if="item.visual === 'candy-bush'">
      <path d="M-45 29h90" stroke="#6d8f68" stroke-linecap="round" stroke-width="6" />
      <g class="candy-bush-cloud"><circle cx="-29" cy="6" r="20" /><circle cx="-8" cy="-3" r="26" /><circle cx="16" cy="4" r="23" /><circle cx="34" cy="12" r="17" /></g>
      <path d="M-22 10q12-20 24 0T25 8" fill="none" stroke="#fff3f8" stroke-linecap="round" stroke-width="5" />
    </template>

    <template v-else-if="item.visual === 'candy-fence'">
      <path d="M-52-13h104M-52 19h104" fill="none" stroke="#8b6faa" stroke-width="6" />
      <g class="candy-fence-posts"><path d="M-43-30v62M-21-30v62M0-30v62M21-30v62M43-30v62" /></g>
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
.pumpkin-lights {
  fill: #ef873b;
  stroke: #83452d;
  stroke-width: 2;
}
.garland-bats path {
  transform-box: fill-box;
  transform-origin: center;
  animation: garland-flutter 1.3s ease-in-out infinite alternate;
}
.garland-bats path:nth-child(2) {
  animation-delay: -0.5s;
}
.garland-bats path:nth-child(3) {
  animation-delay: -0.9s;
}
.candy-bush-cloud {
  fill: #efa5c7;
  stroke: #fff;
  stroke-width: 3;
}
.candy-bush-cloud circle:nth-child(2n) {
  fill: #a9dff1;
}
.candy-fence-posts {
  fill: none;
  stroke: #ef8ebc;
  stroke-dasharray: 8 5;
  stroke-linecap: round;
  stroke-width: 9;
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
@keyframes garland-flutter {
  from {
    transform: scaleY(0.75) translateY(0);
  }
  to {
    transform: scaleY(1.15) translateY(-3px);
  }
}
@media (prefers-reduced-motion: reduce) {
  .furniture-piece {
    animation: none;
  }
}
</style>
