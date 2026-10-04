import { api, attachToken, getAuthToken, setAuthToken } from '../api/client';
import * as authApi from '../api/authApi';
import * as studentsApi from '../api/studentsApi';
import { describeError } from '../api/errors';
import { httpError } from './testUtils';

// Replaces axios's network layer: every request "succeeds" and echoes back
// what was sent, so tests can see exactly what left the client.
function echoAdapter(responseData) {
  return jest.fn(async (config) => ({
    data: responseData ?? { url: config.url, method: config.method, headers: config.headers, body: config.data },
    status: 200,
    statusText: 'OK',
    headers: {},
    config,
  }));
}

afterEach(() => setAuthToken(null));

describe('axios interceptor (Task 8.7)', () => {
  test('attaches the Bearer token to every request once logged in', async () => {
    api.defaults.adapter = echoAdapter();
    setAuthToken('abc.def.ghi');

    const first = await api.get('/api/students');
    const second = await api.delete('/api/students/1');

    expect(first.data.headers.Authorization).toBe('Bearer abc.def.ghi');
    expect(second.data.headers.Authorization).toBe('Bearer abc.def.ghi');
    expect(getAuthToken()).toBe('abc.def.ghi');
  });

  test('sends no Authorization header when logged out', async () => {
    api.defaults.adapter = echoAdapter();

    const response = await api.get('/api/students');

    expect(response.data.headers.Authorization).toBeUndefined();
  });

  test('attachToken leaves the config untouched without a token', () => {
    const config = { headers: {} };

    expect(attachToken(config)).toBe(config);
    expect(config.headers).toEqual({});
  });

  test('targets the API base URL', () => {
    expect(api.defaults.baseURL).toBe('http://localhost:5095');
  });
});

describe('API wrappers', () => {
  test('studentsApi hits the right verbs and routes', async () => {
    const adapter = echoAdapter();
    api.defaults.adapter = adapter;

    await studentsApi.getStudents();
    await studentsApi.createStudent({ name: 'A' });
    const updated = await studentsApi.updateStudent(7, { name: 'B' });
    await studentsApi.deleteStudent(7);

    const calls = adapter.mock.calls.map(([config]) => `${config.method.toUpperCase()} ${config.url}`);
    expect(calls).toEqual(['GET /api/students', 'POST /api/students', 'PUT /api/students/7', 'DELETE /api/students/7']);
    expect(updated).toEqual({ name: 'B', id: 7 });
  });

  test('authApi posts to login and register and returns the body', async () => {
    api.defaults.adapter = echoAdapter({ token: 't' });

    expect(await authApi.login('u', 'p')).toEqual({ token: 't' });
    expect(await authApi.register({ email: 'e' })).toEqual({ token: 't' });
  });
});

describe('describeError', () => {
  test.each([
    [400, { errors: { Email: ['The Email field is not a valid e-mail address.'] } }, 'The Email field is not a valid e-mail address.'],
    [400, {}, 'Please check the form and try again.'],
    [401, '', 'Your session has expired. Please log in again.'],
    [403, '', "You don't have permission to do that."],
    [404, '', 'That record no longer exists.'],
    [409, 'Roll number taken.', 'Roll number taken.'],
    [409, '', 'That conflicts with an existing record.'],
    [500, '', 'The server hit a problem. Please try again.'],
  ])('status %i -> friendly message', (status, data, expected) => {
    expect(describeError(httpError(status, data))).toBe(expected);
  });

  test('no response at all means the server is unreachable', () => {
    expect(describeError(new Error('Network Error'))).toBe("Can't reach the server. Is the API running?");
  });

  test('page-specific overrides win', () => {
    expect(describeError(httpError(401), { 401: 'Invalid username or password.' })).toBe('Invalid username or password.');
  });
});
