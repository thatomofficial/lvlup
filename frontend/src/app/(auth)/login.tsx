import { Link } from 'expo-router';
import React, { useMemo, useState } from 'react';
import {
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
import type { ThemeColors } from '../../constants/theme';
import { ApiError } from '../../lib/api';
import { useAuth } from '../../lib/auth';
import { useTheme } from '../../lib/theme';

export default function LoginScreen() {
  const { signIn } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

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
    justifyContent: 'center',
    padding: 24,
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
