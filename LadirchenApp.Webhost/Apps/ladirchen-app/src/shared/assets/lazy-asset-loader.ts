/** Result shape produced by eager and lazy URL-only `import.meta.glob` calls. */
export type AssetGlobModules = Record<string, string | (() => Promise<string>)>;
function fileNameOf(path: string): string {
  return path.split("/").pop() ?? path;
}
/** Wraps a glob result into a filename-keyed async loader, caching in-flight/resolved promises so repeated lookups dedupe. */
export function createRawAssetLoader(modules: AssetGlobModules): (fileName: string) => Promise<string> {
  const loadersByFileName = new Map(Object.entries(modules).map(([path, load]) => [fileNameOf(path), load]));
  const cache = new Map<string, Promise<string>>();

  return (fileName: string): Promise<string> => {
    const cached = cache.get(fileName);

    if (cached !== undefined) {
      return cached;
    }

    const load = loadersByFileName.get(fileName);

    if (!load) {
      throw new Error(`No bundled asset found for file "${fileName}"`);
    }

    const promise = typeof load === "string" ? Promise.resolve(load) : load();
    cache.set(fileName, promise);

    return promise;
  };
}
/** Builds a typed id -> url loader on top of a raw filename loader (default file name convention: "<id>.webp"). */
export function createLazyAssetLoader<Id extends string>(
  modules: AssetGlobModules,
  resolveFileName: (id: Id) => string = (id) => `${id}.webp`,
): (id: Id) => Promise<string> {
  const loadByFileName = createRawAssetLoader(modules);

  return (id: Id): Promise<string> => loadByFileName(resolveFileName(id));
}
