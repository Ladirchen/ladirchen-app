import foundation from './de/foundation';
import world from './de/world';
import account from './de/account';
import activities from './de/activities';
import catalog from './de/catalog';

export default {
  ...foundation,
  ...world,
  ...account,
  ...activities,
  ...catalog,
} as const;
