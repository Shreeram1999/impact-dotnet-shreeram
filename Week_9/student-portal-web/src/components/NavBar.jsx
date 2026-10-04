import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function NavBar() {
  const { displayName, role, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <header className="navbar">
      <h1>Student Portal</h1>
      <div className="session">
        <span className="who">{displayName}</span>
        <span className={`role-badge role-${role?.toLowerCase()}`}>{role}</span>
        <button type="button" className="secondary" onClick={handleLogout}>
          Log out
        </button>
      </div>
    </header>
  );
}
