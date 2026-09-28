export type FrontendDataSource = "api" | "local-storage";

export interface FrontendRuntimeConfig {
  readonly apiBaseUrl: string;
  readonly dataSource: FrontendDataSource;
}

const normalizeApiBaseUrl = (value: string | undefined): string =>
  (value ?? "").trim().replace(/\/+$/u, "");

const resolveDataSource = (value: string | undefined): FrontendDataSource => {
  if (value === undefined || value === "" || value === "local-storage") {return "local-storage";}
  if (value === "api") {return value;}
  throw new Error(`Unsupported VITE_DATA_SOURCE value: ${value}`);
};

export const frontendRuntimeConfig: FrontendRuntimeConfig = Object.freeze({
  apiBaseUrl: normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL),
  dataSource: resolveDataSource(import.meta.env.VITE_DATA_SOURCE),
});
