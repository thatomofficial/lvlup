export type Rank = 'E' | 'D' | 'C' | 'B' | 'A' | 'S';

export type QuestCategory =
  | 'Strength'
  | 'Stamina'
  | 'Physique'
  | 'Looks'
  | 'WellBeing';

export type QuestDifficulty = 'Easy' | 'Medium' | 'Hard' | 'Elite';

export type QuestType = 'Daily' | 'OneTime';

export interface HunterStats {
  strength: number;
  stamina: number;
  physique: number;
  looks: number;
  wellBeing: number;
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
  stats: HunterStats;
}

export interface Quest {
  id: string;
  title: string;
  description: string | null;
  category: QuestCategory;
  difficulty: QuestDifficulty;
  type: QuestType;
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
