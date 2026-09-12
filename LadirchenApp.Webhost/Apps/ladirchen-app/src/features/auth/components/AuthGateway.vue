<template>
  <main class="auth-gateway">
    <section class="auth-card" aria-labelledby="auth-title">
      <div class="auth-brand">
        <span class="auth-logo" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
        <span><strong>{{ t('common.appName') }}</strong><small>{{ t('auth.brandClaim') }}</small></span>
      </div>

      <div class="auth-switch mt-5" role="tablist" :aria-label="t('auth.accessAria')">
        <button :aria-selected="mode === 'login'" :class="{ active: mode === 'login' }" role="tab" type="button" @click="setMode('login')"><v-icon icon="mdi-login-variant" />{{ t('auth.loginTab') }}</button>
        <button :aria-selected="mode === 'register'" :class="{ active: mode === 'register' }" role="tab" type="button" @click="setMode('register')"><v-icon icon="mdi-home-heart" />{{ t('auth.registerTab') }}</button>
      </div>

      <form v-if="mode === 'login'" class="auth-form mt-5" @submit.prevent="login">
        <div class="auth-heading">
          <p>{{ t('auth.login.eyebrow') }}</p>
          <h1 id="auth-title">{{ t('auth.login.title') }}</h1>
          <span>{{ t('auth.login.description') }}</span>
        </div>
        <v-text-field v-model="loginUsername" autocomplete="username" density="comfortable" :label="t('auth.login.username')" prepend-inner-icon="mdi-account-outline" variant="outlined" />
        <v-text-field v-model="loginPassword" :append-inner-icon="showLoginPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" autocomplete="current-password" density="comfortable" :label="t('auth.login.password')" prepend-inner-icon="mdi-lock-outline" :type="showLoginPassword ? 'text' : 'password'" variant="outlined" @click:append-inner="showLoginPassword = !showLoginPassword" />
        <p v-if="errorMessage" class="auth-error" role="alert">{{ errorMessage }}</p>
        <button class="auth-submit" :disabled="submitting || !loginUsername.trim() || !loginPassword" type="button" @click="login"><v-icon icon="mdi-door-open" /><span>{{ submitting ? t('auth.login.pending') : t('auth.login.submit') }}</span><i aria-hidden="true">→</i></button>
        <div class="demo-login"><v-icon icon="mdi-flask-outline" /><span><strong>{{ t('auth.login.demoTitle') }}</strong><small>{{ t('auth.login.demoHint', { users: demoAccessLabel }) }}</small></span></div>
      </form>

      <form v-else class="auth-form mt-5" @submit.prevent="register">
        <div class="auth-heading">
          <p>{{ t('auth.register.eyebrow') }}</p>
          <h1 id="auth-title">{{ t('auth.register.title') }}</h1>
          <span>{{ t('auth.register.description') }}</span>
        </div>
        <div class="register-grid">
          <v-text-field v-model="familyName" autocomplete="organization" density="comfortable" :label="t('auth.register.familyName')" prepend-inner-icon="mdi-home-heart" variant="outlined" />
          <v-text-field v-model="guardianName" autocomplete="name" density="comfortable" :label="t('auth.register.guardianName')" prepend-inner-icon="mdi-account-heart-outline" variant="outlined" />
        </div>
        <v-text-field v-model="registerUsername" autocomplete="username" density="comfortable" :label="t('auth.login.username')" prepend-inner-icon="mdi-account-outline" variant="outlined" />
        <v-text-field v-model="registerPassword" :append-inner-icon="showRegisterPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" autocomplete="new-password" density="comfortable" :hint="t('auth.register.passwordHint')" :label="t('auth.login.password')" persistent-hint prepend-inner-icon="mdi-lock-plus-outline" :type="showRegisterPassword ? 'text' : 'password'" variant="outlined" @click:append-inner="showRegisterPassword = !showRegisterPassword" />
        <v-text-field v-model="passwordConfirmation" autocomplete="new-password" density="comfortable" :label="t('auth.register.passwordConfirmation')" prepend-inner-icon="mdi-lock-check-outline" type="password" variant="outlined" />
        <p v-if="errorMessage" class="auth-error" role="alert">{{ errorMessage }}</p>
        <button class="auth-submit auth-submit--register" :disabled="submitting || !registrationIsValid" type="button" @click="register"><v-icon icon="mdi-home-plus-outline" /><span>{{ submitting ? t('auth.register.pending') : t('auth.register.submit') }}</span><i aria-hidden="true">✦</i></button>
      </form>
    </section>
  </main>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import { authenticateLocalAccount, localUsernameExists, registerLocalAccount } from '../local-auth';
import { createDomainId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

type AuthMode = 'login' | 'register';

const store = useFamilyWorldStore();
const { t } = useI18n();
const mode = ref<AuthMode>('login');
const loginUsername = ref('');
const loginPassword = ref('');
const registerUsername = ref('');
const registerPassword = ref('');
const passwordConfirmation = ref('');
const familyName = ref('');
const guardianName = ref('');
const showLoginPassword = ref(false);
const showRegisterPassword = ref(false);
const submitting = ref(false);
const errorMessage = ref('');

const demoAccessLabel = computed(() => {
  const names = store.members
    .slice(0, 2)
    .map(member => `„${member.nickname || member.name}“`);
  return names.length > 1 ? `${names[0]} / ${names[1]}` : names[0] || t('auth.login.demoFallback');
});

const registrationIsValid = computed(() =>
  familyName.value.trim().length >= 2
  && guardianName.value.trim().length >= 2
  && registerUsername.value.trim().length >= 3
  && registerPassword.value.length >= 6
  && registerPassword.value === passwordConfirmation.value,
);

const setMode = (nextMode: AuthMode) => {
  mode.value = nextMode;
  errorMessage.value = '';
};

const login = async () => {
  if (submitting.value) return;
  submitting.value = true;
  errorMessage.value = '';
  try {
    const localAccount = await authenticateLocalAccount(loginUsername.value, loginPassword.value);
    if (localAccount) {
      const signedIn = await store.signInToFamily(localAccount.familyId, localAccount.memberId);
      if (!signedIn) errorMessage.value = t('auth.login.loadError');
      return;
    }
    const normalizedUsername = loginUsername.value.trim().toLocaleLowerCase('de');
    const demoMember = store.members.find(member =>
      member.name.trim().toLocaleLowerCase('de') === normalizedUsername
      || member.nickname?.trim().toLocaleLowerCase('de') === normalizedUsername,
    );
    if (loginPassword.value === 'demo' && demoMember && store.signInCurrentFamily(demoMember.id)) return;
    errorMessage.value = t('auth.login.credentialsError');
  } finally {
    submitting.value = false;
  }
};

const register = async () => {
  if (submitting.value || !registrationIsValid.value) return;
  if (localUsernameExists(registerUsername.value)) {
    errorMessage.value = t('auth.register.usernameExists');
    return;
  }
  submitting.value = true;
  errorMessage.value = '';
  try {
    const familyId = createDomainId.family(crypto.randomUUID());
    const guardianId = createDomainId.familyMember(crypto.randomUUID());
    await registerLocalAccount(registerUsername.value, registerPassword.value, familyName.value, familyId, guardianId);
    store.createRegisteredFamily(familyId, guardianId, guardianName.value);
  } catch {
    errorMessage.value = t('auth.register.createError');
  } finally {
    submitting.value = false;
  }
};
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.auth-gateway {
  min-height: calc(100dvh - 3.5rem);
  @apply d-grid place-center;
}
.auth-card {
  width: min(100%, 28.75rem);
  height: min(42.5rem, calc(100dvh - 5rem));
  padding: 1.5rem;
  @apply overflow-auto position-relative;
  border: 0.125rem solid
    color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  border-radius: 1.875rem;
  background:
    radial-gradient(
      circle at 92% 5%,
      color-mix(in srgb, var(--lad-palette-yellow) 25%, transparent),
      transparent 26%
    ),
    linear-gradient(
      155deg,
      var(--lad-palette-surface),
      var(--lad-palette-background) 55%,
      var(--lad-palette-background)
    );
  box-shadow:
    0 0.625rem 0
      color-mix(in srgb, var(--lad-palette-blue-strong) 10%, transparent),
    0 1.875rem 4.375rem
      color-mix(in srgb, var(--lad-palette-muted-750-2) 18%, transparent);
}
.auth-brand {
  @apply d-flex align-center;
  gap: 0.75rem;
}
.auth-logo {
  width: 3.625rem;
  height: 3.625rem;
  @apply position-relative flex-shrink-0 overflow-hidden;
  border: 0.125rem solid var(--lad-palette-white);
  border-radius: 1.1875rem;
  background: var(--lad-palette-white);
  box-shadow: 0 0.3125rem 0
    color-mix(in srgb, var(--lad-palette-teal-700) 12%, transparent);
}
.auth-logo img {
  width: 6.75rem;
  height: 6.75rem;
  @apply position-absolute;
  top: -0.75rem;
  left: -1.6875rem;
  max-width: none;
}
.auth-brand strong,
.auth-brand small {
  @apply d-block;
}
.auth-brand strong {
  color: var(--lad-palette-teal-700);
  font-size: 1.4375rem;
  letter-spacing: -0.04em;
}
.auth-brand small {
  margin-top: 0.125rem;
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.auth-switch {
  padding: 0.3125rem;
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.3125rem;
  border: 0.0625rem solid
    color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  border-radius: 1.125rem;
  background: color-mix(in srgb, var(--lad-palette-white) 65%, transparent);
}
.auth-switch button {
  min-height: 2.8125rem;
  padding: 0.5rem 0.625rem;
  @apply d-flex align-center justify-center cursor-pointer;
  gap: 0.4375rem;
  color: var(--lad-palette-muted);
  border: 0;
  border-radius: 0.875rem;
  background: transparent;
  font: inherit;
  font-size: 0.6875rem;
  font-weight: var(--lad-font-weight-heavy);
}
.auth-switch button.active {
  color: var(--lad-palette-white);
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  );
  box-shadow: 0 0.25rem 0 var(--lad-palette-teal-700);
}
.auth-switch button :deep(.v-icon) {
  font-size: 1.1875rem;
}
.auth-form {
  @apply d-flex flex-column;
}
.auth-heading {
  margin-bottom: 1.0625rem;
}
.auth-heading p {
  margin: 0 0 0.25rem;
  color: var(--lad-palette-blue);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.auth-heading h1 {
  margin: 0;
  color: var(--lad-palette-text);
  font-size: 1.625rem;
  line-height: 1.1;
  letter-spacing: -0.04em;
}
.auth-heading span {
  @apply d-block;
  margin-top: 0.4375rem;
  color: var(--lad-muted);
  font-size: 0.6875rem;
  line-height: 1.45;
}
.register-grid {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.5625rem;
}
.auth-form :deep(.v-input) {
  margin-bottom: 0.1875rem;
}
.auth-error {
  margin: 0 0 0.6875rem;
  padding: 0.5625rem 0.6875rem;
  color: var(--lad-palette-red-600);
  border: 0.0625rem solid
    color-mix(in srgb, var(--lad-palette-red-500) 18%, transparent);
  border-radius: 0.75rem;
  background: var(--lad-palette-surface);
  font-size: 0.625rem;
  font-weight: 750;
}
.auth-submit {
  width: 100%;
  min-height: 3.0625rem;
  padding: 0.5625rem 0.875rem;
  @apply d-flex align-center justify-center cursor-pointer;
  gap: 0.5625rem;
  color: var(--lad-palette-white);
  border: 0.1875rem solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  border-radius: 1.0625rem;
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-350),
    var(--lad-palette-blue-strong)
  );
  box-shadow:
    0 0.3125rem 0 var(--lad-palette-blue-strong),
    0 0.625rem 1.125rem
      color-mix(in srgb, var(--lad-palette-blue-strong) 18%, transparent);
  font: inherit;
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.auth-submit i {
  margin-left: auto;
  font-size: 1rem;
  font-style: normal;
}
.auth-submit:hover:not(:disabled) {
  transform: translateY(-0.125rem);
  box-shadow:
    0 0.4375rem 0 var(--lad-palette-blue-strong),
    0 0.8125rem 1.25rem
      color-mix(in srgb, var(--lad-palette-blue-strong) 20%, transparent);
}
.auth-submit:active:not(:disabled) {
  transform: translateY(0.125rem);
  box-shadow: 0 0.125rem 0 var(--lad-palette-blue-strong);
}
.auth-submit:disabled {
  cursor: default;
  filter: grayscale(0.25);
  opacity: 0.48;
}
.auth-submit--register {
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  );
  box-shadow:
    0 0.3125rem 0 var(--lad-palette-teal-700),
    0 0.625rem 1.125rem
      color-mix(in srgb, var(--lad-palette-teal-700) 18%, transparent);
}
.demo-login {
  margin-top: 0.875rem;
  padding: 0.625rem 0.6875rem;
  @apply d-flex align-center;
  gap: 0.5625rem;
  color: var(--lad-palette-orange-650);
  border: 0.0625rem solid
    color-mix(in srgb, var(--lad-palette-amber-500) 18%, transparent);
  border-radius: 0.875rem;
  background: var(--lad-palette-amber-100);
}
.demo-login > :deep(.v-icon) {
  color: var(--lad-palette-amber-550);
}
.demo-login strong,
.demo-login small {
  @apply d-block;
}
.demo-login strong {
  font-size: 0.5625rem;
}
.demo-login small {
  margin-top: 0.0625rem;
  font-size: 0.5rem;
  line-height: 1.35;
}
@include respond-down(phone) {
  .auth-card {
    padding: 1.125rem;
    border-radius: 1.5rem;
  }
  .register-grid {
    grid-template-columns: 1fr;
  }
  .auth-heading h1 {
    font-size: 1.4375rem;
  }
}
</style>
