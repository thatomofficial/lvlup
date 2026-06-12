import React, { useState } from 'react';
import {
  KeyboardAvoidingView,
  Modal,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

import { colors } from '../constants/theme';
import type { Quest } from '../lib/types';
import { NeonButton } from './NeonButton';

interface CompleteQuestModalProps {
  quest: Quest | null;
  onConfirm(quest: Quest, note: string | undefined): void;
  onCancel(): void;
}

/**
 * Confirmation step before completing a quest, with an optional reflection
 * note ("what did you actually do?") stored on the completion record.
 */
export function CompleteQuestModal({ quest, onConfirm, onCancel }: CompleteQuestModalProps) {
  const [note, setNote] = useState('');

  const handleConfirm = () => {
    if (!quest) {
      return;
    }
    const trimmed = note.trim();
    onConfirm(quest, trimmed.length > 0 ? trimmed : undefined);
    setNote('');
  };

  const handleCancel = () => {
    setNote('');
    onCancel();
  };

  return (
    <Modal visible={quest !== null} transparent animationType="fade" onRequestClose={handleCancel}>
      <KeyboardAvoidingView
        style={styles.backdrop}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <View style={styles.panel}>
          <Text style={styles.title}>{'⚠'} COMPLETE QUEST</Text>
          <Text style={styles.questTitle} numberOfLines={2}>
            {quest?.title}
          </Text>
          {quest?.verification === 'GitHubPush' ? (
            <Text style={styles.verification}>
              Verified quest — the system will check GitHub for a push today.
            </Text>
          ) : null}

          <Text style={styles.noteLabel}>PROOF OF WORK (OPTIONAL)</Text>
          <TextInput
            style={styles.input}
            value={note}
            onChangeText={setNote}
            placeholder="What did you actually do?"
            placeholderTextColor={colors.textDim}
            multiline
            maxLength={280}
          />

          <NeonButton title="CONFIRM" onPress={handleConfirm} />
          <Pressable onPress={handleCancel} style={styles.cancel} hitSlop={8}>
            <Text style={styles.cancelText}>Cancel</Text>
          </Pressable>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  );
}

const styles = StyleSheet.create({
  backdrop: {
    flex: 1,
    backgroundColor: 'rgba(2, 4, 12, 0.85)',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  panel: {
    width: '100%',
    maxWidth: 420,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.primary,
    borderRadius: 10,
    padding: 20,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.6,
    shadowRadius: 16,
    elevation: 12,
  },
  title: {
    color: colors.primary,
    fontSize: 13,
    fontWeight: '800',
    letterSpacing: 3,
    textAlign: 'center',
  },
  questTitle: {
    color: colors.text,
    fontSize: 17,
    fontWeight: '700',
    textAlign: 'center',
    marginTop: 10,
  },
  verification: {
    color: colors.gold,
    fontSize: 12,
    textAlign: 'center',
    marginTop: 8,
  },
  noteLabel: {
    color: colors.textDim,
    fontSize: 10,
    letterSpacing: 2,
    fontWeight: '700',
    marginTop: 16,
    marginBottom: 6,
  },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    backgroundColor: colors.surfaceLight,
    color: colors.text,
    padding: 10,
    minHeight: 64,
    textAlignVertical: 'top',
    marginBottom: 16,
    fontSize: 14,
  },
  cancel: {
    alignSelf: 'center',
    marginTop: 14,
  },
  cancelText: {
    color: colors.textDim,
    fontSize: 13,
  },
});
