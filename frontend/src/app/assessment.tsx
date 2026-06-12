import { useRouter } from 'expo-router';
import React, { useMemo, useState } from 'react';
import {
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { GlowPanel } from '../components/GlowPanel';
import { NeonButton } from '../components/NeonButton';
import { StatRow } from '../components/StatRow';
import { categoryLabels, type ThemeColors } from '../constants/theme';
import { api, ApiError } from '../lib/api';
import { useAuth } from '../lib/auth';
import { useTheme } from '../lib/theme';
import type { AssessmentResult, QuestCategory } from '../lib/types';

interface Question {
  category: QuestCategory;
  question: string;
  options: readonly [string, string, string, string, string];
}

const QUESTIONS: readonly Question[] = [
  {
    category: 'Strength',
    question: 'How often do you strength train?',
    options: ['Never', 'Rarely', '1-2× / week', '3-4× / week', '5+ / week'],
  },
  {
    category: 'Stamina',
    question: 'How often do you get cardio in?',
    options: ['Never', 'Rarely', 'Weekly', 'Few times / week', 'Daily'],
  },
  {
    category: 'Physique',
    question: 'Stretching, mobility or posture work?',
    options: ['Never', 'Rarely', 'Weekly', 'Few times / week', 'Daily'],
  },
  {
    category: 'Looks',
    question: 'How consistent is your skincare / grooming?',
    options: ['None', 'Occasional', 'Some days', 'Most days', 'Every day'],
  },
  {
    category: 'WellBeing',
    question: 'How regular is your Bible reading / quiet time?',
    options: ['Not yet', 'Rarely', 'Weekly', 'Few times / week', 'Daily'],
  },
  {
    category: 'Intelligence',
    question: 'Coding or reading to grow, outside obligations?',
    options: ['Never', 'Rarely', 'Monthly', 'Weekly', 'Daily'],
  },
  {
    category: 'Charisma',
    question: 'Starting a conversation with someone new feels…',
    options: ['Terrifying', 'Hard', 'Doable', 'Comfortable', 'Easy'],
  },
];

/** One-time awakening assessment: sets starting stats so quests match your level. */
export default function AssessmentScreen() {
  const router = useRouter();
  const { token, refreshHunter } = useAuth();
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const [scores, setScores] = useState<Partial<Record<QuestCategory, number>>>({});
  const [result, setResult] = useState<AssessmentResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const allAnswered = QUESTIONS.every((q) => scores[q.category] !== undefined);

  const handleSubmit = async () => {
    if (!token || !allAnswered) {
      return;
    }
    setError(null);
    setBusy(true);
    try {
      const response = await api.submitAssessment(
        token,
        scores as Record<QuestCategory, number>,
      );
      setResult(response);
      await refreshHunter();
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Could not submit the assessment.');
    } finally {
      setBusy(false);
    }
  };

  const handleStarterPack = async () => {
    if (!token) {
      return;
    }
    setBusy(true);
    try {
      await api.createStarterPack(token);
      router.replace('/(tabs)/quests');
    } catch {
      router.replace('/(tabs)');
    }
  };

  if (result) {
    return (
      <SafeAreaView style={styles.safe} edges={['top', 'bottom']}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <Text style={styles.brand}>AWAKENING COMPLETE</Text>
          <Text style={styles.tagline}>YOUR STARTING STATS HAVE BEEN MEASURED</Text>

          <GlowPanel style={styles.panel}>
            <Text style={styles.panelTitle}>{'⚠'} STARTING STATS</Text>
            {QUESTIONS.map((q) => (
              <StatRow
                key={q.category}
                label={categoryLabels[q.category]}
                value={result.stats[statKey(q.category)]}
              />
            ))}
          </GlowPanel>

          <NeonButton
            title="GENERATE STARTER QUESTS AT MY LEVEL"
            onPress={handleStarterPack}
            loading={busy}
          />
          <Pressable onPress={() => router.replace('/(tabs)')} style={styles.skip}>
            <Text style={styles.skipText}>I&apos;ll add my own quests</Text>
          </Pressable>
        </ScrollView>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safe} edges={['top', 'bottom']}>
      <ScrollView contentContainerStyle={styles.scroll}>
        <Text style={styles.brand}>AWAKENING</Text>
        <Text style={styles.tagline}>
          ANSWER HONESTLY — QUESTS START AT YOUR LEVEL, NOT ABOVE IT
        </Text>

        {QUESTIONS.map((q) => (
          <GlowPanel key={q.category} style={styles.panel}>
            <Text style={styles.category}>{categoryLabels[q.category].toUpperCase()}</Text>
            <Text style={styles.question}>{q.question}</Text>
            <View style={styles.optionsRow}>
              {q.options.map((label, index) => {
                const value = index + 1;
                const selected = scores[q.category] === value;
                return (
                  <Pressable
                    key={label}
                    onPress={() =>
                      setScores((prev) => ({ ...prev, [q.category]: value }))
                    }
                    style={[styles.option, selected && styles.optionSelected]}
                    accessibilityState={{ selected }}
                  >
                    <Text
                      style={[styles.optionText, selected && styles.optionTextSelected]}
                    >
                      {label}
                    </Text>
                  </Pressable>
                );
              })}
            </View>
          </GlowPanel>
        ))}

        {error ? <Text style={styles.error}>{error}</Text> : null}

        <NeonButton
          title={allAnswered ? 'COMPLETE AWAKENING' : 'ANSWER ALL QUESTIONS'}
          onPress={handleSubmit}
          loading={busy}
          disabled={!allAnswered}
        />
        <Pressable onPress={() => router.replace('/(tabs)')} style={styles.skip}>
          <Text style={styles.skipText}>Skip for now</Text>
        </Pressable>
      </ScrollView>
    </SafeAreaView>
  );
}

function statKey(category: QuestCategory): keyof AssessmentResult['stats'] {
  switch (category) {
    case 'Strength':
      return 'strength';
    case 'Stamina':
      return 'stamina';
    case 'Physique':
      return 'physique';
    case 'Looks':
      return 'looks';
    case 'WellBeing':
      return 'wellBeing';
    case 'Intelligence':
      return 'intelligence';
    case 'Charisma':
      return 'charisma';
  }
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  safe: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    padding: 20,
    paddingBottom: 40,
  },
  brand: {
    color: colors.accent,
    fontSize: 26,
    fontWeight: '900',
    letterSpacing: 5,
    textAlign: 'center',
    textShadowColor: colors.accent,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 16,
    marginTop: 8,
  },
  tagline: {
    color: colors.textDim,
    textAlign: 'center',
    letterSpacing: 2,
    fontSize: 11,
    marginTop: 6,
    marginBottom: 20,
  },
  panel: {
    padding: 16,
    marginBottom: 14,
  },
  panelTitle: {
    color: colors.accent,
    letterSpacing: 3,
    fontSize: 13,
    fontWeight: '700',
    marginBottom: 14,
    textAlign: 'center',
  },
  category: {
    color: colors.primary,
    fontSize: 11,
    fontWeight: '800',
    letterSpacing: 2.5,
    marginBottom: 6,
  },
  question: {
    color: colors.text,
    fontSize: 15,
    fontWeight: '600',
    marginBottom: 12,
  },
  optionsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  option: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 6,
    backgroundColor: colors.surfaceLight,
  },
  optionSelected: {
    borderColor: colors.primary,
    backgroundColor: colors.primaryDim,
  },
  optionText: {
    color: colors.textDim,
    fontSize: 12,
    fontWeight: '600',
  },
  optionTextSelected: {
    color: colors.primary,
  },
  error: {
    color: colors.danger,
    fontSize: 13,
    marginBottom: 12,
    textAlign: 'center',
  },
  skip: {
    alignSelf: 'center',
    marginTop: 16,
    padding: 8,
  },
  skipText: {
    color: colors.textDim,
    fontSize: 13,
  },
});
