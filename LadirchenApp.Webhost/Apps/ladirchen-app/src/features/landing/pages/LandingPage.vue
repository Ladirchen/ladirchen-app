<template>
  <div class="landing-page">
    <header class="landing-header">
      <a :aria-label="t('landing.aria.home')" class="landing-brand" href="#top">
        <span class="landing-brand__logo"><img alt="" height="58" src="/ladirli-icon-192x192.png" width="58" /></span>
        <strong>Ladirli</strong>
      </a>
      <nav :aria-label="t('landing.aria.navigation')" class="landing-nav">
        <a href="#so-funktionierts">So funktioniert’s</a>
        <a href="#familienalltag">Vorteile</a>
        <a href="#fragen">FAQ</a>
      </nav>
      <RouterLink class="landing-login" to="/login">Anmelden</RouterLink>
    </header>

    <main id="top">
      <section class="landing-hero landing-section">
        <div class="landing-hero__copy">
          <p class="landing-eyebrow">Familienalltag gemeinsam gestalten</p>
          <h1>Gemeinsam anpacken.<br /><span>Fair verteilen.</span></h1>
          <p class="landing-lead">
            Ladirli hilft Familien, Aufgaben und Verantwortung im Alltag gemeinsam zu organisieren – fair, übersichtlich
            und kindgerecht.
          </p>
          <div class="landing-actions">
            <RouterLink class="landing-button landing-button--primary" to="/login">
              Jetzt starten
              <v-icon icon="i-mdi:arrow-right" size="21" />
            </RouterLink>
            <button v-if="!installed" class="landing-button landing-button--secondary" type="button" @click="install">
              <v-icon icon="i-mdi:download" size="21" />
              App installieren
            </button>
            <span v-else class="landing-installed">
              <v-icon icon="i-mdi:check-circle" size="21" />
              App ist installiert
            </span>
          </div>
          <ul class="landing-trust" :aria-label="t('landing.aria.benefits')">
            <li><v-icon icon="i-mdi:check-circle-outline" /> Ohne App-Store nutzbar</li>
            <li><v-icon icon="i-mdi:check-circle-outline" /> Für Smartphone und Desktop</li>
          </ul>
        </div>

        <div class="landing-hero__visual" :aria-label="t('landing.aria.hero')">
          <span class="landing-orbit landing-orbit--task"><v-icon icon="i-mdi:check-bold" /></span>
          <span class="landing-orbit landing-orbit--family"><v-icon icon="i-mdi:account-group" /></span>
          <span class="landing-orbit landing-orbit--reward"><LadirchenCoin animated /></span>
          <img
            class="landing-ladi"
            :src="starterLadi"
            :alt="t('landing.alt.mascot')"
            fetchpriority="high"
            height="512"
            width="512"
          />
          <div class="landing-hero__message">
            <strong>Heute gemeinsam geschafft!</strong>
            <span>Aufgaben werden sichtbar und Erfolge zählen.</span>
          </div>
        </div>
      </section>

      <section id="so-funktionierts" class="landing-section landing-how">
        <div class="landing-heading">
          <p class="landing-eyebrow">So funktioniert Ladirli</p>
          <h2>Ein klarer Ablauf für die ganze Familie</h2>
          <p>Aufgaben planen, gemeinsam erledigen und Fortschritte transparent machen.</p>
        </div>
        <ol class="landing-steps">
          <li v-for="(step, index) in steps" :key="step.title">
            <span class="landing-step__number">{{ index + 1 }}</span>
            <v-icon :icon="step.icon" size="30" />
            <h3>{{ step.title }}</h3>
            <p>{{ step.description }}</p>
          </li>
        </ol>
      </section>

      <section id="familienalltag" class="landing-section landing-showcase">
        <article class="landing-feature">
          <div class="landing-feature__copy">
            <p class="landing-eyebrow">Aufgabenverteilung Familie</p>
            <h2>Jeder weiss, was als Nächstes ansteht</h2>
            <p>
              Bezugspersonen erstellen Haushaltsaufgaben, weisen sie passend zu und behalten offene sowie erledigte
              Beiträge im Blick. Kinder sehen verständlich, was sie übernehmen können.
            </p>
            <ul>
              <li><v-icon icon="i-mdi:check" /> Titel, Beschreibung und Belohnung auf einen Blick</li>
              <li><v-icon icon="i-mdi:check" /> Offene oder fest zugewiesene Aufgaben</li>
              <li><v-icon icon="i-mdi:check" /> Bestätigung durch eine Bezugsperson</li>
            </ul>
          </div>
          <div class="product-phone" :aria-label="t('landing.aria.tasksPreview')">
            <div class="product-phone__top">
              <span class="product-logo"><img alt="" src="/ladirli-icon-192x192.png" /></span>
              <strong>Unsere Aufgaben</strong>
              <span class="product-balance"><LadirchenCoin small /> 12</span>
            </div>
            <div class="product-card product-card--blue">
              <span class="product-icon">🎒</span>
              <div><strong>Schulsachen vorbereiten</strong><small>Heute · 10 Energie</small></div>
              <span class="product-pill">Offen</span>
            </div>
            <div class="product-card product-card--green">
              <span class="product-icon">✨</span>
              <div><strong>Tisch gemeinsam decken</strong><small>2 Ladirli · Teamaufgabe</small></div>
              <span class="product-pill">Mia</span>
            </div>
            <div class="product-card product-card--amber">
              <span class="product-icon">🧺</span>
              <div><strong>Wäsche sortieren</strong><small>Heute · 15 Energie</small></div>
              <span class="product-pill">Noah</span>
            </div>
          </div>
        </article>

        <article class="landing-feature landing-feature--reverse">
          <div class="landing-feature__copy">
            <p class="landing-eyebrow">Motivation, die sichtbar wird</p>
            <h2>Fortschritt, Sparziele und Belohnungen</h2>
            <p>
              Bestätigte Aufgaben bringen Ladirli und Hausenergie. Kinder können für eigene Ziele sparen und im
              Familienshop vereinbarte Belohnungen auswählen.
            </p>
            <ul>
              <li><v-icon icon="i-mdi:check" /> Individuelle und gemeinsame Sparziele</li>
              <li><v-icon icon="i-mdi:check" /> Familien-Shop mit klaren Preisen</li>
              <li><v-icon icon="i-mdi:check" /> Streaks und Familienfortschritt</li>
            </ul>
          </div>
          <div class="goal-preview" :aria-label="t('landing.aria.goalPreview')">
            <div class="goal-preview__art">
              <img :src="sunLadi" :alt="t('landing.alt.progressMascot')" loading="lazy" />
            </div>
            <div class="goal-preview__copy">
              <span class="product-pill">Mein Ziel</span>
              <h3>Ausflug in den Tierpark</h3>
              <p>Gemeinsam Schritt für Schritt sparen.</p>
              <div class="goal-preview__amount"><LadirchenCoin small /> <strong>80 / 120</strong></div>
              <div class="goal-preview__bar"><span></span></div>
            </div>
          </div>
        </article>

        <article class="landing-feature">
          <div class="landing-feature__copy">
            <p class="landing-eyebrow">Eine Familienwelt, die mitwächst</p>
            <h2>Gemeinsame Erfolge werden zum Zuhause</h2>
            <p>
              Erledigte Aufgaben stärken die Hausenergie. Familien gestalten ihre digitale Welt mit Zimmerdesigns,
              Möbeln und Haustieren, die in Ladirli wirklich vorhanden sind.
            </p>
          </div>
          <div class="world-preview" :aria-label="t('landing.aria.worldPreview')">
            <img
              class="world-preview__background"
              :src="familyWorldBackground"
              :alt="t('landing.alt.familyWorld')"
              height="992"
              loading="lazy"
              width="1586"
            />
            <img class="world-preview__house" :src="starterHouse" alt="" loading="lazy" />
            <div class="world-preview__family" aria-hidden="true">
              <span
                v-for="avatar in sampleAvatars"
                :key="avatar.emoji"
                class="sample-avatar"
                :class="`sample-avatar--${avatar.tone}`"
                >{{ avatar.emoji }}</span
              >
            </div>
            <span class="world-preview__pet" aria-hidden="true">🐱</span>
            <span class="world-preview__energy"><v-icon icon="i-mdi:home-heart" /> 72 % Hausenergie</span>
          </div>
        </article>
      </section>

      <section class="landing-section landing-benefits">
        <div class="landing-heading">
          <p class="landing-eyebrow">Für einen übersichtlichen Familienalltag</p>
          <h2>Faire Beteiligung beginnt mit Transparenz</h2>
        </div>
        <div class="landing-benefit-grid">
          <article v-for="benefit in benefits" :key="benefit.title">
            <v-icon :icon="benefit.icon" size="31" />
            <h3>{{ benefit.title }}</h3>
            <p>{{ benefit.description }}</p>
          </article>
        </div>
      </section>

      <section id="installieren" class="landing-section landing-install">
        <div class="landing-install__copy">
          <p class="landing-eyebrow">Ladirli als App</p>
          <h2>Direkt auf dem Smartphone installieren</h2>
          <p>
            Ladirli ist eine Progressive Web App. Sie lässt sich auf dem Startbildschirm öffnen und fühlt sich dort wie
            eine App an – ohne einen Store-Download.
          </p>
          <button v-if="!installed" class="landing-button landing-button--primary" type="button" @click="install">
            <v-icon icon="i-mdi:download" size="21" />
            {{ installButtonLabel }}
          </button>
          <span v-else class="landing-installed landing-installed--light">
            <v-icon icon="i-mdi:check-circle" size="21" /> App ist installiert
          </span>
        </div>
        <div class="landing-install__devices" aria-hidden="true">
          <div class="device-card device-card--android"><v-icon icon="i-mdi:android" /> <span>Android</span></div>
          <div class="device-card device-card--ios"><v-icon icon="i-mdi:apple" /> <span>iPhone & iPad</span></div>
          <div class="device-card device-card--desktop"><v-icon icon="i-mdi:laptop" /> <span>Desktop</span></div>
        </div>
      </section>

      <section id="fragen" class="landing-section landing-faq">
        <div class="landing-heading">
          <p class="landing-eyebrow">Häufige Fragen</p>
          <h2>Gut zu wissen</h2>
        </div>
        <div class="landing-faq__list">
          <details v-for="faq in faqs" :key="faq.question">
            <summary>{{ faq.question }}<v-icon icon="i-mdi:chevron-down" /></summary>
            <p>{{ faq.answer }}</p>
          </details>
        </div>
      </section>

      <section class="landing-section landing-final">
        <img :src="perchedLadi" :alt="t('landing.alt.invitationMascot')" height="512" loading="lazy" width="512" />
        <div>
          <h2>Bereit für einen faireren Familienalltag?</h2>
          <p>Öffne eure Familienwelt und organisiert die nächsten Aufgaben gemeinsam.</p>
          <RouterLink class="landing-button landing-button--primary" to="/login">Jetzt starten</RouterLink>
        </div>
      </section>
    </main>

    <footer class="landing-footer">
      <div class="landing-brand">
        <span class="landing-brand__logo"><img alt="" height="48" src="/ladirli-icon-192x192.png" width="48" /></span>
        <strong>Ladirli</strong>
      </div>
      <p>Familienaufgaben fair, übersichtlich und kindgerecht organisieren.</p>
      <nav :aria-label="t('landing.aria.footerNavigation')">
        <RouterLink to="/login">Login</RouterLink>
        <a href="#fragen">FAQ</a>
        <a href="#installieren">App installieren</a>
      </nav>
    </footer>

    <v-dialog v-model="instructionsOpen" max-width="470">
      <v-card class="installation-dialog pa-6" rounded="xl">
        <div class="installation-dialog__icon"><v-icon icon="i-mdi:cellphone-arrow-down" size="36" /></div>
        <h2>App installieren</h2>
        <template v-if="ios">
          <p>Auf iPhone und iPad erfolgt die Installation über Safari:</p>
          <ol>
            <li>Tippe in Safari auf <strong>Teilen</strong>.</li>
            <li>Wähle <strong>Zum Home-Bildschirm</strong>.</li>
            <li>Bestätige oben rechts mit <strong>Hinzufügen</strong>.</li>
          </ol>
        </template>
        <template v-else>
          <p>
            Öffne das Browsermenü und wähle <strong>App installieren</strong> oder <strong>Zum Startbildschirm</strong>.
          </p>
          <p class="installation-dialog__hint">
            Falls diese Option fehlt, kannst du Ladirli weiterhin im Browser nutzen.
          </p>
        </template>
        <v-btn block color="primary" rounded="lg" variant="flat" @click="instructionsOpen = false">Verstanden</v-btn>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import familyWorldBackground from "@/assets/room-designs/starter-home-ladi-background.webp";
import starterHouse from "@/assets/room-designs/starter-home-ladi-house.webp";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import perchedLadi from "@/assets/ladi/perched-ladi.webp";
import starterLadi from "@/assets/ladi/starter-ladi.webp";
import sunLadi from "@/assets/ladi/sun-ladi.webp";
import LadirchenCoin from "@/shared/components/LadirchenCoin.vue";
import { usePwaInstall } from "@/features/landing/use-pwa-install";

const sampleAvatars = [
  { emoji: "🧑🏽", tone: "mint" },
  { emoji: "👧🏻", tone: "blue" },
  { emoji: "👦🏾", tone: "gold" },
] as const;

const steps = [
  {
    description: "Bezugspersonen erfassen Aufgaben mit Beschreibung, Wert und optionaler Zuweisung.",
    icon: "i-mdi:clipboard-plus-outline",
    title: "Aufgaben planen",
  },
  {
    description: "Kinder sehen ihre offenen Beiträge und markieren erledigte Aufgaben verständlich selbst.",
    icon: "i-mdi:account-child-outline",
    title: "Gemeinsam erledigen",
  },
  {
    description: "Eine Bezugsperson prüft den Beitrag und bestätigt die vereinbarte Belohnung.",
    icon: "i-mdi:check-decagram-outline",
    title: "Fair bestätigen",
  },
  {
    description: "Ladirli, Hausenergie, Sparziele und Streaks machen den Familienfortschritt sichtbar.",
    icon: "i-mdi:chart-line-variant",
    title: "Fortschritt erleben",
  },
];
const benefits = [
  {
    description: "Offene, zugewiesene und erledigte Aufgaben sind für berechtigte Familienmitglieder nachvollziehbar.",
    icon: "i-mdi:eye-outline",
    title: "Mehr Überblick",
  },
  {
    description: "Belohnungen und Bestätigungen folgen den Regeln, die Bezugspersonen in der Familie vereinbaren.",
    icon: "i-mdi:scale-balance",
    title: "Faire Regeln",
  },
  {
    description: "Kinder erhalten eine klare, spielerische Oberfläche; Verwaltungsfunktionen bleiben rollenbasiert.",
    icon: "i-mdi:shield-account-outline",
    title: "Passend für jede Rolle",
  },
];
const faqs = [
  {
    answer:
      "Ladirli ist eine Familienorganisation-App für Haushaltsaufgaben, Beiträge, Sparziele, Belohnungen und gemeinsamen Fortschritt.",
    question: "Was ist Ladirli?",
  },
  {
    answer:
      "Die Oberfläche richtet sich an Familien. Kinder im Grundschul- und frühen Teenageralter sind eine wichtige Nutzergruppe; Bezugspersonen verwalten Aufgaben und Berechtigungen.",
    question: "Für wen ist Ladirli gedacht?",
  },
  {
    answer:
      "Bezugspersonen können Aufgaben offen lassen oder zuweisen. Kinder markieren Beiträge als erledigt, anschliessend können berechtigte Bezugspersonen sie prüfen und bestätigen.",
    question: "Wie funktioniert die Aufgabenverteilung?",
  },
  {
    answer:
      "Auf unterstützten Browsern erscheint eine Installationsoption. Auf dem iPhone öffnest du Ladirli in Safari und wählst über Teilen die Option „Zum Home-Bildschirm“.",
    question: "Wie installiere ich Ladirli?",
  },
  {
    answer:
      "Ja. Ladirli ist als Progressive Web App für moderne Browser auf iPhone, iPad, Android und Desktop ausgelegt. Die Installationsschritte unterscheiden sich je nach Plattform.",
    question: "Funktioniert Ladirli auf iPhone und Android?",
  },
  {
    answer:
      "Familienbereiche erfordern eine Anmeldung. Zugriffe werden zusätzlich im Backend anhand der Familie, Rolle und Berechtigungen geprüft. Öffentliche Seiten enthalten keine privaten Familiendaten.",
    question: "Wie werden Familiendaten geschützt?",
  },
];
const { t } = useI18n();
const { install, installed, instructionsOpen, ios, nativePromptAvailable } = usePwaInstall();
const installButtonLabel = computed(() =>
  nativePromptAvailable.value ? "Jetzt installieren" : "Installationsanleitung",
);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.landing-page {
  color: var(--lad-text);
  background:
    radial-gradient(
      circle at 8% 8%,
      color-mix(in srgb, var(--lad-color-primary-soft) 45%, transparent),
      transparent 22rem
    ),
    radial-gradient(
      circle at 92% 24%,
      color-mix(in srgb, var(--lad-color-info-soft) 55%, transparent),
      transparent 28rem
    ),
    var(--lad-surface);
  font-family: var(--lad-font-body);
}

:global(html) {
  scroll-behavior: smooth;
  scroll-padding-top: 5.5rem;
}

.landing-header,
.landing-section,
.landing-footer {
  width: min(100% - 2rem, 72rem);
  margin-inline: auto;
}

.landing-header {
  position: sticky;
  top: 0;
  z-index: 100;
  width: 100%;
  min-height: 5rem;
  padding-inline: max(1rem, calc((100% - 72rem) / 2));
  --uno: d-flex align-center justify-space-between ga-6;
  border-bottom: 1px solid color-mix(in srgb, var(--lad-border-subtle) 75%, transparent);
  background: color-mix(in srgb, var(--lad-surface) 94%, transparent);
  box-shadow: 0 0.35rem 1.2rem color-mix(in srgb, var(--lad-shadow) 55%, transparent);
  backdrop-filter: blur(0.8rem);
}

.landing-brand {
  --uno: d-flex align-center ga-2 text-decoration-none;
  color: var(--lad-color-primary-deep);
  font-size: 1.45rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: -0.04em;
}

.landing-brand__logo {
  width: 3.35rem;
  height: 3.35rem;
  --uno: d-grid place-center overflow-hidden;
  border: 1px solid var(--lad-border-subtle);
  border-radius: 1rem;
  background: var(--lad-surface-raised);
  box-shadow: 0 0.3rem 0.8rem var(--lad-shadow);
}

.landing-brand__logo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.landing-nav {
  --uno: d-flex align-center ga-7;
}

.landing-nav a,
.landing-login,
.landing-footer a {
  color: var(--lad-text-strong);
  font-size: 0.9rem;
  font-weight: 800;
  text-decoration: none;
}

.landing-nav a:hover,
.landing-nav a:focus-visible,
.landing-footer a:hover,
.landing-footer a:focus-visible {
  color: var(--lad-color-primary-strong);
}

.landing-login {
  padding: 0.65rem 1.1rem;
  color: var(--lad-color-primary-deep);
  border: 1px solid var(--lad-color-primary-soft);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-raised);
}

.landing-login:hover,
.landing-login:focus-visible {
  background: var(--lad-color-primary-soft);
  outline: none;
}

.landing-section {
  padding-block: clamp(4rem, 8vw, 7rem);
  scroll-margin-top: 5.5rem;
}

.landing-section:not(.landing-hero) {
  content-visibility: auto;
  contain-intrinsic-size: auto 52rem;
}

.landing-hero {
  min-height: min(43rem, calc(100dvh - 5rem));
  --uno: d-grid align-center;
  grid-template-columns: minmax(0, 1.04fr) minmax(20rem, 0.96fr);
  gap: clamp(2rem, 6vw, 5rem);
}

.landing-eyebrow {
  margin: 0 0 0.7rem;
  color: var(--lad-color-primary-strong);
  font-size: 0.78rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.landing-hero h1 {
  max-width: 11ch;
  margin: 0;
  font-size: clamp(3rem, 5.8vw, 5.2rem);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: -0.065em;
  line-height: 0.96;
}

.landing-hero h1 span {
  color: var(--lad-color-primary-strong);
}

.landing-lead {
  max-width: 38rem;
  margin: 1.6rem 0 0;
  color: var(--lad-muted);
  font-size: clamp(1.06rem, 2vw, 1.25rem);
  line-height: 1.65;
}

.landing-actions {
  margin-top: 2rem;
  --uno: d-flex flex-wrap align-center ga-3;
}

.landing-button {
  min-height: 3.25rem;
  padding: 0.8rem 1.3rem;
  --uno: d-inline-flex align-center justify-center ga-2 cursor-pointer text-decoration-none;
  border: 0;
  border-radius: 1rem;
  font: inherit;
  font-size: 0.96rem;
  font-weight: var(--lad-font-weight-black);
  transition:
    transform var(--lad-motion-fast) ease,
    box-shadow var(--lad-motion-fast) ease;
}

.landing-button:hover,
.landing-button:focus-visible {
  transform: translateY(-2px);
  outline: none;
}

.landing-button--primary {
  color: var(--lad-text-inverse);
  background: linear-gradient(145deg, var(--lad-color-primary-highlight), var(--lad-color-primary-strong));
  box-shadow: 0 0.35rem 0 var(--lad-color-primary-deep);
}

.landing-button--secondary {
  color: var(--lad-color-primary-deep);
  border: 2px solid var(--lad-color-primary-soft);
  background: var(--lad-surface-raised);
  box-shadow: 0 0.25rem 0 var(--lad-color-primary-soft);
}

.landing-installed {
  --uno: d-inline-flex align-center ga-2;
  color: var(--lad-color-primary-deep);
  font-weight: var(--lad-font-weight-strong);
}

.landing-trust {
  margin: 1.5rem 0 0;
  padding: 0;
  --uno: d-flex flex-wrap ga-4;
  color: var(--lad-text-strong);
  font-size: 0.8rem;
  font-weight: 750;
  list-style: none;
}

.landing-trust li {
  --uno: d-flex align-center ga-1;
}

.landing-trust .v-icon {
  color: var(--lad-color-primary);
}

.landing-hero__visual {
  aspect-ratio: 1.08;
  --uno: position-relative d-grid place-center;
  border: 2px solid var(--lad-color-primary-soft);
  border-radius: 2.75rem;
  background:
    radial-gradient(circle at 70% 20%, var(--lad-color-reward-soft), transparent 24%),
    linear-gradient(145deg, var(--lad-surface-soft), var(--lad-color-info-soft));
  box-shadow:
    0 0 0 0.55rem color-mix(in srgb, var(--lad-surface-raised) 80%, transparent),
    0 1.5rem 4rem var(--lad-shadow);
}

.landing-ladi {
  width: 68%;
  height: auto;
  filter: drop-shadow(0 1.2rem 1.2rem color-mix(in srgb, var(--lad-text) 18%, transparent));
  animation: ladi-float 4.5s ease-in-out infinite;
}

.landing-orbit {
  width: 4.2rem;
  height: 4.2rem;
  --uno: position-absolute d-grid place-center;
  z-index: 2;
  color: var(--lad-color-primary-deep);
  border: 3px solid var(--lad-surface-raised);
  border-radius: 1.35rem;
  background: var(--lad-surface-raised);
  box-shadow: 0 0.6rem 1.5rem var(--lad-shadow);
}

.landing-orbit--task {
  top: 11%;
  left: -4%;
  background: var(--lad-color-primary-soft);
}

.landing-orbit--family {
  top: 3%;
  right: 3%;
  color: var(--lad-color-info-strong);
  background: var(--lad-color-info-soft);
}

.landing-orbit--reward {
  right: -4%;
  bottom: 24%;
}

.landing-hero__message {
  width: min(85%, 24rem);
  padding: 0.9rem 1rem;
  --uno: position-absolute d-flex flex-column;
  bottom: 1.1rem;
  left: 50%;
  z-index: 2;
  color: var(--lad-text);
  border: 1px solid var(--lad-color-primary-soft);
  border-radius: 1rem;
  background: color-mix(in srgb, var(--lad-surface-raised) 92%, transparent);
  box-shadow: 0 0.6rem 1.6rem var(--lad-shadow);
  transform: translateX(-50%);
  backdrop-filter: blur(0.6rem);
}

.landing-hero__message span {
  margin-top: 0.15rem;
  color: var(--lad-muted);
  font-size: 0.75rem;
}

.landing-heading {
  max-width: 43rem;
  margin: 0 auto 2.7rem;
  text-align: center;
}

.landing-heading h2,
.landing-feature h2,
.landing-install h2,
.landing-final h2 {
  margin: 0;
  font-size: clamp(2rem, 4vw, 3.25rem);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: -0.045em;
  line-height: 1.06;
}

.landing-heading > p:last-child,
.landing-feature__copy > p,
.landing-install__copy > p,
.landing-final p {
  color: var(--lad-muted);
  font-size: 1.05rem;
  line-height: 1.65;
}

.landing-how {
  width: 100%;
  padding-inline: max(1rem, calc((100% - 72rem) / 2));
  background: var(--lad-bg);
}

.landing-steps {
  margin: 0;
  padding: 0;
  --uno: d-grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
  list-style: none;
}

.landing-steps li,
.landing-benefit-grid article {
  padding: 1.6rem;
  --uno: position-relative;
  border: 1px solid var(--lad-border-subtle);
  border-radius: 1.4rem;
  background: var(--lad-surface-raised);
  box-shadow: 0 0.4rem 0 var(--lad-color-primary-soft);
}

.landing-steps .v-icon,
.landing-benefit-grid .v-icon {
  color: var(--lad-color-primary-strong);
}

.landing-step__number {
  width: 2rem;
  height: 2rem;
  --uno: position-absolute d-grid place-center;
  top: 1rem;
  right: 1rem;
  color: var(--lad-color-primary-deep);
  border-radius: 50%;
  background: var(--lad-color-primary-soft);
  font-weight: var(--lad-font-weight-black);
}

.landing-steps h3,
.landing-benefit-grid h3 {
  margin: 1rem 0 0.5rem;
  font-size: 1.1rem;
}

.landing-steps p,
.landing-benefit-grid p {
  margin: 0;
  color: var(--lad-muted);
  font-size: 0.86rem;
  line-height: 1.6;
}

.landing-showcase {
  --uno: d-flex flex-column;
  gap: clamp(1.5rem, 3vw, 2.5rem);
}

.landing-feature {
  padding: clamp(2rem, 5vw, 4rem);
  --uno: d-grid align-center;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: clamp(2.5rem, 6vw, 5rem);
  border: 1px solid var(--lad-border-subtle);
  border-radius: 2.5rem;
  background: linear-gradient(
    135deg,
    var(--lad-surface-raised),
    color-mix(in srgb, var(--lad-color-primary-soft) 22%, var(--lad-surface-raised))
  );
  box-shadow: 0 1rem 2.5rem color-mix(in srgb, var(--lad-shadow) 62%, transparent);
}

.landing-feature--reverse {
  background: linear-gradient(
    135deg,
    color-mix(in srgb, var(--lad-color-info-soft) 35%, var(--lad-surface-raised)),
    var(--lad-surface-raised)
  );
}

.landing-feature--reverse .landing-feature__copy {
  order: 2;
}

.landing-feature__copy ul {
  margin: 1.5rem 0 0;
  padding: 0;
  --uno: d-grid ga-3;
  list-style: none;
}

.landing-feature__copy li {
  --uno: d-flex align-start ga-2;
  color: var(--lad-text-strong);
  font-size: 0.9rem;
  font-weight: 750;
}

.landing-feature__copy li .v-icon {
  flex: 0 0 auto;
  color: var(--lad-color-primary-strong);
}

.product-phone,
.goal-preview,
.world-preview {
  border: 0.5rem solid var(--lad-surface-raised);
  border-radius: 2.2rem;
  background: var(--lad-surface-soft);
  box-shadow:
    0 0 0 2px var(--lad-color-primary-soft),
    0 1.4rem 3rem var(--lad-shadow);
}

.product-phone {
  max-width: 29rem;
  padding: 1.2rem;
  --uno: d-grid ga-3;
  transform: rotate(1.5deg);
}

.product-phone__top {
  --uno: d-flex align-center ga-2;
  padding-bottom: 0.8rem;
  border-bottom: 1px solid var(--lad-color-primary-soft);
}

.product-logo {
  width: 2.2rem;
  height: 2.2rem;
  --uno: d-grid place-center overflow-hidden;
  border-radius: 0.65rem;
  background: var(--lad-surface-raised);
}

.product-logo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-balance {
  margin-left: auto;
  --uno: d-flex align-center ga-1;
  color: var(--lad-color-reward-strong);
  font-weight: var(--lad-font-weight-black);
}

.product-card {
  min-height: 5.5rem;
  padding: 0.9rem;
  --uno: d-grid align-center;
  grid-template-columns: auto 1fr auto;
  gap: 0.8rem;
  border-radius: 1.2rem;
}

.product-card--blue {
  background: var(--lad-color-info-soft);
}

.product-card--green {
  background: var(--lad-color-primary-soft);
}

.product-card--amber {
  background: var(--lad-color-reward-soft);
}

.product-card strong,
.product-card small {
  --uno: d-block;
}

.product-card small {
  margin-top: 0.25rem;
  color: var(--lad-muted);
}

.product-icon {
  width: 3rem;
  height: 3rem;
  --uno: d-grid place-center;
  border-radius: 0.9rem;
  background: var(--lad-surface-raised);
  font-size: 1.4rem;
}

.product-pill {
  padding: 0.35rem 0.65rem;
  color: var(--lad-color-primary-deep);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-color-primary-soft) 72%, var(--lad-surface-raised));
  font-size: 0.7rem;
  font-weight: var(--lad-font-weight-strong);
}

.goal-preview {
  min-height: 25rem;
  --uno: d-grid overflow-hidden;
  grid-template-columns: 0.9fr 1.1fr;
  background: linear-gradient(145deg, var(--lad-color-info-soft), var(--lad-surface));
  transform: rotate(-1.5deg);
}

.goal-preview__art {
  --uno: d-grid place-center;
  background: radial-gradient(circle, var(--lad-color-reward-soft), transparent 68%);
}

.goal-preview__art img {
  width: 120%;
  max-width: none;
}

.goal-preview__copy {
  padding: 2rem 1.5rem;
  --uno: d-flex flex-column justify-center;
}

.goal-preview h3 {
  margin: 1rem 0 0.4rem;
  font-size: 1.65rem;
  line-height: 1.08;
}

.goal-preview p {
  color: var(--lad-muted);
  font-size: 0.88rem;
}

.goal-preview__amount {
  margin-top: 1.2rem;
  --uno: d-flex align-center ga-2;
  color: var(--lad-color-reward-strong);
}

.goal-preview__bar {
  height: 0.7rem;
  margin-top: 0.7rem;
  --uno: overflow-hidden;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-raised);
}

.goal-preview__bar span {
  width: 67%;
  height: 100%;
  --uno: d-block;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--lad-color-primary), var(--lad-color-reward));
}

.world-preview {
  --uno: position-relative overflow-hidden;
  aspect-ratio: 1.48;
}

.world-preview__background {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.world-preview__house {
  width: 74%;
  --uno: position-absolute;
  bottom: -2%;
  left: 50%;
  z-index: 1;
  transform: translateX(-50%);
  filter: drop-shadow(0 0.6rem 0.7rem color-mix(in srgb, var(--lad-text) 18%, transparent));
}

.world-preview__family {
  width: 11rem;
  height: 4.2rem;
  position: absolute;
  display: flex;
  align-items: flex-end;
  bottom: 2%;
  left: 2%;
  z-index: 2;
  gap: 0.3rem;
}

.sample-avatar {
  width: 3.15rem;
  height: 3.75rem;
  display: grid;
  flex: 0 0 auto;
  place-items: center;
  border: 2px solid var(--lad-surface-raised);
  border-radius: 1.2rem 1.2rem 0.8rem 0.8rem;
  box-shadow: 0 0.35rem 0.8rem color-mix(in srgb, var(--lad-shadow) 72%, transparent);
  font-size: 2rem;
  line-height: 1;
}

.sample-avatar--mint {
  background: var(--lad-color-primary-soft);
}

.sample-avatar--blue {
  background: var(--lad-color-info-soft);
}

.sample-avatar--gold {
  background: var(--lad-color-reward-soft);
}

.world-preview__pet {
  width: 3.4rem;
  height: 3.4rem;
  position: absolute;
  right: 6%;
  bottom: 2%;
  z-index: 2;
  display: grid;
  place-items: center;
  border: 2px solid var(--lad-surface-raised);
  border-radius: 50%;
  background: var(--lad-color-reward-soft);
  box-shadow: 0 0.35rem 0.8rem color-mix(in srgb, var(--lad-shadow) 72%, transparent);
  font-size: 2rem;
}

.world-preview__energy {
  padding: 0.6rem 0.8rem;
  --uno: position-absolute d-flex align-center ga-2;
  top: 1rem;
  right: 1rem;
  color: var(--lad-color-primary-deep);
  border: 1px solid var(--lad-color-primary-soft);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-surface-raised) 90%, transparent);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
  backdrop-filter: blur(0.5rem);
}

.landing-benefits {
  width: 100%;
  padding-inline: max(1rem, calc((100% - 72rem) / 2));
  background: var(--lad-bg);
}

.landing-benefit-grid {
  --uno: d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
}

.landing-install {
  margin-block: clamp(4rem, 8vw, 7rem);
  padding: clamp(2rem, 5vw, 4.5rem);
  --uno: d-grid align-center;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 2rem;
  color: var(--lad-text-inverse);
  border-radius: 2.5rem;
  background:
    radial-gradient(circle at 88% 16%, color-mix(in srgb, var(--lad-color-reward) 30%, transparent), transparent 24%),
    linear-gradient(145deg, var(--lad-color-primary-deep), var(--lad-color-info-deep));
  box-shadow: 0 1.4rem 3rem color-mix(in srgb, var(--lad-color-primary-deep) 24%, transparent);
}

.landing-install .landing-eyebrow,
.landing-install__copy > p {
  color: var(--lad-color-primary-soft);
}

.landing-install .landing-button--primary {
  color: var(--lad-color-primary-deep);
  background: var(--lad-surface-raised);
  box-shadow: 0 0.35rem 0 var(--lad-color-primary-highlight);
}

.landing-installed--light {
  color: var(--lad-text-inverse);
}

.landing-install__devices {
  --uno: d-grid ga-3;
}

.device-card {
  min-height: 4.7rem;
  padding: 1rem 1.3rem;
  --uno: d-flex align-center ga-4;
  border: 1px solid color-mix(in srgb, var(--lad-surface-raised) 32%, transparent);
  border-radius: 1.3rem;
  background: color-mix(in srgb, var(--lad-surface-raised) 14%, transparent);
  font-weight: var(--lad-font-weight-black);
  backdrop-filter: blur(0.8rem);
}

.device-card .v-icon {
  font-size: 2rem;
}

.device-card--android {
  transform: translateX(-1rem);
}

.device-card--desktop {
  transform: translateX(1rem);
}

.landing-faq__list {
  max-width: 50rem;
  margin-inline: auto;
  --uno: d-grid ga-3;
}

.landing-faq details {
  padding: 1.15rem 1.3rem;
  border: 1px solid var(--lad-border-subtle);
  border-radius: 1rem;
  background: var(--lad-surface-raised);
}

.landing-faq summary {
  --uno: d-flex align-center justify-space-between ga-4 cursor-pointer;
  font-weight: var(--lad-font-weight-black);
  list-style: none;
}

.landing-faq summary::-webkit-details-marker {
  display: none;
}

.landing-faq details[open] summary .v-icon {
  transform: rotate(180deg);
}

.landing-faq details p {
  margin: 1rem 0 0;
  color: var(--lad-muted);
  line-height: 1.65;
}

.landing-final {
  min-height: 25rem;
  padding: clamp(2rem, 5vw, 4rem);
  --uno: d-grid align-center overflow-hidden;
  grid-template-columns: 0.7fr 1.3fr;
  border-radius: 2.5rem;
  border: 1px solid var(--lad-color-primary-soft);
  background:
    radial-gradient(circle at 12% 18%, var(--lad-surface-raised), transparent 24%),
    linear-gradient(145deg, var(--lad-color-reward-soft), var(--lad-color-primary-soft));
  box-shadow: 0 1.2rem 3rem color-mix(in srgb, var(--lad-shadow) 65%, transparent);
}

.landing-final img {
  width: min(100%, 22rem);
  max-height: 24rem;
  margin: 0 auto -4rem;
  align-self: end;
  object-fit: contain;
}

.landing-final > div {
  min-width: 0;
}

.landing-final h2 {
  overflow-wrap: anywhere;
}

.landing-final .landing-button {
  margin-top: 1rem;
}

.landing-footer {
  padding-block: 3rem;
  --uno: d-grid;
  grid-template-columns: auto 1fr auto;
  gap: 1.5rem 3rem;
  border-top: 1px solid var(--lad-border-subtle);
}

.landing-footer > p {
  margin: 0;
  align-self: center;
  color: var(--lad-muted);
}

.landing-footer nav {
  --uno: d-flex align-center flex-wrap ga-4;
}

.landing-footer small {
  grid-column: 1 / -1;
  color: var(--lad-muted);
}

.installation-dialog {
  color: var(--lad-text);
  background: var(--lad-surface);
}

.installation-dialog__icon {
  width: 4rem;
  height: 4rem;
  margin-bottom: 1rem;
  --uno: d-grid place-center;
  color: var(--lad-color-primary-deep);
  border-radius: 1.2rem;
  background: var(--lad-color-primary-soft);
}

.installation-dialog h2 {
  margin: 0;
}

.installation-dialog p,
.installation-dialog li {
  color: var(--lad-muted);
  line-height: 1.6;
}

.installation-dialog ol {
  padding-left: 1.4rem;
}

.installation-dialog__hint {
  padding: 0.8rem;
  border-radius: 0.8rem;
  background: var(--lad-color-info-soft);
}

@keyframes ladi-float {
  0%,
  100% {
    transform: translateY(0) rotate(-1deg);
  }

  50% {
    transform: translateY(-0.8rem) rotate(1deg);
  }
}

@media (max-width: 64rem) {
  .landing-nav {
    display: none;
  }

  .landing-steps {
    grid-template-columns: repeat(2, 1fr);
  }

  .landing-feature {
    grid-template-columns: 1fr;
  }

  .landing-feature--reverse .landing-feature__copy {
    order: 0;
  }

  .product-phone,
  .goal-preview,
  .world-preview {
    width: min(100%, 34rem);
    margin-inline: auto;
  }
}

@media (max-width: 48rem) {
  .landing-hero,
  .landing-feature,
  .landing-install,
  .landing-final {
    grid-template-columns: 1fr;
  }

  .landing-hero {
    padding-top: 3rem;
  }

  .landing-hero__copy {
    text-align: center;
  }

  .landing-hero h1,
  .landing-lead {
    margin-inline: auto;
  }

  .landing-actions,
  .landing-trust {
    justify-content: center;
  }

  .landing-hero__visual {
    width: min(100%, 30rem);
    margin-inline: auto;
  }

  .landing-feature--reverse .landing-feature__copy {
    order: 0;
  }

  .product-phone,
  .goal-preview,
  .world-preview {
    margin-inline: auto;
  }

  .landing-benefit-grid {
    grid-template-columns: 1fr;
  }

  .landing-install__devices {
    margin-inline: 1rem;
  }

  .landing-final {
    text-align: center;
  }

  .landing-final img {
    margin-bottom: -2rem;
  }

  .landing-footer {
    grid-template-columns: 1fr;
  }

  .landing-footer small {
    grid-column: auto;
  }
}

@media (max-width: 34rem) {
  .landing-header,
  .landing-section,
  .landing-footer {
    width: min(100% - 1.25rem, 72rem);
  }

  .landing-header {
    width: 100%;
    padding-inline: 0.625rem;
  }

  .landing-brand {
    font-size: 1.2rem;
  }

  .landing-brand__logo {
    width: 2.8rem;
    height: 2.8rem;
  }

  .landing-login {
    padding: 0.55rem 0.85rem;
  }

  .landing-hero h1 {
    font-size: clamp(2.65rem, 14vw, 4rem);
  }

  .landing-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .landing-button {
    width: 100%;
  }

  .landing-steps {
    grid-template-columns: 1fr;
  }

  .goal-preview {
    min-height: auto;
    grid-template-columns: 1fr;
  }

  .goal-preview__art {
    max-height: 13rem;
    overflow: hidden;
  }

  .goal-preview__art img {
    width: 70%;
  }

  .product-card {
    grid-template-columns: auto 1fr;
  }

  .product-card .product-pill {
    display: none;
  }

  .landing-install {
    margin-inline: 0.625rem;
  }

  .device-card,
  .device-card--android,
  .device-card--desktop {
    transform: none;
  }
}

@media (prefers-reduced-motion: reduce) {
  .landing-page *,
  .landing-page *::before,
  .landing-page *::after {
    scroll-behavior: auto;
    animation-duration: 0.01ms;
    animation-iteration-count: 1;
    transition-duration: 0.01ms;
  }
}
</style>
