import { API_BASE_URL } from '../../constants/api';
import {
  api,
  ApiError,
  parseProblemDetails,
  problemToMessage,
} from '../api';

function jsonResponse(status: number, body: unknown): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(body),
  } as unknown as Response;
}

function nonJsonResponse(status: number): Response {
  return {
    ok: false,
    status,
    json: () => Promise.reject(new SyntaxError('Unexpected token <')),
  } as unknown as Response;
}

describe('parseProblemDetails', () => {
  it('parses a full problem-details payload', () => {
    const problem = parseProblemDetails({
      type: 'https://example.com/errors/validation',
      title: 'Validation failed',
      status: 400,
      detail: 'One or more validation errors occurred.',
      errors: [
        { code: 'PasswordTooShort', description: 'Password min 8 chars' },
        { code: 'EmailInvalid', description: 'Email is invalid' },
      ],
    });

    expect(problem).toEqual({
      type: 'https://example.com/errors/validation',
      title: 'Validation failed',
      status: 400,
      detail: 'One or more validation errors occurred.',
      errors: [
        { code: 'PasswordTooShort', description: 'Password min 8 chars' },
        { code: 'EmailInvalid', description: 'Email is invalid' },
      ],
    });
  });

  it('ignores malformed error entries', () => {
    const problem = parseProblemDetails({
      title: 'Bad request',
      errors: [{ description: 'valid entry' }, 'garbage', { code: 'NoDesc' }],
    });

    expect(problem?.errors).toEqual([{ code: '', description: 'valid entry' }]);
  });

  it('returns null for non-object bodies', () => {
    expect(parseProblemDetails('oops')).toBeNull();
    expect(parseProblemDetails(null)).toBeNull();
    expect(parseProblemDetails([1, 2])).toBeNull();
    expect(parseProblemDetails(42)).toBeNull();
  });
});

describe('problemToMessage', () => {
  it('prefers validation error descriptions, one per line', () => {
    expect(
      problemToMessage(
        {
          detail: 'ignored',
          errors: [
            { code: 'A', description: 'First problem' },
            { code: 'B', description: 'Second problem' },
          ],
        },
        400,
      ),
    ).toBe('First problem\nSecond problem');
  });

  it('falls back to detail, then title', () => {
    expect(problemToMessage({ detail: 'Invalid credentials.' }, 401)).toBe(
      'Invalid credentials.',
    );
    expect(problemToMessage({ title: 'Unauthorized' }, 401)).toBe(
      'Unauthorized',
    );
  });

  it('falls back to a generic message when nothing is usable', () => {
    expect(problemToMessage(null, 500)).toBe('Request failed with status 500');
    expect(problemToMessage({}, 503)).toBe('Request failed with status 503');
  });
});

describe('api client', () => {
  const fetchMock = jest.fn();

  beforeEach(() => {
    fetchMock.mockReset();
    global.fetch = fetchMock as unknown as typeof fetch;
  });

  it('POSTs credentials to /auth/login and returns the auth payload', async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(200, { token: 'jwt-123', hunterId: 'h-1' }),
    );

    const result = await api.login('hunter@guild.io', 'password123');

    expect(result).toEqual({ token: 'jwt-123', hunterId: 'h-1' });
    expect(fetchMock).toHaveBeenCalledWith(
      `${API_BASE_URL}/auth/login`,
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
        }),
        body: JSON.stringify({
          email: 'hunter@guild.io',
          password: 'password123',
        }),
      }),
    );
  });

  it('sends the bearer token on authenticated requests', async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, []));

    await api.getQuests('my-token');

    expect(fetchMock).toHaveBeenCalledWith(
      `${API_BASE_URL}/quests`,
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer my-token',
        }),
      }),
    );
  });

  it('throws ApiError with joined validation descriptions on 400', async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(400, {
        type: 'about:blank',
        title: 'Validation failed',
        status: 400,
        errors: [
          { code: 'PasswordTooShort', description: 'Password min 8 chars' },
          { code: 'NameRequired', description: 'Name is required' },
        ],
      }),
    );

    const promise = api.register('a@b.io', 'short', '');
    await expect(promise).rejects.toBeInstanceOf(ApiError);

    try {
      await api.register('a@b.io', 'short', '');
    } catch (e) {
      const err = e as ApiError;
      expect(err.status).toBe(400);
      expect(err.message).toBe('Password min 8 chars\nName is required');
      expect(err.problem?.errors).toHaveLength(2);
    }
  });

  it('surfaces problem detail for 401 invalid credentials', async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(401, {
        type: 'about:blank',
        title: 'Unauthorized',
        status: 401,
        detail: 'Invalid email or password.',
      }),
    );

    await expect(api.login('a@b.io', 'wrong-pass')).rejects.toThrow(
      'Invalid email or password.',
    );
  });

  it('surfaces a 409 conflict when a quest was already completed', async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(409, {
        title: 'Conflict',
        status: 409,
        detail: 'Quest already completed today.',
      }),
    );

    try {
      await api.completeQuest('token', 'quest-1');
      throw new Error('expected ApiError');
    } catch (e) {
      const err = e as ApiError;
      expect(err).toBeInstanceOf(ApiError);
      expect(err.status).toBe(409);
      expect(err.message).toBe('Quest already completed today.');
    }
  });

  it('falls back to a generic message when the error body is not JSON', async () => {
    fetchMock.mockResolvedValue(nonJsonResponse(500));

    await expect(api.getMe('token')).rejects.toThrow(
      'Request failed with status 500',
    );
  });

  it('returns undefined for 204 responses (delete quest)', async () => {
    fetchMock.mockResolvedValue({
      ok: true,
      status: 204,
      json: () => Promise.reject(new Error('no body')),
    } as unknown as Response);

    await expect(api.deleteQuest('token', 'quest-1')).resolves.toBeUndefined();

    expect(fetchMock).toHaveBeenCalledWith(
      `${API_BASE_URL}/quests/quest-1`,
      expect.objectContaining({ method: 'DELETE' }),
    );
  });
});
