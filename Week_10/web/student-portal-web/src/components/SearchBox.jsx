// Task 8.3 - a controlled input: the parent owns the query in useState and
// re-filters on every keystroke; this component only reports changes.
export default function SearchBox({ value, onChange, matchCount, total }) {
  return (
    <div className="search">
      <label htmlFor="search">Search students</label>
      <input
        id="search"
        type="search"
        placeholder="Name or roll number"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
      <span className="match-count" aria-live="polite">
        {matchCount} of {total} {total === 1 ? 'student' : 'students'}
      </span>
    </div>
  );
}
