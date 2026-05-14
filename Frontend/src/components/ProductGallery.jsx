import { useEffect, useMemo, useState } from 'react';
import { getProductImage } from '../utils/formatters.js';

function ProductGallery({ product }) {
  const images = useMemo(() => {
    const productImages = product?.images?.map((item) => item.imageUrl || item.url) || [];
    return [...new Set([product?.mainImageUrl, ...productImages].filter(Boolean))];
  }, [product]);

  const [active, setActive] = useState(images[0]);

  useEffect(() => {
    setActive(images[0]);
  }, [images]);

  const activeImage = active || getProductImage(product);

  return (
    <div className="space-y-4">
      <div className="relative overflow-hidden rounded-[28px] border border-zinc-200 bg-[radial-gradient(circle_at_top_left,rgba(255,255,255,0.95),rgba(244,244,245,0.85)_35%,rgba(228,228,231,0.7))] p-6 shadow-[0_24px_60px_rgba(15,23,42,0.12)]">
        <div className="absolute inset-x-0 top-0 h-28 bg-[linear-gradient(180deg,rgba(215,25,32,0.08),rgba(215,25,32,0))]" />

        <div className="relative grid aspect-square place-items-center rounded-[22px] bg-white/80 p-4">
          {activeImage ? (
            <img src={activeImage} alt={product?.name} className="h-full w-full object-contain" />
          ) : (
            <span className="grid h-full w-full place-items-center rounded-[22px] bg-zinc-100 text-sm font-black uppercase tracking-[0.18em] text-zinc-400">
              EURO Moto
            </span>
          )}
        </div>
      </div>

      {images.length > 1 && (
        <div className="grid grid-cols-4 gap-3 sm:grid-cols-6">
          {images.slice(0, 8).map((image) => (
            <button
              key={image}
              type="button"
              className={`overflow-hidden rounded-2xl border bg-white p-2 transition ${
                image === active
                  ? 'border-[#d71920] shadow-[0_12px_24px_rgba(215,25,32,0.18)]'
                  : 'border-zinc-200 hover:border-zinc-300 hover:shadow-[0_10px_22px_rgba(15,23,42,0.08)]'
              }`}
              onClick={() => setActive(image)}
            >
              <img src={image} alt={product?.name} className="aspect-square w-full object-contain" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

export default ProductGallery;
