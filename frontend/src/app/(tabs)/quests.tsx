import { Plus } from '../../components/icons';
import { useFocusEffect, useRouter } from 'expo-router';
import React, { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Platform,
  Pressable,
  RefreshControl,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { CompleteQuestModal } from '../../components/CompleteQuestModal';
import { GlowPanel } from '../../components/GlowPanel';
import { LevelUpModal } from '../../components/LevelUpModal';
import { QuestCard } from '../../components/QuestCard';
import { XpToast } from '../../components/XpToast';
import type { ThemeColors } from '../../constants/theme';
import { api, ApiError } from '../../lib/api';
import { useAuth } from '../../lib/auth';
import { useTheme } from '../../lib/theme';
import type { Quest } from '../../lib/types';

/** Tab 2 — the quest log. */
export default function QuestsScreen() {
  const router = useRouter();
  const { token, refreshHunter } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const [quests, setQuests] = useState<Quest[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [completingId, setCompletingId] = useState<string | null>(null);
  const [questToComplete, setQuestToComplete] = useState<Quest | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [levelUp, setLevelUp] = useState<{ visible: boolean; level: number }>({
    visible: false,
    level: 0,
  });

  const loadQuests = useCallback(async () => {
    if (!token) {
      return;
    }
    try {
      setError(null);
      const data = await api.getQuests(token);
      setQuests(data);
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Failed to load quests.');
    } finally {
      setLoading(false);
    }
  }, [token]);

  // Refetch whenever the tab gains focus (e.g. after adding a quest).
  useFocusEffect(
    useCallback(() => {
      void loadQuests();
    }, [loadQuests]),
  );

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadQuests();
    setRefreshing(false);
  }, [loadQuests]);

  const handleComplete = useCallback(
    async (quest: Quest, note: string | undefined) => {
      if (!token) {
        return;
      }
      setCompletingId(quest.id);
      try {
        const result = await api.completeQuest(token, quest.id, note);
        setQuests((prev) =>
          prev.map((q) =>
            q.id === quest.id
              ? {
                  ...q,
                  isCompleted: true,
                  lastCompletedAtUtc: new Date().toISOString(),
                }
              : q,
          ),
        );
        if (result.leveledUp) {
          setLevelUp({ visible: true, level: result.newLevel });
        } else {
          setToast(
            `+${result.xpGained} XP  ·  +${result.statGained} ${result.statCategory}`,
          );
        }
        // Keep the status window in sync.
        void refreshHunter();
      } catch (e) {
        if (e instanceof ApiError && e.status === 409) {
          // Already completed (e.g. stale list) — sync the UI.
          setQuests((prev) =>
            prev.map((q) =>
              q.id === quest.id ? { ...q, isCompleted: true } : q,
            ),
          );
          setToast('Quest already completed.');
        } else {
          setToast(
            e instanceof ApiError ? e.message : 'Failed to complete quest.',
          );
        }
      } finally {
        setCompletingId(null);
      }
    },
    [token, refreshHunter],
  );

  const deleteQuest = useCallback(
    async (quest: Quest) => {
      if (!token) {
        return;
      }
      try {
        await api.deleteQuest(token, quest.id);
        setQuests((prev) => prev.filter((q) => q.id !== quest.id));
      } catch (e) {
        setToast(e instanceof ApiError ? e.message : 'Failed to delete quest.');
      }
    },
    [token],
  );

  const confirmDelete = useCallback(
    (quest: Quest) => {
      if (Platform.OS === 'web') {
        // Alert.alert buttons are not supported on web.
        if (typeof window !== 'undefined' && window.confirm(`Delete quest "${quest.title}"?`)) {
          void deleteQuest(quest);
        }
        return;
      }
      Alert.alert(
        'Abandon Quest',
        `Delete "${quest.title}"? This cannot be undone.`,
        [
          { text: 'Cancel', style: 'cancel' },
          {
            text: 'Delete',
            style: 'destructive',
            onPress: () => void deleteQuest(quest),
          },
        ],
      );
    },
    [deleteQuest],
  );

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <View style={styles.headerRow}>
        <Text style={styles.screenTitle}>{'⚠'} QUEST LOG</Text>
        <Pressable
          onPress={() => router.push('/add-quest')}
          hitSlop={8}
          style={styles.addButton}
          accessibilityLabel="Add quest"
        >
          <Plus size={18} color={colors.background} />
          <Text style={styles.addButtonText}>NEW</Text>
        </Pressable>
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      ) : (
        <FlatList
          data={quests}
          keyExtractor={(quest) => quest.id}
          contentContainerStyle={styles.listContent}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={onRefresh}
              tintColor={colors.primary}
              colors={[colors.primary]}
              progressBackgroundColor={colors.surface}
            />
          }
          ListEmptyComponent={
            <GlowPanel style={styles.emptyPanel}>
              <Text style={styles.emptyTitle}>NO QUESTS AVAILABLE</Text>
              <Text style={styles.emptyText}>
                {error ??
                  'The system has no quests for you. Create one to begin leveling.'}
              </Text>
            </GlowPanel>
          }
          renderItem={({ item }) => (
            <QuestCard
              quest={item}
              isCompleting={completingId === item.id}
              onComplete={setQuestToComplete}
              onDelete={confirmDelete}
            />
          )}
        />
      )}

      <Pressable
        onPress={() => router.push('/add-quest')}
        style={styles.fab}
        accessibilityLabel="Add quest"
      >
        <Plus size={30} color={colors.background} />
      </Pressable>

      <CompleteQuestModal
        quest={questToComplete}
        onConfirm={(quest, note) => {
          setQuestToComplete(null);
          void handleComplete(quest, note);
        }}
        onCancel={() => setQuestToComplete(null)}
      />
      <XpToast message={toast} onHide={() => setToast(null)} />
      <LevelUpModal
        visible={levelUp.visible}
        newLevel={levelUp.level}
        onClose={() => setLevelUp({ visible: false, level: 0 })}
      />
    </SafeAreaView>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  safe: {
    flex: 1,
    backgroundColor: colors.background,
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingTop: 16,
    paddingBottom: 8,
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
  addButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.primary,
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 5,
    gap: 2,
  },
  addButtonText: {
    color: colors.background,
    fontWeight: '800',
    fontSize: 11,
    letterSpacing: 1,
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  listContent: {
    padding: 16,
    paddingBottom: 96,
  },
  emptyPanel: {
    marginTop: 24,
  },
  emptyTitle: {
    color: colors.textDim,
    fontWeight: '800',
    letterSpacing: 2,
    textAlign: 'center',
    marginBottom: 8,
  },
  emptyText: {
    color: colors.textDim,
    fontSize: 13,
    textAlign: 'center',
  },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 24,
    width: 56,
    height: 56,
    borderRadius: 28,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.8,
    shadowRadius: 12,
    elevation: 12,
  },
});
