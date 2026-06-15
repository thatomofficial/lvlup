/** Decodes a base64url string (JWT segments) to text. */
function decodeBase64Url(segment: string): string {
  const base64 = segment.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
  return atob(padded);
}

/** Reads the `exp` claim (unix seconds) from a JWT, or null when unreadable. */
export function getTokenExpiry(token: string): number | null {
  const payload = token.split('.')[1];
  if (!payload) {
    return null;
  }
  try {
    const claims: unknown = JSON.parse(decodeBase64Url(payload));
    if (typeof claims === 'object' && claims !== null && 'exp' in claims) {
      const exp = (claims as { exp: unknown }).exp;
      return typeof exp === 'number' ? exp : null;
    }
    return null;
  } catch {
    return null;
  }
}

/**
 * True when the token's expiry has passed (with a small safety margin so a
 * token about to expire is treated as expired). Unreadable tokens are NOT
 * reported expired — the server stays the authority for those.
 */
export function isTokenExpired(
  token: string,
  nowSeconds: number = Date.now() / 1000,
): boolean {
  const expiry = getTokenExpiry(token);
  return expiry !== null && expiry <= nowSeconds + 30;
}
