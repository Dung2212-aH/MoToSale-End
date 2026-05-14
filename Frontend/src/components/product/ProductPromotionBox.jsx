const defaultPromotions = [
  'Áp dụng Phiếu quà tặng / Mã giảm giá theo sản phẩm.',
  'Giảm giá 10% khi mua từ 5 sản phẩm trở lên.',
  'Tặng 100.000đ mua hàng online tại hệ thống EURO Moto.',
];

function ProductPromotionBox({ promotions = defaultPromotions }) {
  return (
    <div className="rounded-2xl border border-[#f3c7c9] bg-[#fff7f7] p-5">
      <div className="flex items-center gap-3 border-b border-[#f0d6d7] pb-3">
        <span className="grid h-9 w-9 place-items-center rounded-full bg-[#d71920] text-white">
          <svg viewBox="0 0 16 16" className="h-4 w-4 fill-current" aria-hidden="true">
            <path d="M5.52.359A.5.5 0 0 1 6 0h4a.5.5 0 0 1 .474.658L8.694 6H12.5a.5.5 0 0 1 .395.807l-7 9a.5.5 0 0 1-.873-.454L6.823 9.5H3.5a.5.5 0 0 1-.48-.641l2.5-8.5z" />
          </svg>
        </span>
        <h3 className="text-base font-bold text-zinc-950">Danh sách khuyến mãi</h3>
      </div>

      <ul className="mt-4 space-y-3 text-sm leading-6 text-zinc-700">
        {promotions.slice(0, 3).map((promotion) => (
          <li key={promotion} className="flex items-start gap-3">
            <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-[#d71920]" />
            <span>{promotion}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default ProductPromotionBox;
