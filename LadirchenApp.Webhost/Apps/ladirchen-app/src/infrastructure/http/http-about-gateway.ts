import { Client } from "@api/LadirchenApp.Webhost";

export async function fetchApplicationVersion(baseUrl: string): Promise<string | null> {
  const aboutInfo = await new Client(baseUrl).getAbout();

  return aboutInfo?.semVer?.trim() || null;
}
