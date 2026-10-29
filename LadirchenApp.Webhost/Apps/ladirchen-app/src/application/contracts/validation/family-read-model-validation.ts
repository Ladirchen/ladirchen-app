import type { FamilyReadModelType, FamilyReadModel } from "@/application/contracts/family-read-model-contract";
import { isNonNegativeInteger, isRecord, type StateGuard } from "./runtime-validation";

export function isFamilyReadModel<TReadModelType extends FamilyReadModelType, TState>(
  value: unknown,
  readModelType: TReadModelType,
  isState: StateGuard<TState>,
): value is FamilyReadModel<TReadModelType, TState> {
  return (
    isRecord(value) &&
    value.readModelType === readModelType &&
    isNonNegativeInteger(value.revision) &&
    isState(value.state)
  );
}
