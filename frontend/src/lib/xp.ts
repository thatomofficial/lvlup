/**
 * Returns XP progress toward the next level as a percentage in [0, 100].
 * Defensive against bad inputs (zero/negative thresholds, negative XP,
 * overflow past the threshold) so the progress bar can never render
 * outside its track.
 */
export function xpPercent(currentXp: number, xpForNextLevel: number): number {
  if (!Number.isFinite(currentXp) || !Number.isFinite(xpForNextLevel)) {
    return 0;
  }
  if (xpForNextLevel <= 0) {
    return 0;
  }
  const percent = (currentXp / xpForNextLevel) * 100;
  return Math.min(100, Math.max(0, percent));
}

/** Human readable XP label, e.g. "40 / 100 XP". */
export function xpLabel(currentXp: number, xpForNextLevel: number): string {
  return `${currentXp} / ${xpForNextLevel} XP`;
}
