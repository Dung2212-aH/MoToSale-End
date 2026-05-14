import { useCallback, useEffect, useRef, useState } from 'react';
import { getProductImage } from '../../utils/formatters.js';

/**
 * ProductImageGallery – Sliding carousel gallery with smooth transitions.
 *
 * Features:
 *  - Sliding animation when switching images (via color/version/thumbnail)
 *  - Touch / swipe support on mobile
 *  - Auto-scroll thumbnail strip to keep active thumb in view
 *  - Clicking a thumbnail fires onSelectImage (which syncs color in parent)
 *  - Keyboard arrow navigation
 */

function ProductImageGallery({ product, images = [], selectedImage, onSelectImage }) {
  const fallbackImage = getProductImage(product);
  const galleryImages = images.length
    ? images
    : fallbackImage
      ? [{ imageUrl: fallbackImage, altText: product?.name || 'Product' }]
      : [];

  // Active slide index
  const activeIndex = galleryImages.findIndex(
    (img) => img.imageUrl === (selectedImage?.imageUrl || galleryImages[0]?.imageUrl),
  );
  const currentIndex = activeIndex >= 0 ? activeIndex : 0;

  // Refs
  const trackRef = useRef(null);
  const thumbStripRef = useRef(null);
  const containerRef = useRef(null);

  // Touch/swipe state
  const [touchStart, setTouchStart] = useState(null);
  const [touchDelta, setTouchDelta] = useState(0);
  const [isSwiping, setIsSwiping] = useState(false);

  // Slide direction for CSS transition
  const prevImageUrlRef = useRef(galleryImages[currentIndex]?.imageUrl);
  const [slideDirection, setSlideDirection] = useState(null); // 'left' | 'right' | null
  const [isAnimating, setIsAnimating] = useState(false);

  // Navigate to a specific slide
  const goToSlide = useCallback(
    (index) => {
      if (index < 0 || index >= galleryImages.length || index === currentIndex) return;
      const img = galleryImages[index];
      if (img) onSelectImage?.(img);
    },
    [currentIndex, galleryImages, onSelectImage],
  );

  const goNext = useCallback(() => goToSlide(currentIndex + 1), [currentIndex, goToSlide]);
  const goPrev = useCallback(() => goToSlide(currentIndex - 1), [currentIndex, goToSlide]);

  // Detect image change & trigger animation (using URL, not index, since the array reorders on color change)
  useEffect(() => {
    const currentUrl = galleryImages[currentIndex]?.imageUrl;
    if (prevImageUrlRef.current !== currentUrl && currentUrl) {
      const oldIdx = galleryImages.findIndex((img) => img.imageUrl === prevImageUrlRef.current);
      const direction = oldIdx < 0 || currentIndex >= oldIdx ? 'left' : 'right';
      setSlideDirection(direction);
      setIsAnimating(true);
      prevImageUrlRef.current = currentUrl;

      const timer = setTimeout(() => {
        setIsAnimating(false);
        setSlideDirection(null);
      }, 420);

      return () => clearTimeout(timer);
    }
  }, [currentIndex, galleryImages]);

  // Auto-scroll thumbnail strip to keep active thumb visible
  useEffect(() => {
    if (!thumbStripRef.current) return;
    const thumbEl = thumbStripRef.current.children[currentIndex];
    if (thumbEl) {
      thumbEl.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
    }
  }, [currentIndex]);

  // Keyboard navigation
  useEffect(() => {
    function onKeyDown(e) {
      if (!containerRef.current?.contains(document.activeElement) && document.activeElement !== containerRef.current) return;
      if (e.key === 'ArrowLeft') {
        e.preventDefault();
        goPrev();
      } else if (e.key === 'ArrowRight') {
        e.preventDefault();
        goNext();
      }
    }

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [goNext, goPrev]);

  // Touch handlers for swipe
  function handleTouchStart(e) {
    setTouchStart(e.touches[0].clientX);
    setTouchDelta(0);
    setIsSwiping(true);
  }

  function handleTouchMove(e) {
    if (touchStart === null) return;
    const delta = e.touches[0].clientX - touchStart;
    setTouchDelta(delta);
  }

  function handleTouchEnd() {
    if (!isSwiping) return;
    const threshold = 50;

    if (touchDelta < -threshold) {
      goNext();
    } else if (touchDelta > threshold) {
      goPrev();
    }

    setTouchStart(null);
    setTouchDelta(0);
    setIsSwiping(false);
  }

  // Build animation class
  const animationClass = isAnimating
    ? slideDirection === 'left'
      ? 'gallery-slide-in-left'
      : 'gallery-slide-in-right'
    : '';

  return (
    <div
      ref={containerRef}
      className="overflow-hidden rounded-2xl border border-zinc-200 bg-white"
      tabIndex={0}
      role="region"
      aria-label="Bộ sưu tập ảnh sản phẩm"
      style={{ outline: 'none' }}
    >
      {/* Main image area */}
      <div className="relative border-b border-zinc-100 bg-[radial-gradient(circle_at_top,rgba(215,25,32,0.08),transparent_48%),linear-gradient(180deg,#ffffff,#f7f7f7)] p-4 sm:p-6">
        {/* Brand badge */}
        <div className="absolute left-4 top-4 z-10 rounded-full bg-[#d71920] px-3 py-1 text-[11px] font-bold uppercase tracking-[0.18em] text-white">
          EURO Moto
        </div>

        {/* Navigation arrows */}
        {galleryImages.length > 1 && (
          <>
            <button
              type="button"
              className="gallery-nav-btn gallery-nav-prev"
              onClick={goPrev}
              disabled={currentIndex === 0}
              aria-label="Ảnh trước"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="h-5 w-5">
                <polyline points="15 18 9 12 15 6" />
              </svg>
            </button>
            <button
              type="button"
              className="gallery-nav-btn gallery-nav-next"
              onClick={goNext}
              disabled={currentIndex === galleryImages.length - 1}
              aria-label="Ảnh tiếp"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="h-5 w-5">
                <polyline points="9 6 15 12 9 18" />
              </svg>
            </button>
          </>
        )}

        {/* Main image with slide animation */}
        <div
          className="relative flex aspect-square items-center justify-center overflow-hidden rounded-2xl bg-white p-4 sm:p-8"
          ref={trackRef}
          onTouchStart={handleTouchStart}
          onTouchMove={handleTouchMove}
          onTouchEnd={handleTouchEnd}
        >
          {galleryImages[currentIndex] ? (
            <img
              key={galleryImages[currentIndex].imageUrl}
              src={galleryImages[currentIndex].imageUrl}
              alt={galleryImages[currentIndex].altText || product?.name}
              className={`gallery-main-image ${animationClass}`}
              draggable={false}
            />
          ) : (
            <div className="grid h-full w-full place-items-center rounded-2xl bg-zinc-100 text-sm font-bold uppercase tracking-[0.18em] text-zinc-400">
              No Image
            </div>
          )}

          {/* Slide counter */}
          {galleryImages.length > 1 && (
            <div className="absolute bottom-3 right-3 rounded-full bg-black/50 px-3 py-1 text-xs font-semibold text-white backdrop-blur-sm">
              {currentIndex + 1} / {galleryImages.length}
            </div>
          )}
        </div>
      </div>

      {/* Thumbnail strip */}
      <div className="overflow-x-auto px-4 py-4 sm:px-6 gallery-thumb-strip-container">
        <div ref={thumbStripRef} className="flex min-w-max gap-3">
          {galleryImages.map((image, index) => {
            const active = index === currentIndex;

            return (
              <button
                key={`${image.imageUrl}-${index}`}
                type="button"
                className={`gallery-thumb ${active ? 'gallery-thumb--active' : ''}`}
                onClick={() => onSelectImage?.(image)}
                aria-label={`Xem ảnh ${index + 1}`}
                aria-current={active ? 'true' : undefined}
              >
                <img
                  src={image.imageUrl}
                  alt={image.altText || `${product?.name || 'Product'} ${index + 1}`}
                  className="h-full w-full object-contain"
                  draggable={false}
                />
              </button>
            );
          })}
        </div>
      </div>

      {/* Dot indicators for mobile */}
      {galleryImages.length > 1 && galleryImages.length <= 10 && (
        <div className="flex items-center justify-center gap-2 pb-4 sm:hidden">
          {galleryImages.map((_, index) => (
            <button
              key={index}
              type="button"
              className={`gallery-dot ${index === currentIndex ? 'gallery-dot--active' : ''}`}
              onClick={() => goToSlide(index)}
              aria-label={`Đến ảnh ${index + 1}`}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default ProductImageGallery;
