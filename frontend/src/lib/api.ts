import { API_BASE_URL } from './env';
import type {
  AppConfig,
  AssessmentResult,
  AssessmentScores,
  AuthResponse,
  Badge,
  CompleteQuestResult,
  Consistency,
  CreateQuestRequest,
  DisplayNamePreference,
  Hunter,
  Quest,
  StarterPackResult,
} from './types';

/** RFC 7807 validation error entry. */
export interface ApiValidationError {
  code: string;
  description: string;
}

/** RFC 7807 problem details payload returned by the backend on errors. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: ApiValidationError[];
}

/**
 * Parses an unknown response body into ProblemDetails.
 * Returns null when the body is not an object (e.g. HTML error pages).
 */
export function parseProblemDetails(body: unknown): ProblemDetails | null {
  if (typeof body !== 'object' || body === null || Array.isArray(body)) {
    return null;
  }
  const raw = body as Record<string, unknown>;
  const problem: ProblemDetails = {};

  if (typeof raw.type === 'string') problem.type = raw.type;
  if (typeof raw.title === 'string') problem.title = raw.title;
  if (typeof raw.status === 'number') problem.status = raw.status;
  if (typeof raw.detail === 'string') problem.detail = raw.detail;

  if (Array.isArray(raw.errors)) {
    const errors: ApiValidationError[] = [];
    for (const entry of raw.errors) {
      if (typeof entry === 'object' && entry !== null) {
        const e = entry as Record<string, unknown>;
        if (typeof e.description === 'string') {
          errors.push({
            code: typeof e.code === 'string' ? e.code : '',
            description: e.description,
          });
        }
      }
    }
    if (errors.length > 0) {
      problem.errors = errors;
    }
  }

  return problem;
}

/**
 * Builds a user-facing message from a problem-details payload.
 * Validation errors win (all descriptions, one per line), then detail,
 * then title, then a generic status fallback.
 */
export function problemToMessage(
  problem: ProblemDetails | null,
  status: number,
): string {
  if (problem?.errors && problem.errors.length > 0) {
    return problem.errors.map((e) => e.description).join('\n');
  }
  if (problem?.detail) {
    return problem.detail;
  }
  if (problem?.title) {
    return problem.title;
  }
  return `Request failed with status ${status}`;
}

/** Error thrown for any non-2xx API response. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;

  constructor(status: number, problem: ProblemDetails | null) {
    super(problemToMessage(problem, status));
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  token?: string | null;
}

async function request<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const { method = 'GET', body, token } = options;

  const headers: Record<string, string> = {};
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    let problem: ProblemDetails | null = null;
    try {
      problem = parseProblemDetails(await response.json());
    } catch {
      problem = null;
    }
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

/** React Native file descriptor accepted by FormData. */
export interface UploadFile {
  uri: string;
  name: string;
  type: string;
}

async function uploadFile<T>(
  path: string,
  token: string,
  file: UploadFile,
): Promise<T> {
  const form = new FormData();
  form.append('file', file as unknown as Blob);

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
    body: form,
  });

  if (!response.ok) {
    let problem: ProblemDetails | null = null;
    try {
      problem = parseProblemDetails(await response.json());
    } catch {
      problem = null;
    }
    throw new ApiError(response.status, problem);
  }

  return (await response.json()) as T;
}

export const api = {
  register(
    email: string,
    password: string,
    name: string,
    surname: string,
    username: string,
  ): Promise<AuthResponse> {
    return request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: { email, password, name, surname, username },
    });
  },

  login(email: string, password: string): Promise<AuthResponse> {
    return request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: { email, password },
    });
  },

  loginWithGoogle(idToken: string): Promise<AuthResponse> {
    return request<AuthResponse>('/auth/sso/google', {
      method: 'POST',
      body: { idToken },
    });
  },

  getAppConfig(): Promise<AppConfig> {
    return request<AppConfig>('/app/config');
  },

  getMe(token: string): Promise<Hunter> {
    return request<Hunter>('/hunters/me', { token });
  },

  updateDisplayPreference(
    token: string,
    preference: DisplayNamePreference,
  ): Promise<void> {
    return request<void>('/hunters/me/display-preference', {
      method: 'PUT',
      body: { preference },
      token,
    });
  },

  submitAssessment(
    token: string,
    scores: AssessmentScores,
  ): Promise<AssessmentResult> {
    return request<AssessmentResult>('/hunters/me/assessment', {
      method: 'POST',
      body: { scores },
      token,
    });
  },

  createStarterPack(token: string): Promise<StarterPackResult> {
    return request<StarterPackResult>('/quests/starter-pack', {
      method: 'POST',
      token,
    });
  },

  getQuests(token: string): Promise<Quest[]> {
    return request<Quest[]>('/quests', { token });
  },

  createQuest(token: string, quest: CreateQuestRequest): Promise<{ id: string }> {
    return request<{ id: string }>('/quests', {
      method: 'POST',
      body: quest,
      token,
    });
  },

  completeQuest(
    token: string,
    questId: string,
    note?: string,
  ): Promise<CompleteQuestResult> {
    return request<CompleteQuestResult>(`/quests/${questId}/complete`, {
      method: 'POST',
      body: { note: note ?? null },
      token,
    });
  },

  getConsistency(token: string): Promise<Consistency> {
    return request<Consistency>('/hunters/me/consistency', { token });
  },

  getBadges(token: string): Promise<Badge[]> {
    return request<Badge[]>('/hunters/me/badges', { token });
  },

  uploadAvatar(token: string, file: UploadFile): Promise<{ avatarUrl: string }> {
    return uploadFile<{ avatarUrl: string }>('/hunters/me/avatar', token, file);
  },

  updateGitHubUsername(token: string, username: string | null): Promise<void> {
    return request<void>('/hunters/me/github', {
      method: 'PUT',
      body: { username },
      token,
    });
  },

  deleteQuest(token: string, questId: string): Promise<void> {
    return request<void>(`/quests/${questId}`, {
      method: 'DELETE',
      token,
    });
  },
};
