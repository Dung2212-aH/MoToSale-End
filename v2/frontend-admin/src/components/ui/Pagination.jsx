import React from 'react';
import { cn } from '../../utils/cn';

export function Pagination({ page, totalPages, onPageChange, className }) {
  if (!totalPages || totalPages <= 1) return null;

  return (
    <nav className={cn('mt-3', className)} aria-label="Phân trang">
      <ul className="pagination pagination-sm justify-content-center">
        <li className={cn('page-item', page <= 1 && 'disabled')}>
          <button className="page-link" type="button" onClick={() => onPageChange(page - 1)} disabled={page <= 1}>«</button>
        </li>
        {Array.from({ length: totalPages }, (_, index) => index + 1).map((item) => (
          <li key={item} className={cn('page-item', item === page && 'active')}>
            <button className="page-link" type="button" onClick={() => onPageChange(item)}>{item}</button>
          </li>
        ))}
        <li className={cn('page-item', page >= totalPages && 'disabled')}>
          <button className="page-link" type="button" onClick={() => onPageChange(page + 1)} disabled={page >= totalPages}>»</button>
        </li>
      </ul>
    </nav>
  );
}
