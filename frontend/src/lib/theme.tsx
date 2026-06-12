import AsyncStorage from '@react-native-async-storage/async-storage';
import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { useColorScheme } from 'react-native';

import { darkColors, lightColors, type ThemeColors } from '../constants/theme';

export type ThemePreference = 'dark' | 'light' | 'system';
export type ThemeMode = 'dark' | 'light';

const STORAGE_KEY = 'lvlup.themePreference';
const PREFERENCES: readonly ThemePreference[] = ['dark', 'light', 'system'];

interface ThemeContextValue {
  /** The active palette. */
  colors: ThemeColors;
  /** The resolved mode ('system' resolved against the OS setting). */
  mode: ThemeMode;
  /** The stored user preference. */
  preference: ThemePreference;
  setPreference(preference: ThemePreference): void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const systemScheme = useColorScheme();
  const [preference, setPreferenceState] = useState<ThemePreference>('dark');

  // Restore the persisted preference on mount.
  useEffect(() => {
    let cancelled = false;
    AsyncStorage.getItem(STORAGE_KEY)
      .then((stored) => {
        if (!cancelled && PREFERENCES.includes(stored as ThemePreference)) {
          setPreferenceState(stored as ThemePreference);
        }
      })
      .catch(() => {
        // Fall back to the default (dark).
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const setPreference = useCallback((next: ThemePreference) => {
    setPreferenceState(next);
    AsyncStorage.setItem(STORAGE_KEY, next).catch(() => {
      // Preference still applies for this session.
    });
  }, []);

  const mode: ThemeMode =
    preference === 'system' ? (systemScheme === 'light' ? 'light' : 'dark') : preference;

  const value = useMemo<ThemeContextValue>(
    () => ({
      colors: mode === 'light' ? lightColors : darkColors,
      mode,
      preference,
      setPreference,
    }),
    [mode, preference, setPreference],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
}
