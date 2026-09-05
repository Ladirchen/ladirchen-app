import { createDomainId, isUuidValue } from '@/domain/types';
import type { FamilyId, FamilyMemberId } from '@/domain/types';
import { isDomainId, isRecord } from '@/application/contracts/family-aggregate-validation';

const ACCOUNT_STORAGE_KEY = 'ladirchen:local-accounts:v1';

interface StoredLocalAccount {
  username: string;
  passwordHash: string;
  salt: string;
  familyId: string;
  familyName: string;
  memberId: string;
}

export interface LocalAccountSession {
  familyId: FamilyId;
  familyName: string;
  memberId: FamilyMemberId;
}

const normalizeUsername = (username: string) => username.trim().toLocaleLowerCase('de');

const isStoredLocalAccount = (value: unknown): value is StoredLocalAccount =>
  isRecord(value) &&
  typeof value.username === 'string' && value.username.length > 0 && value.username === normalizeUsername(value.username) &&
  typeof value.passwordHash === 'string' && /^[\da-f]{64}$/u.test(value.passwordHash) &&
  typeof value.salt === 'string' && /^[\da-f]{8}-(?:[\da-f]{4}-){3}[\da-f]{12}$/iu.test(value.salt) &&
  isUuidValue(value.familyId) &&
  typeof value.familyName === 'string' && value.familyName.trim().length > 0 &&
  isDomainId(value.memberId);

const loadAccounts = (): StoredLocalAccount[] => {
  try {
    const parsed: unknown = JSON.parse(localStorage.getItem(ACCOUNT_STORAGE_KEY) ?? '[]');
    if (!Array.isArray(parsed)) {return [];}
    return parsed.filter(isStoredLocalAccount);
  } catch {
    return [];
  }
};

const digestPassword = async (password: string, salt: string): Promise<string> => {
  const bytes = new TextEncoder().encode(`${salt}:${password}`);
  const digest = await crypto.subtle.digest('SHA-256', bytes);
  return Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('');
};

export const localUsernameExists = (username: string): boolean =>
  loadAccounts().some(account => account.username === normalizeUsername(username));

export const registerLocalAccount = async (
  username: string,
  password: string,
  familyName: string,
  familyId: FamilyId,
  memberId: FamilyMemberId,
): Promise<void> => {
  const normalizedUsername = normalizeUsername(username);
  const accounts = loadAccounts();
  if (accounts.some(account => account.username === normalizedUsername)) {throw new Error('USERNAME_EXISTS');}
  const salt = crypto.randomUUID();
  accounts.push({
    username: normalizedUsername,
    passwordHash: await digestPassword(password, salt),
    salt,
    familyId,
    familyName: familyName.trim(),
    memberId,
  });
  localStorage.setItem(ACCOUNT_STORAGE_KEY, JSON.stringify(accounts));
};

export const authenticateLocalAccount = async (username: string, password: string): Promise<LocalAccountSession | null> => {
  const account = loadAccounts().find(item => item.username === normalizeUsername(username));
  if (!account || account.passwordHash !== await digestPassword(password, account.salt)) {return null;}
  return {
    familyId: createDomainId.family(account.familyId),
    familyName: account.familyName,
    memberId: createDomainId.familyMember(account.memberId),
  };
};
