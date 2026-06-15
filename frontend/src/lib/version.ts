/** Parses "1.2.3" into numeric parts; missing/garbage segments become 0. */
function parts(version: string): [number, number, number] {
  const segments = version.split('.');
  const num = (index: number): number => {
    const parsed = parseInt(segments[index] ?? '0', 10);
    return Number.isNaN(parsed) ? 0 : parsed;
  };
  return [num(0), num(1), num(2)];
}

/** True when `current` is older than `minimum` (semver-style major.minor.patch). */
export function isVersionBelow(current: string, minimum: string): boolean {
  const a = parts(current);
  const b = parts(minimum);
  for (let i = 0; i < 3; i += 1) {
    if ((a[i] ?? 0) < (b[i] ?? 0)) {
      return true;
    }
    if ((a[i] ?? 0) > (b[i] ?? 0)) {
      return false;
    }
  }
  return false;
}
