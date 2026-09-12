<template>
  <div ref="host" class="pixi-world-foundation" aria-hidden="true" />
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted, ref, watch } from 'vue';
import type { Application, Container, Sprite } from 'pixi.js';

import coinUrl from '@/assets/currency/ladirchen-coin.webp';
import idleLadiUrl from '@/assets/ladi/idle-ladi.webp';
import gardenUrl from '@/assets/room-designs/garden-ladi-hills.webp';
import kitchenUrl from '@/assets/room-designs/kitchen-ladi-classic.webp';
import livingRoomUrl from '@/assets/room-designs/living-ladi-classic.webp';
import exteriorUrl from '@/assets/room-designs/starter-home-ladi-background.webp';

type FoundationView = 'front' | 'side' | 'inside' | 'garden' | 'kitchen';

const props = defineProps<{
  energy: number;
  view: FoundationView;
}>();

const backgroundUrls: Record<FoundationView, string> = {
  front: exteriorUrl,
  garden: gardenUrl,
  inside: livingRoomUrl,
  kitchen: kitchenUrl,
  side: gardenUrl,
};
const energyTints = [0xb8c5b7, 0xc9d3bd, 0xdde0bd, 0xf0e8c3, 0xffffff];

const host = ref<HTMLElement>();
let application: Application | undefined;
let scene: Container | undefined;
let background: Sprite | undefined;
let destroyed = false;
let renderVersion = 0;

const layout = () => {
  if (!application || !scene || !background) return;
  const scale = Math.max(
    application.screen.width / background.texture.width,
    application.screen.height / background.texture.height,
  );
  background.scale.set(scale);
  background.position.set(
    (application.screen.width - background.texture.width * scale) / 2,
    (application.screen.height - background.texture.height * scale) / 2,
  );
};

const renderBackground = async () => {
  if (!application || !scene) return;
  const version = ++renderVersion;
  const pixi = await import('pixi.js');
  const texture = await pixi.Assets.load(backgroundUrls[props.view]);
  if (destroyed || version !== renderVersion || !scene) return;
  background?.destroy();
  background = new pixi.Sprite(texture);
  background.tint = energyTints[Math.min(4, Math.max(0, Math.floor(props.energy / 20)))] ?? 0xffffff;
  scene.addChildAt(background, 0);
  layout();
};

onMounted(async () => {
  if (!host.value) return;
  const pixi = await import('pixi.js');
  if (destroyed || !host.value) return;

  const app = new pixi.Application();
  await app.init({
    antialias: true,
    autoDensity: true,
    backgroundAlpha: 0,
    resolution: Math.min(window.devicePixelRatio, 2),
    resizeTo: host.value,
  });
  if (destroyed || !host.value) {
    app.destroy(true);
    return;
  }

  application = app;
  app.canvas.className = 'pixi-world-foundation__canvas';
  host.value.append(app.canvas);
  scene = new pixi.Container();
  app.stage.addChild(scene);
  await renderBackground();

  const [ladiTexture, coinTexture] = await Promise.all([
    pixi.Assets.load(idleLadiUrl),
    pixi.Assets.load(coinUrl),
  ]);
  if (destroyed || !scene) return;
  const ladi = new pixi.Sprite(ladiTexture);
  ladi.anchor.set(.5, 1);
  ladi.position.set(72, app.screen.height - 48);
  ladi.width = 54;
  ladi.height = 54;
  scene.addChild(ladi);

  const coin = new pixi.Sprite(coinTexture);
  coin.anchor.set(.5);
  coin.position.set(app.screen.width - 44, 44);
  coin.width = 36;
  coin.height = 36;
  scene.addChild(coin);
});

watch(() => props.view, renderBackground);
watch(() => props.energy, () => {
  if (!background) return;
  background.tint = energyTints[Math.min(4, Math.max(0, Math.floor(props.energy / 20)))] ?? 0xffffff;
});

onUnmounted(() => {
  destroyed = true;
  application?.destroy(true, { children: true, texture: false });
});
</script>

<style scoped>
.pixi-world-foundation {
  position: absolute;
  inset: 0;
  z-index: 0;
  overflow: hidden;
  border-radius: 18px;
  pointer-events: none;
}

.pixi-world-foundation :deep(.pixi-world-foundation__canvas) {
  display: block;
  width: 100%;
  height: 100%;
}
</style>
