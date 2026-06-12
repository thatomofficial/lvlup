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

import { BadgesPanel } from '../../components/BadgesPanel';
import { GlowPanel } from '../../components/GlowPanel';
import { RankBadge } from '../../components/RankBadge';
import { colors } from '../../constants/theme';
import { api } from '../../lib/api';
import { useAuth } from '../../lib/auth';
import type { DisplayNamePreference } from '../../lib/types';

/** Tab 3 — hunter profile and account settings. */
export default function ProfileScreen() {
  const { token, hunter, refreshHunter, signOut } = useAuth();
  const [refreshing, setRefreshing] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);
  const [savingPreference, setSavingPreference] = useState(false);
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
        <Text style={styles.screenTitle}>{'⚠'} HUNTER PROFILE</Text>

        {hunter ? (
          <>
            <GlowPanel style={styles.panel}>
              <View style={styles.identityRow}>
                <RankBadge rank={hunter.rank} />
                <View style={styles.identityInfo}>
                  <Text style={styles.hunterName} numberOfLines={1}>
                    {hunter.displayName}
                  </Text>
                  <Text style={styles.subtle}>@{hunter.username}</Text>
                  <Text style={styles.subtle}>{hunter.email}</Text>
                  <Text style={styles.subtle}>
                    {hunter.name} {hunter.surname} · LV.{hunter.level}
                  </Text>
                </View>
              </View>
            </GlowPanel>

            <BadgesPanel token={token} refreshKey={refreshKey} />

            <GlowPanel style={styles.panel}>
              <Text style={styles.sectionTitle}>SETTINGS</Text>

              <View style={styles.settingRow}>
                <Text style={styles.settingLabel}>DISPLAY</Text>
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
                      style={[styles.chip, selected && styles.chipSelected]}
                      accessibilityState={{ selected }}
                    >
                      <Text
                        style={[styles.chipText, selected && styles.chipTextSelected]}
                      >
                        {option.label}
                      </Text>
                    </Pressable>
                  );
                })}
              </View>

              <View style={styles.settingRow}>
                <Text style={styles.settingLabel}>GITHUB</Text>
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
                      style={[styles.chip, styles.chipSelected]}
                    >
                      <Text style={[styles.chipText, styles.chipTextSelected]}>
                        SAVE
                      </Text>
                    </Pressable>
                  </>
                ) : (
                  <>
                    <Text style={styles.githubValue}>
                      {hunter.gitHubUsername
                        ? `@${hunter.gitHubUsername}`
                        : 'not linked'}
                    </Text>
                    <Pressable
                      onPress={() => {
                        setGitHubDraft(hunter.gitHubUsername ?? '');
                        setEditingGitHub(true);
                      }}
                      style={styles.chip}
                    >
                      <Text style={styles.chipText}>EDIT</Text>
                    </Pressable>
                  </>
                )}
              </View>
              <Text style={styles.hint}>
                Linking GitHub lets coding quests verify a real push before
                they can be completed.
              </Text>
            </GlowPanel>

            <GlowPanel style={styles.panel}>
              <Text style={styles.sectionTitle}>ACCOUNT</Text>
              <Pressable
                onPress={signOut}
                style={styles.logoutButton}
                accessibilityLabel="Log out"
              >
                <Ionicons name="log-out-outline" size={16} color={colors.danger} />
                <Text style={styles.logoutText}>LOG OUT</Text>
              </Pressable>
            </GlowPanel>
          </>
        ) : (
          <GlowPanel>
            <Text style={styles.emptyText}>
              Could not load your profile. Pull down to retry.
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
  panel: {
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
    fontSize: 20,
    fontWeight: '800',
    letterSpacing: 1,
  },
  subtle: {
    color: colors.textDim,
    fontSize: 12,
    marginTop: 2,
  },
  sectionTitle: {
    color: colors.accent,
    fontSize: 13,
    fontWeight: '800',
    letterSpacing: 3,
    marginBottom: 12,
  },
  settingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 12,
  },
  settingLabel: {
    color: colors.textDim,
    fontSize: 10,
    letterSpacing: 2,
    fontWeight: '700',
    width: 56,
  },
  chip: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 5,
  },
  chipSelected: {
    borderColor: colors.primary,
    backgroundColor: colors.primaryDim,
  },
  chipText: {
    color: colors.textDim,
    fontSize: 10,
    letterSpacing: 1.5,
    fontWeight: '700',
  },
  chipTextSelected: {
    color: colors.primary,
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
  hint: {
    color: colors.textDim,
    fontSize: 11,
    lineHeight: 16,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 6,
    borderWidth: 1,
    borderColor: colors.danger,
    borderRadius: 8,
    paddingVertical: 10,
  },
  logoutText: {
    color: colors.danger,
    fontSize: 12,
    letterSpacing: 2,
    fontWeight: '800',
  },
  emptyText: {
    color: colors.textDim,
    textAlign: 'center',
  },
});
