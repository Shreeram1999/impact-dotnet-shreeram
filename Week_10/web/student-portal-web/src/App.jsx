import { Navigate, Route, Routes } from 'react-router-dom';
import ProtectedRoute from './auth/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import NavBar from './components/NavBar';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ReportsPage from './pages/ReportsPage';
import StudentsPage from './pages/StudentsPage';

function Protected({ children }) {
  return (
    <ProtectedRoute>
      <NavBar />
      <main className="content">{children}</main>
    </ProtectedRoute>
  );
}

// /login and /register are public; /students (Academics service) and
// /reports (Reporting service) need a token. Every request goes through
// the gateway.
export default function App() {
  return (
    <ErrorBoundary>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/students" element={<Protected><StudentsPage /></Protected>} />
        <Route path="/reports" element={<Protected><ReportsPage /></Protected>} />
        <Route path="*" element={<Navigate to="/students" replace />} />
      </Routes>
    </ErrorBoundary>
  );
}
