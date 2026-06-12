import React, { useCallback, useState } from 'react';
import {
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ConsistencyPanel } from '../../components/ConsistencyPanel';
import { GlowPanel } from '../../components/GlowPanel';
import { RankBadge } from '../../components/RankBadge';
import { StatRow } from '../../components/StatRow';
import { XpBar } from '../../components/XpBar';
import { categoryLabels, colors } from '../../constants/theme';
import { useAuth } from '../../lib/auth';

/** Tab 1 — the Solo Leveling style STATUS WINDOW. */
export default function StatusScreen() {
  const { token, hunter, refreshHunter } = useAuth();
  const [refreshing, setRefreshing] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    try {
      await refreshHunter();
      setRefreshKey((key) => key + 1);
    } finally {
      setRefreshing(false);
    }
  }, [refreshHunter]);

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <ScrollView
        contentContainerStyle={styles.scroll}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={onRefresh}
            tintColor={colors.primary}
            colors={[colors.primary]}
            progressBackgroundColor={colors.surface}
          />
        }
      >
        <Text style={styles.screenTitle}>{'⚠'} STATUS WINDOW</Text>

        {hunter ? (
          <>
            <GlowPanel style={styles.identityPanel}>
              <View style={styles.identityRow}>
                <RankBadge rank={hunter.rank} />
                <View style={styles.identityInfo}>
                  <Text style={styles.hunterName} numberOfLines={1}>
                    {hunter.displayName}
                  </Text>
                  <Text style={styles.hunterTitle}>
                    HUNTER · @{hunter.username}
                  </Text>
                  <View style={styles.levelRow}>
                    <Text style={styles.levelLabel}>LV.</Text>
                    <Text style={styles.levelValue}>{hunter.level}</Text>
                  </View>
                </View>
              </View>

              <View style={styles.xpSection}>
                <XpBar
                  currentXp={hunter.currentXp}
                  xpForNextLevel={hunter.xpForNextLevel}
                />
                <Text style={styles.totalXp}>
                  TOTAL XP: {hunter.totalXp}
                </Text>
              </View>
            </GlowPanel>

            <ConsistencyPanel token={token} refreshKey={refreshKey} />

            <GlowPanel style={styles.statsPanel}>
              <Text style={styles.sectionTitle}>STATS</Text>
              <StatRow
                label={categoryLabels.Strength}
                value={hunter.stats.strength}
              />
              <StatRow
                label={categoryLabels.Stamina}
                value={hunter.stats.stamina}
              />
              <StatRow
                label={categoryLabels.Physique}
                value={hunter.stats.physique}
              />
              <StatRow label={categoryLabels.Looks} value={hunter.stats.looks} />
              <StatRow
                label={categoryLabels.WellBeing}
                value={hunter.stats.wellBeing}
              />
              <StatRow
                label={categoryLabels.Intelligence}
                value={hunter.stats.intelligence}
              />
              <StatRow
                label={categoryLabels.Charisma}
                value={hunter.stats.charisma}
              />
            </GlowPanel>
          </>
        ) : (
          <GlowPanel>
            <Text style={styles.emptyText}>
              Could not load your status. Pull down to retry.
            </Text>
          </GlowPanel>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    padding: 16,
    paddingBottom: 32,
  },
  screenTitle: {
    color: colors.primary,
    fontSize: 16,
    fontWeight: '800',
    letterSpacing: 3,
    textShadowColor: colors.glow,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 12,
    marginBottom: 16,
  },
  identityPanel: {
    marginBottom: 16,
  },
  identityRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 16,
  },
  identityInfo: {
    flex: 1,
  },
  hunterName: {
    color: colors.text,
    fontSize: 22,
    fontWeight: '800',
    letterSpacing: 1,
  },
  hunterTitle: {
    color: colors.textDim,
    fontSize: 11,
    letterSpacing: 3,
    marginTop: 2,
  },
  levelRow: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    marginTop: 8,
    gap: 6,
  },
  levelLabel: {
    color: colors.textDim,
    fontSize: 14,
    fontWeight: '700',
    marginBottom: 4,
  },
  levelValue: {
    color: colors.gold,
    fontSize: 34,
    fontWeight: '900',
    textShadowColor: colors.gold,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 12,
    lineHeight: 38,
  },
  xpSection: {
    marginTop: 18,
  },
  totalXp: {
    color: colors.textDim,
    fontSize: 11,
    letterSpacing: 1.5,
    marginTop: 8,
    textAlign: 'right',
  },
  statsPanel: {
    marginBottom: 16,
  },
  sectionTitle: {
    color: colors.accent,
    fontSize: 13,
    fontWeight: '800',
    letterSpacing: 3,
    marginBottom: 10,
  },
  emptyText: {
    color: colors.textDim,
    textAlign: 'center',
  },
});
