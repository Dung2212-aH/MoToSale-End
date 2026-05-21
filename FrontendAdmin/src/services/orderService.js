import api from './api';

const orderService = {
  getAll: (params) => api.get('/orders', { params }),
  getById: (id) => api.get(`/orders/${id}`),
  updateStatus: (id, data) => api.put(`/orders/${id}/status`, data),
  cancel: (id, data) => api.put(`/orders/${id}/cancel`, { ...data, lyDoHuyDon: data?.lyDoHuyDon || data?.reason }),
};

export default orderService;
