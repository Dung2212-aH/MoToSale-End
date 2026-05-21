import api from './api';

const voucherService = {
  getAll: (params) => api.get('/vouchers', { params }),
  getById: (id) => api.get(`/vouchers/${id}`),
  create: (data) => api.post('/vouchers', data),
  update: (id, data) => api.put(`/vouchers/${id}`, data),
  delete: (id) => api.delete(`/vouchers/${id}`),
};

export default voucherService;
