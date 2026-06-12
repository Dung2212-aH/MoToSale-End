import api from './api';

const paymentService = {
  getAll: (params) => api.get('/payments', { params }),
  confirm: (id) => api.patch(`/payments/${id}/confirm`, {}),
  cancel: (id, data) => api.patch(`/payments/${id}/cancel`, { ...data, lyDoHuy: data?.lyDoHuy || data?.reason }),
};

export default paymentService;
