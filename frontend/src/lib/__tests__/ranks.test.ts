import { isRank, rankColor, RANKS } from '../ranks';

describe('rankColor', () => {
  it.each([
    ['E', '#9CA3AF'],
    ['D', '#4ADE80'],
    ['C', '#38BDF8'],
    ['B', '#A78BFA'],
    ['A', '#FB923C'],
    ['S', '#FFD700'],
  ])('maps rank %s to %s', (rank, expected) => {
    expect(rankColor(rank)).toBe(expected);
  });

  it('falls back to gray for unknown ranks', () => {
    expect(rankColor('Z')).toBe('#9CA3AF');
    expect(rankColor('')).toBe('#9CA3AF');
    expect(rankColor('s')).toBe('#9CA3AF'); // case-sensitive enum strings
  });
});

describe('isRank', () => {
  it('accepts all known ranks', () => {
    for (const rank of RANKS) {
      expect(isRank(rank)).toBe(true);
    }
  });

  it('rejects unknown values', () => {
    expect(isRank('F')).toBe(false);
    expect(isRank('SS')).toBe(false);
  });
});
