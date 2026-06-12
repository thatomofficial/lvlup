import React, { useEffect, useMemo, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import type { ThemeColors } from '../constants/theme';
import { api } from '../lib/api';
import { heatLevel } from '../lib/heat';
import { useTheme } from '../lib/theme';
import type { Consistency, ConsistencyDay } from '../lib/types';
import { GlowPanel } from './GlowPanel';

const WEEK_DAYS = 7;

interface ConsistencyPanelProps {
  token: string | null;
  /** Bump to trigger a refetch (e.g. on pull-to-refresh). */
  refreshKey: number;
}

/** Streak, shields, discipline score and a 12-week completion heatmap. */
export function ConsistencyPanel({ token, refreshKey }: ConsistencyPanelProps) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const [consistency, setConsistency] = useState<Consistency | null>(null);

  useEffect(() => {
    if (!token) {
      return;
    }
    let cancelled = false;
    api
      .getConsistency(token)
      .then((data) => {
        if (!cancelled) {
          setConsistency(data);
        }
      })
      .catch(() => {
        // Non-critical panel: keep the previous data on failure.
      });
    return () => {
      cancelled = true;
    };
  }, [token, refreshKey]);

  if (!consistency) {
    return null;
  }

  const weeks: ConsistencyDay[][] = [];
  for (let i = 0; i < consistency.days.length; i += WEEK_DAYS) {
    weeks.push(consistency.days.slice(i, i + WEEK_DAYS));
  }

  return (
    <GlowPanel style={styles.panel}>
      <Text style={styles.sectionTitle}>CONSISTENCY</Text>

      <View style={styles.metricsRow}>
        <Metric
          styles={styles}
          label="STREAK"
          value={`${consistency.currentStreak}d`}
          highlight
        />
        <Metric styles={styles} label="BEST" value={`${consistency.longestStreak}d`} />
        <Metric
          styles={styles}
          label="SHIELDS"
          value={'🛡'.repeat(consistency.shields) || '—'}
        />
        <Metric
          styles={styles}
          label="DISCIPLINE"
          value={`${consistency.disciplineScore}%`}
        />
      </View>

      <View style={styles.heatmap}>
        {weeks.map((week) => (
          <View key={week[0]?.date ?? 'week'} style={styles.weekColumn}>
            {week.map((day) => (
              <View
                key={day.date}
                style={[
                  styles.dayCell,
                  { backgroundColor: colors.heat[heatLevel(day.completions)] },
                  day.shielded && styles.shieldedCell,
                ]}
              />
            ))}
          </View>
        ))}
      </View>
      <Text style={styles.legend}>LAST 12 WEEKS · GOLD = STREAK SHIELD USED</Text>
    </GlowPanel>
  );
}

function Metric({
  styles,
  label,
  value,
  highlight = false,
}: {
  styles: ReturnType<typeof createStyles>;
  label: string;
  value: string;
  highlight?: boolean;
}) {
  return (
    <View style={styles.metric}>
      <Text style={[styles.metricValue, highlight && styles.metricValueHighlight]}>
        {value}
      </Text>
      <Text style={styles.metricLabel}>{label}</Text>
    </View>
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
  metricsRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 14,
  },
  metric: {
    alignItems: 'center',
    flex: 1,
  },
  metricValue: {
    color: colors.text,
    fontSize: 18,
    fontWeight: '800',
  },
  metricValueHighlight: {
    color: colors.gold,
    textShadowColor: colors.gold,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 10,
  },
  metricLabel: {
    color: colors.textDim,
    fontSize: 9,
    letterSpacing: 1.5,
    marginTop: 3,
    fontWeight: '700',
  },
  heatmap: {
    flexDirection: 'row',
    gap: 3,
    justifyContent: 'center',
  },
  weekColumn: {
    gap: 3,
  },
  dayCell: {
    width: 11,
    height: 11,
    borderRadius: 2,
  },
  shieldedCell: {
    borderWidth: 1,
    borderColor: colors.gold,
  },
  legend: {
    color: colors.textDim,
    fontSize: 9,
    letterSpacing: 1.2,
    textAlign: 'center',
    marginTop: 10,
  },
});
