import { useEffect, useState } from 'react';
import { getEnrollmentSummary, getTerms } from '../api/reportsApi';
import { describeError } from '../api/errors';

const UNAVAILABLE = new Set([502, 503, 504]);

// Task 10.4 / 10.10 - the reporting view. Data comes from the Reporting
// service (EF DB First over the pre-existing warehouse). If that one
// service is down, the gateway answers 502 and this page says so, while the
// Students page (Academics) and login (Identity) keep working.
export default function ReportsPage() {
  const [terms, setTerms] = useState([]);
  const [termId, setTermId] = useState(null);
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(null);
    Promise.all([terms.length ? Promise.resolve(terms) : getTerms(), getEnrollmentSummary(termId)])
      .then(([loadedTerms, loadedSummary]) => {
        if (!active) return;
        setTerms(loadedTerms);
        setSummary(loadedSummary);
      })
      .catch((err) => {
        if (!active) return;
        setError(
          UNAVAILABLE.has(err?.response?.status) || !err?.response
            ? 'Reporting is temporarily unavailable. Students and courses still work - try the reports again later.'
            : describeError(err),
        );
      })
      .finally(() => active && setLoading(false));
    return () => {
      active = false;
    };
    // `terms` is only a cache: the effect re-runs when the selected term changes.
  }, [termId]);

  if (loading) {
    return (
      <p className="loading" role="status">
        Loading reports...
      </p>
    );
  }

  if (error) {
    return (
      <p className="form-error" role="alert">
        {error}
      </p>
    );
  }

  return (
    <section>
      <div className="toolbar">
        <div className="search">
          <label htmlFor="term">Term</label>
          <select id="term" value={summary.term.termId} onChange={(e) => setTermId(Number(e.target.value))}>
            {terms.map((t) => (
              <option key={t.termId} value={t.termId}>
                {t.name}
              </option>
            ))}
          </select>
        </div>
        <p className="match-count">
          {summary.totalEnrolled} enrolments, {summary.totalCompleted} completed
        </p>
      </div>

      <h2>Enrollment summary - {summary.term.name}</h2>
      {summary.courses.length === 0 ? (
        <p className="empty" role="status">
          No enrolments recorded for this term
        </p>
      ) : (
        <table className="students" aria-label="Enrollment summary">
          <thead>
            <tr>
              <th>Course</th>
              <th>Title</th>
              <th>Department</th>
              <th>Enrolled</th>
              <th>Completed</th>
              <th>Average</th>
              <th>Pass rate</th>
            </tr>
          </thead>
          <tbody>
            {summary.courses.map((c) => (
              <tr key={c.courseCode}>
                <td>{c.courseCode}</td>
                <td>{c.title}</td>
                <td>{c.department}</td>
                <td>{c.enrolled}</td>
                <td>{c.completed}</td>
                <td>{c.averageScore ?? '-'}</td>
                <td>{c.passRate == null ? '-' : `${c.passRate}%`}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
