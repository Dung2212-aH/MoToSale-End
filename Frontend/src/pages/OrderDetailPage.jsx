import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { orderApi, paymentApi } from '../services/api.js';
import Breadcrumb from '../components/Breadcrumb.jsx';
import { useAuth } from '../contexts/AuthContext.jsx';
import { formatCurrency } from '../utils/formatters.js';
import {
  getOrderStatusLabel, getOrderStatusColor,
  getShippingStatusLabel, getShippingStatusColor,
  getPaymentStatusLabel, getPaymentStatusColor,
  getPaymentMethodLabel, getOrderTypeLabel, getReceivingMethodLabel,
} from '../utils/statusMappings.js';
import ReviewModal from '../components/product/ReviewModal.jsx';

function Badge({ label, colorClass }) {
  return <span className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-bold ${colorClass}`}>{label}</span>;
}

const SHIPPING_STEPS = ['NotShipped', 'AwaitingPickup', 'Preparing', 'InTransit', 'Delivered'];

function OrderDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isAuthenticated: isAuth } = useAuth();
  const [order, setOrder] = useState(null);
  const [details, setDetails] = useState([]);
  const [vouchers, setVouchers] = useState([]);
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cancelling, setCancelling] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [reviewProduct, setReviewProduct] = useState(null);

  useEffect(() => {
    if (!isAuth) { navigate('/login', { replace: true }); return; }
    fetchOrder();
  }, [id, isAuth]);

  async function fetchOrder() {
    setLoading(true); setError('');
    try {
      const res = await orderApi.getOrderById(id);
      setOrder(res.order || res);
      setDetails(res.details?.$values || res.details || []);
      setVouchers(res.vouchers?.$values || res.vouchers || []);
      // Fetch payments
      try {
        const payRes = await paymentApi.getPaymentsByOrder(id);
        setPayments(Array.isArray(payRes) ? payRes : payRes?.$values || []);
      } catch { setPayments([]); }
    } catch (err) {
      setError(err?.message || 'Không thể tải thông tin đơn hàng');
    } finally { setLoading(false); }
  }

  async function handleCancel() {
    setCancelling(true);
    try {
      await orderApi.cancelOrder(id, cancelReason.trim() || null);
      setShowCancelModal(false);
      fetchOrder();
    } catch (err) {
      setError(err?.response?.data?.message || err?.message || 'Hủy đơn thất bại');
    } finally { setCancelling(false); }
  }

  if (!isAuth) return null;

  if (loading) return (
    <div className="flex items-center justify-center py-32">
      <svg className="h-8 w-8 animate-spin text-[#d71920]" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
    </div>
  );

  if (error) return (
    <div className="mx-auto max-w-[800px] px-4 py-16 text-center">
      <div className="rounded-2xl border border-red-200 bg-red-50 p-6 text-sm text-red-700">{error}
        <button onClick={fetchOrder} className="ml-2 font-bold text-[#d71920] hover:underline">Thử lại</button>
      </div>
    </div>
  );

  if (!order) return null;

  const canCancel = ['Pending', 'AwaitingPayment'].includes(order.orderStatus);
  const currentShipIdx = SHIPPING_STEPS.indexOf(order.shippingStatus);

  return (
    <>
      <Breadcrumb items={[{ label: 'Đơn hàng', to: '/orders' }, { label: `#${order.orderCode || order.id}` }]} />

      <section className="bg-[linear-gradient(180deg,#f5f6f8_0%,#ffffff_26%)] px-4 py-10">
        <div className="mx-auto w-full max-w-[1000px] space-y-6">

          {/* ── Header ── */}
          <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <div>
                <div className="text-[11px] font-extrabold uppercase tracking-[0.18em] text-zinc-400">Đơn hàng</div>
                <h1 className="mt-1 text-[26px] font-black text-zinc-950 sm:text-[30px]">#{order.orderCode || order.id}</h1>
                <p className="mt-1 text-sm text-zinc-500">
                  Đặt ngày {new Date(order.createdAt).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
              <div className="flex flex-wrap gap-2">
                <Badge label={getOrderStatusLabel(order.orderStatus)} colorClass={getOrderStatusColor(order.orderStatus)} />
                <Badge label={getPaymentStatusLabel(order.paymentStatus)} colorClass={getPaymentStatusColor(order.paymentStatus)} />
              </div>
            </div>
            {canCancel && (
              <button onClick={() => setShowCancelModal(true)} className="mt-4 rounded-full border border-red-200 px-5 py-2 text-sm font-bold text-red-600 transition hover:bg-red-50">Hủy đơn hàng</button>
            )}
          </div>

          {/* ── Shipping Timeline ── */}
          <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-black text-zinc-950">Trạng thái vận chuyển</h2>
            <div className="mt-1 mb-5">
              <Badge label={getShippingStatusLabel(order.shippingStatus)} colorClass={getShippingStatusColor(order.shippingStatus)} />
            </div>
            <div className="flex items-center gap-0">
              {SHIPPING_STEPS.map((step, i) => {
                const done = i <= currentShipIdx && currentShipIdx >= 0;
                const active = i === currentShipIdx;
                return (
                  <div key={step} className="flex flex-1 flex-col items-center relative">
                    {i > 0 && <div className={`absolute top-3 right-1/2 w-full h-0.5 -translate-y-1/2 ${i <= currentShipIdx ? 'bg-green-400' : 'bg-zinc-200'}`} style={{ zIndex: 0 }} />}
                    <div className={`relative z-10 flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold ${active ? 'bg-green-500 text-white ring-4 ring-green-100' : done ? 'bg-green-400 text-white' : 'bg-zinc-200 text-zinc-400'}`}>
                      {done ? '✓' : i + 1}
                    </div>
                    <div className={`mt-2 text-center text-[10px] font-bold leading-tight ${active ? 'text-green-600' : done ? 'text-green-500' : 'text-zinc-400'}`}>
                      {getShippingStatusLabel(step)}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* ── Recipient Info ── */}
          <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-black text-zinc-950">Thông tin nhận hàng</h2>
            <dl className="mt-4 grid gap-3 sm:grid-cols-2">
              <DT label="Họ tên" value={order.shippingFullName} />
              <DT label="Số điện thoại" value={order.shippingPhoneNumber} />
              <DT label="Email" value={order.shippingEmail || '—'} />
              <DT label="Phương thức" value={getReceivingMethodLabel(order.receivingMethod)} />
              <DT label="Địa chỉ" value={[order.shippingAddressLine, order.shippingWard, order.shippingDistrict, order.shippingProvince].filter(Boolean).join(', ') || '—'} className="sm:col-span-2" />
              {order.note && <DT label="Ghi chú" value={order.note} className="sm:col-span-2" />}
            </dl>
          </div>

          {/* ── Products ── */}
          <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-black text-zinc-950">Sản phẩm đã đặt</h2>
            <div className="mt-4 overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-zinc-200 text-xs font-bold uppercase tracking-wider text-zinc-400">
                    <th className="pb-2 text-left">Sản phẩm</th>
                    <th className="pb-2 text-right">Đơn giá</th>
                    <th className="pb-2 text-center">SL</th>
                    <th className="pb-2 text-right">Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  {details.map((d, i) => (
                    <tr key={d.id || i} className="border-b border-zinc-100 last:border-0">
                      <td className="py-3">
                        <div className="font-bold text-zinc-900">{d.productNameSnapshot || d.productName || 'Sản phẩm'}</div>
                        {d.skuSnapshot && <div className="text-xs text-zinc-400 mt-0.5">SKU: {d.skuSnapshot}</div>}
                        {(order.orderStatus === 'Completed' || order.orderStatus === 'Delivered') && (
                          <button
                            onClick={() => setReviewProduct(d)}
                            className="mt-2 text-xs font-bold text-[#d71920] hover:underline"
                          >
                            Đánh giá sản phẩm
                          </button>
                        )}
                      </td>
                      <td className="py-3 text-right whitespace-nowrap">{formatCurrency(d.unitPrice)}</td>
                      <td className="py-3 text-center">{d.quantity}</td>
                      <td className="py-3 text-right whitespace-nowrap font-bold">{formatCurrency(d.lineTotal || d.unitPrice * d.quantity)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* ── Financial Summary ── */}
          <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-black text-zinc-950">Thông tin thanh toán</h2>
            <dl className="mt-4 space-y-3 text-sm">
              <div className="flex justify-between"><span className="text-zinc-500">Loại đơn</span><span className="font-bold">{getOrderTypeLabel(order.orderType)}</span></div>
              <div className="flex justify-between"><span className="text-zinc-500">Tạm tính</span><span className="font-bold">{formatCurrency(order.subtotal)}</span></div>
              {order.discountAmount > 0 && (
                <div className="flex justify-between text-green-600"><span>Giảm voucher</span><span className="font-bold">-{formatCurrency(order.discountAmount)}</span></div>
              )}
              <div className="flex justify-between"><span className="text-zinc-500">Phí giao hàng</span><span className="font-bold">{formatCurrency(order.shippingFee)}</span></div>
              <div className="flex justify-between border-t border-zinc-200 pt-3 text-base text-[#d71920]"><span className="font-extrabold">Tổng cộng</span><span className="text-xl font-black">{formatCurrency(order.totalAmount)}</span></div>
              {order.depositAmount > 0 && (
                <>
                  <div className="flex justify-between text-amber-600"><span>Đặt cọc</span><span className="font-bold">{formatCurrency(order.depositAmount)}</span></div>
                  <div className="flex justify-between"><span className="text-zinc-500">Còn lại</span><span className="font-bold text-red-600">{formatCurrency(order.remainingAmount)}</span></div>
                </>
              )}
            </dl>

            {/* Vouchers applied */}
            {vouchers.length > 0 && (
              <div className="mt-5 border-t border-zinc-200 pt-4">
                <h3 className="text-sm font-bold text-zinc-700">Voucher đã áp dụng</h3>
                {vouchers.map((v, i) => (
                  <div key={i} className="mt-2 flex items-center gap-2 rounded-xl bg-green-50 border border-green-200 px-3 py-2">
                    <span className="text-lg">🎫</span>
                    <div className="flex-1 min-w-0">
                      <span className="text-sm font-bold text-green-700">{v.voucherCodeSnapshot}</span>
                      <span className="ml-2 text-xs text-green-600">({v.discountTypeSnapshot === 'Percent' ? `${v.discountValueSnapshot}%` : formatCurrency(v.discountValueSnapshot)})</span>
                    </div>
                    <span className="text-sm font-bold text-green-700">-{formatCurrency(v.discountAmount)}</span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* ── Payment History ── */}
          {payments.length > 0 && (
            <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
              <h2 className="text-lg font-black text-zinc-950">Lịch sử thanh toán</h2>
              <div className="mt-4 space-y-3">
                {payments.map((p, i) => (
                  <div key={p.id || i} className="flex flex-wrap items-center gap-3 rounded-2xl bg-zinc-50 p-4">
                    <div className="flex-1 min-w-0">
                      <div className="text-sm font-bold text-zinc-900">{getPaymentMethodLabel(p.paymentMethod)}</div>
                      <div className="mt-0.5 text-xs text-zinc-500">
                        {p.paymentCode} • {new Date(p.createdAt).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                      </div>
                      {p.transactionRef && <div className="text-xs text-zinc-400 mt-0.5">Ref: {p.transactionRef}</div>}
                    </div>
                    <div className="text-right">
                      <div className="text-base font-black text-zinc-900">{formatCurrency(p.amount)}</div>
                      <Badge label={getPaymentStatusLabel(p.paymentStatus || p.status)} colorClass={getPaymentStatusColor(p.paymentStatus || p.status)} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex flex-wrap items-center justify-center gap-3 pt-2">
            <Link to="/orders" className="rounded-full border border-zinc-200 px-8 py-3 text-sm font-bold text-zinc-700 transition hover:bg-zinc-100">← Tất cả đơn hàng</Link>
            <Link to="/products" className="rounded-full bg-[#d71920] px-8 py-3 text-sm font-bold text-white transition hover:bg-[#b61016]">Tiếp tục mua sắm</Link>
          </div>
        </div>
      </section>

      {/* Cancel Modal */}
      {showCancelModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md rounded-[22px] bg-white p-6 shadow-2xl">
            <h3 className="text-lg font-black text-zinc-950">Hủy đơn hàng</h3>
            <p className="mt-2 text-sm text-zinc-600">Bạn có chắc muốn hủy đơn #{order.orderCode || order.id}?</p>
            <textarea
              className="mt-4 w-full rounded-xl border border-zinc-200 bg-zinc-50 p-3 text-sm outline-none focus:border-[#d71920]"
              rows={3} placeholder="Lý do hủy (tùy chọn)..."
              value={cancelReason} onChange={(e) => setCancelReason(e.target.value)}
            />
            <div className="mt-4 flex justify-end gap-2">
              <button onClick={() => setShowCancelModal(false)} className="rounded-full border border-zinc-200 px-6 py-2 text-sm font-bold text-zinc-600 transition hover:bg-zinc-100">Đóng</button>
              <button onClick={handleCancel} disabled={cancelling} className="rounded-full bg-red-600 px-6 py-2 text-sm font-bold text-white transition hover:bg-red-700 disabled:bg-zinc-300">
                {cancelling ? 'Đang hủy...' : 'Xác nhận hủy'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Review Modal */}
      <ReviewModal
        isOpen={!!reviewProduct}
        onClose={() => setReviewProduct(null)}
        product={reviewProduct}
        orderId={order.id}
      />
    </>
  );
}

function DT({ label, value, className = '' }) {
  return (
    <div className={className}>
      <dt className="text-xs font-bold uppercase tracking-wider text-zinc-400">{label}</dt>
      <dd className="mt-0.5 text-sm font-medium text-zinc-800">{value}</dd>
    </div>
  );
}

export default OrderDetailPage;
