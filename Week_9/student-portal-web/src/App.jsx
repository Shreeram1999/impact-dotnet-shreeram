import { Navigate, Route, Routes } from 'react-router-dom';
import ProtectedRoute from './auth/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import NavBar from './components/NavBar';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import StudentsPage from './pages/StudentsPage';

// Task 8.6 - /login and /register are public; /students needs a token.
// Anything else lands on /students (which itself redirects to /login when
// logged out). Task 9.2 - an ErrorBoundary around everything, so a render
// error shows a message instead of a blank page.
export default function App() {
  return (
    <ErrorBoundary>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="/students"
          element={
            <ProtectedRoute>
              <NavBar />
              <main className="content">
                <StudentsPage />
              </main>
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to="/students" replace />} />
      </Routes>
    </ErrorBoundary>
  );
}
