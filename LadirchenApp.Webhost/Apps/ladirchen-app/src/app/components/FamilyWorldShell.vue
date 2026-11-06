<template>
  <div class="family-world-background family-world-background--fullscreen" :class="shellLayoutClasses">
    <div v-if="!appHydrated" class="family-world-loading" role="status">
      <v-progress-circular color="primary" indeterminate />
    </div>
    <div v-else-if="!store.isAuthenticated" class="family-world-loading" role="status">
      <v-progress-circular color="primary" indeterminate />
    </div>
    <template v-else>
      <LadirchenIntro @finished="handleIntroFinished" />

      <div class="family-world-shell position-relative d-flex flex-column overflow-hidden mx-auto">
        <FamilyWorldHeader />

        <main ref="contentElement" class="family-world-content flex-grow-1 overflow-auto">
          <RouterView v-slot="{ Component, route }">
            <Transition name="family-world-page" mode="out-in">
              <div :key="route.path" class="family-world-route">
                <component :is="Component" />
              </div>
            </Transition>
          </RouterView>
        </main>

        <FamilyWorldNavigation />

        <AsyncGlobalLadiGuide v-if="showLadiGuide" />
      </div>

      <FamilyWorldDialogs />
    </template>
  </div>
</template>

<script lang="ts" setup>
import { computed, defineAsyncComponent, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useDisplay } from "vuetify";
import LadirchenIntro from "@/features/onboarding/components/LadirchenIntro.vue";
import { useFamilyWorldStore } from "@/stores/family-world";
import { CURRENT_TIME_REFRESH_INTERVAL_MS } from "@/shared/runtime-timing";
import FamilyWorldDialogs from "./FamilyWorldDialogs.vue";
import FamilyWorldHeader from "./FamilyWorldHeader.vue";
import FamilyWorldNavigation from "./FamilyWorldNavigation.vue";

const AsyncGlobalLadiGuide = defineAsyncComponent(() => import("./GlobalLadiGuide.vue"));
let clockTimer: ReturnType<typeof window.setInterval> | undefined;
const store = useFamilyWorldStore();
const currentRoute = useRoute();
const router = useRouter();
const { height: viewportHeight, md, platform, sm, width: viewportWidth, xs } = useDisplay();
const appHydrated = ref(false);
const introFinished = ref(false);
const contentElement = ref<HTMLElement | null>(null);
const showLadiGuide = computed(() => appHydrated.value && introFinished.value && store.viewerRole === "child");
const isLandscape = computed(() => viewportWidth.value > viewportHeight.value);
const shellLayoutClasses = computed(() => ({
  "family-world-background--compact": xs.value,
  "family-world-background--expanded-padding": sm.value || (md.value && platform.value.touch),
  "family-world-background--landscape": md.value && isLandscape.value,
}));

function handleIntroFinished() {
  introFinished.value = true;
  store.revealNextContributionReward();
}

function resetContentScroll() {
  window.requestAnimationFrame(() => contentElement.value?.scrollTo({ top: 0, left: 0 }));
}

async function restoreAuthenticationSession() {
  appHydrated.value = false;

  try {
    const restored = await store.restoreAuthenticatedSession();

    if (!restored) {
      await router.replace({ path: "/login", query: { redirect: currentRoute.fullPath } });
    }
  } finally {
    appHydrated.value = true;
  }
}

void restoreAuthenticationSession();
onMounted(() => {
  store.refreshCurrentTime();
  clockTimer = window.setInterval(() => store.refreshCurrentTime(), CURRENT_TIME_REFRESH_INTERVAL_MS);
});
onBeforeUnmount(() => {
  if (clockTimer !== undefined) {
    window.clearInterval(clockTimer);
  }
});
watch(isLandscape, resetContentScroll);
watch(
  () => store.isAuthenticated,
  (authenticated) => {
    if (appHydrated.value && !authenticated) {
      void router.replace({ path: "/login", query: { redirect: currentRoute.fullPath } });
    }
  },
);
watch(
  () => [store.signedInMember?.guardianAccess, currentRoute.path] as const,
  ([access, path]) => {
    if (access === "external" && !["/app/contributions", "/app/wishes", "/app/shop", "/app/profile"].includes(path)) {
      void router.replace("/app/contributions");
    }
  },
  { immediate: true },
);
</script>

<style src="@/styles/family-world.scss"></style>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.family-world-loading {
  min-height: calc(100dvh - 3.5rem);
  --uno: d-grid place-center;
}
.family-world-session-error h1 {
  font-size: rem(24);
}
.family-world-page-enter-active,
.family-world-page-leave-active {
  transition:
    opacity 0.22s ease,
    transform 0.22s cubic-bezier(0.22, 0.8, 0.26, 1);
}
.family-world-page-enter-from {
  opacity: 0;
  transform: translateY(14px) scale(0.992);
}
.family-world-page-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.996);
}
@include reduced-motion {
  .family-world-page-enter-active,
  .family-world-page-leave-active {
    transition: none;
  }
}
</style>
