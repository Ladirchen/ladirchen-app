import foundation from "./en/foundation";
import world from "./en/world";
import account from "./en/account";
import activities from "./en/activities";
import catalog from "./en/catalog";

export default {
  ...foundation,
  ...world,
  ...account,
  ...activities,
  ...catalog,
} as const;
