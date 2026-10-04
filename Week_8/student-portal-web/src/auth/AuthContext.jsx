import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import * as authApi from '../api/authApi';
import { setAuthToken } from '../api/client';
import { decodeTokenPayload } from './jwt';

const AuthContext = createContext(null);

// Task 8.7 - holds the session (token + who the token says you are) in
// React state, and hands the token to the axios interceptor.
export function AuthProvider({ children }) {
  const [session, setSession] = useState(null);

  const login = useCallback(async (username, password) => {
    const data = await authApi.login(username, password);
    const claims = decodeTokenPayload(data.token) ?? {};
    const next = {
      token: data.token,
      username: claims.sub ?? data.username,
      role: claims.role ?? data.role,
      displayName: claims.name ?? data.username,
    };
    setAuthToken(next.token); // before any state update, so the next request carries it
    setSession(next);
    return next;
  }, []);

  const logout = useCallback(() => {
    setAuthToken(null);
    setSession(null);
  }, []);

  const value = useMemo(
    () => ({
      token: session?.token ?? null,
      username: session?.username ?? null,
      displayName: session?.displayName ?? null,
      role: session?.role ?? null,
      isAuthenticated: session !== null,
      isTeacher: session?.role === 'Teacher',
      login,
      logout,
    }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside <AuthProvider>.');
  }
  return context;
}
