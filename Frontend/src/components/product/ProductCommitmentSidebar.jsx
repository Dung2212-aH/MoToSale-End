const commitments = [
  'Sản phẩm chính hãng',
  'Giá tốt trực tiếp',
  'Combo quà chất lượng',
  'Trả góp lãi suất thấp',
  'Bảo hành 3 - 5 năm',
  'Giao hàng tận nhà',
];

function ProductCommitmentSidebar() {
  return (
    <aside className="rounded-2xl border border-zinc-200 bg-white p-5">
      <h3 className="border-b border-zinc-100 pb-3 text-lg font-bold text-zinc-950">Cam kết bán hàng</h3>

      <ul className="mt-4 space-y-3">
        {commitments.map((item, index) => (
          <li key={item} className="flex items-center gap-3 rounded-2xl bg-zinc-50 px-3 py-3">
            <span className="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-[#fff1f2] text-[#d71920]">
              <svg viewBox="0 0 24 24" className="h-5 w-5 fill-current" aria-hidden="true">
                <path d={index % 2 === 0 ? 'M12 2 4 5v6c0 5 3.4 9.7 8 11 4.6-1.3 8-6 8-11V5l-8-3Zm-1 14-4-4 1.4-1.4 2.6 2.6 5.6-5.6L18 9l-7 7Z' : 'M12 2a10 10 0 1 0 10 10A10 10 0 0 0 12 2Zm4.3 8.7-4.9 4.9a1 1 0 0 1-1.4 0l-2.3-2.3 1.4-1.4 1.6 1.6 4.2-4.2Z'} />
              </svg>
            </span>
            <span className="text-sm font-medium text-zinc-700">{item}</span>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export default ProductCommitmentSidebar;
