import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import ReportsPage from '../pages/ReportsPage';
import NavBar from '../components/NavBar';
import * as reportsApi from '../api/reportsApi';
import { api } from '../api/client';
import { describeError } from '../api/errors';
import { useAuth } from '../auth/AuthContext';
import { httpError } from './testUtils';

jest.mock('../api/reportsApi', () => ({
  ...jest.requireActual('../api/reportsApi'),
  getTerms: jest.fn(),
  getEnrollmentSummary: jest.fn(),
}));
jest.mock('../auth/AuthContext', () => ({ useAuth: jest.fn() }));

const terms = [
  { termId: 1, name: 'Spring 2026', startsOn: '2026-01-05', endsOn: '2026-05-15' },
  { termId: 2, name: 'Monsoon 2026', startsOn: '2026-06-01', endsOn: '2026-11-20' },
];

const summary = (term, courses) => ({
  term,
  totalEnrolled: courses.reduce((n, c) => n + c.enrolled, 0),
  totalCompleted: courses.reduce((n, c) => n + c.completed, 0),
  courses,
});

const cs201 = { courseCode: 'CS201', title: 'Web APIs', department: 'Computer Science', credits: 3, enrolled: 4, completed: 3, averageScore: 70.3, passRate: 66.7 };
const math201 = { courseCode: 'MATH201', title: 'Linear Algebra', department: 'Mathematics', credits: 3, enrolled: 3, completed: 1, averageScore: null, passRate: null };
const math101 = { courseCode: 'MATH101', title: 'Calculus I', department: 'Mathematics', credits: 4, enrolled: 4, completed: 4, averageScore: 66.5, passRate: 75 };

beforeEach(() => jest.resetAllMocks());

describe('ReportsPage (Task 10.4)', () => {
  test('loads the latest term by default and shows each course row', async () => {
    reportsApi.getTerms.mockResolvedValue(terms);
    reportsApi.getEnrollmentSummary.mockResolvedValue(summary(terms[1], [cs201, math201]));

    render(<ReportsPage />);

    expect(screen.getByText('Loading reports...')).toBeInTheDocument();
    expect(await screen.findByText('Enrollment summary - Monsoon 2026')).toBeInTheDocument();
    expect(reportsApi.getEnrollmentSummary).toHaveBeenCalledWith(null);
    const cs = screen.getByText('CS201').closest('tr');
    expect(within(cs).getByText('70.3')).toBeInTheDocument();
    expect(within(cs).getByText('66.7%')).toBeInTheDocument();
    const math = screen.getByText('MATH201').closest('tr');
    expect(within(math).getAllByText('-')).toHaveLength(2); // no scores yet
    expect(screen.getByText('7 enrolments, 4 completed')).toBeInTheDocument();
  });

  test('choosing another term reloads that term (terms are not refetched)', async () => {
    reportsApi.getTerms.mockResolvedValue(terms);
    reportsApi.getEnrollmentSummary
      .mockResolvedValueOnce(summary(terms[1], [cs201]))
      .mockResolvedValueOnce(summary(terms[0], [math101]));
    render(<ReportsPage />);
    await screen.findByText('Enrollment summary - Monsoon 2026');

    await userEvent.selectOptions(screen.getByLabelText('Term'), '1');

    expect(await screen.findByText('Enrollment summary - Spring 2026')).toBeInTheDocument();
    expect(reportsApi.getEnrollmentSummary).toHaveBeenLastCalledWith(1);
    expect(reportsApi.getTerms).toHaveBeenCalledTimes(1);
  });

  test('a term with no enrolments shows an empty state', async () => {
    reportsApi.getTerms.mockResolvedValue(terms);
    reportsApi.getEnrollmentSummary.mockResolvedValue(summary(terms[0], []));

    render(<ReportsPage />);

    expect(await screen.findByText('No enrolments recorded for this term')).toBeInTheDocument();
  });

  // Task 10.10 - Reporting down: the gateway answers 502, the page says the
  // reports are unavailable instead of failing.
  test.each([[502], [503]])('a %i from the gateway means reporting is unavailable', async (status) => {
    reportsApi.getTerms.mockRejectedValue(httpError(status));
    reportsApi.getEnrollmentSummary.mockRejectedValue(httpError(status));

    render(<ReportsPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Reporting is temporarily unavailable. Students and courses still work');
  });

  test('no response at all is also treated as unavailable', async () => {
    reportsApi.getTerms.mockRejectedValue(new Error('Network Error'));
    reportsApi.getEnrollmentSummary.mockRejectedValue(new Error('Network Error'));

    render(<ReportsPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Reporting is temporarily unavailable');
  });

  test('any other error uses the normal friendly message', async () => {
    reportsApi.getTerms.mockResolvedValue(terms);
    reportsApi.getEnrollmentSummary.mockRejectedValue(httpError(404));

    render(<ReportsPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('That record no longer exists.');
  });

  test('a response arriving after unmount is ignored', async () => {
    let resolve;
    reportsApi.getTerms.mockResolvedValue(terms);
    reportsApi.getEnrollmentSummary.mockReturnValue(new Promise((r) => (resolve = r)));
    const errorSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    const { unmount } = render(<ReportsPage />);
    unmount();
    resolve(summary(terms[1], [cs201]));
    await Promise.resolve();

    expect(errorSpy).not.toHaveBeenCalled();
    errorSpy.mockRestore();
  });

  test('a failure after unmount is ignored too', async () => {
    let reject;
    reportsApi.getTerms.mockResolvedValue(terms);
    reportsApi.getEnrollmentSummary.mockReturnValue(new Promise((_, r) => (reject = r)));

    const { unmount } = render(<ReportsPage />);
    unmount();
    reject(httpError(502));
    await Promise.resolve();

    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});

describe('reportsApi', () => {
  test('calls the Reporting service through the gateway prefix', async () => {
    const actual = jest.requireActual('../api/reportsApi');
    const seen = [];
    api.defaults.adapter = async (config) => {
      seen.push(`${config.url}${config.params?.termId ? `?termId=${config.params.termId}` : ''}`);
      return { data: [], status: 200, statusText: 'OK', headers: {}, config };
    };

    await actual.getTerms();
    await actual.getEnrollmentSummary(2);
    await actual.getEnrollmentSummary(null);

    expect(seen).toEqual([
      '/reporting/api/reports/terms',
      '/reporting/api/reports/enrollment-summary?termId=2',
      '/reporting/api/reports/enrollment-summary',
    ]);
  });

  test('describeError explains a gateway 502/503/504', () => {
    expect(describeError(httpError(504))).toBe('That part of the portal is temporarily unavailable. Please try again shortly.');
  });
});

describe('NavBar (Week 10)', () => {
  test('links to Students and Reports', () => {
    useAuth.mockReturnValue({ displayName: 'Meera Iyer', role: 'Teacher', logout: jest.fn() });

    render(
      <MemoryRouter initialEntries={['/reports']}>
        <NavBar />
      </MemoryRouter>,
    );

    expect(screen.getByRole('link', { name: 'Students' })).toHaveAttribute('href', '/students');
    expect(screen.getByRole('link', { name: 'Reports' })).toHaveClass('active');
  });
});
