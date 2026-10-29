# ladirchen-app

Scaffolded with Vuetify CLI.

## ❗️ Documentation

- Primary docs: <https://vuetifyjs.com/>
- Getting started guide: <https://vuetifyjs.com/en/getting-started/installation/>
- Community support: <https://community.vuetifyjs.com/>
- Issue tracker: <https://issues.vuetifyjs.com/>

## 🧱 Stack

- Framework: Vue 3 + Vite
- UI Library: Vuetify
- Language: TypeScript
- Package manager: pnpm

## 🧭 Start Here

- Main entry: `src/main.ts`
- Main app component: `src/App.vue`
- Application shell: `src/app/`
- Capability modules: `src/features/`
- Domain model: `src/domain/`
- Plugin setup: `src/plugins/`

## 📁 Project Structure

- `src/main.ts` — application entry point
- `src/App.vue` — root component
- `src/app/` — application shell and navigation
- `src/application/` — application ports and use-case boundaries
- `src/domain/` — framework-independent family-world model and rules
- `src/features/` — capability-oriented pages, components, and feature data
- `src/infrastructure/` — persistence adapters and initial fixture data
- `src/plugins/` — plugin registration and setup
- `src/shared/` — reusable presentation components without feature ownership
- `src/stores/` — Pinia state orchestration
- `src/styles/` — global design tokens, shell styles, and theme settings
- `public/` — static public files

Domain implementations are grouped by capability under `src/domain/avatar`, `contributions`, `family`, `house`, `ladi`, `savings`, `shop`, and `shared`. Cross-layer code imports the owning domain module directly; each domain may expose a local `index.ts` entry point when several files form one public API.

Feature modules do not import other feature modules. Reusable presentation and visual assets belong in `src/shared/`; feature behavior is composed through explicit props and events. ESLint enforces this dependency boundary.

## ✨ Enabled Features

- ESLint
- Vuetify MCP
- Pinia
- Vue I18n
- Vue Router
- UnoCSS + Vuetify Preset

## 💿 Install

Use your selected package manager (pnpm) to install dependencies:

```bash
pnpm install
```

## 🚀 Quick Start

```bash
pnpm install
pnpm dev
```

## 🏗️ Build

```bash
pnpm build
```

## 🧪 Available Scripts

- `pnpm dev`
- `pnpm build`
- `pnpm preview`
- `pnpm build-only`
- `pnpm type-check`
- `pnpm test`
- `pnpm i18n:check`
- `pnpm colors:check`
- `pnpm styles:check`
- `pnpm lint`
- `pnpm lint:fix`
- `pnpm generate:design`

## 🌐 Localization

German and English messages live as flat, alphabetically sorted JSON files in `src/locales/` (`de.json`, `en.json`); `de.json` defines the typed translation keys. Messages are loaded on demand through the `LocaleMessagesSource` port (currently bundled JSON imports, wired in `src/app/composition-root.ts`), so they can later be served by an API. UI components reference message keys through Vue I18n. `pnpm i18n:check` verifies flat structure, key order, locale parity, static translation-key references, visible template text, and translatable static attributes. Add a locale by providing a JSON file with the same keys, registering it in `SUPPORTED_LOCALES` in `src/locales/index.ts`, and adding it to the bundled locale source.

## 🔌 Backend integration boundary

The application uses backend-driven HTTP adapters on the current origin. Set `VITE_API_BASE_URL` only when the API is hosted elsewhere. Each aggregate adapter expects:

```text
GET /api/families/{familyId}/aggregates/{aggregateType}
PUT /api/families/{familyId}/aggregates/{aggregateType}
```

Responses use the versioned snapshot contracts from `src/application/contracts/`; writes carry an `expectedRevision`, and the API must return HTTP 409 for concurrent-update conflicts. On a conflict, the application reloads the current snapshot, performs a three-way merge against the locally loaded base, and retries with the new revision. Runtime guards validate every loaded snapshot before it reaches the store. Parallel hydration retains successful aggregates independently and remains retryable when only part of the backend is unavailable.

In API mode the application also connects to:

```text
/hubs/family-updates
```

After another family member changes an aggregate, SignalR publishes its type and revision. Pinia then reloads only that aggregate and remains the reactive UI read-model cache. The current server snapshot repository and authentication store are in-memory adapters and must be replaced by durable persistence before production deployment. Business actions use backend commands; the frontend does not persist aggregate snapshots.

Family profile, avatar, contribution, savings, family reward-shop, progression and house-customization writes in API mode use the backend-owned command endpoint:

```text
POST /api/families/{familyId}/commands
```

These actions no longer mutate Pinia first. The backend validates each command, applies the C# domain policies, saves all affected snapshots atomically, publishes their revisions through SignalR, and Pinia reloads the snapshots as a reactive read-model cache. The savings-interest simulator also obtains its projection from the backend. Local-storage mode retains the former prototype behavior only for offline UI development.

Authentication is exposed through `src/application/ports/authentication-gateway.ts`. Registration and authentication run through the server, where passwords are salted and hashed with PBKDF2 and family registration initializes all aggregate snapshots.

## 🎨 Adding Designs

Use the design generator for avatar faces, face shapes, hair, outfits, accessories, furniture, outdoor accessories, and house themes. See [DESIGN_GENERATION.md](./DESIGN_GENERATION.md) for commands and options.

### Design-system colour boundary

Application UI must use the semantic tokens in `src/styles/main.scss`, such as `--lad-surface`, `--lad-text`, `--lad-color-primary-*`, `--lad-color-info-*`, `--lad-color-reward-*`, and the shared border, shadow, and gradient tokens. Components must not select a raw `--lad-palette-*` value directly.

Raw palette access is intentionally limited to the central token definitions and audited SVG/CSS illustration renderers such as avatars, furniture, the family-world scene, mascots, and animated decorative icons. `pnpm colors:check` enforces this boundary. When a normal UI component needs a new colour role, add a semantic token rather than adding the component to the illustration allowlist.

Component selectors must provide their own stable specificity instead of overriding Vuetify with `!important`. The only exception is reduced-motion accessibility CSS, which must reliably override component animations and transitions. `pnpm styles:check` enforces this rule.

## 🤖 Vuetify MCP Server

This project is configured with the Vuetify Model Context Protocol (MCP) server.
To install and configure the MCP server for your favorite IDE (Cursor, Trae, Windsurf, VS Code, Claude Desktop, etc.) run:

```bash
pnpm dlx @vuetify/mcp-cli
```

This will open an interactive setup wizard to help you connect your AI assistant to the Vuetify ecosystem.

## 💪 Support Vuetify Development

This project uses Vuetify - an MIT licensed Open Source project. We are glad to welcome contributors and any support for ongoing development:

- Contribute to Vuetify and ecosystem projects: <https://github.com/vuetifyjs>
- Request enterprise support: <https://support.vuetifyjs.com/>
- Sponsor on GitHub: <https://github.com/sponsors/vuetifyjs>
- Support on Open Collective: <https://opencollective.com/vuetify>
