export interface FamilyWorldPersistence {
  readList: <T>(key: string, fallback: () => T[]) => T[];
  readNumber: (key: string, fallback: number) => number;
  readText: (key: string) => string | null;
  writeList: <T>(key: string, value: T[]) => void;
  writeText: (key: string, value: string) => void;
}
