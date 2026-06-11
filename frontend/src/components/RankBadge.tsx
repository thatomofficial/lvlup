import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { colors } from '../constants/theme';
import { rankColor } from '../lib/ranks';

interface RankBadgeProps {
  rank: string;
  size?: number;
}

/** Color-coded hunter rank badge (E gray → S gold). */
export function RankBadge({ rank, size = 84 }: RankBadgeProps) {
  const color = rankColor(rank);

  return (
    <View
      style={[
        styles.badge,
        {
          width: size,
          height: size,
          borderRadius: size / 6,
          borderColor: color,
          shadowColor: color,
        },
      ]}
    >
      <Text style={[styles.rankText, { color, fontSize: size * 0.5 }]}>
        {rank}
      </Text>
      <Text style={[styles.rankLabel, { fontSize: size * 0.12 }]}>RANK</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 2,
    backgroundColor: colors.surfaceLight,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.8,
    shadowRadius: 10,
    elevation: 10,
  },
  rankText: {
    fontWeight: '900',
    letterSpacing: 2,
  },
  rankLabel: {
    color: colors.textDim,
    letterSpacing: 3,
    marginTop: 2,
  },
});
