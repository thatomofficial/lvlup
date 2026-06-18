import { Trash } from './icons';
import React, { useMemo } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';

import {
  categoryLabels,
  difficultyColors,
  type ThemeColors,
} from '../constants/theme';
import { useTheme } from '../lib/theme';
import type { Quest } from '../lib/types';
import { Chip } from './Chip';

interface QuestCardProps {
  quest: Quest;
  isCompleting: boolean;
  onComplete(quest: Quest): void;
  onDelete(quest: Quest): void;
}

export function QuestCard({
  quest,
  isCompleting,
  onComplete,
  onDelete,
}: QuestCardProps) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const done = quest.isCompleted;
  const completeLabel = done
    ? quest.type === 'Daily'
      ? 'DONE TODAY'
      : 'COMPLETED'
    : 'COMPLETE';

  return (
    <Pressable
      onLongPress={() => onDelete(quest)}
      style={[styles.card, done && styles.cardDone]}
    >
      <View style={styles.headerRow}>
        <Text style={[styles.title, done && styles.titleDone]} numberOfLines={2}>
          {quest.title}
        </Text>
        <Pressable
          hitSlop={8}
          onPress={() => onDelete(quest)}
          accessibilityLabel={`Delete quest ${quest.title}`}
        >
          <Trash size={18} color={colors.textDim} />
        </Pressable>
      </View>

      {quest.description ? (
        <Text style={styles.description} numberOfLines={3}>
          {quest.description}
        </Text>
      ) : null}

      <View style={styles.chipRow}>
        <Chip label={categoryLabels[quest.category]} color={colors.primary} />
        <Chip
          label={quest.difficulty.toUpperCase()}
          color={difficultyColors[quest.difficulty]}
        />
        <Chip
          label={quest.type === 'Daily' ? 'DAILY' : 'ONE-TIME'}
          color={quest.type === 'Daily' ? colors.accent : colors.textDim}
        />
      </View>

      <View style={styles.footerRow}>
        <Text style={styles.reward}>
          +{quest.xpReward} XP{'  '}
          <Text style={styles.rewardStat}>
            +{quest.statReward} {categoryLabels[quest.category]}
          </Text>
        </Text>

        <Pressable
          disabled={done || isCompleting}
          onPress={() => onComplete(quest)}
          style={({ pressed }) => [
            styles.completeButton,
            done && styles.completeButtonDone,
            pressed && !done && styles.completeButtonPressed,
          ]}
          accessibilityLabel={`Complete quest ${quest.title}`}
        >
          {isCompleting ? (
            <ActivityIndicator size="small" color={colors.background} />
          ) : (
            <Text
              style={[styles.completeText, done && styles.completeTextDone]}
            >
              {completeLabel}
            </Text>
          )}
        </Pressable>
      </View>
    </Pressable>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 10,
    padding: 14,
    marginBottom: 12,
  },
  cardDone: {
    opacity: 0.55,
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    gap: 8,
  },
  title: {
    flex: 1,
    color: colors.text,
    fontSize: 16,
    fontWeight: '700',
    letterSpacing: 0.5,
  },
  titleDone: {
    textDecorationLine: 'line-through',
    color: colors.textDim,
  },
  description: {
    color: colors.textDim,
    fontSize: 13,
    marginTop: 4,
  },
  chipRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginTop: 10,
  },
  footerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: 10,
  },
  reward: {
    color: colors.primary,
    fontSize: 13,
    fontWeight: '700',
  },
  rewardStat: {
    color: colors.accent,
    fontWeight: '600',
  },
  completeButton: {
    backgroundColor: colors.primary,
    borderRadius: 6,
    paddingHorizontal: 14,
    paddingVertical: 7,
    minWidth: 104,
    alignItems: 'center',
  },
  completeButtonPressed: {
    opacity: 0.7,
  },
  completeButtonDone: {
    backgroundColor: colors.surfaceLight,
    borderWidth: 1,
    borderColor: colors.border,
  },
  completeText: {
    color: colors.background,
    fontWeight: '800',
    fontSize: 12,
    letterSpacing: 1,
  },
  completeTextDone: {
    color: colors.textDim,
  },
});
