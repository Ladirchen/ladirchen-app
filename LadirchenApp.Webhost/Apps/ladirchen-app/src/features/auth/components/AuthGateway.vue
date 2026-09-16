<template>
  <main class="auth-gateway">
    <section class="auth-card" aria-labelledby="auth-title">
      <div class="auth-brand">
        <span class="auth-logo" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
        <span><strong>Ladirchen</strong><small>Eure Familienwelt beginnt hier.</small></span>
      </div>

      <div class="auth-switch mt-5" role="tablist" aria-label="Zugang auswählen">
        <button :aria-selected="mode === 'login'" :class="{ active: mode === 'login' }" role="tab" type="button" @click="setMode('login')"><v-icon icon="mdi-login-variant" />Anmelden</button>
        <button :aria-selected="mode === 'register'" :class="{ active: mode === 'register' }" role="tab" type="button" @click="setMode('register')"><v-icon icon="mdi-home-heart" />Familie gründen</button>
      </div>

      <form v-if="mode === 'login'" class="auth-form mt-5" @submit.prevent="login">
        <div class="auth-heading">
          <p>Willkommen zurück</p>
          <h1 id="auth-title">In eure Familienwelt</h1>
          <span>Melde dich mit deinem Benutzernamen und Passwort an.</span>
        </div>
        <v-text-field v-model="loginUsername" autocomplete="username" density="comfortable" label="Benutzername" prepend-inner-icon="mdi-account-outline" variant="outlined" />
        <v-text-field v-model="loginPassword" :append-inner-icon="showLoginPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" autocomplete="current-password" density="comfortable" label="Passwort" prepend-inner-icon="mdi-lock-outline" :type="showLoginPassword ? 'text' : 'password'" variant="outlined" @click:append-inner="showLoginPassword = !showLoginPassword" />
        <p v-if="errorMessage" class="auth-error" role="alert">{{ errorMessage }}</p>
        <button class="auth-submit" :disabled="submitting || !loginUsername.trim() || !loginPassword" type="button" @click="login"><v-icon icon="mdi-door-open" /><span>{{ submitting ? 'Anmeldung läuft …' : 'Familienwelt öffnen' }}</span><i aria-hidden="true">→</i></button>
        <div class="demo-login"><v-icon icon="mdi-flask-outline" /><span><strong>Demo-Zugang</strong><small>Zum Testen: Benutzername {{ demoAccessLabel }}, Passwort „demo“.</small></span></div>
      </form>

      <form v-else class="auth-form mt-5" @submit.prevent="register">
        <div class="auth-heading">
          <p>Ein neues Zuhause</p>
          <h1 id="auth-title">Familie gründen</h1>
          <span>Lege den ersten Familienzugang an. Kinder und weitere Bezugspersonen fügst du direkt danach hinzu.</span>
        </div>
        <div class="register-grid">
          <v-text-field v-model="familyName" autocomplete="organization" density="comfortable" label="Name eurer Familie" prepend-inner-icon="mdi-home-heart" variant="outlined" />
          <v-text-field v-model="guardianName" autocomplete="name" density="comfortable" label="Dein Name" prepend-inner-icon="mdi-account-heart-outline" variant="outlined" />
        </div>
        <v-text-field v-model="registerUsername" autocomplete="username" density="comfortable" label="Benutzername" prepend-inner-icon="mdi-account-outline" variant="outlined" />
        <v-text-field v-model="registerPassword" :append-inner-icon="showRegisterPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" autocomplete="new-password" density="comfortable" hint="Mindestens 6 Zeichen" label="Passwort" persistent-hint prepend-inner-icon="mdi-lock-plus-outline" :type="showRegisterPassword ? 'text' : 'password'" variant="outlined" @click:append-inner="showRegisterPassword = !showRegisterPassword" />
        <v-text-field v-model="passwordConfirmation" autocomplete="new-password" density="comfortable" label="Passwort wiederholen" prepend-inner-icon="mdi-lock-check-outline" type="password" variant="outlined" />
        <p v-if="errorMessage" class="auth-error" role="alert">{{ errorMessage }}</p>
        <button class="auth-submit auth-submit--register" :disabled="submitting || !registrationIsValid" type="button" @click="register"><v-icon icon="mdi-home-plus-outline" /><span>{{ submitting ? 'Familie wird angelegt …' : 'Familie anlegen' }}</span><i aria-hidden="true">✦</i></button>
      </form>
    </section>
  </main>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';

import { authenticateLocalAccount, localUsernameExists, registerLocalAccount } from '../local-auth';
import { createDomainId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

type AuthMode = 'login' | 'register';

const store = useFamilyWorldStore();
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
  return names.length > 1 ? `${names[0]} oder ${names[1]}` : names[0] || 'eines Familienmitglieds';
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
      if (!signedIn) errorMessage.value = 'Der Familienzugang konnte nicht geladen werden.';
      return;
    }
    const normalizedUsername = loginUsername.value.trim().toLocaleLowerCase('de');
    const demoMember = store.members.find(member =>
      member.name.trim().toLocaleLowerCase('de') === normalizedUsername
      || member.nickname?.trim().toLocaleLowerCase('de') === normalizedUsername,
    );
    if (loginPassword.value === 'demo' && demoMember && store.signInCurrentFamily(demoMember.id)) return;
    errorMessage.value = 'Benutzername oder Passwort stimmt nicht.';
  } finally {
    submitting.value = false;
  }
};

const register = async () => {
  if (submitting.value || !registrationIsValid.value) return;
  if (localUsernameExists(registerUsername.value)) {
    errorMessage.value = 'Dieser Benutzername ist bereits vergeben.';
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
    errorMessage.value = 'Die Familie konnte lokal nicht angelegt werden. Bitte versuche es erneut.';
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
.auth-gateway {
  min-height: calc(100dvh - 56px);
  @apply d-grid place-center;
}
.auth-card {
  width: min(100%, 460px);
  height: min(680px, calc(100dvh - 80px));
  padding: 24px;
  @apply overflow-auto position-relative;
  border: 2px solid rgba(78, 143, 221, 0.16);
  border-radius: 30px;
  background:
    radial-gradient(
      circle at 92% 5%,
      rgba(255, 214, 91, 0.27),
      transparent 26%
    ),
    linear-gradient(155deg, #fffdf8, #edf8ff 55%, #eefaf4);
  box-shadow:
    0 10px 0 rgba(58, 127, 174, 0.1),
    0 30px 70px rgba(48, 76, 66, 0.18);
}
.auth-brand {
  @apply d-flex align-center;
  gap: 12px;
}
.auth-logo {
  width: 58px;
  height: 58px;
  @apply position-relative flex-shrink-0 overflow-hidden;
  border: 2px solid #fff;
  border-radius: 19px;
  background: #fff;
  box-shadow: 0 5px 0 rgba(47, 125, 97, 0.13);
}
.auth-logo img {
  width: 108px;
  height: 108px;
  @apply position-absolute;
  top: -12px;
  left: -27px;
  max-width: none;
}
.auth-brand strong,
.auth-brand small {
  @apply d-block;
}
.auth-brand strong {
  color: #286a59;
  font-size: 23px;
  letter-spacing: -0.04em;
}
.auth-brand small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 10px;
}
.auth-switch {
  padding: 5px;
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 5px;
  border: 1px solid rgba(73, 151, 198, 0.14);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.64);
}
.auth-switch button {
  min-height: 45px;
  padding: 8px 10px;
  @apply d-flex align-center justify-center cursor-pointer;
  gap: 7px;
  color: #6f7f79;
  border: 0;
  border-radius: 14px;
  background: transparent;
  font: inherit;
  font-size: 11px;
  font-weight: 900;
}
.auth-switch button.active {
  color: #fff;
  background: linear-gradient(145deg, #55bd96, #328e70);
  box-shadow: 0 4px 0 #277258;
}
.auth-switch button :deep(.v-icon) {
  font-size: 19px;
}
.auth-form {
  @apply d-flex flex-column;
}
.auth-heading {
  margin-bottom: 17px;
}
.auth-heading p {
  margin: 0 0 4px;
  color: #4e8fdd;
  font-size: 9px;
  font-weight: 950;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.auth-heading h1 {
  margin: 0;
  color: #253843;
  font-size: 26px;
  line-height: 1.1;
  letter-spacing: -0.04em;
}
.auth-heading span {
  @apply d-block;
  margin-top: 7px;
  color: var(--lad-muted);
  font-size: 11px;
  line-height: 1.45;
}
.register-grid {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 9px;
}
.auth-form :deep(.v-input) {
  margin-bottom: 3px;
}
.auth-error {
  margin: 0 0 11px;
  padding: 9px 11px;
  color: #9b3b36;
  border: 1px solid rgba(208, 74, 67, 0.18);
  border-radius: 12px;
  background: #fff0ed;
  font-size: 10px;
  font-weight: 750;
}
.auth-submit {
  width: 100%;
  min-height: 49px;
  padding: 9px 14px;
  @apply d-flex align-center justify-center cursor-pointer;
  gap: 9px;
  color: #fff;
  border: 3px solid rgba(255, 255, 255, 0.78);
  border-radius: 17px;
  background: linear-gradient(145deg, #5bb8ef, #397fd2);
  box-shadow:
    0 5px 0 #2f6db3,
    0 10px 18px rgba(50, 111, 177, 0.17);
  font: inherit;
  font-size: 12px;
  font-weight: 950;
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.auth-submit i {
  margin-left: auto;
  font-size: 16px;
  font-style: normal;
}
.auth-submit:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 #2f6db3,
    0 13px 20px rgba(50, 111, 177, 0.2);
}
.auth-submit:active:not(:disabled) {
  transform: translateY(2px);
  box-shadow: 0 2px 0 #2f6db3;
}
.auth-submit:disabled {
  cursor: default;
  filter: grayscale(0.25);
  opacity: 0.48;
}
.auth-submit--register {
  background: linear-gradient(145deg, #59c69a, #2f9570);
  box-shadow:
    0 5px 0 #267356,
    0 10px 18px rgba(39, 119, 88, 0.17);
}
.demo-login {
  margin-top: 14px;
  padding: 10px 11px;
  @apply d-flex align-center;
  gap: 9px;
  color: #6d5b32;
  border: 1px solid rgba(218, 160, 51, 0.18);
  border-radius: 14px;
  background: #fff6d8;
}
.demo-login > :deep(.v-icon) {
  color: #c8841d;
}
.demo-login strong,
.demo-login small {
  @apply d-block;
}
.demo-login strong {
  font-size: 9px;
}
.demo-login small {
  margin-top: 1px;
  font-size: 8px;
  line-height: 1.35;
}
@media (max-width: 430px) {
  .auth-card {
    padding: 18px;
    border-radius: 24px;
  }
  .register-grid {
    grid-template-columns: 1fr;
  }
  .auth-heading h1 {
    font-size: 23px;
  }
}
</style>
