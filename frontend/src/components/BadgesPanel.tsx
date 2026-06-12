import React, { useEffect, useMemo, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { categoryLabels, type ThemeColors } from '../constants/theme';
import { api } from '../lib/api';
import { useTheme } from '../lib/theme';
import type { Badge, BadgeTier, QuestCategory } from '../lib/types';
import { GlowPanel } from './GlowPanel';

const TIER_COLORS: Record<BadgeTier, string> = {
  Iron: '#9CA3AF',
  Steel: '#60A5FA',
  Mythril: '#A78BFA',
  Monarch: '#FFD700',
};

const TIER_NUMERALS: Record<BadgeTier, string> = {
  Iron: 'I',
  Steel: 'II',
  Mythril: 'III',
  Monarch: 'IV',
};

interface BadgesPanelProps {
  token: string | null;
  /** Bump to trigger a refetch (e.g. on pull-to-refresh). */
  refreshKey: number;
}

/** Per-category badge lines: four tiers earned by quest completions. */
export function BadgesPanel({ token, refreshKey }: BadgesPanelProps) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const [badges, setBadges] = useState<Badge[] | null>(null);

  useEffect(() => {
    if (!token) {
      return;
    }
    let cancelled = false;
    api
      .getBadges(token)
      .then((data) => {
        if (!cancelled) {
          setBadges(data);
        }
      })
      .catch(() => {
        // Non-critical panel: keep the previous data on failure.
      });
    return () => {
      cancelled = true;
    };
  }, [token, refreshKey]);

  if (!badges) {
    return null;
  }

  const categories = [...new Set(badges.map((badge) => badge.category))];

  return (
    <GlowPanel style={styles.panel}>
      <Text style={styles.sectionTitle}>BADGES</Text>

      {categories.map((category) => {
        const line = badges.filter((badge) => badge.category === category);
        const nextBadge = line.find((badge) => !badge.isEarned);

        return (
          <View key={category} style={styles.row}>
            <View style={styles.rowInfo}>
              <Text style={styles.categoryLabel}>
                {categoryLabels[category as QuestCategory].toUpperCase()}
              </Text>
              <Text style={styles.progressText}>
                {nextBadge
                  ? `${nextBadge.completionsInCategory}/${nextBadge.requiredCompletions} → ${nextBadge.name}`
                  : 'MAX RANK'}
              </Text>
            </View>
            <View style={styles.medals}>
              {line.map((badge) => (
                <View
                  key={badge.tier}
                  style={[
                    styles.medal,
                    badge.isEarned
                      ? {
                          borderColor: TIER_COLORS[badge.tier],
                          backgroundColor: `${TIER_COLORS[badge.tier]}22`,
                        }
                      : styles.medalLocked,
                  ]}
                  accessibilityLabel={`${badge.name}${badge.isEarned ? ' earned' : ' locked'}`}
                >
                  <Text
                    style={[
                      styles.medalText,
                      badge.isEarned
                        ? { color: TIER_COLORS[badge.tier] }
                        : styles.medalTextLocked,
                    ]}
                  >
                    {TIER_NUMERALS[badge.tier]}
                  </Text>
                </View>
              ))}
            </View>
          </View>
        );
      })}
      <Text style={styles.legend}>
        EARN TIERS BY COMPLETING QUESTS · I=5 II=25 III=75 IV=200
      </Text>
    </GlowPanel>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  panel: {
    marginBottom: 16,
  },
  sectionTitle: {
    color: colors.accent,
    fontSize: 13,
    fontWeight: '800',
    letterSpacing: 3,
    marginBottom: 12,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 10,
  },
  rowInfo: {
    flex: 1,
    paddingRight: 8,
  },
  categoryLabel: {
    color: colors.text,
    fontSize: 11,
    fontWeight: '800',
    letterSpacing: 1.5,
  },
  progressText: {
    color: colors.textDim,
    fontSize: 10,
    marginTop: 2,
  },
  medals: {
    flexDirection: 'row',
    gap: 6,
  },
  medal: {
    width: 26,
    height: 26,
    borderRadius: 13,
    borderWidth: 1.5,
    alignItems: 'center',
    justifyContent: 'center',
  },
  medalLocked: {
    borderColor: colors.border,
    backgroundColor: colors.surfaceLight,
  },
  medalText: {
    fontSize: 9,
    fontWeight: '900',
  },
  medalTextLocked: {
    color: colors.textDim,
  },
  legend: {
    color: colors.textDim,
    fontSize: 9,
    letterSpacing: 1.2,
    textAlign: 'center',
    marginTop: 6,
  },
});
