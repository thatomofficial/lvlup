import AsyncStorage from '@react-native-async-storage/async-storage';
import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

import { api, ApiError } from './api';
import { isTokenExpired } from './jwt';
import type { Hunter } from './types';

const TOKEN_KEY = 'lvlup.token';
const HUNTER_ID_KEY = 'lvlup.hunterId';

interface AuthContextValue {
  /** JWT, null when signed out. */
  token: string | null;
  /** Current hunter profile, refreshed from /hunters/me. */
  hunter: Hunter | null;
  /** True while restoring the persisted session on app start. */
  isLoading: boolean;
  signIn(email: string, password: string): Promise<void>;
  signInWithGoogle(idToken: string): Promise<void>;
  signUp(
    email: string,
    password: string,
    name: string,
    surname: string,
    username: string,
  ): Promise<void>;
  signOut(): Promise<void>;
  /** Re-fetches /hunters/me; signs out on 401. */
  refreshHunter(): Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [hunter, setHunter] = useState<Hunter | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const clearSession = useCallback(async () => {
    setToken(null);
    setHunter(null);
    await AsyncStorage.multiRemove([TOKEN_KEY, HUNTER_ID_KEY]);
  }, []);

  // Restore persisted session on mount.
  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const storedToken = await AsyncStorage.getItem(TOKEN_KEY);
        if (!storedToken) {
          return;
        }
        // Expired sessions are dropped locally - no wasted server round trip.
        if (isTokenExpired(storedToken)) {
          await AsyncStorage.multiRemove([TOKEN_KEY, HUNTER_ID_KEY]);
          return;
        }
        const me = await api.getMe(storedToken);
        if (!cancelled) {
          setToken(storedToken);
          setHunter(me);
        }
      } catch {
        // Expired/invalid token or backend unreachable: start signed out.
        await AsyncStorage.multiRemove([TOKEN_KEY, HUNTER_ID_KEY]);
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const establishSession = useCallback(
    async (newToken: string, hunterId: string) => {
      await AsyncStorage.multiSet([
        [TOKEN_KEY, newToken],
        [HUNTER_ID_KEY, hunterId],
      ]);
      const me = await api.getMe(newToken);
      setToken(newToken);
      setHunter(me);
    },
    [],
  );

  const signIn = useCallback(
    async (email: string, password: string) => {
      const result = await api.login(email, password);
      await establishSession(result.token, result.hunterId);
    },
    [establishSession],
  );

  const signInWithGoogle = useCallback(
    async (idToken: string) => {
      const result = await api.loginWithGoogle(idToken);
      await establishSession(result.token, result.hunterId);
    },
    [establishSession],
  );

  const signUp = useCallback(
    async (
      email: string,
      password: string,
      name: string,
      surname: string,
      username: string,
    ) => {
      const result = await api.register(email, password, name, surname, username);
      await establishSession(result.token, result.hunterId);
    },
    [establishSession],
  );

  const signOut = useCallback(async () => {
    await clearSession();
  }, [clearSession]);

  const refreshHunter = useCallback(async () => {
    if (!token) {
      return;
    }
    try {
      const me = await api.getMe(token);
      setHunter(me);
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        await clearSession();
        return;
      }
      throw error;
    }
  }, [token, clearSession]);

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      hunter,
      isLoading,
      signIn,
      signInWithGoogle,
      signUp,
      signOut,
      refreshHunter,
    }),
    [token, hunter, isLoading, signIn, signInWithGoogle, signUp, signOut, refreshHunter],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
