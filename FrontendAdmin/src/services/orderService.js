import api from './api';

const orderService = {
  getAll: (params) => api.get('/orders', { params }),
  getById: (id) => api.get(`/orders/${id}`),
  updateStatus: (id, data) => api.put(`/orders/${id}/status`, data),
  cancel: (id, data) => api.put(`/orders/${id}/cancel`, { ...data, lyDoHuyDon: data?.lyDoHuyDon || data?.reason }),
  getPaymentInfo: (id) => api.get(`/orders/${id}/payment-info`),
  confirmPayment: (id, data) => api.post(`/orders/${id}/confirm-payment`, data),
  confirmRefund: (id, refundId, data) => api.post(`/orders/${id}/refunds/${refundId}/confirm`, data),
};

export default orderService;
