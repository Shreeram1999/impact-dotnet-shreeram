import { useEffect, useMemo, useState } from 'react';
import { createStudent, deleteStudent, getStudents, updateStudent } from '../api/studentsApi';
import { describeError } from '../api/errors';
import { useAuth } from '../auth/AuthContext';
import SearchBox from '../components/SearchBox';
import StudentForm from '../components/StudentForm';
import StudentList from '../components/StudentList';

// Tasks 8.3/8.4/8.8 - the student list:
//   * fetched once on mount with useEffect (loading -> rows / empty / error);
//   * filtered live from a search query held in useState;
//   * Create/Edit/Delete rendered ONLY for the Teacher role. A Student sees
//     a read-only table. That's a convenience: the API independently returns
//     403 if a Student calls a write endpoint directly.
export default function StudentsPage() {
  const { isTeacher } = useAuth();
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(null);
  const [query, setQuery] = useState('');
  const [editing, setEditing] = useState(null); // null | 'new' | student
  const [formError, setFormError] = useState(null);
  const [saving, setSaving] = useState(false);
  const [notice, setNotice] = useState(null);

  useEffect(() => {
    // Ignore the response if the component unmounted (or React StrictMode
    // ran the effect twice) before it arrived.
    let active = true;
    getStudents()
      .then((data) => active && setStudents(data))
      .catch((error) => active && setLoadError(describeError(error)))
      .finally(() => active && setLoading(false));
    return () => {
      active = false;
    };
  }, []);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return students;
    return students.filter((s) => s.name.toLowerCase().includes(q) || s.rollNumber.toLowerCase().includes(q));
  }, [students, query]);

  const openForm = (target) => {
    setFormError(null);
    setNotice(null);
    setEditing(target);
  };

  const handleSave = async (values) => {
    setSaving(true);
    setFormError(null);
    try {
      if (editing === 'new') {
        const created = await createStudent(values);
        setStudents((current) => [...current, created]);
        setNotice(`Added ${created.name}.`);
      } else {
        const updated = await updateStudent(editing.id, values);
        setStudents((current) => current.map((s) => (s.id === updated.id ? updated : s)));
        setNotice(`Saved ${updated.name}.`);
      }
      setEditing(null);
    } catch (error) {
      setFormError(describeError(error));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (student) => {
    if (!window.confirm(`Delete ${student.name}?`)) return;
    setNotice(null);
    try {
      await deleteStudent(student.id);
      setStudents((current) => current.filter((s) => s.id !== student.id));
      setNotice(`Deleted ${student.name}.`);
    } catch (error) {
      setNotice(describeError(error));
    }
  };

  if (loading) {
    return (
      <p className="loading" role="status">
        Loading students...
      </p>
    );
  }

  if (loadError) {
    return (
      <p className="form-error" role="alert">
        {loadError}
      </p>
    );
  }

  return (
    <section>
      <div className="toolbar">
        <SearchBox value={query} onChange={setQuery} matchCount={filtered.length} total={students.length} />
        {isTeacher && editing === null && (
          <button type="button" onClick={() => openForm('new')}>
            Add student
          </button>
        )}
      </div>

      {notice && (
        <p className="notice" role="status">
          {notice}
        </p>
      )}

      {isTeacher && editing !== null && (
        <StudentForm
          key={editing === 'new' ? 'new' : editing.id}
          student={editing === 'new' ? null : editing}
          onSubmit={handleSave}
          onCancel={() => setEditing(null)}
          error={formError}
          saving={saving}
        />
      )}

      {!isTeacher && <p className="hint">You have read-only access.</p>}

      <StudentList students={filtered} canEdit={isTeacher} onEdit={openForm} onDelete={handleDelete} />
    </section>
  );
}
