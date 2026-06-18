import { Redirect, Tabs } from 'expo-router';
import { ChartBar, Lightning, User } from '../../components/icons';
import React from 'react';

import { useAuth } from '../../lib/auth';
import { useTheme } from '../../lib/theme';

export default function TabsLayout() {
  const { token, isLoading } = useAuth();
  const { colors } = useTheme();

  // Guard: unauthenticated users never see the tabs.
  if (!isLoading && !token) {
    return <Redirect href="/(auth)/login" />;
  }

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: {
          backgroundColor: colors.surface,
          borderTopColor: colors.border,
          borderTopWidth: 1,
        },
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textDim,
        tabBarLabelStyle: { fontSize: 11, letterSpacing: 1 },
        sceneStyle: { backgroundColor: colors.background },
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'STATUS',
          tabBarIcon: ({ color, size, focused }) => (
            <ChartBar size={size} color={color} weight={focused ? 'fill' : 'regular'} />
          ),
        }}
      />
      <Tabs.Screen
        name="quests"
        options={{
          title: 'QUESTS',
          tabBarIcon: ({ color, size, focused }) => (
            <Lightning size={size} color={color} weight={focused ? 'fill' : 'regular'} />
          ),
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'PROFILE',
          tabBarIcon: ({ color, size, focused }) => (
            <User size={size} color={color} weight={focused ? 'fill' : 'regular'} />
          ),
        }}
      />
    </Tabs>
  );
}
