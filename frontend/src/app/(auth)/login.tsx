import * as Google from 'expo-auth-session/providers/google';
import { Link } from 'expo-router';
import * as WebBrowser from 'expo-web-browser';
import React, { useEffect, useMemo, useState } from 'react';
import {
  Image,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { FormField } from '../../components/FormField';
import { GlowPanel } from '../../components/GlowPanel';
import { NeonButton } from '../../components/NeonButton';
import {
  BRAND_LOGO_SIZE,
  BRAND_LOGO_TOP_PADDING,
} from '../../constants/branding';
import type { ThemeColors } from '../../constants/theme';
import { ApiError } from '../../lib/api';
import { useAuth } from '../../lib/auth';
import { useTheme } from '../../lib/theme';

WebBrowser.maybeCompleteAuthSession();

const GOOGLE_WEB_CLIENT_ID = process.env.EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID;
const GOOGLE_ANDROID_CLIENT_ID = process.env.EXPO_PUBLIC_GOOGLE_ANDROID_CLIENT_ID;
const SSO_CONFIGURED = Boolean(GOOGLE_WEB_CLIENT_ID ?? GOOGLE_ANDROID_CLIENT_ID);

export default function LoginScreen() {
  const { signIn, signInWithGoogle } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [googleBusy, setGoogleBusy] = useState(false);

  // Hook must always run; the placeholder id is never used because the
  // button is hidden until real client ids are configured.
  const [googleRequest, googleResponse, promptGoogle] = Google.useIdTokenAuthRequest({
    clientId: GOOGLE_WEB_CLIENT_ID ?? 'unconfigured.apps.googleusercontent.com',
    androidClientId: GOOGLE_ANDROID_CLIENT_ID,
  });

  useEffect(() => {
    if (googleResponse?.type !== 'success') {
      if (googleResponse?.type === 'error' || googleResponse?.type === 'dismiss') {
        setGoogleBusy(false);
      }
      return;
    }
    const idToken = googleResponse.params['id_token'];
    if (!idToken) {
      setGoogleBusy(false);
      setError('Google sign-in did not return a token.');
      return;
    }
    signInWithGoogle(idToken)
      .catch((e: unknown) => {
        setError(e instanceof ApiError ? e.message : 'Google sign-in failed.');
      })
      .finally(() => setGoogleBusy(false));
  }, [googleResponse, signInWithGoogle]);

  const handleGoogle = async () => {
    setError(null);
    setGoogleBusy(true);
    await promptGoogle();
  };

  const handleLogin = async () => {
    setError(null);
    if (!email.trim() || !password) {
      setError('Email and password are required.');
      return;
    }
    setSubmitting(true);
    try {
      await signIn(email.trim(), password);
      // Redirect is handled by the (auth) layout once token is set.
    } catch (e) {
      setError(
        e instanceof ApiError
          ? e.message
          : 'Could not reach the server. Is the backend running?',
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
        >
          <Image
            source={require('../../assets/splash-icon.png')}
            style={styles.logo}
            resizeMode="contain"
          />
          <Text style={styles.brand}>LVL{'↑'}UP</Text>
          <Text style={styles.tagline}>ARISE, HUNTER.</Text>

          <GlowPanel style={styles.panel}>
            <Text style={styles.panelTitle}>{'⚠'} SYSTEM LOGIN</Text>

            <FormField
              label="Email"
              value={email}
              onChangeText={setEmail}
              autoCapitalize="none"
              autoComplete="email"
              keyboardType="email-address"
              placeholder="hunter@guild.io"
            />
            <FormField
              label="Password"
              value={password}
              onChangeText={setPassword}
              secureTextEntry
              placeholder="••••••••"
            />

            {error ? <Text style={styles.error}>{error}</Text> : null}

            <NeonButton
              title="ENTER THE GATE"
              onPress={handleLogin}
              loading={submitting}
            />

            {SSO_CONFIGURED ? (
              <NeonButton
                title="CONTINUE WITH GOOGLE"
                onPress={handleGoogle}
                loading={googleBusy}
                disabled={!googleRequest}
                variant="outline"
                style={styles.googleButton}
              />
            ) : null}

            <Link href="/(auth)/forgot-password" style={styles.link}>
              <Text style={styles.linkAccent}>Forgot password?</Text>
            </Link>

            <Link href="/(auth)/register" style={styles.link}>
              <Text style={styles.linkText}>
                No account? <Text style={styles.linkAccent}>Awaken here</Text>
              </Text>
            </Link>
          </GlowPanel>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  safe: {
    flex: 1,
    backgroundColor: colors.background,
  },
  flex: {
    flex: 1,
  },
  scroll: {
    flexGrow: 1,
    justifyContent: 'flex-start',
    paddingHorizontal: 24,
    paddingTop: BRAND_LOGO_TOP_PADDING,
    paddingBottom: 24,
  },
  logo: {
    width: BRAND_LOGO_SIZE,
    height: BRAND_LOGO_SIZE,
    alignSelf: 'center',
    marginBottom: 12,
  },
  brand: {
    color: colors.primary,
    fontSize: 40,
    fontWeight: '900',
    letterSpacing: 6,
    textAlign: 'center',
    textShadowColor: colors.glow,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 18,
  },
  tagline: {
    color: colors.textDim,
    textAlign: 'center',
    letterSpacing: 4,
    fontSize: 12,
    marginTop: 6,
    marginBottom: 28,
  },
  panel: {
    padding: 20,
  },
  panelTitle: {
    color: colors.accent,
    letterSpacing: 3,
    fontSize: 13,
    fontWeight: '700',
    marginBottom: 18,
    textAlign: 'center',
  },
  error: {
    color: colors.danger,
    fontSize: 13,
    marginBottom: 12,
  },
  googleButton: {
    marginTop: 12,
  },
  link: {
    marginTop: 18,
    alignSelf: 'center',
  },
  linkText: {
    color: colors.textDim,
    fontSize: 13,
  },
  linkAccent: {
    color: colors.primary,
    fontWeight: '700',
  },
});
