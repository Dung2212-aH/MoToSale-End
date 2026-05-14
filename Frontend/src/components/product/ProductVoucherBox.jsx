import { useEffect, useState } from 'react';

import { voucherApi } from '../../services/api.js';

function formatMoney(value) {
  return `${Number(value || 0).toLocaleString('vi-VN')}đ`;
}

function formatVoucherDescription(voucher) {
  if (voucher.description) {
    return voucher.description;
  }

  const minOrder = formatMoney(voucher.minOrderValue);

  if (voucher.discountType === 'Percent') {
    const maxDiscount = voucher.maxDiscountValue ? `, tối đa ${formatMoney(voucher.maxDiscountValue)}` : '';
    return `Giảm ${Number(voucher.discountValue || 0).toLocaleString('vi-VN')}% cho đơn từ ${minOrder}${maxDiscount}.`;
  }

  if (voucher.discountType === 'FreeShipping') {
    return `Miễn phí vận chuyển cho đơn từ ${minOrder}.`;
  }

  return `Giảm ${formatMoney(voucher.discountValue)} cho đơn từ ${minOrder}.`;
}

function ProductVoucherBox({ product }) {
  const [vouchers, setVouchers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [copiedCode, setCopiedCode] = useState('');

  useEffect(() => {
    let alive = true;

    async function loadVouchers() {
      setLoading(true);

      try {
        const data = await voucherApi.listVouchers({
          productId: product?.id,
          categoryId: product?.categoryId,
          brandId: product?.brandId,
        });

        if (alive) {
          setVouchers(Array.isArray(data) ? data : data?.$values || []);
        }
      } catch {
        if (alive) {
          setVouchers([]);
        }
      } finally {
        if (alive) {
          setLoading(false);
        }
      }
    }

    loadVouchers();

    return () => {
      alive = false;
    };
  }, [product?.id, product?.categoryId, product?.brandId]);

  async function copyCode(code) {
    try {
      await navigator.clipboard.writeText(code);
      setCopiedCode(code);
      window.setTimeout(() => setCopiedCode(''), 1800);
    } catch {
      setCopiedCode('');
    }
  }

  if (!loading && vouchers.length === 0) {
    return null;
  }

  return (
    <div className="rounded-2xl border border-zinc-200 bg-white">
      <div className="flex items-center gap-3 border-b border-zinc-100 bg-[linear-gradient(90deg,#fff5f5,#ffffff)] px-5 py-4">
        <div className="grid h-11 w-11 place-items-center rounded-full bg-[#fff0f0] text-[#d71920]">
          <svg viewBox="0 0 24 24" className="h-6 w-6 fill-current" aria-hidden="true">
            <path d="M5 3h8a2 2 0 0 1 2 2v2h1a3 3 0 0 1 3 3v3h-2v-3a1 1 0 0 0-1-1h-1v2a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Zm0 2v6h8V5H5Zm3 10h11a2 2 0 0 1 2 2v4H10a2 2 0 0 1-2-2v-4Zm2 2v2h9v-2h-9Z" />
          </svg>
        </div>
        <div>
          <div className="text-base font-bold text-zinc-950">Nhận voucher ngay !!!</div>
          <div className="text-sm text-zinc-500">Lưu mã và dùng ở bước thanh toán.</div>
        </div>
      </div>

      <div className="grid gap-4 p-5 lg:grid-cols-3">
        {loading &&
          [1, 2, 3].map((item) => (
            <div key={item} className="h-32 animate-pulse rounded-2xl border border-dashed border-zinc-200 bg-zinc-50" />
          ))}

        {!loading &&
          vouchers.map((voucher) => {
            const copied = copiedCode === voucher.code;

            return (
              <div key={voucher.code} className="rounded-2xl border border-dashed border-[#f3c7c9] bg-[#fff9f9] p-4">
                <div className="text-sm leading-6 text-zinc-700">
                  Nhập mã <b>{voucher.code}</b>. {formatVoucherDescription(voucher)}
                </div>
                <button
                  type="button"
                  className={`mt-4 inline-flex min-h-10 items-center justify-center rounded-xl px-4 text-sm font-bold transition ${
                    copied ? 'bg-zinc-900 text-white' : 'bg-[#d71920] text-white hover:bg-[#b9161c]'
                  }`}
                  onClick={() => copyCode(voucher.code)}
                >
                  {copied ? 'Đã sao chép' : 'Sao chép'}
                </button>
              </div>
            );
          })}
      </div>
    </div>
  );
}

export default ProductVoucherBox;
