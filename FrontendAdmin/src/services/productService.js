import api from './api';

const mapSearchParams = (params = {}) => ({
  ...params,
  giaTu: params.giaTu ?? params.minPrice,
  giaDen: params.giaDen ?? params.maxPrice,
});

const productService = {
  getAll: (params) => api.get('/products', { params: mapSearchParams(params) }),
  create: (data) => api.post('/products', data),
  update: (id, data) => api.patch(`/products/${id}`, data),
  delete: (id) => api.delete(`/products/${id}`),
  getVariants: (productId) => api.get(`/products/${productId}/variants`),
  createVariant: (productId, data) => api.post(`/products/${productId}/variants`, data),
  updateVariant: (productId, variantId, data) => api.patch(`/products/${productId}/variants/${variantId}`, data),
  deleteVariant: (productId, variantId) => api.delete(`/products/${productId}/variants/${variantId}`),
  getImages: (productId) => api.get(`/products/${productId}/images`),
  // Pass undefined Content-Type so axios overrides the instance-level
  // 'application/json' default and auto-sets multipart/form-data with the boundary.
  uploadImage: (productId, formData) => api.post(`/products/${productId}/images`, formData, {
    headers: { 'Content-Type': undefined },
  }),
  deleteImage: (productId, imageId) => api.delete(`/products/${productId}/images/${imageId}`),
  getCompatibilities: (productId) => api.get(`/products/${productId}/compatibilities`),
  createCompatibility: (productId, data) => api.post(`/products/${productId}/compatibilities`, data),
  updateCompatibility: (productId, compatibilityId, data) => api.put(`/products/${productId}/compatibilities/${compatibilityId}`, data),
  deleteCompatibility: (productId, compatibilityId) => api.delete(`/products/${productId}/compatibilities/${compatibilityId}`),
  // Sản phẩm bán kèm / liên quan
  getRelatedProducts: (productId) => api.get(`/products/${productId}/related`),
  createRelatedItem: (productId, data) => api.post(`/products/${productId}/related`, data),
  updateRelatedItem: (productId, relatedId, data) => api.put(`/products/${productId}/related/${relatedId}`, data),
  deleteRelatedItem: (productId, relatedId) => api.delete(`/products/${productId}/related/${relatedId}`),
  // Khuyến mại đang áp dụng, tuổi tồn kho, mã vạch
  getApplicableVouchers: (productId) => api.get(`/products/${productId}/promotions`),
  getInventoryAging: (productId) => api.get(`/products/${productId}/inventory-aging`),
  getBarcodes: (productId) => api.get(`/products/${productId}/barcodes`),
};

export default productService;
