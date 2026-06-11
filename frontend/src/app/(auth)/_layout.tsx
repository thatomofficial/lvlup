import { Redirect, Stack } from 'expo-router';
import React from 'react';

import { colors } from '../../constants/theme';
import { useAuth } from '../../lib/auth';

export default function AuthLayout() {
  const { token, isLoading } = useAuth();

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
