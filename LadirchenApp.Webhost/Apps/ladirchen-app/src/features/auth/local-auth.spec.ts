import { describe, expect, it } from 'vitest';

import type { ClientStorage } from '@/application/ports/client-storage';
import { createDomainId } from '@/domain/shared/identifiers';
import { createLocalAuthenticationGateway } from './local-auth';

const createMemoryStorage = (): ClientStorage => {
  const values = new Map<string, string>();
  return {
    getItem: key => values.get(key) ?? null,
    removeItem: key => { values.delete(key); },
    setItem: (key, value) => { values.set(key, value); },
  };
};

describe('local authentication gateway', () => {
  it('stores and authenticates an account through the injected storage port', async () => {
    const gateway = createLocalAuthenticationGateway(createMemoryStorage());
    const familyId = createDomainId.family('c776a9b6-c37d-4a51-91ba-269a67274478');
    const memberId = createDomainId.familyMember('member-guardian');

    await gateway.register({
      familyId,
      familyName: 'Musterfamilie',
      memberId,
      password: 'safe-demo-password',
      username: 'Laura',
    });

    await expect(gateway.authenticate('laura', 'safe-demo-password')).resolves.toEqual({
      familyId,
      familyName: 'Musterfamilie',
      memberId,
    });
    await expect(gateway.authenticate('laura', 'wrong-password')).resolves.toBeNull();
  });

  it('isolates account state between storage adapters', async () => {
    const firstGateway = createLocalAuthenticationGateway(createMemoryStorage());
    const secondGateway = createLocalAuthenticationGateway(createMemoryStorage());

    await firstGateway.register({
      familyId: createDomainId.family('c776a9b6-c37d-4a51-91ba-269a67274478'),
      familyName: 'Musterfamilie',
      memberId: createDomainId.familyMember('member-guardian'),
      password: 'safe-demo-password',
      username: 'laura',
    });

    await expect(firstGateway.usernameExists('laura')).resolves.toBe(true);
    await expect(secondGateway.usernameExists('laura')).resolves.toBe(false);
  });
});
