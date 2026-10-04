import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import LoginPage from '../pages/LoginPage';
import RegisterPage from '../pages/RegisterPage';
import * as authApi from '../api/authApi';
import { useAuth } from '../auth/AuthContext';
import { httpError } from './testUtils';

jest.mock('../api/authApi');
jest.mock('../auth/AuthContext', () => ({ useAuth: jest.fn() }));

// Shows where navigation ended up, and with what router state.
function Landing({ label }) {
  const location = useLocation();
  return <p>{`${label} ${JSON.stringify(location.state ?? null)}`}</p>;
}

function renderRoutes(initialEntry) {
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/students" element={<Landing label="students page" />} />
        <Route path="/elsewhere" element={<Landing label="elsewhere page" />} />
      </Routes>
    </MemoryRouter>,
  );
}

beforeEach(() => {
  jest.resetAllMocks();
  useAuth.mockReturnValue({ login: jest.fn() }); // registration navigates to the login page
});

describe('LoginPage (Task 8.6)', () => {
  test('Log in is disabled until both fields are filled', async () => {
    useAuth.mockReturnValue({ login: jest.fn() });
    renderRoutes('/login');
    const button = screen.getByRole('button', { name: 'Log in' });

    expect(button).toBeDisabled();
    await userEvent.type(screen.getByLabelText('Email or username'), 'teacher1');
    expect(button).toBeDisabled();
    await userEvent.type(screen.getByLabelText('Password'), 'Teacher@123');
    expect(button).toBeEnabled();
  });

  test('a successful login goes to /students', async () => {
    const login = jest.fn().mockResolvedValue({});
    useAuth.mockReturnValue({ login });
    renderRoutes('/login');

    await userEvent.type(screen.getByLabelText('Email or username'), ' teacher1 ');
    await userEvent.type(screen.getByLabelText('Password'), 'Teacher@123');
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }));

    expect(login).toHaveBeenCalledWith('teacher1', 'Teacher@123');
    expect(await screen.findByText(/students page/)).toBeInTheDocument();
  });

  test('after login, returns to the page ProtectedRoute bounced you from', async () => {
    useAuth.mockReturnValue({ login: jest.fn().mockResolvedValue({}) });
    render(
      <MemoryRouter initialEntries={[{ pathname: '/login', state: { from: '/elsewhere' } }]}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/elsewhere" element={<Landing label="elsewhere page" />} />
        </Routes>
      </MemoryRouter>,
    );

    await userEvent.type(screen.getByLabelText('Email or username'), 'a');
    await userEvent.type(screen.getByLabelText('Password'), 'b');
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }));

    expect(await screen.findByText(/elsewhere page/)).toBeInTheDocument();
  });

  test('a 401 shows "Invalid username or password" and stays on the page', async () => {
    useAuth.mockReturnValue({ login: jest.fn().mockRejectedValue(httpError(401)) });
    renderRoutes('/login');

    await userEvent.type(screen.getByLabelText('Email or username'), 'teacher1');
    await userEvent.type(screen.getByLabelText('Password'), 'wrong');
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid username or password.');
    expect(screen.getByRole('button', { name: 'Log in' })).toBeEnabled();
  });

  test('arriving from registration shows a notice and pre-fills the email', () => {
    useAuth.mockReturnValue({ login: jest.fn() });
    render(
      <MemoryRouter initialEntries={[{ pathname: '/login', state: { registered: 'new@school.example' } }]}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText('Registration successful - please log in.')).toBeInTheDocument();
    expect(screen.getByLabelText('Email or username')).toHaveValue('new@school.example');
  });
});

describe('RegisterPage (Task 8.5)', () => {
  async function fillValidForm(user) {
    await user.type(screen.getByLabelText('Name'), 'New Student');
    await user.type(screen.getByLabelText('Date of birth'), '2005-05-20');
    await user.type(screen.getByLabelText('Email'), 'new@school.example');
    await user.type(screen.getByLabelText('Password'), 'Passw0rd');
  }

  test('Register stays disabled until every field is valid', async () => {
    const user = userEvent.setup();
    renderRoutes('/register');
    const button = screen.getByRole('button', { name: 'Register' });

    expect(button).toBeDisabled();
    await fillValidForm(user);
    expect(button).toBeEnabled();

    await user.clear(screen.getByLabelText('Password'));
    await user.type(screen.getByLabelText('Password'), 'weak');
    expect(button).toBeDisabled();
  });

  test('inline errors appear once a field has been visited', async () => {
    const user = userEvent.setup();
    renderRoutes('/register');

    expect(screen.queryByText('Enter a valid email address.')).not.toBeInTheDocument();
    await user.type(screen.getByLabelText('Email'), 'not-an-email');
    await user.tab();

    expect(screen.getByText('Enter a valid email address.')).toBeInTheDocument();
  });

  test('every field shows its own inline error after being visited empty', async () => {
    const user = userEvent.setup();
    renderRoutes('/register');

    for (const label of ['Name', 'Date of birth', 'Designation', 'Email', 'Password']) {
      await user.click(screen.getByLabelText(label));
    }
    await user.tab();

    expect(screen.getByText('Name must be at least 2 characters.')).toBeInTheDocument();
    expect(screen.getByText('Date of birth is required.')).toBeInTheDocument();
    expect(screen.getByText('Use 8+ characters with upper case, lower case and a digit.')).toBeInTheDocument();
  });

  test('a successful registration goes to /login with the new username', async () => {
    const user = userEvent.setup();
    authApi.register.mockResolvedValue({ username: 'new@school.example', role: 'Student', displayName: 'New Student' });
    renderRoutes('/register');

    await fillValidForm(user);
    await user.selectOptions(screen.getByLabelText('Designation'), 'Teacher');
    await user.click(screen.getByRole('button', { name: 'Register' }));

    expect(authApi.register).toHaveBeenCalledWith({
      name: 'New Student', dateOfBirth: '2005-05-20', designation: 'Teacher', email: 'new@school.example', password: 'Passw0rd',
    });
    expect(await screen.findByText('Registration successful - please log in.')).toBeInTheDocument();
  });

  test.each([
    [409, 'An account with that email already exists.'],
    [403, "Teacher accounts can't be self-registered."],
  ])('a %i from the API is explained', async (status, message) => {
    const user = userEvent.setup();
    authApi.register.mockRejectedValue(httpError(status));
    renderRoutes('/register');

    await fillValidForm(user);
    await user.click(screen.getByRole('button', { name: 'Register' }));

    expect(await screen.findByRole('alert')).toHaveTextContent(message);
  });
});
