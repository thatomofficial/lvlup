import { Platform } from 'react-native';

/**
 * Backend base URL, resolved in order:
 * 1. EXPO_PUBLIC_API_URL — inlined at bundle time from the active .env file
 *    (.env.development for `expo start`, .env.production for builds/exports)
 *    or from an EAS build profile's `env` block.
 * 2. Platform-aware local default — Android emulators cannot reach the host
 *    machine via `localhost`, so they use the special alias 10.0.2.2; web and
 *    iOS simulators talk to localhost directly.
 *
 * For a physical device on your LAN, set EXPO_PUBLIC_API_URL to your
 * machine's LAN IP (e.g. http://192.168.1.20:5180) in .env.development.
 */
const DEFAULT_DEV_URL: string = Platform.select({
  android: 'http://10.0.2.2:5180',
  default: 'http://localhost:5180',
});

export const API_BASE_URL: string =
  process.env.EXPO_PUBLIC_API_URL ?? DEFAULT_DEV_URL;
