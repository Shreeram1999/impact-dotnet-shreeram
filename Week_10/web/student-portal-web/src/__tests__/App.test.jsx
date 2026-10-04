import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import App from '../App';
import { AuthProvider } from '../auth/AuthContext';
import * as authApi from '../api/authApi';
import * as studentsApi from '../api/studentsApi';
import { fakeJwt, sampleStudents } from './testUtils';

jest.mock('../api/authApi');
jest.mock('../api/studentsApi');

function renderApp(path = '/') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AuthProvider>
        <App />
      </AuthProvider>
    </MemoryRouter>,
  );
}

// Task 8.10 in miniature (the real round-trip runs against the live API):
// log in through the real form and context, land on the list, log out.
describe('App routing', () => {
  test('an unknown URL while logged out ends on the login page', () => {
    renderApp('/nowhere');

    expect(screen.getByRole('heading', { name: 'Log in' })).toBeInTheDocument();
  });

  test('the register link leads to the registration form', async () => {
    renderApp('/login');

    await userEvent.click(screen.getByRole('link', { name: 'Create an account' }));

    expect(screen.getByRole('heading', { name: 'Create an account' })).toBeInTheDocument();
  });

  test('login -> student list with the user shown in the nav bar -> logout', async () => {
    const user = userEvent.setup();
    authApi.login.mockResolvedValue({ token: fakeJwt({ sub: 'teacher1', role: 'Teacher', name: 'Meera Iyer' }) });
    studentsApi.getStudents.mockResolvedValue(sampleStudents);
    renderApp('/students');

    await user.type(screen.getByLabelText('Email or username'), 'teacher1');
    await user.type(screen.getByLabelText('Password'), 'Teacher@123');
    await user.click(screen.getByRole('button', { name: 'Log in' }));

    expect(await screen.findByText('Asha Kumar')).toBeInTheDocument();
    expect(screen.getByText('Meera Iyer')).toBeInTheDocument();
    expect(screen.getByText('Teacher')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Log out' }));
    expect(screen.getByRole('heading', { name: 'Log in' })).toBeInTheDocument();
  });
});
