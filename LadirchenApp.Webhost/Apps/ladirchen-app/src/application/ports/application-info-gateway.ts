export interface ApplicationInfo {
  readonly semVer: string;
  readonly informationalVersion: string;
}

export interface ApplicationInfoGateway {
  getInfo: () => Promise<ApplicationInfo>;
}
