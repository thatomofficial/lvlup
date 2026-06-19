import Constants from 'expo-constants';
import * as Network from 'expo-network';
import { useRouter } from 'expo-router';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Image,
  StyleSheet,
  Text,
  View,
} from 'react-native';

import { NeonButton } from '../components/NeonButton';
import type { ThemeColors } from '../constants/theme';
import { api } from '../lib/api';
import { useAuth } from '../lib/auth';
import { useTheme } from '../lib/theme';
import { isVersionBelow } from '../lib/version';

type StepStatus = 'pending' | 'running' | 'ok' | 'failed' | 'skipped';

interface BootState {
  network: StepStatus;
  version: StepStatus;
  /** Set when the server's minimum version is above this build. */
  updateRequired: boolean;
  /** Set when the network check fails; shows the retry button. */
  offline: boolean;
}

const INITIAL_BOOT: BootState = {
  network: 'running',
  version: 'pending',
  updateRequired: false,
  offline: false,
};

/**
 * Boot splash: runs the startup checks — network link, app version, session
 * restore (auth) and profile sync — silently in the background, then routes to
 * login, the awakening assessment, or the tabs. The user only sees a minimal
 * splash; UI is surfaced only when something needs them (offline, or a
 * required update).
 */
export default function BootScreen() {
  const router = useRouter();
  const { token, hunter, isLoading: authLoading } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const [boot, setBoot] = useState<BootState>(INITIAL_BOOT);
  const [attempt, setAttempt] = useState(0);

  // Network link, then version gate — run in the background.
  useEffect(() => {
    let cancelled = false;

    (async () => {
      setBoot(INITIAL_BOOT);

      const networkState = await Network.getNetworkStateAsync().catch(() => null);
      const online =
        networkState?.isConnected === true && networkState.isInternetReachable !== false;
      if (cancelled) {
        return;
      }
      if (!online) {
        setBoot((prev) => ({ ...prev, network: 'failed', offline: true }));
        return;
      }
      setBoot((prev) => ({ ...prev, network: 'ok', version: 'running' }));

      try {
        const config = await api.getAppConfig();
        if (cancelled) {
          return;
        }
        const currentVersion = Constants.expoConfig?.version ?? '0.0.0';
        if (isVersionBelow(currentVersion, config.minimumVersion)) {
          setBoot((prev) => ({ ...prev, version: 'failed', updateRequired: true }));
          return;
        }
        setBoot((prev) => ({ ...prev, version: 'ok' }));
      } catch {
        // Config endpoint unreachable: don't block boot — auth/sync surface
        // a dead server on their own.
        if (!cancelled) {
          setBoot((prev) => ({ ...prev, version: 'skipped' }));
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [attempt]);

  // Auth (session restore + profile sync) resolves via the auth context.
  const checksDone =
    boot.network === 'ok' &&
    (boot.version === 'ok' || boot.version === 'skipped') &&
    !authLoading;

  // Route once every gate has resolved.
  useEffect(() => {
    if (!checksDone || boot.updateRequired) {
      return;
    }
    if (!token) {
      router.replace('/(auth)/login');
      return;
    }
    const needsAssessment =
      hunter !== null && !hunter.hasCompletedAssessment && hunter.totalXp === 0;
    router.replace(needsAssessment ? '/assessment' : '/(tabs)');
  }, [checksDone, boot.updateRequired, token, hunter, router]);

  const retry = useCallback(() => setAttempt((n) => n + 1), []);

  return (
    <View style={styles.container}>
      <Image source={require('../assets/icon.png')} style={styles.logo} />
      <Text style={styles.title}>LVLUP</Text>

      {boot.offline ? (
        <View style={styles.actionArea}>
          <Text style={styles.notice}>NO ACTIVE CONNECTION DETECTED</Text>
          <NeonButton title="RETRY" onPress={retry} variant="outline" />
        </View>
      ) : boot.updateRequired ? (
        <View style={styles.actionArea}>
          <Text style={styles.notice}>
            A NEWER VERSION IS REQUIRED TO CONTINUE.{'\n'}UPDATE THE APP TO KEEP HUNTING.
          </Text>
        </View>
      ) : (
        <ActivityIndicator
          size="small"
          color={colors.textDim}
          style={styles.spinner}
        />
      )}
    </View>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.background,
      alignItems: 'center',
      justifyContent: 'center',
      padding: 32,
    },
    logo: {
      width: 96,
      height: 96,
      borderRadius: 22,
    },
    title: {
      color: colors.primary,
      fontSize: 24,
      fontWeight: '900',
      letterSpacing: 8,
      marginTop: 16,
      textShadowColor: colors.glow,
      textShadowOffset: { width: 0, height: 0 },
      textShadowRadius: 14,
    },
    spinner: {
      marginTop: 28,
    },
    actionArea: {
      marginTop: 28,
      alignSelf: 'stretch',
      gap: 12,
    },
    notice: {
      color: colors.danger,
      fontSize: 11,
      letterSpacing: 1.5,
      textAlign: 'center',
      lineHeight: 18,
    },
  });
