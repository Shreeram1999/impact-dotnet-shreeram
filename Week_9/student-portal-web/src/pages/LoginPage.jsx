import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { describeError } from '../api/errors';
import { useAuth } from '../auth/AuthContext';

// Task 8.6 - POSTs to api/auth/login through AuthContext, then sends the
// user back to wherever ProtectedRoute bounced them from (default /students).
export default function LoginPage() {
  const { login, sessionExpired } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [username, setUsername] = useState(location.state?.registered ?? '');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const canSubmit = username.trim() !== '' && password !== '' && !submitting;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await login(username.trim(), password);
      navigate(location.state?.from ?? '/students', { replace: true });
    } catch (err) {
      setError(describeError(err, { 401: 'Invalid username or password.' }));
      setSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <form className="card" onSubmit={handleSubmit} aria-label="Log in">
        <h1>Student Portal</h1>
        <h2>Log in</h2>
        {sessionExpired && (
          <p className="notice warning" role="status">
            Your session has expired. Please log in again.
          </p>
        )}
        {location.state?.registered && (
          <p className="notice" role="status">
            Registration successful - please log in.
          </p>
        )}
        <div className="field">
          <label htmlFor="username">Email or username</label>
          <input id="username" value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" />
        </div>
        <div className="field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
          />
        </div>
        {error && (
          <p className="form-error" role="alert">
            {error}
          </p>
        )}
        <button type="submit" disabled={!canSubmit}>
          {submitting ? 'Logging in...' : 'Log in'}
        </button>
        <p className="switch">
          New here? <Link to="/register">Create an account</Link>
        </p>
      </form>
    </main>
  );
}
