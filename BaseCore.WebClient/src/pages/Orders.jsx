import React, { useCallback, useEffect, useState } from 'react';
import AdminPage from '../components/admin/AdminPage';
import DataTable, { FilterBar, Pagination } from '../components/admin/DataTable';
import { TextArea, SelectInput, StatusBadge, formatMoney } from '../components/admin/FormControls';
import { ConfirmActionButton, ErrorState, LoadingState } from '../components/admin/UiState';
import { getApiErrorMessage, normalizePagedResponse, orderApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const orderStatuses = ['Pending', 'Checkout', 'AwaitingPayment', 'Confirmed', 'Processing', 'Completed', 'Cancelled'];
const paymentStatuses = ['Unpaid', 'Pending', 'Paid', 'PartiallyPaid', 'Refunded', 'PartiallyRefunded', 'Failed'];
const shippingStatuses = ['NotShipped', 'Preparing', 'Shipping', 'Delivered', 'PickupReady', 'PickedUp', 'Cancelled'];

const Orders = () => {
  const { user, isAdmin } = useAuth();
  const canManage = isAdmin() || user?.roles?.includes('Staff') || user?.role === 'Staff';

  const [orders, setOrders] = useState([]);
  const [query, setQuery] = useState({ trangThaiDonHang: '', trangThaiThanhToan: '' });
  const [page, setPage] = useState(1);
  const [pageSize] = useState(12);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detail, setDetail] = useState(null);
  const [statusForm, setStatusForm] = useState({ orderStatus: '', shippingStatus: '', fulfillmentNote: '' });
  const [cancelReason, setCancelReason] = useState('');

  const loadOrders = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize };
      if (query.trangThaiDonHang) params.trangThaiDonHang = query.trangThaiDonHang;
      if (query.trangThaiThanhToan) params.trangThaiThanhToan = query.trangThaiThanhToan;

      const response = await orderApi.getAll(params);
      const result = normalizePagedResponse(response.data);
      setOrders(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc don hang.'));
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, query]);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  const loadDetail = async (order) => {
    setShowModal(true);
    setDetail(null);
    setDetailLoading(true);
    setError('');
    try {
      const response = await orderApi.getById(order.maDonHang);
      const payload = response.data;
      setDetail(payload);
      setStatusForm({
        orderStatus: payload.trangThaiDonHang || '',
        shippingStatus: payload.trangThaiVanChuyen || '',
        fulfillmentNote: payload.ghiChuGiaoNhan || '',
      });
      setCancelReason('');
    } catch (err) {
      setError(getApiErrorMessage(err, 'Khong tai duoc chi tiet don hang.'));
    } finally {
      setDetailLoading(false);
    }
  };

  const closeModal = () => {
    if (saving) return;
    setShowModal(false);
    setDetail(null);
    setCancelReason('');
  };

  const refreshDetail = async () => {
    if (!detail?.maDonHang) return;
    const response = await orderApi.getById(detail.maDonHang);
    setDetail(response.data);
    await loadOrders();
  };

  const runAction = async (action, fallback) => {
    setSaving(true);
    setError('');
    try {
      await action();
      await refreshDetail();
    } catch (err) {
      setError(getApiErrorMessage(err, fallback));
    } finally {
      setSaving(false);
    }
  };

  const updateOrderStatus = (event) => {
    event.preventDefault();
    if (!detail?.maDonHang || !statusForm.orderStatus) return;
    runAction(() => orderApi.updateStatus(detail.maDonHang, statusForm.orderStatus), 'Khong cap nhat duoc trang thai don hang.');
  };

  const updateShippingStatus = (event) => {
    event.preventDefault();
    if (!detail?.maDonHang || !statusForm.shippingStatus) return;
    runAction(() => orderApi.updateShipping(detail.maDonHang, statusForm), 'Khong cap nhat duoc trang thai giao nhan.');
  };

  const cancelOrder = () => {
    if (!detail?.maDonHang || !cancelReason.trim()) {
      setError('Cancellation reason is required.');
      return;
    }
    runAction(() => orderApi.cancel(detail.maDonHang, cancelReason.trim()), 'Khong huy duoc don hang.');
  };

  const columns = [
    {
      key: 'maDonHangKinhDoanh',
      label: 'Order',
      render: (order) => (
        <div>
          <button type="button" className="btn btn-link p-0 font-weight-bold" onClick={() => loadDetail(order)}>
            {order.maDonHangKinhDoanh || `#${order.maDonHang}`}
          </button>
          <div className="text-muted small">{order.ngayTao ? new Date(order.ngayTao).toLocaleString('vi-VN') : '-'}</div>
        </div>
      ),
    },
    { key: 'loaiDonHang', label: 'Type' },
    { key: 'tongThanhToan', label: 'Total', className: 'text-right', render: (order) => formatMoney(order.tongThanhToan) },
    { key: 'trangThaiDonHang', label: 'Order', render: (order) => <StatusBadge value={order.trangThaiDonHang} /> },
    { key: 'trangThaiThanhToan', label: 'Payment', render: (order) => <StatusBadge value={order.trangThaiThanhToan} /> },
    { key: 'trangThaiVanChuyen', label: 'Shipping', render: (order) => <StatusBadge value={order.trangThaiVanChuyen} /> },
  ];

  const itemColumns = [
    { key: 'tenSanPhamSnapshot', label: 'Item', render: (item) => <strong>{item.tenSanPhamSnapshot}</strong> },
    { key: 'skuSnapshot', label: 'SKU', render: (item) => item.skuSnapshot || '-' },
    { key: 'soLuong', label: 'Qty', className: 'text-right' },
    { key: 'donGia', label: 'Unit price', className: 'text-right', render: (item) => formatMoney(item.donGia) },
    { key: 'thanhTien', label: 'Total', className: 'text-right', render: (item) => formatMoney(item.thanhTien) },
  ];

  return (
    <AdminPage title="Orders" subtitle="Order endpoints currently available in OrderService.">
      {error && <ErrorState message={error} onRetry={loadOrders} />}

      <div className="card">
        <div className="card-header">
          <FilterBar onSubmit={(event) => { event.preventDefault(); setPage(1); loadOrders(); }}>
            <div className="col-md-4">
              <SelectInput label="Order status" value={query.trangThaiDonHang} onChange={(event) => { setPage(1); setQuery((current) => ({ ...current, trangThaiDonHang: event.target.value })); }}>
                <option value="">All order statuses</option>
                {orderStatuses.map((status) => <option key={status} value={status}>{status}</option>)}
              </SelectInput>
            </div>
            <div className="col-md-4">
              <SelectInput label="Payment status" value={query.trangThaiThanhToan} onChange={(event) => { setPage(1); setQuery((current) => ({ ...current, trangThaiThanhToan: event.target.value })); }}>
                <option value="">All payment statuses</option>
                {paymentStatuses.map((status) => <option key={status} value={status}>{status}</option>)}
              </SelectInput>
            </div>
            <div className="col-md-2 d-flex align-items-end">
              <button type="submit" className="btn btn-primary btn-block">Apply</button>
            </div>
            <div className="col-md-2 d-flex align-items-end">
              <button type="button" className="btn btn-outline-secondary btn-block" onClick={() => { setQuery({ trangThaiDonHang: '', trangThaiThanhToan: '' }); setPage(1); }}>
                Reset
              </button>
            </div>
          </FilterBar>
        </div>
        <div className="card-body p-0">
          {loading ? <LoadingState label="Loading orders..." /> : <DataTable columns={columns} rows={orders} rowKey="maDonHang" emptyTitle="No orders found" />}
        </div>
        <div className="card-footer">
          <Pagination page={page} totalPages={totalPages} totalCount={totalCount} label="orders" onPageChange={setPage} />
        </div>
      </div>

      {showModal && (
        <>
          <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1" role="dialog" aria-modal="true">
            <div className="modal-dialog modal-xl" role="document">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Order {detail?.maDonHangKinhDoanh || ''}</h5>
                  <button type="button" className="close" onClick={closeModal} disabled={saving}>
                    <span>&times;</span>
                  </button>
                </div>
                <div className="modal-body">
                  {detailLoading || !detail ? <LoadingState label="Loading order detail..." /> : (
                    <>
                      <div className="row">
                        <div className="col-md-4">
                          <div className="admin-form-section">
                            <h3 className="admin-section-title">Customer</h3>
                            <div>{detail.hoTenNhanHang}</div>
                            <div className="text-muted small">{detail.soDienThoaiNhanHang}</div>
                            <div className="text-muted small">{detail.emailNhanHang || '-'}</div>
                            <div className="mt-2">{detail.diaChiNhanHang}</div>
                          </div>
                        </div>
                        <div className="col-md-4">
                          <div className="admin-form-section">
                            <h3 className="admin-section-title">Fulfillment</h3>
                            <div>Method: {detail.phuongThucNhanHang}</div>
                            <div>Showroom ID: {detail.maShowroom || '-'}</div>
                            <div>Appointment: {detail.ngayHenNhanXe ? new Date(detail.ngayHenNhanXe).toLocaleString('vi-VN') : '-'}</div>
                            <div className="text-muted small">{detail.ghiChuGiaoNhan || '-'}</div>
                          </div>
                        </div>
                        <div className="col-md-4">
                          <div className="admin-form-section">
                            <h3 className="admin-section-title">Totals</h3>
                            <div className="d-flex justify-content-between"><span>Subtotal</span><strong>{formatMoney(detail.tongTienHang)}</strong></div>
                            <div className="d-flex justify-content-between"><span>Discount</span><strong>{formatMoney(detail.tienGiam)}</strong></div>
                            <div className="d-flex justify-content-between"><span>Shipping</span><strong>{formatMoney(detail.phiVanChuyen)}</strong></div>
                            <div className="d-flex justify-content-between"><span>Total</span><strong>{formatMoney(detail.tongThanhToan)}</strong></div>
                          </div>
                        </div>
                      </div>

                      <DataTable columns={itemColumns} rows={detail.items || []} rowKey="maChiTietDonHang" emptyTitle="No order items" />

                      {canManage && (
                        <div className="row mt-3">
                          <div className="col-lg-6">
                            <form className="admin-form-section" onSubmit={updateOrderStatus}>
                              <h3 className="admin-section-title">Order status</h3>
                              <SelectInput label="Status" value={statusForm.orderStatus} onChange={(event) => setStatusForm((current) => ({ ...current, orderStatus: event.target.value }))}>
                                {orderStatuses.map((status) => <option key={status} value={status}>{status}</option>)}
                              </SelectInput>
                              <button type="submit" className="btn btn-primary" disabled={saving}>Update order</button>
                            </form>
                          </div>
                          <div className="col-lg-6">
                            <form className="admin-form-section" onSubmit={updateShippingStatus}>
                              <h3 className="admin-section-title">Shipping status</h3>
                              <SelectInput label="Status" value={statusForm.shippingStatus} onChange={(event) => setStatusForm((current) => ({ ...current, shippingStatus: event.target.value }))}>
                                {shippingStatuses.map((status) => <option key={status} value={status}>{status}</option>)}
                              </SelectInput>
                              <TextArea label="Fulfillment note" rows={2} value={statusForm.fulfillmentNote} onChange={(event) => setStatusForm((current) => ({ ...current, fulfillmentNote: event.target.value }))} />
                              <button type="submit" className="btn btn-primary" disabled={saving}>Update shipping</button>
                            </form>
                          </div>
                          <div className="col-lg-12">
                            <div className="admin-form-section mb-0">
                              <h3 className="admin-section-title">Cancel order</h3>
                              <TextArea label="Reason" rows={2} value={cancelReason} onChange={(event) => setCancelReason(event.target.value)} />
                              <ConfirmActionButton className="btn btn-outline-danger" confirmMessage="Cancel this order?" onConfirm={cancelOrder} disabled={saving}>
                                Cancel order
                              </ConfirmActionButton>
                            </div>
                          </div>
                        </div>
                      )}
                    </>
                  )}
                </div>
              </div>
            </div>
          </div>
          <div className="modal-backdrop fade show"></div>
        </>
      )}
    </AdminPage>
  );
};

export default Orders;
