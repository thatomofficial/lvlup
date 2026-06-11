import { xpLabel, xpPercent } from '../xp';

describe('xpPercent', () => {
  it('computes the percentage toward the next level', () => {
    expect(xpPercent(50, 100)).toBe(50);
    expect(xpPercent(25, 100)).toBe(25);
    expect(xpPercent(1, 3)).toBeCloseTo(33.333, 2);
  });

  it('returns 0 at the start of a level', () => {
    expect(xpPercent(0, 100)).toBe(0);
  });

  it('clamps overflow at 100', () => {
    expect(xpPercent(150, 100)).toBe(100);
  });

  it('clamps negative XP to 0', () => {
    expect(xpPercent(-10, 100)).toBe(0);
  });

  it('guards against a zero or negative threshold', () => {
    expect(xpPercent(50, 0)).toBe(0);
    expect(xpPercent(50, -100)).toBe(0);
  });

  it('guards against non-finite inputs', () => {
    expect(xpPercent(Number.NaN, 100)).toBe(0);
    expect(xpPercent(50, Number.POSITIVE_INFINITY)).toBe(0);
  });
});

describe('xpLabel', () => {
  it('formats the current and required XP', () => {
    expect(xpLabel(40, 100)).toBe('40 / 100 XP');
  });
});
