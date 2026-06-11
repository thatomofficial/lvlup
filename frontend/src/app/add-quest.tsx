import { useRouter } from 'expo-router';
import React, { useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { FormField } from '../components/FormField';
import { NeonButton } from '../components/NeonButton';
import { OptionSelector } from '../components/OptionSelector';
import { categoryLabels, colors } from '../constants/theme';
import { api, ApiError } from '../lib/api';
import { useAuth } from '../lib/auth';
import type { QuestCategory, QuestDifficulty, QuestType } from '../lib/types';

const CATEGORIES: readonly QuestCategory[] = [
  'Strength',
  'Stamina',
  'Physique',
  'Looks',
  'WellBeing',
];
const DIFFICULTIES: readonly QuestDifficulty[] = [
  'Easy',
  'Medium',
  'Hard',
  'Elite',
];
const TYPES: readonly QuestType[] = ['Daily', 'OneTime'];

const DIFFICULTY_HINTS: Record<QuestDifficulty, string> = {
  Easy: '+10 XP · +1 stat',
  Medium: '+25 XP · +2 stat',
  Hard: '+50 XP · +3 stat',
  Elite: '+100 XP · +5 stat',
};

/** Modal route for creating a new quest. */
export default function AddQuestScreen() {
  const router = useRouter();
  const { token } = useAuth();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState<QuestCategory>('Strength');
  const [difficulty, setDifficulty] = useState<QuestDifficulty>('Easy');
  const [type, setType] = useState<QuestType>('Daily');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    setError(null);
    if (!title.trim()) {
      setError('Title is required.');
      return;
    }
    if (!token) {
      setError('You are not signed in.');
      return;
    }
    setSubmitting(true);
    try {
      const trimmedDescription = description.trim();
      await api.createQuest(token, {
        title: title.trim(),
        ...(trimmedDescription ? { description: trimmedDescription } : {}),
        category,
        difficulty,
        type,
      });
      router.back();
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Failed to create quest.');
      setSubmitting(false);
    }
  };

  return (
    <SafeAreaView style={styles.safe} edges={['top', 'bottom']}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.headerRow}>
            <Text style={styles.title}>{'⚠'} NEW QUEST</Text>
            <Pressable onPress={() => router.back()} hitSlop={8}>
              <Text style={styles.cancel}>CANCEL</Text>
            </Pressable>
          </View>

          <FormField
            label="Title"
            value={title}
            onChangeText={setTitle}
            placeholder="Do 100 push-ups"
            maxLength={120}
          />
          <FormField
            label="Description (optional)"
            value={description}
            onChangeText={setDescription}
            placeholder="The path to becoming the strongest hunter…"
            multiline
            numberOfLines={3}
            style={styles.multiline}
          />

          <OptionSelector
            label="Category"
            options={CATEGORIES}
            value={category}
            onChange={(value) => setCategory(value)}
            renderLabel={(c) => categoryLabels[c]}
          />
          <OptionSelector
            label="Difficulty"
            options={DIFFICULTIES}
            value={difficulty}
            onChange={(value) => setDifficulty(value)}
          />
          <Text style={styles.hint}>{DIFFICULTY_HINTS[difficulty]}</Text>

          <OptionSelector
            label="Type"
            options={TYPES}
            value={type}
            onChange={(value) => setType(value)}
            renderLabel={(t) => (t === 'Daily' ? 'Daily' : 'One-Time')}
          />

          {error ? <Text style={styles.error}>{error}</Text> : null}

          <NeonButton
            title="REGISTER QUEST"
            onPress={handleSubmit}
            loading={submitting}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: {
    flex: 1,
    backgroundColor: colors.background,
  },
  flex: {
    flex: 1,
  },
  scroll: {
    padding: 20,
    paddingBottom: 40,
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 20,
  },
  title: {
    color: colors.primary,
    fontSize: 16,
    fontWeight: '800',
    letterSpacing: 3,
  },
  cancel: {
    color: colors.textDim,
    fontSize: 12,
    letterSpacing: 1.5,
    fontWeight: '700',
  },
  multiline: {
    minHeight: 72,
    textAlignVertical: 'top',
  },
  hint: {
    color: colors.textDim,
    fontSize: 12,
    marginTop: -8,
    marginBottom: 16,
  },
  error: {
    color: colors.danger,
    fontSize: 13,
    marginBottom: 12,
  },
});
