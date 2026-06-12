import React, { useMemo } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import type { ThemeColors } from '../constants/theme';
import { useTheme } from '../lib/theme';
import { xpLabel, xpPercent } from '../lib/xp';

interface XpBarProps {
  currentXp: number;
  xpForNextLevel: number;
}

/** Glowing XP progress bar with "current / next" label. */
export function XpBar({ currentXp, xpForNextLevel }: XpBarProps) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const percent = xpPercent(currentXp, xpForNextLevel);

  return (
    <View>
      <View style={styles.labelRow}>
        <Text style={styles.caption}>XP</Text>
        <Text style={styles.caption}>{xpLabel(currentXp, xpForNextLevel)}</Text>
      </View>
      <View style={styles.track}>
        <View style={[styles.fill, { width: `${percent}%` }]} />
      </View>
    </View>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  labelRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 6,
  },
  caption: {
    color: colors.textDim,
    fontSize: 12,
    letterSpacing: 1.5,
  },
  track: {
    height: 12,
    borderRadius: 6,
    backgroundColor: colors.surfaceLight,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
  },
  fill: {
    height: '100%',
    borderRadius: 6,
    backgroundColor: colors.primary,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.9,
    shadowRadius: 6,
  },
});
