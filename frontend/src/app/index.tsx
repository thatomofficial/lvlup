import { Redirect } from 'expo-router';
import React, { useMemo } from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';

import type { ThemeColors } from '../constants/theme';
import { useAuth } from '../lib/auth';
import { useTheme } from '../lib/theme';

/** Entry route: waits for session restore, then routes to auth, awakening or tabs. */
export default function Index() {
  const { token, hunter, isLoading } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  if (isLoading) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  if (!token) {
    return <Redirect href="/(auth)/login" />;
  }

  // New hunters take the awakening assessment first, so their starting
  // stats and quest difficulties match their actual level.
  const needsAssessment =
    hunter !== null && !hunter.hasCompletedAssessment && hunter.totalXp === 0;

  return <Redirect href={needsAssessment ? '/assessment' : '/(tabs)'} />;
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
