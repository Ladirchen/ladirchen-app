const EMAIL_INPUT_PATTERN = /^[^@\s]+@[^\s@][^\s.@]*\.[^\s@]+$/u;
export const EMAIL_INPUT_MAXIMUM_LENGTH = 320;

export function isEmailAddressInput(value: string): boolean {
  const normalized = value.trim();

  return normalized.length <= EMAIL_INPUT_MAXIMUM_LENGTH && EMAIL_INPUT_PATTERN.test(normalized);
}
