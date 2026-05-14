import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const MainLayout = ({ children }) => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout, isAdmin } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isActive = (path) => (location.pathname === path ? 'active' : '');
  const navGroups = [
    {
      label: 'OPERATIONS',
      items: [
        { to: '/', icon: 'fas fa-tachometer-alt', label: 'Dashboard' },
        { to: '/showrooms', icon: 'fas fa-store', label: 'Showrooms' },
        { to: '/orders', icon: 'fas fa-file-invoice-dollar', label: 'Orders' },
      ],
    },
    {
      label: 'CATALOG',
      items: [
        { to: '/products', icon: 'fas fa-motorcycle', label: 'Products' },
        { to: '/categories', icon: 'fas fa-tags', label: 'Categories' },
      ],
    },
    {
      label: 'SYSTEM',
      items: [...(isAdmin() ? [{ to: '/users', icon: 'fas fa-cogs', label: 'System' }] : [])],
    },
  ].filter((group) => group.items.length > 0);

  return (
    <div className="wrapper">
      <nav className="main-header navbar navbar-expand navbar-white navbar-light">
        <ul className="navbar-nav">
          <li className="nav-item">
            <a className="nav-link" data-widget="pushmenu" href="#sidebar" role="button">
              <i className="fas fa-bars"></i>
            </a>
          </li>
          <li className="nav-item d-none d-sm-inline-block">
            <Link to="/" className="nav-link">Dashboard</Link>
          </li>
        </ul>

        <ul className="navbar-nav ml-auto">
          <li className="nav-item dropdown">
            <a className="nav-link" data-toggle="dropdown" href="#user-menu">
              <i className="far fa-user mr-1"></i>
              {user?.name || user?.username}
            </a>
            <div className="dropdown-menu dropdown-menu-right">
              <span className="dropdown-item dropdown-header">{user?.email}</span>
              <span className="dropdown-item-text px-3 text-muted small">
                Role: {user?.role || 'User'}
              </span>
              <div className="dropdown-divider"></div>
              <button className="dropdown-item" onClick={handleLogout}>
                <i className="fas fa-sign-out-alt mr-2"></i> Sign out
              </button>
            </div>
          </li>
        </ul>
      </nav>

      <aside className="main-sidebar sidebar-dark-primary elevation-4" id="sidebar">
        <Link to="/" className="brand-link">
          <span className="brand-text font-weight-light ml-3">
            <b>Motor</b> Admin
          </span>
        </Link>

        <div className="sidebar">
          <div className="user-panel mt-3 pb-3 mb-3 d-flex">
            <div className="image">
              <i className="fas fa-user-circle fa-2x text-light"></i>
            </div>
            <div className="info">
              <Link to="#" className="d-block">{user?.name || user?.username}</Link>
              <span className="text-muted small">{user?.role || 'User'}</span>
            </div>
          </div>

          <nav className="mt-2">
            <ul className="nav nav-pills nav-sidebar flex-column" data-widget="treeview" role="menu">
              {navGroups.map((group) => (
                <React.Fragment key={group.label}>
                  <li className="admin-sidebar-heading">{group.label}</li>
                  {group.items.map((item) => (
                    <li className="nav-item" key={item.to}>
                      <Link to={item.to} className={`nav-link ${isActive(item.to)}`}>
                        <i className={`nav-icon ${item.icon}`}></i>
                        <p>{item.label}</p>
                      </Link>
                    </li>
                  ))}
                </React.Fragment>
              ))}
            </ul>
          </nav>
        </div>
      </aside>

      {children}

      <footer className="main-footer">
        <strong>Copyright &copy; 2026 <a href="#">BaseCore Motorcycle Showroom</a>.</strong>
        <div className="float-right d-none d-sm-inline-block">
          <b>Version</b> 1.0.0
        </div>
      </footer>
    </div>
  );
};

export default MainLayout;
