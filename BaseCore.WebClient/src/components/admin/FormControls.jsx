import React from 'react';

export const Field = ({ label, children, required, hint, className = 'form-group' }) => (
    <div className={className}>
        {label && (
            <label>
                {label}
                {required && <span className="text-danger ml-1">*</span>}
            </label>
        )}
        {children}
        {hint && <small className="form-text text-muted">{hint}</small>}
    </div>
);

export const TextInput = ({ label, required, hint, ...props }) => (
    <Field label={label} required={required} hint={hint}>
        <input className="form-control" required={required} {...props} />
    </Field>
);

export const SelectInput = ({ label, required, hint, children, ...props }) => (
    <Field label={label} required={required} hint={hint}>
        <select className="form-control" required={required} {...props}>
            {children}
        </select>
    </Field>
);

export const TextArea = ({ label, required, hint, rows = 3, ...props }) => (
    <Field label={label} required={required} hint={hint}>
        <textarea className="form-control" rows={rows} required={required} {...props} />
    </Field>
);

export const SwitchInput = ({ id, label, checked, onChange, disabled }) => (
    <div className="custom-control custom-switch">
        <input
            type="checkbox"
            className="custom-control-input"
            id={id}
            checked={checked}
            disabled={disabled}
            onChange={(event) => onChange(event.target.checked)}
        />
        <label className="custom-control-label" htmlFor={id}>{label}</label>
    </div>
);

export const displayText = (value) => {
    const labels = {
        Active: 'Active',
        Inactive: 'Inactive',
        Available: 'Available',
        Reserved: 'Reserved',
        Sold: 'Sold',
        Hidden: 'Hidden',
        Motorcycle: 'Motorcycle',
        Accessory: 'Accessory',
        Pending: 'Pending',
        Checkout: 'Checkout',
        AwaitingPayment: 'Awaiting payment',
        Confirmed: 'Confirmed',
        Processing: 'Processing',
        Completed: 'Completed',
        Cancelled: 'Cancelled',
        Unpaid: 'Unpaid',
        Paid: 'Paid',
        PartiallyPaid: 'Partially paid',
        Refunded: 'Refunded',
        PartiallyRefunded: 'Partially refunded',
        Failed: 'Failed',
        NotShipped: 'Not shipped',
        Preparing: 'Preparing',
        Shipping: 'Shipping',
        Delivered: 'Delivered',
        PickedUp: 'Picked up at showroom',
        FullPayment: 'Full payment',
        Deposit: 'Deposit',
        Installment: 'Installment',
        Full: 'Full payment',
        Remaining: 'Remaining',
        Draft: 'Draft',
        Published: 'Published',
        Archived: 'Archived',
        New: 'New',
        Resolved: 'Resolved',
        Closed: 'Closed',
        Approved: 'Approved',
        Rejected: 'Rejected',
        Primary: 'Primary image',
    };

    return labels[value] || value || '-';
};

export const StatusBadge = ({ value }) => {
    const normalized = String(value || '').toLowerCase();
    const badgeClass =
        normalized.includes('active') || normalized.includes('available') || normalized.includes('paid') || normalized.includes('published')
            ? 'badge-success'
            : normalized.includes('pending') || normalized.includes('processing') || normalized.includes('awaiting')
                ? 'badge-warning'
                : normalized.includes('cancel') || normalized.includes('failed') || normalized.includes('hidden') || normalized.includes('sold')
                    ? 'badge-danger'
                    : 'badge-secondary';

    return <span className={`badge ${badgeClass}`}>{displayText(value)}</span>;
};

export const formatMoney = (value) => {
    const amount = Number(value || 0);
    return amount.toLocaleString('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 });
};

