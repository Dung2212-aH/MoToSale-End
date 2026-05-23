import React from 'react';
import { cn } from '../../utils/cn';

export function DataTable({ columns, rows, rowKey = 'id', emptyText = 'Không có dữ liệu', className }) {
  return (
    <div className="table-responsive">
      <table className={cn('table table-bordered table-striped table-sm', className)}>
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={column.key} className={column.className}>{column.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="text-center text-muted py-4">{emptyText}</td>
            </tr>
          ) : rows.map((row, index) => (
            <tr key={typeof rowKey === 'function' ? rowKey(row) : row[rowKey] ?? index}>
              {columns.map((column) => (
                <td key={column.key} className={column.cellClassName}>
                  {column.render ? column.render(row, index) : row[column.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
