# Project Rules

## General

- Follow the existing code style and patterns.
- Use pnpm for running project commands.
- Keep code in TypeScript unless migration is required.

## Stack

- Framework: Vue 3 + Vite
- UI Library: Vuetify
- Enabled Features: ESLint, Vuetify MCP, Pinia, Vue I18n, Vue Router, UnoCSS + Vuetify Preset

## State, Composables, and Localization

- Keep Pinia stores in `src/stores/`, organized by domain or feature. Use descriptive kebab-case filenames.
- Put reusable composables in `src/shared/composables/` and feature-specific composables alongside their feature. Name composable files `use-*.ts`.
- Keep Vue I18n messages in flat, alphabetically sorted locale JSON files under `src/locales/`, using one file per locale (for example, `de.json` and `en.json`).

## TypeScript (`.ts` and `.vue` files)

- Use named function declarations for top-level methods and multi-line logic. Use arrow functions only for one-line callbacks passed inline (e.g., array methods, event handlers under 3 lines).
- Always use braces for control-flow bodies, including single-statement bodies.
- Put a blank line before and after `if`/`else`, `try`/`catch`/`finally`, `for`, and `while` constructs.
- Put a blank line before `return`, unless it is the only statement in its block.

## Vue Components

- In `<script setup>`, keep imports first, then group declarations in this order: local `const` declarations, native `let`/`var` declarations, Vue composable calls (`use...`), refs, computed values, function declarations, and lifecycle callbacks (`onMounted`, etc.). Preserve required initialization dependencies when they prevent this order. Example:

  ```ts
  import { computed, onMounted, ref } from 'vue'
  import { useStore } from '@/stores/example'

  const MAX_ITEMS = 10 // local const
  let cache // native let/var
  const store = useStore() // composable call
  const items = ref<Item[]>([]) // ref
  const visibleItems = computed(() => items.value.slice(0, MAX_ITEMS)) // computed

  function loadItems() { // function declaration
    // ...
  }

  onMounted(() => { // lifecycle callback
    loadItems()
  })

  // Override: if `cache` must exist before `store` can be constructed,
  // declare `cache` immediately before the composable call that needs it.
  ```

- Define props using `defineProps<Props>()` with an exported `Props` interface, and define emits using `defineEmits<Emits>()` with an exported `Emits` interface.
- In `<template>`, use kebab-case for component tags and props, and use `v-if`/`v-for` on separate elements rather than combining them on the same element.
- Keep `<style>` sections scoped (`<style scoped>`) and prefer UnoCSS/Vuetify utility classes over custom CSS rules.
