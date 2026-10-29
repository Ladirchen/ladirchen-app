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

The application uses backend-driven HTTP adapters on the current origin. Set `VITE_API_BASE_URL` only when the API is hosted elsewhere. Each read-model adapter expects:

```text
GET /api/families/{familyId}/read-models/{readModelType}
POST /api/families/{familyId}/commands
```

Responses contain a read-model type, revision and validated state from `src/application/contracts/`; persistence schema versions and audit metadata stay on the server. Runtime guards validate every loaded read model before it reaches the store. Parallel hydration retains successful read models independently and remains retryable when only part of the backend is unavailable. Business writes are expressed only as commands; the frontend never saves persistence snapshots. Every command carries a client-generated `commandId`, and retries reuse that identifier so an uncertain HTTP response cannot apply a write twice. If an atomic command save detects a revision conflict, the API returns HTTP 409 and the command gateway retries the same command once against the newly loaded server state.

Family read-model requests, commands, projections, and SignalR subscriptions require an HTTP-only family-session cookie. The server verifies that the route or subscription family matches the authenticated family. The shared-device member switch remains a UI concept inside that authenticated family; command handlers still verify the selected actor's current membership and permissions.

In API mode the application also connects to:

```text
/hubs/family-updates
```

After another family member changes an aggregate, SignalR publishes its type, revision, and audit metadata separately from the HTTP read model. Pinia then reloads only the corresponding read model and remains the reactive UI read-model cache. After a SignalR reconnection, the client subscribes to the family again and reloads every read model so that changes missed during the interruption are recovered. Authentication accounts and aggregate snapshots are stored in PostgreSQL through Npgsql. The server applies EF Core migrations during startup, saves multi-aggregate commands transactionally, and rejects stale aggregate revisions with HTTP 409. Production can use `DATABASE_URL`, `POSTGRES_URL`, or `ConnectionStrings__Ladirchen`; local development uses the `Ladirchen` connection string from `appsettings.json`.

Family profile, avatar, contribution, savings, family reward-shop, progression and house-customization writes in API mode use the backend-owned command endpoint:

```text
POST /api/families/{familyId}/commands
```

These actions no longer mutate Pinia first. The backend validates each command and saves its receipt, all affected snapshots, and durable notification outbox entries in one PostgreSQL transaction. A separate dispatcher publishes committed revisions through SignalR and retries failed deliveries, so a notification failure cannot turn an already committed command into an HTTP failure. Pinia reloads the read models into its reactive cache. The savings-interest simulator also obtains its projection from the backend. Production and local development use the HTTP backend; fixtures are limited to tests and explicitly isolated visual-development tooling.

Authentication is exposed through `src/application/ports/authentication-gateway.ts`. Registration and authentication run through the server, where passwords are salted and hashed with PBKDF2, an HTTP-only cookie is issued, and family registration initializes all aggregate snapshots in one database transaction. The product currently uses a shared-device identity model: a signed-in family manager may switch to another family member, while members without `ManageFamily` may issue commands only as themselves. Authentication endpoints are rate-limited and repeated failed sign-ins temporarily lock the account. Sign-out invalidates the server cookie and disconnects the SignalR client.

The production application settles every closed ISO week through an idempotent background workflow. The immutable weekly progression evaluation determines each personal Ladi stage, and that stage determines the authoritative savings-interest credit. Completed personal goals receive a redeemable voucher immediately; family-goal vouchers require consent from every participating family member before redemption.

Local SPA development uses the workspace-local `node_modules/.bin/vp`. The web host prepends that directory to its child-process `PATH`, so no globally installed Vite+ executable is required.

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
