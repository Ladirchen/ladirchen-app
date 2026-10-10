type SerializableRecord = Record<string, unknown>;
function isRecord(value: unknown): value is SerializableRecord {

  return typeof value === "object" && value !== null && !Array.isArray(value);
}
function isIdentifiedRecord(value: unknown): value is SerializableRecord & {
  readonly id: string;
} {

  return isRecord(value) && typeof value.id === "string";
}
function isIdentifiedRecordArray(value: unknown): value is Array<
  SerializableRecord & {
    readonly id: string;
  }
> {

  return Array.isArray(value) && value.every(isIdentifiedRecord);
}
function valuesEqual(left: unknown, right: unknown): boolean {

  return JSON.stringify(left) === JSON.stringify(right);
}
function mergeIdentifiedRecords(
  base: Array<
    SerializableRecord & {
      readonly id: string;
    }
  >,
  local: Array<
    SerializableRecord & {
      readonly id: string;
    }
  >,
  remote: Array<
    SerializableRecord & {
      readonly id: string;
    }
  >,
): unknown[] {
  const baseById = new Map(base.map((entry) => [entry.id, entry]));
  const localById = new Map(local.map((entry) => [entry.id, entry]));
  const remoteById = new Map(remote.map((entry) => [entry.id, entry]));
  const orderedIds = [...new Set([...remote.map((entry) => entry.id), ...local.map((entry) => entry.id)])];

  return orderedIds.flatMap(function callback(id) {
    const baseEntry = baseById.get(id);
    const localEntry = localById.get(id);
    const remoteEntry = remoteById.get(id);

    if (localEntry === undefined) {

      return baseEntry === undefined && remoteEntry !== undefined ? [remoteEntry] : [];
    }

    if (remoteEntry === undefined) {

      if (baseEntry !== undefined && valuesEqual(localEntry, baseEntry)) {

        return [];
      }

      return [localEntry];
    }

    if (baseEntry === undefined) {

      return [mergeRecords({}, localEntry, remoteEntry)];
    }

    return [mergeRecords(baseEntry, localEntry, remoteEntry)];
  });
}
function mergeRecords(
  base: SerializableRecord,
  local: SerializableRecord,
  remote: SerializableRecord,
): SerializableRecord {
  const merged: SerializableRecord = {};
  const keys = new Set([...Object.keys(base), ...Object.keys(local), ...Object.keys(remote)]);

  for (const key of keys) {
    const value = mergeValue(base[key], local[key], remote[key]);

    if (value !== undefined) {
      merged[key] = value;
    }

  }

  return merged;
}
function mergeValue(base: unknown, local: unknown, remote: unknown): unknown {

  if (valuesEqual(local, base)) {

    return remote;
  }

  if (valuesEqual(remote, base) || valuesEqual(local, remote)) {

    return local;
  }

  if (isRecord(local) && isRecord(remote)) {

    return mergeRecords(isRecord(base) ? base : {}, local, remote);
  }

  if (isIdentifiedRecordArray(local) && isIdentifiedRecordArray(remote)) {

    return mergeIdentifiedRecords(isIdentifiedRecordArray(base) ? base : [], local, remote);
  }

  return local;
}
export type AggregateConflictResolver<TState> = (base: TState, local: TState, remote: TState) => TState;
export function mergeSerializableAggregateState<TState>(base: TState, local: TState, remote: TState): TState {

  return mergeValue(base, local, remote) as TState;
}
