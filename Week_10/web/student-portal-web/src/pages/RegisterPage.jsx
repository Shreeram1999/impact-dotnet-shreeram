import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { register } from '../api/authApi';
import { describeError } from '../api/errors';
import { isValid, validateRegistration } from '../validation';

const EMPTY = { name: '', dateOfBirth: '', designation: 'Student', email: '', password: '' };

// Task 8.5 - registration as controlled inputs with inline validation.
// Errors show once a field has been visited, and Register stays disabled
// until the whole form is valid. On success the user goes to /login with
// their email pre-filled.
export default function RegisterPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState(EMPTY);
  const [touched, setTouched] = useState({});
  const [serverError, setServerError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const errors = validateRegistration(form);
  const update = (field) => (event) => setForm({ ...form, [field]: event.target.value });
  const blur = (field) => () => setTouched({ ...touched, [field]: true });
  const showError = (field) => touched[field] && errors[field];

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setServerError(null);
    try {
      const created = await register({ ...form, name: form.name.trim(), email: form.email.trim() });
      navigate('/login', { state: { registered: created.username } });
    } catch (error) {
      setServerError(
        describeError(error, {
          403: "Teacher accounts can't be self-registered. Register as a Student or ask an administrator.",
          409: 'An account with that email already exists.',
        }),
      );
      setSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <form className="card" onSubmit={handleSubmit} aria-label="Register" noValidate>
        <h1>Student Portal</h1>
        <h2>Create an account</h2>

        <div className="field">
          <label htmlFor="name">Name</label>
          <input id="name" value={form.name} onChange={update('name')} onBlur={blur('name')} />
          {showError('name') && <span className="field-error">{errors.name}</span>}
        </div>

        <div className="field">
          <label htmlFor="dateOfBirth">Date of birth</label>
          <input id="dateOfBirth" type="date" value={form.dateOfBirth} onChange={update('dateOfBirth')} onBlur={blur('dateOfBirth')} />
          {showError('dateOfBirth') && <span className="field-error">{errors.dateOfBirth}</span>}
        </div>

        <div className="field">
          <label htmlFor="designation">Designation</label>
          <select id="designation" value={form.designation} onChange={update('designation')} onBlur={blur('designation')}>
            <option value="Student">Student</option>
            <option value="Teacher">Teacher</option>
          </select>
          {showError('designation') && <span className="field-error">{errors.designation}</span>}
        </div>

        <div className="field">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" value={form.email} onChange={update('email')} onBlur={blur('email')} autoComplete="email" />
          {showError('email') && <span className="field-error">{errors.email}</span>}
        </div>

        <div className="field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={form.password}
            onChange={update('password')}
            onBlur={blur('password')}
            autoComplete="new-password"
          />
          {showError('password') && <span className="field-error">{errors.password}</span>}
        </div>

        {serverError && (
          <p className="form-error" role="alert">
            {serverError}
          </p>
        )}

        <button type="submit" disabled={!isValid(errors) || submitting}>
          {submitting ? 'Registering...' : 'Register'}
        </button>
        <p className="switch">
          Already registered? <Link to="/login">Log in</Link>
        </p>
      </form>
    </main>
  );
}
