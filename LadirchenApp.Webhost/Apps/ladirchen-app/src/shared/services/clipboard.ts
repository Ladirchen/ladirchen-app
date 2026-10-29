export interface TextClipboard {
  writeText: (value: string) => Promise<void>;
}

export async function copyTextToClipboard(
  value: string,
  clipboard: TextClipboard | null | undefined = globalThis.navigator?.clipboard,
): Promise<boolean> {
  if (!clipboard) {
    return false;
  }

  try {
    await clipboard.writeText(value);

    return true;
  } catch {
    return false;
  }
}
