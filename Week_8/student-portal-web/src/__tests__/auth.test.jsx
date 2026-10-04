import { act, render, renderHook, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useNavigate } from 'react-router-dom';
import userEvent from '@testing-library/user-event';
import { AuthProvider, useAuth } from '../auth/AuthContext';
import ProtectedRoute from '../auth/ProtectedRoute';
import { decodeTokenPayload, roleFromToken } from '../auth/jwt';
import * as authApi from '../api/authApi';
import { getAuthToken, setAuthToken } from '../api/client';
import { fakeJwt } from './testUtils';

jest.mock('../api/authApi');

afterEach(() => setAuthToken(null));

describe('jwt helpers (Task 8.8)', () => {
  test('reads the role and UTF-8 claims from the payload', () => {
    const token = fakeJwt({ sub: 'teacher1', role: 'Teacher', name: 'Meera Iyer ✓' });

    expect(roleFromToken(token)).toBe('Teacher');
    expect(decodeTokenPayload(token).name).toBe('Meera Iyer ✓');
  });

  test.each([['not-a-jwt'], [''], [null]])('returns null for %p', (token) => {
    expect(decodeTokenPayload(token)).toBeNull();
    expect(roleFromToken(token)).toBeNull();
  });
});

describe('AuthContext (Task 8.7)', () => {
  const wrapper = ({ children }) => <AuthProvider>{children}</AuthProvider>;

  test('login stores the token for the interceptor and exposes the role from the token', async () => {
    authApi.login.mockResolvedValue({ token: fakeJwt({ sub: 'teacher1', role: 'Teacher', name: 'Meera Iyer' }), username: 'teacher1', role: 'Teacher' });
    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isAuthenticated).toBe(false);
    await act(() => result.current.login('teacher1', 'Teacher@123'));

    expect(authApi.login).toHaveBeenCalledWith('teacher1', 'Teacher@123');
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.isTeacher).toBe(true);
    expect(result.current.displayName).toBe('Meera Iyer');
    expect(getAuthToken()).toBe(result.current.token);
  });

  test('falls back to the response body if the token cannot be decoded', async () => {
    authApi.login.mockResolvedValue({ token: 'opaque', username: 'student1', role: 'Student' });
    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(() => result.current.login('student1', 'x'));

    expect(result.current.role).toBe('Student');
    expect(result.current.isTeacher).toBe(false);
    expect(result.current.username).toBe('student1');
  });

  test('a failed login leaves you logged out', async () => {
    authApi.login.mockRejectedValue(new Error('401'));
    const { result } = renderHook(() => useAuth(), { wrapper });

    await expect(act(() => result.current.login('x', 'y'))).rejects.toThrow('401');
    expect(result.current.isAuthenticated).toBe(false);
    expect(getAuthToken()).toBeNull();
  });

  test('logout clears the session and the interceptor token', async () => {
    authApi.login.mockResolvedValue({ token: fakeJwt({ sub: 'a', role: 'Student' }) });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await act(() => result.current.login('a', 'b'));

    act(() => result.current.logout());

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.role).toBeNull();
    expect(getAuthToken()).toBeNull();
  });

  test('useAuth outside the provider is a programming error', () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});
    expect(() => renderHook(() => useAuth())).toThrow('useAuth must be used inside <AuthProvider>.');
    console.error.mockRestore();
  });
});

describe('ProtectedRoute (Task 8.6)', () => {
  function LogInThenVisitStudents() {
    const { login } = useAuth();
    const navigate = useNavigate();
    return (
      <button type="button" onClick={async () => { await login('a', 'b'); navigate('/students'); }}>
        log in
      </button>
    );
  }

  function renderAt(path) {
    return render(
      <MemoryRouter initialEntries={[path]}>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LogInThenVisitStudents />} />
            <Route path="/students" element={<ProtectedRoute><p>secret students</p></ProtectedRoute>} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );
  }

  test('visiting /students without a token redirects to /login', () => {
    renderAt('/students');

    expect(screen.getByRole('button', { name: 'log in' })).toBeInTheDocument();
    expect(screen.queryByText('secret students')).not.toBeInTheDocument();
  });

  test('after logging in, the protected page renders', async () => {
    authApi.login.mockResolvedValue({ token: fakeJwt({ sub: 'a', role: 'Student' }) });
    renderAt('/login');

    await userEvent.click(screen.getByRole('button', { name: 'log in' }));

    expect(await screen.findByText('secret students')).toBeInTheDocument();
  });
});
