import { heatLevel } from '../heat';

describe('heatLevel', () => {
  it('returns 0 for no completions', () => {
    expect(heatLevel(0)).toBe(0);
  });

  it('returns 0 for negative counts', () => {
    expect(heatLevel(-1)).toBe(0);
  });

  it('returns 1 for a single completion', () => {
    expect(heatLevel(1)).toBe(1);
  });

  it('returns 2 for two or three completions', () => {
    expect(heatLevel(2)).toBe(2);
    expect(heatLevel(3)).toBe(2);
  });

  it('returns 3 for four or more completions', () => {
    expect(heatLevel(4)).toBe(3);
    expect(heatLevel(10)).toBe(3);
  });
});
