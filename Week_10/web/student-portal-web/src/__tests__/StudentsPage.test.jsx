import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import StudentsPage from '../pages/StudentsPage';
import * as studentsApi from '../api/studentsApi';
import { useAuth } from '../auth/AuthContext';
import { httpError, sampleStudents } from './testUtils';

jest.mock('../api/studentsApi');
jest.mock('../auth/AuthContext', () => ({ useAuth: jest.fn() }));

function renderAs(role, students = sampleStudents) {
  useAuth.mockReturnValue({ isTeacher: role === 'Teacher', role });
  studentsApi.getStudents.mockResolvedValue(students.map((s) => ({ ...s })));
  return render(<StudentsPage />);
}

async function fillStudentForm(user, values) {
  for (const [label, value] of Object.entries(values)) {
    const input = screen.getByLabelText(label);
    await user.clear(input);
    await user.type(input, value);
  }
}

beforeEach(() => {
  jest.resetAllMocks();
});

describe('loading (Task 8.4)', () => {
  test('shows a loading indicator, then the rows fetched by useEffect', async () => {
    renderAs('Student');

    expect(screen.getByText('Loading students...')).toBeInTheDocument();
    expect(await screen.findByText('Asha Kumar')).toBeInTheDocument();
    expect(studentsApi.getStudents).toHaveBeenCalledTimes(1);
  });

  test('shows the empty state when the API returns no students', async () => {
    renderAs('Student', []);

    expect(await screen.findByText('No students found')).toBeInTheDocument();
  });

  test('shows a friendly error when the API is unreachable', async () => {
    useAuth.mockReturnValue({ isTeacher: false });
    studentsApi.getStudents.mockRejectedValue(new Error('Network Error'));

    render(<StudentsPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent("Can't reach the server");
  });

  test('ignores a response that arrives after unmount', async () => {
    useAuth.mockReturnValue({ isTeacher: false });
    let resolve;
    studentsApi.getStudents.mockReturnValue(new Promise((r) => (resolve = r)));
    const errorSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    const { unmount } = render(<StudentsPage />);
    unmount();
    resolve(sampleStudents);
    await Promise.resolve();

    expect(errorSpy).not.toHaveBeenCalled();
    errorSpy.mockRestore();
  });
});

describe('search (Task 8.3)', () => {
  test('filters rows live as the user types, by name or roll number, with a counter', async () => {
    const user = userEvent.setup();
    renderAs('Student');
    await screen.findByText('Asha Kumar');

    await user.type(screen.getByLabelText('Search students'), 'ro');
    expect(screen.getByText('Rohit Menon')).toBeInTheDocument();
    expect(screen.queryByText('Asha Kumar')).not.toBeInTheDocument();
    expect(screen.getByText('1 of 3 students')).toBeInTheDocument();

    await user.clear(screen.getByLabelText('Search students'));
    await user.type(screen.getByLabelText('Search students'), 'r003');
    expect(screen.getByText('Divya Nair')).toBeInTheDocument();
  });

  test('a search with no matches shows the empty state', async () => {
    const user = userEvent.setup();
    renderAs('Student');
    await screen.findByText('Asha Kumar');

    await user.type(screen.getByLabelText('Search students'), 'zzz');

    expect(screen.getByText('No students found')).toBeInTheDocument();
    expect(screen.getByText('0 of 3 students')).toBeInTheDocument();
  });
});

describe('role-based UI (Task 8.8)', () => {
  test('a Student sees no write controls at all', async () => {
    renderAs('Student');
    await screen.findByText('Asha Kumar');

    expect(screen.queryByRole('button', { name: 'Add student' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /^Edit/ })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /^Delete/ })).not.toBeInTheDocument();
    expect(screen.getByText('You have read-only access.')).toBeInTheDocument();
  });

  test('a Teacher sees Add, Edit and Delete', async () => {
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');

    expect(screen.getByRole('button', { name: 'Add student' })).toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: /^Edit/ })).toHaveLength(3);
    expect(screen.getAllByRole('button', { name: /^Delete/ })).toHaveLength(3);
    expect(screen.queryByText('You have read-only access.')).not.toBeInTheDocument();
  });
});

describe('teacher CRUD', () => {
  test('create: Save stays disabled until valid, then the new row appears', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.createStudent.mockImplementation(async (s) => ({ ...s, id: 10 }));

    await user.click(screen.getByRole('button', { name: 'Add student' }));
    const save = screen.getByRole('button', { name: 'Save' });
    expect(save).toBeDisabled();

    await fillStudentForm(user, { Name: 'Kiran Das', Age: '22', 'Roll number': 'R010', Email: 'kiran@school.example', Score: '77' });
    expect(save).toBeEnabled();
    await user.click(save);

    expect(studentsApi.createStudent).toHaveBeenCalledWith({
      name: 'Kiran Das', age: 22, rollNumber: 'R010', email: 'kiran@school.example', score: 77, enrolledOn: null,
    });
    expect(await screen.findByText('Kiran Das')).toBeInTheDocument();
    expect(screen.getByText('Added Kiran Das.')).toBeInTheDocument();
    expect(screen.queryByRole('form', { name: 'Add student' })).not.toBeInTheDocument();
  });

  test('create: a 409 from the API is shown inside the form', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.createStudent.mockRejectedValue(httpError(409, "Roll number 'R001' is already in use."));

    await user.click(screen.getByRole('button', { name: 'Add student' }));
    await fillStudentForm(user, { Name: 'Dup', Age: '20', 'Roll number': 'R001', Email: 'dup@school.example', Score: '50' });
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByRole('alert')).toHaveTextContent("Roll number 'R001' is already in use.");
    expect(screen.getByRole('form', { name: 'Add student' })).toBeInTheDocument();
  });

  test('edit: the form is pre-filled and the row is updated', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.updateStudent.mockImplementation(async (id, s) => ({ ...s, id }));

    await user.click(screen.getByRole('button', { name: 'Edit Rohit Menon' }));
    expect(screen.getByLabelText('Name')).toHaveValue('Rohit Menon');
    await fillStudentForm(user, { Score: '70' });
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(studentsApi.updateStudent).toHaveBeenCalledWith(2, expect.objectContaining({ score: 70, rollNumber: 'R002' }));
    expect(await screen.findByText('Saved Rohit Menon.')).toBeInTheDocument();
    const row = screen.getByText('Rohit Menon').closest('tr');
    expect(within(row).getByText('70')).toBeInTheDocument();
  });

  test('cancel closes the form without saving', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');

    await user.click(screen.getByRole('button', { name: 'Add student' }));
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.queryByRole('form')).not.toBeInTheDocument();
    expect(studentsApi.createStudent).not.toHaveBeenCalled();
  });

  test('delete: confirmed -> removed; cancelled -> kept', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.deleteStudent.mockResolvedValue();
    const confirm = jest.spyOn(window, 'confirm');

    confirm.mockReturnValueOnce(false);
    await user.click(screen.getByRole('button', { name: 'Delete Asha Kumar' }));
    expect(studentsApi.deleteStudent).not.toHaveBeenCalled();

    confirm.mockReturnValueOnce(true);
    await user.click(screen.getByRole('button', { name: 'Delete Asha Kumar' }));
    await waitFor(() => expect(screen.queryByText('Asha Kumar')).not.toBeInTheDocument());
    expect(studentsApi.deleteStudent).toHaveBeenCalledWith(1);
    expect(screen.getByText('Deleted Asha Kumar.')).toBeInTheDocument();
    confirm.mockRestore();
  });

  test('delete: a server rejection is reported and the row stays', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.deleteStudent.mockRejectedValue(httpError(403));
    jest.spyOn(window, 'confirm').mockReturnValue(true);

    await user.click(screen.getByRole('button', { name: 'Delete Asha Kumar' }));

    expect(await screen.findByText("You don't have permission to do that.")).toBeInTheDocument();
    expect(screen.getByText('Asha Kumar')).toBeInTheDocument();
    window.confirm.mockRestore();
  });
});

// Task 9.2 / 9.4 - success vs error API handling: each failure becomes a
// readable message in the page, never a crash.
describe('error handling (Task 9.2)', () => {
  test('a 400 from the API shows the server validation message in the form', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.createStudent.mockRejectedValue(httpError(400, { errors: { Email: ['The Email field is not a valid e-mail address.'] } }));

    await user.click(screen.getByRole('button', { name: 'Add student' }));
    await fillStudentForm(user, { Name: 'X', Age: '20', 'Roll number': 'R9', Email: 'x@school.example', Score: '50' });
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('The Email field is not a valid e-mail address.');
  });

  test('a 500 while saving is reported and the form stays open for a retry', async () => {
    const user = userEvent.setup();
    renderAs('Teacher');
    await screen.findByText('Asha Kumar');
    studentsApi.updateStudent.mockRejectedValue(httpError(500));

    await user.click(screen.getByRole('button', { name: 'Edit Asha Kumar' }));
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('The server hit a problem. Please try again.');
    expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  });

  test('a 500 while loading shows an error instead of an empty table', async () => {
    useAuth.mockReturnValue({ isTeacher: false });
    studentsApi.getStudents.mockRejectedValue(httpError(500));

    render(<StudentsPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('The server hit a problem.');
    expect(screen.queryByText('No students found')).not.toBeInTheDocument();
  });
});
