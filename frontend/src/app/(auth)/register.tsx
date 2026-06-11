import { Link } from 'expo-router';
import React, { useState } from 'react';
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
import { colors } from '../../constants/theme';
import { ApiError } from '../../lib/api';
import { useAuth } from '../../lib/auth';

export default function RegisterScreen() {
  const { signUp } = useAuth();
  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleRegister = async () => {
    setError(null);
    if (!name.trim() || !surname.trim() || !username.trim() || !email.trim() || !password) {
      setError('Name, surname, username, email and password are required.');
      return;
    }
    if (!/^[a-zA-Z0-9_]{3,30}$/.test(username.trim())) {
      setError('Username must be 3-30 characters: letters, digits or underscores.');
      return;
    }
    if (password.length < 8) {
      setError('Password must be at least 8 characters.');
      return;
    }
    setSubmitting(true);
    try {
      await signUp(email.trim(), password, name.trim(), surname.trim(), username.trim());
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
          <Text style={styles.brand}>AWAKENING</Text>
          <Text style={styles.tagline}>REGISTER AS A NEW HUNTER</Text>

          <GlowPanel style={styles.panel}>
            <Text style={styles.panelTitle}>{'⚠'} HUNTER REGISTRATION</Text>

            <FormField
              label="First Name"
              value={name}
              onChangeText={setName}
              placeholder="Jin-Woo"
            />
            <FormField
              label="Surname"
              value={surname}
              onChangeText={setSurname}
              placeholder="Sung"
            />
            <FormField
              label="Username"
              value={username}
              onChangeText={setUsername}
              autoCapitalize="none"
              placeholder="shadow_monarch"
            />
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
              placeholder="min. 8 characters"
            />

            {error ? <Text style={styles.error}>{error}</Text> : null}

            <NeonButton
              title="AWAKEN"
              onPress={handleRegister}
              loading={submitting}
            />

            <Link href="/(auth)/login" style={styles.link}>
              <Text style={styles.linkText}>
                Already a hunter? <Text style={styles.linkAccent}>Log in</Text>
              </Text>
            </Link>
          </GlowPanel>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
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
    color: colors.accent,
    fontSize: 32,
    fontWeight: '900',
    letterSpacing: 6,
    textAlign: 'center',
    textShadowColor: colors.accent,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 18,
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
