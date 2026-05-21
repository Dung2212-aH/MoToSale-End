import api from './api';

const paymentService = {
  getAll: (params) => api.get('/payments', { params }),
  getById: (id) => api.get(`/payments/${id}`),
  confirm: (id) => api.patch(`/payments/${id}/confirm`, {}),
  cancel: (id, data) => api.patch(`/payments/${id}/cancel`, { ...data, lyDoHuy: data?.lyDoHuy || data?.reason }),
};

export default paymentService;
