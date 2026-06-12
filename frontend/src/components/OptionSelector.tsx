import React, { useMemo } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import type { ThemeColors } from '../constants/theme';
import { useTheme } from '../lib/theme';

interface OptionSelectorProps<T extends string> {
  label: string;
  options: readonly T[];
  value: T;
  onChange(value: T): void;
  /** Optional per-option display label (defaults to the raw value). */
  renderLabel?(option: T): string;
}

/** Chip-style single-select used for category / difficulty / type pickers. */
export function OptionSelector<T extends string>({
  label,
  options,
  value,
  onChange,
  renderLabel,
}: OptionSelectorProps<T>) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);

  return (
    <View style={styles.container}>
      <Text style={styles.label}>{label.toUpperCase()}</Text>
      <View style={styles.row}>
        {options.map((option) => {
          const selected = option === value;
          return (
            <Pressable
              key={option}
              onPress={() => onChange(option)}
              style={[styles.option, selected && styles.optionSelected]}
              accessibilityState={{ selected }}
            >
              <Text
                style={[styles.optionText, selected && styles.optionTextSelected]}
              >
                {renderLabel ? renderLabel(option) : option}
              </Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  container: {
    marginBottom: 16,
  },
  label: {
    color: colors.textDim,
    fontSize: 11,
    letterSpacing: 2,
    marginBottom: 8,
  },
  row: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  option: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 6,
    paddingHorizontal: 12,
    paddingVertical: 8,
    backgroundColor: colors.surfaceLight,
  },
  optionSelected: {
    borderColor: colors.primary,
    backgroundColor: colors.primaryDim,
  },
  optionText: {
    color: colors.textDim,
    fontSize: 13,
    fontWeight: '600',
  },
  optionTextSelected: {
    color: colors.primary,
  },
});
