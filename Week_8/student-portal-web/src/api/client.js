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

api.interceptors.request.use(attachToken);
