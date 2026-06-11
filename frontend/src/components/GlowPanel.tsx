import React from 'react';
import { StyleSheet, View, type ViewStyle } from 'react-native';

import { colors } from '../constants/theme';

interface GlowPanelProps {
  children: React.ReactNode;
  style?: ViewStyle;
}

/** Dark panel with the glowing blue border used across the "system window" UI. */
export function GlowPanel({ children, style }: GlowPanelProps) {
  return <View style={[styles.panel, style]}>{children}</View>;
}

const styles = StyleSheet.create({
  panel: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.glow,
    borderRadius: 10,
    padding: 16,
    shadowColor: colors.glow,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.6,
    shadowRadius: 12,
    elevation: 8,
  },
});
