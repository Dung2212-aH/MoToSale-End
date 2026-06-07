import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const Sidebar = ({ collapsed = false }) => {
  const location = useLocation();
  const { user, isAdmin } = useAuth();

  const isActive = (path) => (location.pathname === path ? 'active' : '');
  const isActiveGroup = (prefix) => (location.pathname.startsWith(prefix) ? 'active' : '');

  return (
    <aside className={`main-sidebar sidebar-dark-primary elevation-4 sidebar-no-expand ${collapsed ? 'is-collapsed' : ''}`}>
      <Link to="/" className="brand-link">
        <span className="brand-text font-weight-light ml-3">
          <b>MoToSale</b> Admin
        </span>
      </Link>

      <div className="sidebar">
        <div className="user-panel mt-3 pb-3 mb-3 d-flex">
          <div className="image">
            <i className="fas fa-user-circle fa-2x text-light"></i>
          </div>
          <div className="info">
            <Link to="#" className="d-block">{user?.hoTen || user?.name || 'Admin'}</Link>
          </div>
        </div>

        <nav className="mt-2">
          <ul className="nav nav-pills nav-sidebar flex-column" role="menu">
            <li className="nav-item">
              <Link to="/" className={`nav-link ${isActive('/')}`}>
                <i className="nav-icon fas fa-tachometer-alt"></i>
                <p>Tổng quan</p>
              </Link>
            </li>

            <li className="nav-header">DANH MỤC & KINH DOANH</li>
            <li className="nav-item">
              <Link to="/motorcycles" className={`nav-link ${isActiveGroup('/motorcycles')}`}>
                <i className="nav-icon fas fa-motorcycle"></i>
                <p>Xe máy</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/parts" className={`nav-link ${isActiveGroup('/parts')}`}>
                <i className="nav-icon fas fa-cogs"></i>
                <p>Phụ tùng</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/categories" className={`nav-link ${isActiveGroup('/categories')}`}>
                <i className="nav-icon fas fa-tags"></i>
                <p>Danh mục</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/brands" className={`nav-link ${isActiveGroup('/brands')}`}>
                <i className="nav-icon fas fa-industry"></i>
                <p>Hãng xe & Dòng xe</p>
              </Link>
            </li>

            <li className="nav-header">ĐƠN HÀNG</li>
            <li className="nav-item">
              <Link to="/orders" className={`nav-link ${isActiveGroup('/orders')}`}>
                <i className="nav-icon fas fa-shopping-cart"></i>
                <p>Đơn hàng</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/installments" className={`nav-link ${isActiveGroup('/installments')}`}>
                <i className="nav-icon fas fa-calendar-check"></i>
                <p>Duyệt kỳ trả góp</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/vouchers" className={`nav-link ${isActiveGroup('/vouchers')}`}>
                <i className="nav-icon fas fa-ticket-alt"></i>
                <p>Voucher</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/inventory" className={`nav-link ${isActiveGroup('/inventory')}`}>
                <i className="nav-icon fas fa-warehouse"></i>
                <p>Tồn kho</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/stock-documents" className={`nav-link ${isActiveGroup('/stock-documents')}`}>
                <i className="nav-icon fas fa-clipboard-check"></i>
                <p>Phiếu kho</p>
              </Link>
            </li>

            <li className="nav-header">NGƯỜI DÙNG & NỘI DUNG</li>
            {isAdmin() && (
              <li className="nav-item">
                <Link to="/users" className={`nav-link ${isActiveGroup('/users')}`}>
                  <i className="nav-icon fas fa-users"></i>
                  <p>Người dùng</p>
                </Link>
              </li>
            )}
            <li className="nav-item">
              <Link to="/customers" className={`nav-link ${isActiveGroup('/customers')}`}>
                <i className="nav-icon fas fa-user-tag"></i>
                <p>Khách hàng</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/warranties" className={`nav-link ${isActiveGroup('/warranties')}`}>
                <i className="nav-icon fas fa-tools"></i>
                <p>Bảo hành</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/reviews" className={`nav-link ${isActiveGroup('/reviews')}`}>
                <i className="nav-icon fas fa-star"></i>
                <p>Đánh giá</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/posts" className={`nav-link ${isActiveGroup('/posts')}`}>
                <i className="nav-icon fas fa-newspaper"></i>
                <p>Bài viết</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/faq" className={`nav-link ${isActiveGroup('/faq')}`}>
                <i className="nav-icon fas fa-question-circle"></i>
                <p>FAQ</p>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/contacts" className={`nav-link ${isActiveGroup('/contacts')}`}>
                <i className="nav-icon fas fa-envelope"></i>
                <p>Liên hệ</p>
              </Link>
            </li>

            <li className="nav-header">BÁO CÁO</li>
            <li className="nav-item">
              <Link to="/reports" className={`nav-link ${isActiveGroup('/reports')}`}>
                <i className="nav-icon fas fa-chart-bar"></i>
                <p>Báo cáo & Thống kê</p>
              </Link>
            </li>
            {isAdmin() && (
              <li className="nav-item">
                <Link to="/audit-logs" className={`nav-link ${isActiveGroup('/audit-logs')}`}>
                  <i className="nav-icon fas fa-clipboard-list"></i>
                  <p>Nhật ký hệ thống</p>
                </Link>
              </li>
            )}
            <li className="nav-item">
              <Link to="/settings" className={`nav-link ${location.pathname === '/settings' ? 'active' : ''}`}>
                <i className="nav-icon fas fa-cog"></i>
                <p>Cấu hình vận hành</p>
              </Link>
            </li>
            {isAdmin() && (
              <li className="nav-item">
                <Link to="/settings/payment" className={`nav-link ${isActive('/settings/payment')}`}>
                  <i className="nav-icon fas fa-qrcode"></i>
                  <p>Cấu hình thanh toán</p>
                </Link>
              </li>
            )}
          </ul>
        </nav>
      </div>
    </aside>
  );
};

export default Sidebar;
