export type FrontendDataSource = "api" | "local-storage";
export interface FrontendRuntimeConfig {
  readonly apiBaseUrl: string;
  readonly dataSource: FrontendDataSource;
}
function normalizeApiBaseUrl(value: string | undefined): string {
  return (value ?? "").trim().replace(/\/+$/u, "");
}
function resolveDataSource(value: string | undefined): FrontendDataSource {
  if (value === undefined || value === "" || value === "api") {
    return "api";
  }

  if (value === "local-storage") {
    return value;
  }

  throw new Error(`Unsupported VITE_DATA_SOURCE value: ${value}`);
}
export const frontendRuntimeConfig: FrontendRuntimeConfig = Object.freeze({
  apiBaseUrl: normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL),
  dataSource: resolveDataSource(import.meta.env.VITE_DATA_SOURCE),
});
