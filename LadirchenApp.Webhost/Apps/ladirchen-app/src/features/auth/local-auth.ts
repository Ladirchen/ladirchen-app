import { createDomainId, isUuidValue } from '@/domain/shared/identifiers';
import type { FamilyId, FamilyMemberId } from '@/domain/shared/identifiers';
import { isDomainId, isRecord } from '@/application/contracts/family-aggregate-validation';
import type { AuthenticationGateway, AuthenticationSession } from '@/application/ports/authentication-gateway';
import type { ClientStorage } from '@/application/ports/client-storage';

const ACCOUNT_STORAGE_KEY = 'ladirchen:local-accounts:v1';

interface StoredLocalAccount {
  username: string;
  passwordHash: string;
  salt: string;
  familyId: string;
  familyName: string;
  memberId: string;
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

const loadAccounts = (storage: ClientStorage): StoredLocalAccount[] => {
  try {
    const parsed: unknown = JSON.parse(storage.getItem(ACCOUNT_STORAGE_KEY) ?? '[]');
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

const localUsernameExists = (storage: ClientStorage, username: string): boolean =>
  loadAccounts(storage).some(account => account.username === normalizeUsername(username));

const registerLocalAccount = async (
  storage: ClientStorage,
  username: string,
  password: string,
  familyName: string,
  familyId: FamilyId,
  memberId: FamilyMemberId,
): Promise<void> => {
  const normalizedUsername = normalizeUsername(username);
  const accounts = loadAccounts(storage);
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
  storage.setItem(ACCOUNT_STORAGE_KEY, JSON.stringify(accounts));
};

const authenticateLocalAccount = async (storage: ClientStorage, username: string, password: string): Promise<AuthenticationSession | null> => {
  const account = loadAccounts(storage).find(item => item.username === normalizeUsername(username));
  if (!account || account.passwordHash !== await digestPassword(password, account.salt)) {return null;}
  return {
    familyId: createDomainId.family(account.familyId),
    familyName: account.familyName,
    memberId: createDomainId.familyMember(account.memberId),
  };
};

export const createLocalAuthenticationGateway = (storage: ClientStorage): AuthenticationGateway => ({
  authenticate: (username, password) => authenticateLocalAccount(storage, username, password),
  register: command => registerLocalAccount(
    storage,
    command.username,
    command.password,
    command.familyName,
    command.familyId,
    command.memberId,
  ),
  usernameExists: async username => localUsernameExists(storage, username),
});
