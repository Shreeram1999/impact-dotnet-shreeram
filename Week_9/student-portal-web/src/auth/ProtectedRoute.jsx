import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';

// Task 8.6 - no token, no page: unauthenticated visitors are redirected to
// /login, remembering where they were heading so login can send them back.
// This only guards the UI. The API rejects the same visitor with a 401 anyway.
export default function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return children;
}
