import { isVersionBelow } from '../version';

describe('isVersionBelow', () => {
  it('detects an older major version', () => {
    expect(isVersionBelow('1.9.9', '2.0.0')).toBe(true);
  });

  it('detects an older minor version', () => {
    expect(isVersionBelow('1.1.5', '1.2.0')).toBe(true);
  });

  it('detects an older patch version', () => {
    expect(isVersionBelow('1.2.0', '1.2.1')).toBe(true);
  });

  it('treats equal versions as not below', () => {
    expect(isVersionBelow('1.2.3', '1.2.3')).toBe(false);
  });

  it('treats newer versions as not below', () => {
    expect(isVersionBelow('2.0.0', '1.9.9')).toBe(false);
  });

  it('pads missing segments with zero', () => {
    expect(isVersionBelow('1.2', '1.2.1')).toBe(true);
  });
});
