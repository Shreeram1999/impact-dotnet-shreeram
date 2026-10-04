import { act, render, renderHook, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { api, getAuthToken, setAuthToken, setRetryDelay, setUnauthorizedHandler, shouldRetry } from '../api/client';
import * as authApi from '../api/authApi';
import * as studentsApi from '../api/studentsApi';
import { AuthProvider, useAuth } from '../auth/AuthContext';
import ErrorBoundary from '../components/ErrorBoundary';
import App from '../App';
import { fakeJwt, httpError, sampleStudents } from './testUtils';

jest.mock('../api/authApi');

// A scripted network layer: each call takes the next outcome from the list.
// A number is an HTTP status (2xx resolves, anything else rejects the way
// axios does), 'network' means no response at all.
function scriptedAdapter(...outcomes) {
  return jest.fn(async (config) => {
    const next = outcomes.shift();
    if (next === 'network') {
      const error = new Error('Network Error');
      error.config = config;
      throw error;
    }
    if (next >= 200 && next < 300) {
      return { data: { ok: true }, status: next, statusText: 'OK', headers: {}, config };
    }
    const error = httpError(next);
    error.config = config;
    throw error;
  });
}

beforeAll(() => setRetryDelay(0));
afterEach(() => {
  setAuthToken(null);
  setUnauthorizedHandler(null);
});

describe('retry on a transient failure (Task 9.10)', () => {
  test('a GET that fails with 500 once is retried and succeeds', async () => {
    const adapter = scriptedAdapter(500, 200);
    api.defaults.adapter = adapter;

    const response = await api.get('/api/students');

    expect(response.status).toBe(200);
    expect(adapter).toHaveBeenCalledTimes(2);
  });

  test('a GET with no response (API restarting) is retried once', async () => {
    const adapter = scriptedAdapter('network', 200);
    api.defaults.adapter = adapter;

    await expect(api.get('/api/students')).resolves.toHaveProperty('status', 200);
    expect(adapter).toHaveBeenCalledTimes(2);
  });

  test('only ONE retry: a second failure is reported to the caller', async () => {
    const adapter = scriptedAdapter(503, 503);
    api.defaults.adapter = adapter;

    await expect(api.get('/api/students')).rejects.toHaveProperty('response.status', 503);
    expect(adapter).toHaveBeenCalledTimes(2);
  });

  test('a POST is never retried (it is not idempotent)', async () => {
    const adapter = scriptedAdapter(500, 200);
    api.defaults.adapter = adapter;

    await expect(api.post('/api/students', {})).rejects.toHaveProperty('response.status', 500);
    expect(adapter).toHaveBeenCalledTimes(1);
  });

  test('4xx responses are not retried', () => {
    expect(shouldRetry({ config: { method: 'get' }, response: { status: 404 } })).toBe(false);
    expect(shouldRetry({ response: { status: 500 } })).toBe(false); // no config to replay
  });
});

describe('401 handling (Task 9.2)', () => {
  test('a 401 from a data call fires the unauthorized handler', async () => {
    const handler = jest.fn();
    setUnauthorizedHandler(handler);
    api.defaults.adapter = scriptedAdapter(401);

    await expect(api.get('/api/students')).rejects.toHaveProperty('response.status', 401);
    expect(handler).toHaveBeenCalledTimes(1);
  });

  test('a 401 from the login call itself is just a wrong password', async () => {
    const handler = jest.fn();
    setUnauthorizedHandler(handler);
    api.defaults.adapter = scriptedAdapter(401);

    await expect(api.post('/api/auth/login', {})).rejects.toBeDefined();
    expect(handler).not.toHaveBeenCalled();
  });

  test('a 403 does not end the session', async () => {
    const handler = jest.fn();
    setUnauthorizedHandler(handler);
    api.defaults.adapter = scriptedAdapter(403);

    await expect(api.delete('/api/students/1')).rejects.toBeDefined();
    expect(handler).not.toHaveBeenCalled();
  });

  test('AuthProvider ends the session and flags it as expired; logging in again clears the flag', async () => {
    authApi.login.mockResolvedValue({ token: fakeJwt({ sub: 'teacher1', role: 'Teacher' }) });
    api.defaults.adapter = scriptedAdapter(401);
    const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });
    await act(() => result.current.login('teacher1', 'x'));

    await act(async () => {
      await api.get('/api/students').catch(() => {});
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.sessionExpired).toBe(true);
    expect(getAuthToken()).toBeNull();

    await act(() => result.current.login('teacher1', 'x'));
    expect(result.current.sessionExpired).toBe(false);
  });

  test('end to end in the app: an expired token mid-session lands on login with a notice', async () => {
    const user = userEvent.setup();
    authApi.login.mockResolvedValue({ token: fakeJwt({ sub: 'teacher1', role: 'Teacher', name: 'Meera Iyer' }) });
    jest.spyOn(studentsApi, 'getStudents').mockResolvedValue(sampleStudents);
    api.defaults.adapter = scriptedAdapter(401);
    render(
      <MemoryRouter initialEntries={['/students']}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MemoryRouter>,
    );

    await user.type(screen.getByLabelText('Email or username'), 'teacher1');
    await user.type(screen.getByLabelText('Password'), 'x');
    await user.click(screen.getByRole('button', { name: 'Log in' }));
    await screen.findByText('Asha Kumar');

    jest.spyOn(window, 'confirm').mockReturnValue(true);
    await user.click(screen.getByRole('button', { name: 'Delete Asha Kumar' })); // real deleteStudent -> 401

    expect(await screen.findByText('Your session has expired. Please log in again.')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Log in' })).toBeInTheDocument();
    window.confirm.mockRestore();
    studentsApi.getStudents.mockRestore();
  });
});

describe('ErrorBoundary (Task 9.2)', () => {
  function Broken() {
    throw new Error('render bug');
  }

  test('a render error shows a recoverable message instead of a blank page', async () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});
    const onReload = jest.fn();

    render(
      <ErrorBoundary onReload={onReload}>
        <Broken />
      </ErrorBoundary>,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Something went wrong.');
    await userEvent.click(screen.getByRole('button', { name: 'Reload' }));
    expect(onReload).toHaveBeenCalled();
    console.error.mockRestore();
  });

  test('children render normally when nothing throws', () => {
    render(
      <ErrorBoundary>
        <p>fine</p>
      </ErrorBoundary>,
    );

    expect(screen.getByText('fine')).toBeInTheDocument();
  });
});
