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
 * Boot splash: runs the startup checks in sequence — network link, app
 * version, session restore (auth) and profile sync — then routes to login,
 * the awakening assessment, or the tabs.
 */
export default function BootScreen() {
  const router = useRouter();
  const { token, hunter, isLoading: authLoading } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const [boot, setBoot] = useState<BootState>(INITIAL_BOOT);
  const [attempt, setAttempt] = useState(0);

  // Steps 1-2: network link, then version gate.
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

  // Steps 3-4 derive from the auth context (session restore + profile sync).
  const authStatus: StepStatus = authLoading ? 'running' : token ? 'ok' : 'skipped';
  const syncStatus: StepStatus = authLoading
    ? 'pending'
    : !token
      ? 'skipped'
      : hunter
        ? 'ok'
        : 'failed';

  const checksDone =
    boot.network === 'ok' &&
    (boot.version === 'ok' || boot.version === 'skipped') &&
    !authLoading;

  // Route once every gate has resolved (small delay so the window is readable).
  useEffect(() => {
    if (!checksDone || boot.updateRequired) {
      return;
    }
    const handle = setTimeout(() => {
      if (!token) {
        router.replace('/(auth)/login');
        return;
      }
      const needsAssessment =
        hunter !== null && !hunter.hasCompletedAssessment && hunter.totalXp === 0;
      router.replace(needsAssessment ? '/assessment' : '/(tabs)');
    }, 450);
    return () => clearTimeout(handle);
  }, [checksDone, boot.updateRequired, token, hunter, router]);

  const retry = useCallback(() => setAttempt((n) => n + 1), []);

  return (
    <View style={styles.container}>
      <Image source={require('../../assets/icon.png')} style={styles.logo} />
      <Text style={styles.title}>LVLUP</Text>
      <Text style={styles.subtitle}>SYSTEM BOOT</Text>

      <View style={styles.steps}>
        <BootStep label="NETWORK LINK" status={boot.network} styles={styles} colors={colors} />
        <BootStep label="SYSTEM VERSION" status={boot.version} styles={styles} colors={colors} />
        <BootStep label="AUTHENTICATION" status={authStatus} styles={styles} colors={colors} />
        <BootStep label="DATA SYNC" status={syncStatus} styles={styles} colors={colors} />
      </View>

      {boot.offline ? (
        <View style={styles.actionArea}>
          <Text style={styles.notice}>NO ACTIVE CONNECTION DETECTED</Text>
          <NeonButton title="RETRY" onPress={retry} variant="outline" />
        </View>
      ) : null}

      {boot.updateRequired ? (
        <View style={styles.actionArea}>
          <Text style={styles.notice}>
            A NEWER VERSION IS REQUIRED TO CONTINUE.{'\n'}UPDATE THE APP TO KEEP HUNTING.
          </Text>
        </View>
      ) : null}
    </View>
  );
}

function BootStep({
  label,
  status,
  styles,
  colors,
}: {
  label: string;
  status: StepStatus;
  styles: ReturnType<typeof createStyles>;
  colors: ThemeColors;
}) {
  const glyph =
    status === 'ok' ? '✓' : status === 'failed' ? '✕' : status === 'skipped' ? '—' : null;
  const glyphColor =
    status === 'ok' ? colors.success : status === 'failed' ? colors.danger : colors.textDim;

  return (
    <View style={styles.stepRow}>
      <View style={styles.stepGlyph}>
        {status === 'running' ? (
          <ActivityIndicator size="small" color={colors.primary} />
        ) : (
          <Text style={[styles.stepGlyphText, { color: glyphColor }]}>{glyph ?? '·'}</Text>
        )}
      </View>
      <Text style={[styles.stepLabel, status === 'pending' && styles.stepLabelPending]}>
        {label}
      </Text>
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
    subtitle: {
      color: colors.textDim,
      fontSize: 11,
      letterSpacing: 4,
      marginTop: 4,
      marginBottom: 28,
    },
    steps: {
      alignSelf: 'stretch',
      maxWidth: 280,
      width: '100%',
      marginHorizontal: 'auto',
      gap: 10,
    },
    stepRow: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 10,
      alignSelf: 'center',
      width: 220,
    },
    stepGlyph: {
      width: 20,
      alignItems: 'center',
    },
    stepGlyphText: {
      fontSize: 14,
      fontWeight: '800',
    },
    stepLabel: {
      color: colors.text,
      fontSize: 12,
      letterSpacing: 2,
      fontWeight: '700',
    },
    stepLabelPending: {
      color: colors.textDim,
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
