import type { QuestCategory, QuestDifficulty } from '../lib/types';

/**
 * Solo Leveling inspired palettes. Dark is the signature look (deep navy,
 * neon blue/purple glow); light keeps the same accent language on bright
 * surfaces. Components receive the active palette via useTheme().
 */
export const darkColors = {
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
  /** Consistency heatmap intensities, none -> high. */
  heat: ['#10183A', '#0E4A6B', '#1B7FB8', '#38BDF8'],
};

export type ThemeColors = typeof darkColors;

export const lightColors: ThemeColors = {
  background: '#EEF2F9',
  surface: '#FFFFFF',
  surfaceLight: '#E3E9F5',
  border: '#C6D2E8',
  glow: '#3B82F6',
  primary: '#0369A1',
  primaryDim: '#CDE7F8',
  accent: '#6D28D9',
  text: '#101828',
  textDim: '#5D6C8B',
  danger: '#DC2626',
  success: '#15803D',
  gold: '#A16207',
  heat: ['#E2E8F0', '#BFDBFE', '#60A5FA', '#1D4ED8'],
};

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
  Intelligence: 'Intelligence',
  Charisma: 'Charisma',
};

export const fonts = {
  /** Monospace gives the "system window" feel of the Solo Leveling UI. */
  mono: 'monospace',
} as const;
