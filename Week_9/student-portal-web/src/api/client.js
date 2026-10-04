import axios from 'axios';
import { API_BASE_URL } from '../config';

// Task 8.7 - the JWT lives in memory only: here (for the interceptor) and in
// AuthContext (for the UI). It isn't put in localStorage, where any injected
// script could read it. The cost is that a page refresh logs you out, which
// is acceptable for this portal.
let currentToken = null;

export function setAuthToken(token) {
  currentToken = token;
}

export function getAuthToken() {
  return currentToken;
}

// Task 9.2 - who to tell when the API says the token is no longer valid
// (expired or revoked): AuthContext registers a handler that ends the
// session, and ProtectedRoute then sends the user to /login.
let unauthorizedHandler = null;

export function setUnauthorizedHandler(handler) {
  unauthorizedHandler = handler;
}

let retryDelayMs = 300;

export function setRetryDelay(ms) {
  retryDelayMs = ms;
}

// One shared axios instance for every API call.
export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Task 8.7 - the request interceptor: every request made through `api`
// carries `Authorization: Bearer <token>` once the user has logged in, so no
// individual call has to remember to add it.
export function attachToken(config) {
  if (currentToken) {
    config.headers.Authorization = `Bearer ${currentToken}`;
  }
  return config;
}

// Task 9.10 (stretch) - resilience: a GET that fails with a 5xx or no
// response at all is retried ONCE after a short pause, which rides out a
// momentary blip (e.g. the API restarting). Only GETs are retried, because
// they're idempotent. Retrying a POST could create the same student twice.
export function shouldRetry(error) {
  const config = error.config;
  if (!config || config.retried) return false;
  if ((config.method ?? 'get').toLowerCase() !== 'get') return false;
  return !error.response || error.response.status >= 500;
}

export async function handleResponseError(error) {
  if (shouldRetry(error)) {
    error.config.retried = true;
    await new Promise((resolve) => setTimeout(resolve, retryDelayMs));
    return api.request(error.config);
  }

  // Task 9.2 - a 401 on anything except the login/register calls themselves
  // means the session is over. (A 401 from /api/auth/login just means a
  // wrong password, and the login form handles that.)
  const isAuthCall = (error.config?.url ?? '').startsWith('/api/auth/');
  if (error.response?.status === 401 && !isAuthCall && unauthorizedHandler) {
    unauthorizedHandler();
  }

  return Promise.reject(error);
}

api.interceptors.request.use(attachToken);
api.interceptors.response.use((response) => response, handleResponseError);
