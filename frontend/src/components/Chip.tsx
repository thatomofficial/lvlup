import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

interface ChipProps {
  label: string;
  color: string;
}

/** Small outlined tag used for category / difficulty / quest type. */
export function Chip({ label, color }: ChipProps) {
  return (
    <View style={[styles.chip, { borderColor: color }]}>
      <Text style={[styles.text, { color }]}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  chip: {
    borderWidth: 1,
    borderRadius: 4,
    paddingHorizontal: 8,
    paddingVertical: 2,
    marginRight: 6,
    marginBottom: 4,
  },
  text: {
    fontSize: 11,
    fontWeight: '700',
    letterSpacing: 0.8,
  },
});
