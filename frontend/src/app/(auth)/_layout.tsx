import { Redirect, Stack } from 'expo-router';
import React from 'react';

import { useAuth } from '../../lib/auth';
import { useTheme } from '../../lib/theme';

export default function AuthLayout() {
  const { token, isLoading } = useAuth();
  const { colors } = useTheme();

  // Once signed in, the auth stack is never shown.
  if (!isLoading && token) {
    return <Redirect href="/(tabs)" />;
  }

  return (
    <Stack
      screenOptions={{
        headerShown: false,
        contentStyle: { backgroundColor: colors.background },
      }}
    />
  );
}
