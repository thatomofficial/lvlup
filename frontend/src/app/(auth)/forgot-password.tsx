import { useRouter } from 'expo-router';
import React, { useMemo, useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { FormField } from '../../components/FormField';
import { GlowPanel } from '../../components/GlowPanel';
import { NeonButton } from '../../components/NeonButton';
import type { ThemeColors } from '../../constants/theme';
import { api, ApiError } from '../../lib/api';
import { useTheme } from '../../lib/theme';

type Phase = 'request' | 'confirm' | 'done';

export default function ForgotPasswordScreen() {
  const router = useRouter();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const [phase, setPhase] = useState<Phase>('request');
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleRequest = async () => {
    setError(null);
    if (!email.trim()) {
      setError('Enter your email.');
      return;
    }
    setBusy(true);
    try {
      await api.requestPasswordReset(email.trim());
      setPhase('confirm');
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Could not reach the server.');
    } finally {
      setBusy(false);
    }
  };

  const handleConfirm = async () => {
    setError(null);
    if (!code.trim() || !newPassword) {
      setError('Enter the code and your new password.');
      return;
    }
    if (newPassword.length < 8) {
      setError('Password must be at least 8 characters.');
      return;
    }
    setBusy(true);
    try {
      await api.resetPassword(email.trim(), code.trim(), newPassword);
      setPhase('done');
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Could not reach the server.');
    } finally {
      setBusy(false);
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
          <Text style={styles.brand}>RECOVER ACCESS</Text>
          <Text style={styles.tagline}>RESTORE YOUR HUNTER LICENSE</Text>

          {phase === 'request' ? (
            <GlowPanel style={styles.panel}>
              <Text style={styles.panelTitle}>{'⚠'} REQUEST RESET CODE</Text>
              <FormField
                label="Email"
                value={email}
                onChangeText={setEmail}
                autoCapitalize="none"
                autoComplete="email"
                keyboardType="email-address"
                placeholder="hunter@guild.io"
              />
              {error ? <Text style={styles.error}>{error}</Text> : null}
              <NeonButton title="SEND CODE" onPress={handleRequest} loading={busy} />
            </GlowPanel>
          ) : null}

          {phase === 'confirm' ? (
            <GlowPanel style={styles.panel}>
              <Text style={styles.panelTitle}>{'⚠'} ENTER CODE</Text>
              <Text style={styles.hint}>
                If an account exists for {email.trim()}, a 6-digit code is on its way.
              </Text>
              <FormField
                label="Reset code"
                value={code}
                onChangeText={setCode}
                keyboardType="number-pad"
                placeholder="000000"
                maxLength={6}
              />
              <FormField
                label="New password"
                value={newPassword}
                onChangeText={setNewPassword}
                secureTextEntry
                placeholder="min. 8 characters"
              />
              {error ? <Text style={styles.error}>{error}</Text> : null}
              <NeonButton title="RESET PASSWORD" onPress={handleConfirm} loading={busy} />
              <Pressable onPress={() => setPhase('request')} style={styles.link}>
                <Text style={styles.linkText}>Use a different email</Text>
              </Pressable>
            </GlowPanel>
          ) : null}

          {phase === 'done' ? (
            <GlowPanel style={styles.panel}>
              <Text style={styles.panelTitle}>{'✓'} PASSWORD RESET</Text>
              <Text style={styles.hint}>
                Your password has been changed. Log in with your new password.
              </Text>
              <NeonButton title="BACK TO LOGIN" onPress={() => router.replace('/(auth)/login')} />
            </GlowPanel>
          ) : null}

          {phase !== 'done' ? (
            <Pressable onPress={() => router.replace('/(auth)/login')} style={styles.link}>
              <Text style={styles.linkText}>
                Remembered it? <Text style={styles.linkAccent}>Log in</Text>
              </Text>
            </Pressable>
          ) : null}
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
    fontSize: 30,
    fontWeight: '900',
    letterSpacing: 4,
    textAlign: 'center',
    textShadowColor: colors.glow,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 16,
  },
  tagline: {
    color: colors.textDim,
    textAlign: 'center',
    letterSpacing: 3,
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
    marginBottom: 16,
    textAlign: 'center',
  },
  hint: {
    color: colors.textDim,
    fontSize: 12,
    lineHeight: 18,
    marginBottom: 16,
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
