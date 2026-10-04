// Task 8.8 - read claims (role, sub, name) straight out of the JWT. A JWT's
// payload is only Base64url-encoded, not encrypted (Week 6), so the client
// can read it. It can't CHANGE it, though: the server checks the signature
// on every request, which is why the role check that actually protects data
// stays on the server.
export function decodeTokenPayload(token) {
  try {
    const part = token.split('.')[1];
    const base64 = part.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const bytes = Uint8Array.from(atob(padded), (c) => c.charCodeAt(0));
    return JSON.parse(new TextDecoder().decode(bytes));
  } catch {
    return null;
  }
}

export function roleFromToken(token) {
  return decodeTokenPayload(token)?.role ?? null;
}
