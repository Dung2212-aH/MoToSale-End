import React from 'react';
import { cn } from '../../utils/cn';

export function Loading({ label = 'Đang tải...', className }) {
  return (
    <div className={cn('py-6 text-center text-primary', className)}>
      <span className="spinner-border" role="status" aria-hidden="true" />
      <span className="sr-only">{label}</span>
    </div>
  );
}

export function EmptyState({ icon = 'fas fa-inbox', title = 'Không có dữ liệu', className }) {
  return (
    <div className={cn('py-6 text-center text-muted', className)}>
      <i className={cn(icon, 'mb-2 text-2xl')} />
      <p className="m-0">{title}</p>
    </div>
  );
}
