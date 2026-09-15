<template>
  <div ref="host" class="pixi-room-scene" aria-hidden="true" />
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted, ref } from 'vue';
import type { Application, Container } from 'pixi.js';
import type * as PixiNamespace from 'pixi.js';

import type { RoomDesignDefinition, RoomSceneOverlayDefinition } from '@/domain/house';
import { PERCENTAGE_BASE } from '@/domain/shared/numbers';
import { MILLISECONDS_PER_SECOND } from '@/domain/shared/time';
import { ROOM_DESIGN_ASSET_URLS, roomDesignBackgroundAssetId } from '@/shared/visuals/house/room-design-assets';
import { visualColorPalette } from '@/theme/color-palette';

const props = defineProps<{
  design: RoomDesignDefinition;
  energy: number;
}>();

const MAXIMUM_DEVICE_PIXEL_RATIO = 2;
const OVERLAY_FLOAT_FREQUENCY = 1.15;
const OVERLAY_FLOAT_ROTATION = 0.055;
const OVERLAY_SPIN_SPEED = 0.45;

const host = ref<HTMLElement>();
let application: Application | undefined;
let resizeObserver: ResizeObserver | undefined;
let destroyed = false;

const paletteColor = (token: string, fallback: string): string => {
  const value = getComputedStyle(document.documentElement).getPropertyValue(token).trim();
  return value || fallback;
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
    preference: 'webgl',
    resolution: Math.min(window.devicePixelRatio, MAXIMUM_DEVICE_PIXEL_RATIO),
    resizeTo: host.value,
  });
  if (destroyed || !host.value) {
    app.destroy(true);
    return;
  }

  application = app;
  app.canvas.className = 'pixi-room-scene__canvas';
  host.value.append(app.canvas);

  const scene = new pixi.Container();
  const texture = await pixi.Assets.load(ROOM_DESIGN_ASSET_URLS[roomDesignBackgroundAssetId(props.design, props.energy)]);
  if (destroyed) return;

  const background = new pixi.Sprite(texture);
  background.width = props.design.canvasWidth;
  background.height = props.design.canvasHeight;
  scene.addChild(background);

  const accent = paletteColor('--lad-palette-amber-500', visualColorPalette['amber-500']);
  const outline = paletteColor('--lad-palette-teal-700', visualColorPalette['teal-700']);
  const leaf = paletteColor('--lad-palette-green-500', visualColorPalette['green-500']);
  const overlayNodes = props.design.overlays.map((overlay) => {
    const node = new pixi.Container();
    node.position.set(
      props.design.canvasWidth * overlay.xPercent / PERCENTAGE_BASE,
      props.design.canvasHeight * overlay.yPercent / PERCENTAGE_BASE,
    );
    node.scale.set(overlay.scale);
    node.addChild(createOverlay(pixi, overlay, { accent, leaf, outline }));
    scene.addChild(node);
    return { definition: overlay, node };
  });
  app.stage.addChild(scene);

  const layout = () => {
    const width = app.screen.width;
    const height = app.screen.height;
    const scale = Math.max(width / props.design.canvasWidth, height / props.design.canvasHeight);
    scene.scale.set(scale);
    scene.position.set(
      (width - props.design.canvasWidth * scale) / 2,
      (height - props.design.canvasHeight * scale) / 2,
    );
  };
  layout();
  resizeObserver = new ResizeObserver(layout);
  resizeObserver.observe(host.value);

  if (!window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    let elapsed = 0;
    app.ticker.add((ticker) => {
      elapsed += ticker.deltaMS / MILLISECONDS_PER_SECOND;
      overlayNodes.forEach(({ definition, node }) => {
        node.rotation = definition.motion === 'spin'
          ? elapsed * OVERLAY_SPIN_SPEED
          : Math.sin(elapsed * OVERLAY_FLOAT_FREQUENCY) * OVERLAY_FLOAT_ROTATION;
      });
    });
  }
});

onUnmounted(() => {
  destroyed = true;
  resizeObserver?.disconnect();
  application?.destroy(true, { children: true, texture: false });
});

type Pixi = typeof PixiNamespace;

const createOverlay = (
  pixi: Pixi,
  overlay: RoomSceneOverlayDefinition,
  colors: { accent: string; leaf: string; outline: string },
): Container => {
  const container = new pixi.Container();
  if (overlay.id === 'garden-wind-spinner') {
    for (let index = 0; index < 6; index += 1) {
      const petal = new pixi.Graphics()
        .ellipse(0, -25, 8, 20)
        .fill(colors.accent)
        .stroke({ color: colors.outline, width: 3 });
      petal.rotation = index * Math.PI / 3;
      container.addChild(petal);
    }
    container.addChild(new pixi.Graphics()
      .circle(0, 0, 9)
      .fill(colors.accent)
      .stroke({ color: colors.outline, width: 3 }));
    return container;
  }

  for (let index = 0; index < 4; index += 1) {
    const leaf = new pixi.Graphics()
      .ellipse(index * 18, Math.abs(index - 1.5) * 6, 13, 7)
      .fill(colors.leaf)
      .stroke({ color: colors.outline, width: 3 });
    leaf.rotation = index % 2 === 0 ? -.35 : .35;
    container.addChild(leaf);
  }
  return container;
};
</script>

<style lang="scss" scoped>
.pixi-room-scene {
  position: absolute;
  inset: 0;
  overflow: hidden;
  border-radius: 1rem 1rem 0.75rem 0.75rem;
  pointer-events: none;
}

.pixi-room-scene::after {
  content: "";
  position: absolute;
  inset: 0;
  z-index: 2;
  border-radius: inherit;
  background:
    linear-gradient(
      180deg,
      color-mix(in srgb, var(--lad-palette-background) 32%, transparent),
      transparent 12% 72%,
      color-mix(in srgb, var(--lad-palette-amber-100) 94%, transparent) 100%
    ),
    linear-gradient(
      90deg,
      color-mix(in srgb, var(--lad-palette-background) 52%, transparent),
      transparent 10% 90%,
      color-mix(in srgb, var(--lad-palette-background) 46%, transparent)
    );
  box-shadow:
    inset 0 0 1.75rem
      color-mix(in srgb, var(--lad-palette-background) 30%, transparent),
    inset 0 -1.5rem 2rem
      color-mix(in srgb, var(--lad-palette-amber-100) 44%, transparent);
}

.pixi-room-scene :deep(.pixi-room-scene__canvas) {
  display: block;
  width: 100%;
  height: 100%;
}
</style>
