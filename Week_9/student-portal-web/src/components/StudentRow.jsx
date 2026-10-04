// Task 8.2 - one student, rendered purely from props. The Edit/Delete cell
// only exists when the parent says the current user may edit (Task 8.8).
export default function StudentRow({ student, canEdit = false, onEdit, onDelete }) {
  return (
    <tr>
      <td>{student.name}</td>
      <td>{student.rollNumber}</td>
      <td>{student.age}</td>
      <td>{student.email}</td>
      <td>{student.score}</td>
      <td>{student.enrolledOn ?? '-'}</td>
      {canEdit && (
        <td className="actions">
          <button type="button" onClick={() => onEdit(student)} aria-label={`Edit ${student.name}`}>
            Edit
          </button>
          <button type="button" className="danger" onClick={() => onDelete(student)} aria-label={`Delete ${student.name}`}>
            Delete
          </button>
        </td>
      )}
    </tr>
  );
}
