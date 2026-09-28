import { appendTranslationKey } from "./translation-keys";
import type { TranslationKey } from "./translation-keys";

const acceptsTranslationKey = (_key: TranslationKey): void => {};

const verifyTranslationKeyTypes = (): void => {
  acceptsTranslationKey("common.save");
  appendTranslationKey("seed.goals.bike", "title");

  // @ts-expect-error Misspelled keys must fail during type-checking.
  acceptsTranslationKey("common.svae");
  // @ts-expect-error Saving-goal namespaces only expose their translated title.
  appendTranslationKey("seed.goals.bike", "description");
};

void verifyTranslationKeyTypes;
