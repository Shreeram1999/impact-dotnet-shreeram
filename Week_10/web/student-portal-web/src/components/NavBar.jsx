import { NavLink, useNavigate } from 'react-router-dom';
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
      <div className="brand">
        <h1>Student Portal</h1>
        <nav aria-label="Main">
          <NavLink to="/students">Students</NavLink>
          <NavLink to="/reports">Reports</NavLink>
        </nav>
      </div>
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
