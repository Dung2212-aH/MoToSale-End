import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const Navbar = ({ onToggleSidebar }) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [profileOpen, setProfileOpen] = useState(false);
  const displayName = user?.fullName || user?.hoTen || user?.name || (user?.roles?.includes('Staff') ? 'Nhân viên' : 'Admin');

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="main-header navbar navbar-expand navbar-white navbar-light">
      <ul className="navbar-nav">
        <li className="nav-item">
          <button className="nav-link nav-button" type="button" onClick={onToggleSidebar} aria-label="Mở/đóng menu">
            <i className="fas fa-bars"></i>
          </button>
        </li>
        <li className="nav-item d-none d-sm-inline-block">
          <span className="nav-link">Hệ thống quản trị MoToSale</span>
        </li>
      </ul>

      <ul className="navbar-nav ml-auto">
        <li className={`nav-item dropdown ${profileOpen ? 'show' : ''}`}>
          <button className="nav-link nav-button" type="button" onClick={() => setProfileOpen((value) => !value)}>
            <i className="far fa-user"></i> {displayName}
          </button>
          <div className={`dropdown-menu dropdown-menu-right ${profileOpen ? 'show' : ''}`}>
            <span className="dropdown-item dropdown-header">
              {user?.email}
            </span>
            <div className="dropdown-divider"></div>
            <button className="dropdown-item" onClick={handleLogout}>
              <i className="fas fa-sign-out-alt mr-2"></i> Đăng xuất
            </button>
          </div>
        </li>
      </ul>
    </nav>
  );
};

export default Navbar;
