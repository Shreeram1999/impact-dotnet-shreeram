import { api } from './client';

export async function login(username, password) {
  const { data } = await api.post('/identity/api/auth/login', { username, password });
  return data; // { token, expiresAtUtc, username, role }
}

export async function register(form) {
  const { data } = await api.post('/identity/api/auth/register', form);
  return data; // { username, role, displayName }
}
