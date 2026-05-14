import React from 'react';
import { Link } from 'react-router-dom';

const AdminPage = ({ title, subtitle, breadcrumbs = [], actions, children }) => (
  <div className="content-wrapper">
    <div className="content-header">
      <div className="container-fluid">
        <div className="row align-items-center mb-2">
          <div className="col-sm-7">
            <h1 className="admin-page-title m-0">{title}</h1>
            {subtitle && <div className="admin-page-subtitle">{subtitle}</div>}
          </div>
          <div className="col-sm-5">
            <div className="d-flex align-items-center justify-content-sm-end mt-2 mt-sm-0">
              {actions}
            </div>
            <ol className="breadcrumb float-sm-right mt-2 mb-0">
              <li className="breadcrumb-item">
                <Link to="/">Dashboard</Link>
              </li>
              {breadcrumbs.map((item) => (
                <li key={item.label} className={`breadcrumb-item ${item.active ? 'active' : ''}`}>
                  {item.to && !item.active ? <Link to={item.to}>{item.label}</Link> : item.label}
                </li>
              ))}
            </ol>
          </div>
        </div>
      </div>
    </div>

    <section className="content">
      <div className="container-fluid">{children}</div>
    </section>
  </div>
);

export default AdminPage;
