import React from 'react';
import { Link } from 'react-router-dom';

const StatCard = ({ color = 'info', icon = 'fas fa-chart-bar', label, value, to, footer = 'Chi tiết' }) => {
  return (
    <div className="col-lg-3 col-6">
      <div className={`small-box bg-${color}`}>
        <div className="inner">
          <h3>{value}</h3>
          <p>{label}</p>
        </div>
        <div className="icon">
          <i className={icon}></i>
        </div>
        {to && (
          <Link to={to} className="small-box-footer">
            {footer} <i className="fas fa-arrow-circle-right"></i>
          </Link>
        )}
      </div>
    </div>
  );
};

export default StatCard;
