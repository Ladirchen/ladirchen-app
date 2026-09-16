/// <reference types="vite/client" />
/// <reference types="vite-plugin-vue-layouts-next/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_DATA_SOURCE?: 'api' | 'local-storage';
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
