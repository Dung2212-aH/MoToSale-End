export function formatCurrency(value) {
  const amount = Number(value || 0);
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

export function normalizeImageUrl(url) {
  if (!url) {
    return '';
  }
  if (/^(https?:|data:|blob:)/i.test(url)) {
    return url;
  }
  if (url.startsWith('//')) {
    return `https:${url}`;
  }
  if (url.startsWith('/uploads/')) {
    const assetBaseUrl = import.meta.env.VITE_API_ASSET_BASE_URL || '';
    return `${assetBaseUrl}${url}`;
  }
  return url;
}

export function getProductPrice(product) {
  return product?.salePrice ?? product?.basePrice ?? product?.price ?? 0;
}

export function getProductDiscountPercent(product) {
  const explicitPercent = product?.discountPercent ?? product?.tyLeGiam ?? product?.TyLeGiam;

  if (Number(explicitPercent) > 0) {
    return Math.round(Number(explicitPercent));
  }

  const basePrice = Number(product?.basePrice || 0);
  const salePrice = Number(product?.salePrice ?? product?.price ?? 0);

  if (!basePrice || !salePrice || salePrice >= basePrice) {
    return null;
  }

  return Math.round(((basePrice - salePrice) * 100) / basePrice);
}

export function getProductImage(product) {
  const variantImages = (product?.variants || [])
    .flatMap((variant) => variant?.images || [])
    .map((image) => image?.imageUrl)
    .filter(Boolean);
  const linkedVariantImage = product?.images?.find((image) => image?.productVariantId && image?.imageUrl)?.imageUrl;
  const primaryImage = product?.images?.find((image) => image?.isPrimary)?.imageUrl;

  return normalizeImageUrl(
    variantImages[0] ||
      linkedVariantImage ||
      primaryImage ||
      product?.images?.[0]?.imageUrl ||
      product?.mainImageUrl ||
      product?.imageUrl ||
      '',
  );
}
