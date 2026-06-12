import { getInitials } from '../initials';

describe('getInitials', () => {
  it('takes the first letter of name and surname', () => {
    expect(getInitials('Jin-Woo', 'Sung')).toBe('JS');
  });

  it('uppercases lowercase names', () => {
    expect(getInitials('thato', 'mokgotsi')).toBe('TM');
  });

  it('ignores surrounding whitespace', () => {
    expect(getInitials('  Ada ', ' Lovelace ')).toBe('AL');
  });

  it('falls back to a single initial when the surname is empty', () => {
    expect(getInitials('Ada', '')).toBe('A');
  });

  it('falls back to a question mark when both are empty', () => {
    expect(getInitials('', '   ')).toBe('?');
  });
});
