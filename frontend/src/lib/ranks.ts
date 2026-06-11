import type { Rank } from './types';

export const RANKS: readonly Rank[] = ['E', 'D', 'C', 'B', 'A', 'S'];

const RANK_COLORS: Record<Rank, string> = {
  E: '#9CA3AF', // gray
  D: '#4ADE80', // green
  C: '#38BDF8', // blue
  B: '#A78BFA', // purple
  A: '#FB923C', // orange
  S: '#FFD700', // gold
};

const FALLBACK_COLOR = '#9CA3AF';

export function isRank(value: string): value is Rank {
  return (RANKS as readonly string[]).includes(value);
}

/**
 * Maps a hunter rank to its display color.
 * Unknown values fall back to the E-rank gray so the UI never breaks
 * if the backend introduces a new rank.
 */
export function rankColor(rank: string): string {
  return isRank(rank) ? RANK_COLORS[rank] : FALLBACK_COLOR;
}
