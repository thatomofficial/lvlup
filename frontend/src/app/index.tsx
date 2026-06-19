import Constants from 'expo-constants';
import * as Network from 'expo-network';
import { useRouter } from 'expo-router';
import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  Animated,
  Dimensions,
  Image,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { NeonButton } from '../components/NeonButton';
import {
  BRAND_LOGO_SIZE,
  BRAND_LOGO_SPLASH_SIZE,
  BRAND_LOGO_TOP_PADDING,
} from '../constants/branding';
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

const START_SCALE = BRAND_LOGO_SPLASH_SIZE / BRAND_LOGO_SIZE;
const FADE_IN_MS = 500;
const MORPH_MS = 650;

/**
 * Boot splash: runs the startup checks — network link, app version, session
 * restore (auth) and profile sync — in the background, then morphs the logo
 * from screen-centre down onto the login screen's logo slot before routing, a
 * shared-element-style hand-off. UI is surfaced only when the user is needed
 * (offline, or a required update).
 */
export default function BootScreen() {
  const router = useRouter();
  const { token, hunter, isLoading: authLoading } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const insets = useSafeAreaInsets();
  const screenH = Dimensions.get('window').height;

  const [boot, setBoot] = useState<BootState>(INITIAL_BOOT);
  const [attempt, setAttempt] = useState(0);

  const logoOpacity = useRef(new Animated.Value(0)).current;
  const logoScale = useRef(new Animated.Value(START_SCALE)).current;
  const logoOffsetY = useRef(new Animated.Value(0)).current;
  const morphStarted = useRef(false);

  // Distance from screen-centre to where the login screen renders its logo.
  const targetOffsetY =
    insets.top + BRAND_LOGO_TOP_PADDING + BRAND_LOGO_SIZE / 2 - screenH / 2;

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

  // Fade the logo in on mount.
  useEffect(() => {
    Animated.timing(logoOpacity, {
      toValue: 1,
      duration: FADE_IN_MS,
      useNativeDriver: true,
    }).start();
  }, [logoOpacity]);

  // Once every gate resolves, morph the logo onto the login slot, then route.
  useEffect(() => {
    if (!checksDone || boot.updateRequired || morphStarted.current) {
      return;
    }
    morphStarted.current = true;

    Animated.parallel([
      Animated.timing(logoScale, {
        toValue: 1,
        duration: MORPH_MS,
        useNativeDriver: true,
      }),
      Animated.timing(logoOffsetY, {
        toValue: targetOffsetY,
        duration: MORPH_MS,
        useNativeDriver: true,
      }),
    ]).start(({ finished }) => {
      if (!finished) {
        return;
      }
      if (!token) {
        router.replace('/(auth)/login');
      } else if (
        hunter !== null &&
        !hunter.hasCompletedAssessment &&
        hunter.totalXp === 0
      ) {
        router.replace('/assessment');
      } else {
        router.replace('/(tabs)');
      }
    });
  }, [
    checksDone,
    boot.updateRequired,
    token,
    hunter,
    targetOffsetY,
    logoScale,
    logoOffsetY,
    router,
  ]);

  const retry = useCallback(() => {
    morphStarted.current = false;
    setAttempt((n) => n + 1);
  }, []);

  if (boot.offline || boot.updateRequired) {
    return (
      <View style={styles.container}>
        <Image
          source={require('../assets/icon.png')}
          style={styles.staticLogo}
          resizeMode="contain"
        />
        <View style={styles.actionArea}>
          <Text style={styles.notice}>
            {boot.offline
              ? 'NO ACTIVE CONNECTION DETECTED'
              : 'A NEWER VERSION IS REQUIRED TO CONTINUE.\nUPDATE THE APP TO KEEP HUNTING.'}
          </Text>
          {boot.offline ? (
            <NeonButton title="RETRY" onPress={retry} variant="outline" />
          ) : null}
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Animated.Image
        source={require('../assets/icon.png')}
        resizeMode="contain"
        style={[
          styles.morphLogo,
          {
            opacity: logoOpacity,
            transform: [{ translateY: logoOffsetY }, { scale: logoScale }],
          },
        ]}
      />
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
    morphLogo: {
      width: BRAND_LOGO_SIZE,
      height: BRAND_LOGO_SIZE,
    },
    staticLogo: {
      width: BRAND_LOGO_SIZE,
      height: BRAND_LOGO_SIZE,
      marginBottom: 24,
    },
    actionArea: {
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
