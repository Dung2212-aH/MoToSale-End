import api from './api';

const send = (method, url, data) => {
  const isFormData = typeof FormData !== 'undefined' && data instanceof FormData;
  return api.request({
    method,
    url,
    data,
    headers: isFormData ? { 'Content-Type': 'multipart/form-data' } : undefined,
  });
};

const brandService = {
  getAll: (params) => api.get('/brands', { params }),
  getById: (id) => api.get(`/brands/${id}`),
  create: (data) => api.post('/brands', data),
  update: (id, data) => api.put(`/brands/${id}`, data),
  uploadLogo: (id, formData) => send('post', `/brands/${id}/logo`, formData),
  delete: (id) => api.delete(`/brands/${id}`),
  // Vehicle Models
  getModels: (brandId) => api.get('/models', { params: { brandId } }),
  getAllModels: (params) => api.get('/models', { params }),
  createModel: (data) => api.post('/models', data),
  updateModel: (id, data) => api.put(`/models/${id}`, data),
  deleteModel: (id) => api.delete(`/models/${id}`),
};

export default brandService;
