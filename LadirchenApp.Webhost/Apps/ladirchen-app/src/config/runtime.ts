export interface FrontendRuntimeConfig {
  readonly apiBaseUrl: string;
}
function normalizeApiBaseUrl(value: string | undefined): string {
  return (value ?? "").trim().replace(/\/+$/u, "");
}
export const frontendRuntimeConfig: FrontendRuntimeConfig = Object.freeze({
  apiBaseUrl: normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL),
});
