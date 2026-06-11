import type { QuestCategory, QuestDifficulty } from '../lib/types';

/** Solo Leveling inspired dark palette: deep navy backgrounds, neon blue/purple glow. */
export const colors = {
  background: '#04060F',
  surface: '#0A1024',
  surfaceLight: '#111A3C',
  border: '#1E2C5C',
  glow: '#3B82F6',
  primary: '#38BDF8',
  primaryDim: '#0E4A6B',
  accent: '#8B5CF6',
  text: '#E2E8F0',
  textDim: '#7C8DB5',
  danger: '#EF4444',
  success: '#22C55E',
  gold: '#FFD700',
} as const;

export const difficultyColors: Record<QuestDifficulty, string> = {
  Easy: '#4ADE80',
  Medium: '#FACC15',
  Hard: '#FB923C',
  Elite: '#E879F9',
};

export const categoryLabels: Record<QuestCategory, string> = {
  Strength: 'Strength',
  Stamina: 'Stamina',
  Physique: 'Physique',
  Looks: 'Looks',
  WellBeing: 'Well-Being',
};

export const fonts = {
  /** Monospace gives the "system window" feel of the Solo Leveling UI. */
  mono: 'monospace',
} as const;
