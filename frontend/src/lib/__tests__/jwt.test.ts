import { getTokenExpiry, isTokenExpired } from '../jwt';

function makeToken(payload: object): string {
  const body = Buffer.from(JSON.stringify(payload))
    .toString('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `header.${body}.signature`;
}

describe('getTokenExpiry', () => {
  it('reads the exp claim', () => {
    expect(getTokenExpiry(makeToken({ exp: 1750000000 }))).toBe(1750000000);
  });

  it('returns null when exp is missing', () => {
    expect(getTokenExpiry(makeToken({ sub: 'abc' }))).toBeNull();
  });

  it('returns null for malformed tokens', () => {
    expect(getTokenExpiry('not-a-jwt')).toBeNull();
  });
});

describe('isTokenExpired', () => {
  const now = 1750000000;

  it('reports expired when exp is in the past', () => {
    expect(isTokenExpired(makeToken({ exp: now - 60 }), now)).toBe(true);
  });

  it('reports expired within the 30s safety margin', () => {
    expect(isTokenExpired(makeToken({ exp: now + 10 }), now)).toBe(true);
  });

  it('reports valid when exp is comfortably in the future', () => {
    expect(isTokenExpired(makeToken({ exp: now + 3600 }), now)).toBe(false);
  });

  it('treats unreadable tokens as not locally expired', () => {
    expect(isTokenExpired('garbage', now)).toBe(false);
  });
});
