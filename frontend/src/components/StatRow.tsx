import React, { useMemo } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import type { ThemeColors } from '../constants/theme';
import { useTheme } from '../lib/theme';

interface StatRowProps {
  label: string;
  value: number;
}

/** A stat value is visualised against this soft cap; the bar clamps at 100. */
const STAT_BAR_MAX = 100;

export function StatRow({ label, value }: StatRowProps) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const percent = Math.min(100, Math.max(0, (value / STAT_BAR_MAX) * 100));

  return (
    <View style={styles.row}>
      <Text style={styles.label}>{label.toUpperCase()}</Text>
      <View style={styles.track}>
        <View style={[styles.fill, { width: `${percent}%` }]} />
      </View>
      <Text style={styles.value}>{value}</Text>
    </View>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    marginVertical: 7,
  },
  label: {
    width: 96,
    color: colors.text,
    fontSize: 12,
    letterSpacing: 1.2,
  },
  track: {
    flex: 1,
    height: 8,
    borderRadius: 4,
    backgroundColor: colors.surfaceLight,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
    marginHorizontal: 10,
  },
  fill: {
    height: '100%',
    backgroundColor: colors.accent,
    borderRadius: 4,
  },
  value: {
    width: 36,
    textAlign: 'right',
    color: colors.primary,
    fontWeight: '700',
    fontSize: 14,
  },
});
