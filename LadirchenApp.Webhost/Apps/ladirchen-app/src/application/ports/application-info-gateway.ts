export const applicationDetailFields = [
  "name",
  "version",
  "fileVersion",
  "branch",
  "sha",
  "title",
  "product",
  "company",
  "description",
  "configuration",
  "copyright",
] as const;

type ApplicationDetailField = (typeof applicationDetailFields)[number];

export interface ApplicationInfo extends Readonly<Partial<Record<ApplicationDetailField, string>>> {
  readonly semVer: string;
  readonly informationalVersion: string;
}

export interface ApplicationInfoGateway {
  getInfo: () => Promise<ApplicationInfo>;
}
