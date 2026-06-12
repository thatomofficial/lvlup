export type Rank = 'E' | 'D' | 'C' | 'B' | 'A' | 'S';

export type QuestCategory =
  | 'Strength'
  | 'Stamina'
  | 'Physique'
  | 'Looks'
  | 'WellBeing'
  | 'Intelligence'
  | 'Charisma';

export type QuestDifficulty = 'Easy' | 'Medium' | 'Hard' | 'Elite';

export type QuestType = 'Daily' | 'OneTime';

export type QuestVerification = 'None' | 'GitHubPush';

export interface HunterStats {
  strength: number;
  stamina: number;
  physique: number;
  looks: number;
  wellBeing: number;
  intelligence: number;
  charisma: number;
}

export type DisplayNamePreference = 'FullName' | 'Username';

export interface Hunter {
  id: string;
  name: string;
  surname: string;
  username: string;
  displayName: string;
  displayNamePreference: DisplayNamePreference;
  email: string;
  level: number;
  currentXp: number;
  xpForNextLevel: number;
  totalXp: number;
  rank: Rank;
  avatarUrl: string | null;
  hasCompletedAssessment: boolean;
  gitHubUsername: string | null;
  stats: HunterStats;
}

/** 1-5 self-assessment score per stat category. */
export type AssessmentScores = Record<QuestCategory, number>;

export interface AssessmentResult {
  stats: HunterStats;
  recommendedDifficulties: Record<QuestCategory, QuestDifficulty>;
}

export interface StarterPackResult {
  createdCount: number;
  skippedCount: number;
}

export type BadgeTier = 'Iron' | 'Steel' | 'Mythril' | 'Monarch';

export interface Badge {
  category: QuestCategory;
  tier: BadgeTier;
  name: string;
  requiredCompletions: number;
  completionsInCategory: number;
  isEarned: boolean;
  progressPercent: number;
}

export interface Quest {
  id: string;
  title: string;
  description: string | null;
  category: QuestCategory;
  difficulty: QuestDifficulty;
  type: QuestType;
  verification: QuestVerification;
  xpReward: number;
  statReward: number;
  isCompleted: boolean;
  lastCompletedAtUtc: string | null;
}

export interface AuthResponse {
  token: string;
  hunterId: string;
}

export interface CreateQuestRequest {
  title: string;
  description?: string;
  category: QuestCategory;
  difficulty: QuestDifficulty;
  type: QuestType;
  verification?: QuestVerification;
}

export interface ConsistencyDay {
  date: string;
  completions: number;
  shielded: boolean;
}

export interface Consistency {
  currentStreak: number;
  longestStreak: number;
  shields: number;
  disciplineScore: number;
  days: ConsistencyDay[];
}

export interface CompleteQuestResult {
  xpGained: number;
  statCategory: QuestCategory;
  statGained: number;
  leveledUp: boolean;
  newLevel: number;
  currentXp: number;
  xpForNextLevel: number;
}
