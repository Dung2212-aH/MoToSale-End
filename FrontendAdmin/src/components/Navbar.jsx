import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const Navbar = () => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="main-header navbar navbar-expand navbar-white navbar-light">
      <ul className="navbar-nav">
        <li className="nav-item">
          <a className="nav-link" data-widget="pushmenu" href="#" role="button">
            <i className="fas fa-bars"></i>
          </a>
        </li>
        <li className="nav-item d-none d-sm-inline-block">
          <span className="nav-link">Hệ thống quản trị MoToSale</span>
        </li>
      </ul>

      <ul className="navbar-nav ml-auto">
        <li className="nav-item dropdown">
          <a className="nav-link" data-toggle="dropdown" href="#">
            <i className="far fa-user"></i> {user?.hoTen || user?.name || 'Admin'}
          </a>
          <div className="dropdown-menu dropdown-menu-right">
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
