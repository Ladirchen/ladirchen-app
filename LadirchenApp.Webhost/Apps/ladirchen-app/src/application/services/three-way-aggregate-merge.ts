type SerializableRecord = Record<string, unknown>;

const isRecord = (value: unknown): value is SerializableRecord =>
  typeof value === 'object' && value !== null && !Array.isArray(value);

const isIdentifiedRecord = (value: unknown): value is SerializableRecord & { readonly id: string } =>
  isRecord(value) && typeof value.id === 'string';

const isIdentifiedRecordArray = (value: unknown): value is Array<SerializableRecord & { readonly id: string }> =>
  Array.isArray(value) && value.every(isIdentifiedRecord);

const valuesEqual = (left: unknown, right: unknown): boolean =>
  JSON.stringify(left) === JSON.stringify(right);

const mergeIdentifiedRecords = (
  base: Array<SerializableRecord & { readonly id: string }>,
  local: Array<SerializableRecord & { readonly id: string }>,
  remote: Array<SerializableRecord & { readonly id: string }>,
): unknown[] => {
  const baseById = new Map(base.map(entry => [entry.id, entry]));
  const localById = new Map(local.map(entry => [entry.id, entry]));
  const remoteById = new Map(remote.map(entry => [entry.id, entry]));
  const orderedIds = [...new Set([...remote.map(entry => entry.id), ...local.map(entry => entry.id)])];

  return orderedIds.flatMap((id) => {
    const baseEntry = baseById.get(id);
    const localEntry = localById.get(id);
    const remoteEntry = remoteById.get(id);

    if (localEntry === undefined) {
      return baseEntry === undefined && remoteEntry !== undefined ? [remoteEntry] : [];
    }
    if (remoteEntry === undefined) {
      if (baseEntry !== undefined && valuesEqual(localEntry, baseEntry)) {return [];}
      return [localEntry];
    }
    if (baseEntry === undefined) {
      return [mergeRecords({}, localEntry, remoteEntry)];
    }
    return [mergeRecords(baseEntry, localEntry, remoteEntry)];
  });
};

const mergeRecords = (base: SerializableRecord, local: SerializableRecord, remote: SerializableRecord): SerializableRecord => {
  const merged: SerializableRecord = {};
  const keys = new Set([...Object.keys(base), ...Object.keys(local), ...Object.keys(remote)]);
  for (const key of keys) {
    const value = mergeValue(base[key], local[key], remote[key]);
    if (value !== undefined) {merged[key] = value;}
  }
  return merged;
};

const mergeValue = (base: unknown, local: unknown, remote: unknown): unknown => {
  if (valuesEqual(local, base)) {return remote;}
  if (valuesEqual(remote, base) || valuesEqual(local, remote)) {return local;}
  if (isRecord(local) && isRecord(remote)) {
    return mergeRecords(isRecord(base) ? base : {}, local, remote);
  }
  if (isIdentifiedRecordArray(local) && isIdentifiedRecordArray(remote)) {
    return mergeIdentifiedRecords(isIdentifiedRecordArray(base) ? base : [], local, remote);
  }
  return local;
};

export type AggregateConflictResolver<TState> = (
  base: TState,
  local: TState,
  remote: TState,
) => TState;

export const mergeSerializableAggregateState = <TState>(base: TState, local: TState, remote: TState): TState =>
  mergeValue(base, local, remote) as TState;
