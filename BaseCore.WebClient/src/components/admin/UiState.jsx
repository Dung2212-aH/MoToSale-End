import React from 'react';

export const LoadingState = ({ label = 'Loading data...' }) => (
  <div className="admin-state">
    <div className="spinner-border text-primary" role="status">
      <span className="sr-only">Loading...</span>
    </div>
    <div className="mt-2 text-muted">{label}</div>
  </div>
);

export const EmptyState = ({ title = 'No records found', description, icon = 'fas fa-inbox' }) => (
  <div className="admin-empty-state">
    <i className={`${icon} admin-empty-icon`}></i>
    <h4>{title}</h4>
    {description && <p>{description}</p>}
  </div>
);

export const ErrorState = ({ message, onRetry }) => (
  <div className="alert alert-danger d-flex justify-content-between align-items-center">
    <div>
      <i className="fas fa-exclamation-triangle mr-2"></i>
      {message || 'Something went wrong.'}
    </div>
    {onRetry && (
      <button type="button" className="btn btn-sm btn-outline-danger" onClick={onRetry}>
        <i className="fas fa-redo mr-1"></i> Retry
      </button>
    )}
  </div>
);

export const ConfirmActionButton = ({ children, confirmMessage, onConfirm, className = 'btn btn-sm btn-outline-danger', disabled, title }) => {
  const handleClick = () => {
    if (!disabled && window.confirm(confirmMessage || 'Are you sure?')) {
      onConfirm?.();
    }
  };

  return (
    <button type="button" className={className} onClick={handleClick} disabled={disabled} title={title}>
      {children}
    </button>
  );
};
