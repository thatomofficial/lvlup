import { Ionicons } from '@expo/vector-icons';
import React, { useCallback, useState } from 'react';
import {
  Pressable,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ConsistencyPanel } from '../../components/ConsistencyPanel';
import { GlowPanel } from '../../components/GlowPanel';
import { RankBadge } from '../../components/RankBadge';
import { StatRow } from '../../components/StatRow';
import { XpBar } from '../../components/XpBar';
import { categoryLabels, colors } from '../../constants/theme';
import { api } from '../../lib/api';
import { useAuth } from '../../lib/auth';
import type { DisplayNamePreference } from '../../lib/types';

/** Tab 1 — the Solo Leveling style STATUS WINDOW. */
export default function StatusScreen() {
  const { token, hunter, refreshHunter, signOut } = useAuth();
  const [refreshing, setRefreshing] = useState(false);
  const [savingPreference, setSavingPreference] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);
  const [editingGitHub, setEditingGitHub] = useState(false);
  const [gitHubDraft, setGitHubDraft] = useState('');

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    try {
      await refreshHunter();
      setRefreshKey((key) => key + 1);
    } finally {
      setRefreshing(false);
    }
  }, [refreshHunter]);

  const saveGitHubUsername = useCallback(async () => {
    if (!token) {
      return;
    }
    try {
      const value = gitHubDraft.trim();
      await api.updateGitHubUsername(token, value.length > 0 ? value : null);
      await refreshHunter();
      setEditingGitHub(false);
    } catch {
      // Validation errors keep the editor open for correction.
    }
  }, [token, gitHubDraft, refreshHunter]);

  const changeDisplayPreference = useCallback(
    async (preference: DisplayNamePreference) => {
      if (!token || !hunter || hunter.displayNamePreference === preference) {
        return;
      }
      setSavingPreference(true);
      try {
        await api.updateDisplayPreference(token, preference);
        await refreshHunter();
      } catch {
        // Leave the previous preference in place; pull-to-refresh re-syncs.
      } finally {
        setSavingPreference(false);
      }
    },
    [token, hunter, refreshHunter],
  );

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
        <View style={styles.headerRow}>
          <Text style={styles.screenTitle}>{'⚠'} STATUS WINDOW</Text>
          <Pressable
            onPress={signOut}
            hitSlop={8}
            style={styles.logoutButton}
            accessibilityLabel="Log out"
          >
            <Ionicons name="log-out-outline" size={16} color={colors.textDim} />
            <Text style={styles.logoutText}>LOGOUT</Text>
          </Pressable>
        </View>

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

              <View style={styles.displayRow}>
                <Text style={styles.displayLabel}>DISPLAY</Text>
                {(
                  [
                    { value: 'FullName', label: 'FULL NAME' },
                    { value: 'Username', label: 'USERNAME' },
                  ] as const
                ).map((option) => {
                  const selected = hunter.displayNamePreference === option.value;
                  return (
                    <Pressable
                      key={option.value}
                      onPress={() => changeDisplayPreference(option.value)}
                      disabled={savingPreference}
                      style={[
                        styles.displayOption,
                        selected && styles.displayOptionSelected,
                      ]}
                      accessibilityState={{ selected }}
                    >
                      <Text
                        style={[
                          styles.displayOptionText,
                          selected && styles.displayOptionTextSelected,
                        ]}
                      >
                        {option.label}
                      </Text>
                    </Pressable>
                  );
                })}
              </View>

              <View style={styles.githubRow}>
                <Text style={styles.displayLabel}>GITHUB</Text>
                {editingGitHub ? (
                  <>
                    <TextInput
                      style={styles.githubInput}
                      value={gitHubDraft}
                      onChangeText={setGitHubDraft}
                      placeholder="username"
                      placeholderTextColor={colors.textDim}
                      autoCapitalize="none"
                      autoCorrect={false}
                    />
                    <Pressable
                      onPress={saveGitHubUsername}
                      style={[styles.displayOption, styles.displayOptionSelected]}
                    >
                      <Text style={[styles.displayOptionText, styles.displayOptionTextSelected]}>
                        SAVE
                      </Text>
                    </Pressable>
                  </>
                ) : (
                  <>
                    <Text style={styles.githubValue}>
                      {hunter.gitHubUsername ? `@${hunter.gitHubUsername}` : 'not linked'}
                    </Text>
                    <Pressable
                      onPress={() => {
                        setGitHubDraft(hunter.gitHubUsername ?? '');
                        setEditingGitHub(true);
                      }}
                      style={styles.displayOption}
                    >
                      <Text style={styles.displayOptionText}>EDIT</Text>
                    </Pressable>
                  </>
                )}
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
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  screenTitle: {
    color: colors.primary,
    fontSize: 16,
    fontWeight: '800',
    letterSpacing: 3,
    textShadowColor: colors.glow,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 12,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 6,
    paddingHorizontal: 8,
    paddingVertical: 4,
  },
  logoutText: {
    color: colors.textDim,
    fontSize: 10,
    letterSpacing: 1.5,
    fontWeight: '700',
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
  displayRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginTop: 16,
  },
  displayLabel: {
    color: colors.textDim,
    fontSize: 10,
    letterSpacing: 2,
    fontWeight: '700',
    marginRight: 4,
  },
  displayOption: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 5,
  },
  displayOptionSelected: {
    borderColor: colors.primary,
    backgroundColor: colors.primaryDim,
  },
  displayOptionText: {
    color: colors.textDim,
    fontSize: 10,
    letterSpacing: 1.5,
    fontWeight: '700',
  },
  displayOptionTextSelected: {
    color: colors.primary,
  },
  githubRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginTop: 12,
  },
  githubValue: {
    color: colors.text,
    fontSize: 12,
    flex: 1,
  },
  githubInput: {
    flex: 1,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 6,
    backgroundColor: colors.surfaceLight,
    color: colors.text,
    paddingHorizontal: 8,
    paddingVertical: 4,
    fontSize: 12,
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
