import React, { useEffect, useMemo, useRef } from 'react';
import {
  Animated,
  Modal,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';

import type { ThemeColors } from '../constants/theme';
import { useTheme } from '../lib/theme';

interface LevelUpModalProps {
  visible: boolean;
  newLevel: number;
  onClose(): void;
}

/** Solo Leveling style "ding!" celebration shown when the hunter levels up. */
export function LevelUpModal({ visible, newLevel, onClose }: LevelUpModalProps) {
  const { colors } = useTheme();
  const styles = useMemo(() => createStyles(colors), [colors]);
  const scale = useRef(new Animated.Value(0.6)).current;
  const opacity = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    if (visible) {
      scale.setValue(0.6);
      opacity.setValue(0);
      Animated.parallel([
        Animated.spring(scale, {
          toValue: 1,
          friction: 5,
          useNativeDriver: true,
        }),
        Animated.timing(opacity, {
          toValue: 1,
          duration: 200,
          useNativeDriver: true,
        }),
      ]).start();
    }
  }, [visible, scale, opacity]);

  return (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
      <View style={styles.backdrop}>
        <Animated.View
          style={[styles.panel, { opacity, transform: [{ scale }] }]}
        >
          <Text style={styles.notice}>! NOTIFICATION</Text>
          <Text style={styles.levelUp}>LEVEL UP!</Text>
          <Text style={styles.message}>
            You are now level <Text style={styles.level}>{newLevel}</Text>
          </Text>
          <Pressable style={styles.button} onPress={onClose}>
            <Text style={styles.buttonText}>CONTINUE</Text>
          </Pressable>
        </Animated.View>
      </View>
    </Modal>
  );
}

const createStyles = (colors: ThemeColors) => StyleSheet.create({
  backdrop: {
    flex: 1,
    backgroundColor: 'rgba(2, 4, 12, 0.85)',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  panel: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: colors.surface,
    borderWidth: 2,
    borderColor: colors.primary,
    borderRadius: 12,
    padding: 28,
    alignItems: 'center',
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.9,
    shadowRadius: 24,
    elevation: 16,
  },
  notice: {
    color: colors.accent,
    fontSize: 12,
    letterSpacing: 4,
    marginBottom: 14,
  },
  levelUp: {
    color: colors.primary,
    fontSize: 36,
    fontWeight: '900',
    letterSpacing: 4,
    textShadowColor: colors.glow,
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 16,
  },
  message: {
    color: colors.text,
    fontSize: 16,
    marginTop: 12,
  },
  level: {
    color: colors.gold,
    fontWeight: '900',
  },
  button: {
    marginTop: 24,
    borderWidth: 1,
    borderColor: colors.primary,
    borderRadius: 6,
    paddingHorizontal: 28,
    paddingVertical: 10,
  },
  buttonText: {
    color: colors.primary,
    fontWeight: '800',
    letterSpacing: 2,
    fontSize: 13,
  },
});
