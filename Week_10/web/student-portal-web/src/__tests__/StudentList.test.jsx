import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import StudentList from '../components/StudentList';
import SearchBox from '../components/SearchBox';
import { sampleStudents } from './testUtils';

describe('StudentList', () => {
  test('renders one row per student from props', () => {
    render(<StudentList students={sampleStudents} />);

    const rows = screen.getAllByRole('row').slice(1); // skip the header row
    expect(rows).toHaveLength(3);
    expect(within(rows[0]).getByText('Asha Kumar')).toBeInTheDocument();
    expect(within(rows[1]).getByText('-')).toBeInTheDocument(); // null enrolledOn
  });

  test('shows the empty state when there are no students', () => {
    render(<StudentList students={[]} />);

    expect(screen.getByRole('status')).toHaveTextContent('No students found');
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('read-only: no Actions column and no write buttons', () => {
    render(<StudentList students={sampleStudents} canEdit={false} />);

    expect(screen.queryByText('Actions')).not.toBeInTheDocument();
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  test('editable: Edit and Delete call back with the student', async () => {
    const onEdit = jest.fn();
    const onDelete = jest.fn();
    render(<StudentList students={sampleStudents} canEdit onEdit={onEdit} onDelete={onDelete} />);

    await userEvent.click(screen.getByRole('button', { name: 'Edit Rohit Menon' }));
    await userEvent.click(screen.getByRole('button', { name: 'Delete Divya Nair' }));

    expect(screen.getByText('Actions')).toBeInTheDocument();
    expect(onEdit).toHaveBeenCalledWith(sampleStudents[1]);
    expect(onDelete).toHaveBeenCalledWith(sampleStudents[2]);
  });
});

describe('SearchBox', () => {
  test('reports every keystroke and shows the match counter', async () => {
    const onChange = jest.fn();
    render(<SearchBox value="" onChange={onChange} matchCount={2} total={3} />);

    await userEvent.type(screen.getByLabelText('Search students'), 'as');

    expect(onChange).toHaveBeenNthCalledWith(1, 'a');
    expect(screen.getByText('2 of 3 students')).toBeInTheDocument();
  });

  test('uses the singular for a single student', () => {
    render(<SearchBox value="" onChange={() => {}} matchCount={1} total={1} />);

    expect(screen.getByText('1 of 1 student')).toBeInTheDocument();
  });
});
