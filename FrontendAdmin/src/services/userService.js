import api from './api';

const userService = {
  getAll: (params) => api.get('/users/all', { params }),
  getCustomers: (params) => api.get('/users/customers', { params }),
  updateCustomerCareNote: (id, data) => api.patch(`/users/customers/${id}/care-note`, data),
  getById: (id) => api.get(`/users/${id}`),
  create: (data) => api.post('/users', { ...data, role: data.vaiTro || data.role }),
  update: (id, data) => api.put(`/users/${id}`, { ...data, role: data.vaiTro || data.role }),
  updateStatus: (id, data) => api.patch(`/users/${id}/status`, data),
  delete: (id) => api.delete(`/users/${id}`),
};

export default userService;
