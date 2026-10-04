import { useState } from 'react';
import { isValid, validateStudent } from '../validation';

const EMPTY = { name: '', age: '', rollNumber: '', email: '', score: '', enrolledOn: '' };

function toForm(student) {
  if (!student) return EMPTY;
  return {
    name: student.name,
    age: String(student.age),
    rollNumber: student.rollNumber,
    email: student.email,
    score: String(student.score),
    enrolledOn: student.enrolledOn ?? '',
  };
}

// Teacher-only create/edit form (rendered by StudentsPage only for the
// Teacher role). Controlled inputs, inline validation, Save disabled until
// the input is valid. A server-side rejection (e.g. 409 duplicate roll
// number) arrives through the `error` prop.
export default function StudentForm({ student, onSubmit, onCancel, error, saving = false }) {
  const [form, setForm] = useState(() => toForm(student));
  const [touched, setTouched] = useState({});
  const errors = validateStudent(form);

  const update = (field) => (event) => setForm({ ...form, [field]: event.target.value });
  const blur = (field) => () => setTouched({ ...touched, [field]: true });

  const handleSubmit = (event) => {
    event.preventDefault();
    onSubmit({
      name: form.name.trim(),
      age: Number(form.age),
      rollNumber: form.rollNumber.trim(),
      email: form.email.trim(),
      score: Number(form.score),
      enrolledOn: form.enrolledOn || null,
    });
  };

  const field = (name, label, type = 'text') => (
    <div className="field">
      <label htmlFor={`student-${name}`}>{label}</label>
      <input
        id={`student-${name}`}
        type={type}
        value={form[name]}
        onChange={update(name)}
        onBlur={blur(name)}
        aria-invalid={touched[name] && errors[name] ? 'true' : 'false'}
      />
      {touched[name] && errors[name] && <span className="field-error">{errors[name]}</span>}
    </div>
  );

  return (
    <form className="card student-form" onSubmit={handleSubmit} aria-label={student ? 'Edit student' : 'Add student'}>
      <h2>{student ? `Edit ${student.name}` : 'Add student'}</h2>
      {field('name', 'Name')}
      {field('age', 'Age', 'number')}
      {field('rollNumber', 'Roll number')}
      {field('email', 'Email', 'email')}
      {field('score', 'Score', 'number')}
      {field('enrolledOn', 'Enrolled on', 'date')}
      {error && (
        <p className="form-error" role="alert">
          {error}
        </p>
      )}
      <div className="buttons">
        <button type="submit" disabled={!isValid(errors) || saving}>
          {saving ? 'Saving...' : 'Save'}
        </button>
        <button type="button" className="secondary" onClick={onCancel}>
          Cancel
        </button>
      </div>
    </form>
  );
}
