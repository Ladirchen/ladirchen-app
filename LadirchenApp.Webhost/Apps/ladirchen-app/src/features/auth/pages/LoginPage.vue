<template>
  <div class="login-page">
    <RouterLink :aria-label="t('landing.aria.home')" class="login-page__back" to="/">
      <v-icon icon="i-mdi:arrow-left" size="20" />
      <span>Zur Startseite</span>
    </RouterLink>

    <div v-if="restoring" class="login-page__loading" role="status">
      <v-progress-circular color="primary" indeterminate />
      <span>Anmeldung wird geprüft …</span>
    </div>
    <AuthGateway v-else />
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import AuthGateway from "@/features/auth/components/AuthGateway.vue";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const restoring = ref(true);

function destinationAfterLogin(): string {
  const redirect = typeof route.query.redirect === "string" ? route.query.redirect : "/app";

  return redirect.startsWith("/app") ? redirect : "/app";
}

async function restoreSession(): Promise<void> {
  try {
    const restored = await store.restoreAuthenticatedSession();

    if (restored) {
      await router.replace(destinationAfterLogin());
    }
  } finally {
    restoring.value = false;
  }
}

onMounted(() => {
  void restoreSession();
});

watch(
  () => store.isAuthenticated,
  (authenticated) => {
    if (authenticated) {
      void router.replace(destinationAfterLogin());
    }
  },
);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.login-page {
  min-height: 100dvh;
  padding: 4.5rem 1rem 2rem;
  --uno: position-relative d-grid place-center;
  color: var(--lad-text);
  background:
    radial-gradient(circle at 12% 10%, color-mix(in srgb, var(--lad-color-info) 13%, transparent), transparent 28rem),
    radial-gradient(
      circle at 88% 88%,
      color-mix(in srgb, var(--lad-color-primary) 16%, transparent),
      transparent 32rem
    ),
    var(--lad-bg);
}

.login-page__back {
  padding: 0.65rem 0.9rem;
  --uno: position-absolute d-flex align-center ga-2 text-decoration-none;
  top: 1.25rem;
  left: max(1rem, calc((100vw - 72rem) / 2));
  color: var(--lad-color-primary-deep);
  border-radius: var(--lad-radius-pill);
  font-size: 0.875rem;
  font-weight: var(--lad-font-weight-strong);
}

.login-page__back:hover,
.login-page__back:focus-visible {
  background: var(--lad-color-primary-soft);
  outline: none;
}

.login-page__loading {
  --uno: d-grid place-center ga-4;
  color: var(--lad-text-strong);
  font-weight: var(--lad-font-weight-strong);
}

.login-page :deep(.auth-gateway) {
  min-height: auto;
}

@media (max-width: 30rem) {
  .login-page {
    padding: 4rem 0.75rem 1rem;
  }

  .login-page :deep(.auth-card) {
    height: min(43rem, calc(100dvh - 5rem));
  }
}
</style>
