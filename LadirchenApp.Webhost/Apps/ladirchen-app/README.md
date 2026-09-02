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
- `pnpm lint`
- `pnpm lint:fix`

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
