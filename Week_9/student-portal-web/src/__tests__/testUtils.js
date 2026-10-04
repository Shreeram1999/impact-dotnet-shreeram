// Builds an unsigned JWT-shaped string with the given claims - enough for
// the client, which only ever READS the payload (verification is the
// server's job).
export function fakeJwt(claims) {
  const encode = (value) =>
    Buffer.from(JSON.stringify(value)).toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(claims)}.signature`;
}

// The shape axios rejects with when the server answers with an error status.
export function httpError(status, data = '') {
  const error = new Error(`Request failed with status code ${status}`);
  error.response = { status, data };
  return error;
}

export const sampleStudents = [
  { id: 1, name: 'Asha Kumar', rollNumber: 'R001', age: 20, email: 'asha@school.example', score: 82, enrolledOn: '2026-06-01' },
  { id: 2, name: 'Rohit Menon', rollNumber: 'R002', age: 21, email: 'rohit@school.example', score: 64, enrolledOn: null },
  { id: 3, name: 'Divya Nair', rollNumber: 'R003', age: 19, email: 'divya@school.example', score: 91, enrolledOn: '2026-06-15' },
];
