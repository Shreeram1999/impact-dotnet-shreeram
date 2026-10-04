import StudentRow from './StudentRow';

// Task 8.2/8.4 - maps the array to rows, keyed by the database Id (stable
// across re-renders and filtering, unlike an array index), with an explicit
// empty state.
export default function StudentList({ students, canEdit = false, onEdit, onDelete }) {
  if (students.length === 0) {
    return (
      <p className="empty" role="status">
        No students found
      </p>
    );
  }

  return (
    <table className="students">
      <thead>
        <tr>
          <th>Name</th>
          <th>Roll number</th>
          <th>Age</th>
          <th>Email</th>
          <th>Score</th>
          <th>Enrolled on</th>
          {canEdit && <th>Actions</th>}
        </tr>
      </thead>
      <tbody>
        {students.map((student) => (
          <StudentRow key={student.id} student={student} canEdit={canEdit} onEdit={onEdit} onDelete={onDelete} />
        ))}
      </tbody>
    </table>
  );
}
