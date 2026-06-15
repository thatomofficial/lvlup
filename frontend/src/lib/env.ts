import { Platform } from 'react-native';

/**
 * Resolved environment configuration. Mirrors the work-repo convention:
 * EXPO_PUBLIC_APP_ENV selects the target, and the API base URL is derived
 * from it. All EXPO_PUBLIC_* values are inlined at bundle time from the
 * active .env file (or an EAS build profile's `env` block).
 */
const VALID_ENVS = ['local', 'development', 'qa', 'production'] as const;

export type AppEnv = (typeof VALID_ENVS)[number];

const DEFAULT_LOCAL_PORT = '5180';

function getAppEnv(): AppEnv {
  const value = process.env.EXPO_PUBLIC_APP_ENV ?? 'local';
  if (!VALID_ENVS.includes(value as AppEnv)) {
    throw new Error(`EXPO_PUBLIC_APP_ENV must be one of: ${VALID_ENVS.join(', ')}.`);
  }
  return value as AppEnv;
}

/**
 * Zero-config local default: Android emulators reach the host via the special
 * alias 10.0.2.2; web and iOS simulators use localhost. The prestart sync
 * script overrides this with your LAN IP (EXPO_PUBLIC_LOCAL_API_HOST) so
 * physical devices work too.
 */
function platformLocalUrl(): string {
  return Platform.select({
    android: `http://10.0.2.2:${DEFAULT_LOCAL_PORT}`,
    default: `http://localhost:${DEFAULT_LOCAL_PORT}`,
  });
}

function getApiBaseUrl(appEnv: AppEnv): string {
  if (appEnv === 'local') {
    const host = process.env.EXPO_PUBLIC_LOCAL_API_HOST;
    const port = process.env.EXPO_PUBLIC_LOCAL_API_PORT ?? DEFAULT_LOCAL_PORT;
    const url = host ? `http://${host}:${port}` : platformLocalUrl();
    return url.replace(/\/+$/, '');
  }

  const url = process.env.EXPO_PUBLIC_API_URL;
  if (!url) {
    throw new Error(`EXPO_PUBLIC_API_URL is required when EXPO_PUBLIC_APP_ENV=${appEnv}.`);
  }
  return url.replace(/\/+$/, '');
}

export const APP_ENV: AppEnv = getAppEnv();
export const API_BASE_URL: string = getApiBaseUrl(APP_ENV);
