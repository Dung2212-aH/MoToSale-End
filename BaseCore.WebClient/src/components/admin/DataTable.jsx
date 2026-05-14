import React from 'react';
import { EmptyState } from './UiState';

const DataTable = ({ columns, rows, rowKey = 'id', emptyTitle, emptyDescription }) => (
  <div className="table-responsive">
    <table className="table table-bordered table-hover admin-table">
      <thead>
        <tr>
          {columns.map((column) => (
            <th key={column.key} style={column.style}>
              {column.label}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.length === 0 ? (
          <tr>
            <td colSpan={columns.length} className="admin-table-empty-cell">
              <EmptyState title={emptyTitle || 'No data found'} description={emptyDescription} />
            </td>
          </tr>
        ) : (
          rows.map((row) => (
            <tr key={typeof rowKey === 'function' ? rowKey(row) : row[rowKey]}>
              {columns.map((column) => (
                <td key={column.key} className={column.className}>
                  {column.render ? column.render(row) : row[column.key]}
                </td>
              ))}
            </tr>
          ))
        )}
      </tbody>
    </table>
  </div>
);

export const Pagination = ({ page, totalPages, totalCount, onPageChange, label = 'items' }) => {
  const safeTotalPages = Math.max(totalPages || 0, 0);
  const pages = [];
  const start = Math.max(1, page - 2);
  const end = Math.min(safeTotalPages, page + 2);

  for (let i = start; i <= end; i += 1) {
    pages.push(i);
  }

  return (
    <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center mt-3">
      <span className="text-muted mb-2 mb-md-0">Total: {totalCount} {label}</span>
      <nav aria-label="Table pagination">
        <ul className="pagination mb-0">
          <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}>
            <button className="page-link" type="button" onClick={() => onPageChange(page - 1)}>
              Previous
            </button>
          </li>
          {pages.map((item) => (
            <li key={item} className={`page-item ${item === page ? 'active' : ''}`}>
              <button className="page-link" type="button" onClick={() => onPageChange(item)}>
                {item}
              </button>
            </li>
          ))}
          <li className={`page-item ${page >= safeTotalPages || safeTotalPages === 0 ? 'disabled' : ''}`}>
            <button className="page-link" type="button" onClick={() => onPageChange(page + 1)}>
              Next
            </button>
          </li>
        </ul>
      </nav>
    </div>
  );
};

export const FilterBar = ({ children, onSubmit }) => (
  <form className="admin-filter-bar mb-3" onSubmit={onSubmit}>
    <div className="form-row align-items-end">{children}</div>
  </form>
);

export default DataTable;
