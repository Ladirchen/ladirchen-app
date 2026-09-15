<template>
  <main class="auth-gateway">
    <section class="auth-card" aria-labelledby="auth-title">
      <div class="auth-brand">
        <span class="auth-logo" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
        <span><strong>{{ t('common.appName') }}</strong><small>{{ t('auth.brandClaim') }}</small></span>
      </div>

      <div class="auth-switch mt-5" role="tablist" :aria-label="t('auth.accessAria')">
        <button :aria-selected="mode === 'login'" :class="{ active: mode === 'login' }" role="tab" type="button" @click="setMode('login')"><v-icon icon="i-mdi:login-variant" />{{ t('auth.loginTab') }}</button>
        <button :aria-selected="mode === 'register'" :class="{ active: mode === 'register' }" role="tab" type="button" @click="setMode('register')"><v-icon icon="i-mdi:home-heart" />{{ t('auth.registerTab') }}</button>
      </div>

      <form v-if="mode === 'login'" class="auth-form mt-5" @submit.prevent="login">
        <div class="auth-heading">
          <p>{{ t('auth.login.eyebrow') }}</p>
          <h1 id="auth-title">{{ t('auth.login.title') }}</h1>
          <span>{{ t('auth.login.description') }}</span>
        </div>
        <v-text-field v-model="loginUsername" autocomplete="username" density="comfortable" :label="t('auth.login.username')" prepend-inner-icon="i-mdi:account-outline" variant="outlined" />
        <v-text-field v-model="loginPassword" :append-inner-icon="showLoginPassword ? 'i-mdi:eye-off-outline' : 'i-mdi:eye-outline'" autocomplete="current-password" density="comfortable" :label="t('auth.login.password')" prepend-inner-icon="i-mdi:lock-outline" :type="showLoginPassword ? 'text' : 'password'" variant="outlined" @click:append-inner="showLoginPassword = !showLoginPassword" />
        <p v-if="errorMessage" class="auth-error" role="alert">{{ errorMessage }}</p>
        <button class="auth-submit" :disabled="submitting || !loginUsername.trim() || !loginPassword" type="submit"><v-icon icon="i-mdi:door-open" /><span>{{ submitting ? t('auth.login.pending') : t('auth.login.submit') }}</span><i aria-hidden="true">→</i></button>
        <div class="demo-login"><v-icon icon="i-mdi:flask-outline" /><span><strong>{{ t('auth.login.demoTitle') }}</strong><small>{{ t('auth.login.demoHint', { users: demoAccessLabel }) }}</small></span></div>
      </form>

      <form v-else class="auth-form mt-5" @submit.prevent="register">
        <div class="auth-heading">
          <p>{{ t('auth.register.eyebrow') }}</p>
          <h1 id="auth-title">{{ t('auth.register.title') }}</h1>
          <span>{{ t('auth.register.description') }}</span>
        </div>
        <div class="register-grid">
          <v-text-field v-model="familyName" autocomplete="organization" density="comfortable" :label="t('auth.register.familyName')" prepend-inner-icon="i-mdi:home-heart" variant="outlined" />
          <v-text-field v-model="guardianName" autocomplete="name" density="comfortable" :label="t('auth.register.guardianName')" prepend-inner-icon="i-mdi:account-heart-outline" variant="outlined" />
        </div>
        <v-text-field v-model="registerUsername" autocomplete="username" density="comfortable" :label="t('auth.login.username')" prepend-inner-icon="i-mdi:account-outline" variant="outlined" />
        <v-text-field v-model="registerPassword" :append-inner-icon="showRegisterPassword ? 'i-mdi:eye-off-outline' : 'i-mdi:eye-outline'" autocomplete="new-password" density="comfortable" :hint="t('auth.register.passwordHint')" :label="t('auth.login.password')" persistent-hint prepend-inner-icon="i-mdi:lock-plus-outline" :type="showRegisterPassword ? 'text' : 'password'" variant="outlined" @click:append-inner="showRegisterPassword = !showRegisterPassword" />
        <v-text-field v-model="passwordConfirmation" autocomplete="new-password" density="comfortable" :label="t('auth.register.passwordConfirmation')" prepend-inner-icon="i-mdi:lock-check-outline" type="password" variant="outlined" />
        <p v-if="errorMessage" class="auth-error" role="alert">{{ errorMessage }}</p>
        <button class="auth-submit auth-submit--register" :disabled="submitting || !registrationIsValid" type="submit"><v-icon icon="i-mdi:home-plus-outline" /><span>{{ submitting ? t('auth.register.pending') : t('auth.register.submit') }}</span><i aria-hidden="true">✦</i></button>
      </form>
    </section>
  </main>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import { authenticationGateway } from '@/app/composition-root';
import { createDomainId } from '@/domain/shared/identifiers';
import { AUTH_INPUT_RULES, DEMO_CREDENTIALS } from '@/features/auth/auth-config';
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
    .slice(0, AUTH_INPUT_RULES.demoMemberCount)
    .map(member => `„${member.nickname || member.name}“`);
  return names.length > 1 ? `${names[0]} / ${names[1]}` : names[0] || t('auth.login.demoFallback');
});

const registrationIsValid = computed(() =>
  familyName.value.trim().length >= AUTH_INPUT_RULES.familyNameMinimumLength
  && guardianName.value.trim().length >= AUTH_INPUT_RULES.guardianNameMinimumLength
  && registerUsername.value.trim().length >= AUTH_INPUT_RULES.usernameMinimumLength
  && registerPassword.value.length >= AUTH_INPUT_RULES.passwordMinimumLength
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
    const localAccount = await authenticationGateway.authenticate(loginUsername.value, loginPassword.value);
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
    if (loginPassword.value === DEMO_CREDENTIALS.password && demoMember && store.signInCurrentFamily(demoMember.id)) return;
    errorMessage.value = t('auth.login.credentialsError');
  } finally {
    submitting.value = false;
  }
};

const register = async () => {
  if (submitting.value || !registrationIsValid.value) return;
  if (await authenticationGateway.usernameExists(registerUsername.value)) {
    errorMessage.value = t('auth.register.usernameExists');
    return;
  }
  submitting.value = true;
  errorMessage.value = '';
  try {
    const familyId = createDomainId.family(crypto.randomUUID());
    const guardianId = createDomainId.familyMember(crypto.randomUUID());
    await authenticationGateway.register({
      familyId,
      familyName: familyName.value,
      memberId: guardianId,
      password: registerPassword.value,
      username: registerUsername.value,
    });
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
  border: rem(2) solid
    color-mix(in srgb, var(--lad-color-info) 15%, transparent);
  border-radius: rem(30);
  background:
    radial-gradient(
      circle at 92% 5%,
      color-mix(in srgb, var(--lad-color-reward) 25%, transparent),
      transparent 26%
    ),
    linear-gradient(
      155deg,
      var(--lad-surface),
      var(--lad-surface-soft) 55%,
      var(--lad-surface-soft)
    );
  box-shadow:
    0 rem(10) 0
      color-mix(in srgb, var(--lad-color-info-strong) 10%, transparent),
    0 rem(30) rem(70)
      color-mix(in srgb, var(--lad-text-warm) 18%, transparent);
}
.auth-brand {
  @apply d-flex align-center;
  gap: 0.75rem;
}
.auth-logo {
  width: rem(58);
  height: rem(58);
  @apply position-relative flex-shrink-0 overflow-hidden;
  border: rem(2) solid var(--lad-border-on-accent);
  border-radius: rem(19);
  background: var(--lad-surface-raised);
  box-shadow: 0 rem(5) 0
    color-mix(in srgb, var(--lad-color-primary-deep) 12%, transparent);
}
.auth-logo img {
  width: 6.75rem;
  height: 6.75rem;
  @apply position-absolute;
  top: -0.75rem;
  left: -rem(27);
  max-width: none;
}
.auth-brand strong,
.auth-brand small {
  @apply d-block;
}
.auth-brand strong {
  color: var(--lad-color-primary-deep);
  font-size: rem(23);
  letter-spacing: -0.04em;
}
.auth-brand small {
  margin-top: rem(2);
  color: var(--lad-muted);
  font-size: rem(10);
}
.auth-switch {
  padding: rem(5);
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: rem(5);
  border: rem(1) solid
    color-mix(in srgb, var(--lad-color-info) 15%, transparent);
  border-radius: rem(18);
  background: color-mix(in srgb, var(--lad-surface-raised) 65%, transparent);
}
.auth-switch button {
  min-height: rem(45);
  padding: 0.5rem rem(10);
  @apply d-flex align-center justify-center cursor-pointer;
  gap: rem(7);
  color: var(--lad-muted);
  border: 0;
  border-radius: rem(14);
  background: transparent;
  font: inherit;
  font-size: rem(11);
  font-weight: var(--lad-font-weight-heavy);
}
.auth-switch button.active {
  color: var(--lad-text-inverse);
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow: 0 0.25rem 0 var(--lad-color-primary-deep);
}
.auth-switch button :deep(.v-icon) {
  font-size: rem(19);
}
.auth-form {
  @apply d-flex flex-column;
}
.auth-heading {
  margin-bottom: rem(17);
}
.auth-heading p {
  margin: 0 0 0.25rem;
  color: var(--lad-color-info);
  font-size: rem(9);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.auth-heading h1 {
  @apply ma-0;
  color: var(--lad-text);
  font-size: rem(26);
  line-height: 1.1;
  letter-spacing: -0.04em;
}
.auth-heading span {
  @apply d-block;
  margin-top: rem(7);
  color: var(--lad-muted);
  font-size: rem(11);
  line-height: 1.45;
}
.register-grid {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: rem(9);
}
.auth-form :deep(.v-input) {
  margin-bottom: rem(3);
}
.auth-error {
  margin: 0 0 rem(11);
  padding: rem(9) rem(11);
  color: var(--lad-color-danger-strong);
  border: rem(1) solid
    color-mix(in srgb, var(--lad-color-danger-muted) 18%, transparent);
  border-radius: 0.75rem;
  background: var(--lad-surface);
  font-size: rem(10);
  font-weight: 750;
}
.auth-submit {
  width: 100%;
  min-height: rem(49);
  padding: rem(9) rem(14);
  @apply d-flex align-center justify-center cursor-pointer;
  gap: rem(9);
  color: var(--lad-text-inverse);
  border: rem(3) solid
    color-mix(in srgb, var(--lad-border-on-accent) 80%, transparent);
  border-radius: rem(17);
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info-strong)
  );
  box-shadow:
    0 rem(5) 0 var(--lad-color-info-strong),
    0 rem(10) rem(18)
      color-mix(in srgb, var(--lad-color-info-strong) 18%, transparent);
  font: inherit;
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.auth-submit i {
  @apply ml-auto;
  font-size: 1rem;
  font-style: normal;
}
.auth-submit:hover:not(:disabled) {
  transform: translateY(-rem(2));
  box-shadow:
    0 rem(7) 0 var(--lad-color-info-strong),
    0 rem(13) 1.25rem
      color-mix(in srgb, var(--lad-color-info-strong) 20%, transparent);
}
.auth-submit:active:not(:disabled) {
  transform: translateY(rem(2));
  box-shadow: 0 rem(2) 0 var(--lad-color-info-strong);
}
.auth-submit:disabled {
  cursor: default;
  filter: grayscale(0.25);
  opacity: 0.48;
}
.auth-submit--register {
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow:
    0 rem(5) 0 var(--lad-color-primary-deep),
    0 rem(10) rem(18)
      color-mix(in srgb, var(--lad-color-primary-deep) 18%, transparent);
}
.demo-login {
  margin-top: rem(14);
  padding: rem(10) rem(11);
  @apply d-flex align-center;
  gap: rem(9);
  color: var(--lad-color-accent-warm-deep);
  border: rem(1) solid
    color-mix(in srgb, var(--lad-color-reward-accent) 18%, transparent);
  border-radius: rem(14);
  background: var(--lad-color-reward-soft);
}
.demo-login > :deep(.v-icon) {
  color: var(--lad-color-reward-shadow);
}
.demo-login strong,
.demo-login small {
  @apply d-block;
}
.demo-login strong {
  font-size: rem(9);
}
.demo-login small {
  margin-top: rem(1);
  font-size: 0.5rem;
  line-height: 1.35;
}
@include respond-down(phone) {
  .auth-card {
    padding: rem(18);
    border-radius: 1.5rem;
  }
  .register-grid {
    grid-template-columns: 1fr;
  }
  .auth-heading h1 {
    font-size: rem(23);
  }
}
</style>
