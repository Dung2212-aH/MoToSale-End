import api from './api';

const productService = {
  getAll: (params) => api.get('/products', { params }),
  getById: (id) => api.get(`/products/${id}`),
  create: (data) => api.post('/products', data),
  update: (id, data) => api.patch(`/products/${id}`, data),
  delete: (id) => api.delete(`/products/${id}`),
  getVariants: (productId) => api.get(`/products/${productId}/variants`),
  createVariant: (productId, data) => api.post(`/products/${productId}/variants`, data),
  updateVariant: (productId, variantId, data) => api.patch(`/products/${productId}/variants/${variantId}`, data),
  deleteVariant: (productId, variantId) => api.delete(`/products/${productId}/variants/${variantId}`),
  getImages: (productId) => api.get(`/products/${productId}/images`),
  uploadImage: (productId, formData) => api.post(`/products/${productId}/images`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
  deleteImage: (productId, imageId) => api.delete(`/products/${productId}/images/${imageId}`),
};

export default productService;
