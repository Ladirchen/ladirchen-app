import { readdir } from "node:fs/promises";
import path from "node:path";

export const walkFiles = async (directory) => (await Promise.all(
  (await readdir(directory, { withFileTypes: true })).map((entry) => {
    const entryPath = path.join(directory, entry.name);
    return entry.isDirectory() ? walkFiles(entryPath) : [entryPath];
  }),
)).flat();
