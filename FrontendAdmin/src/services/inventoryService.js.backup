import api from './api';

const inventoryService = {
  getAll: (params) => api.get('/inventory', { params }),
  sync: () => api.post('/inventory/sync'),
  getHolds: (params) => api.get('/inventory/holds', { params }),
  getAdjustments: (params) => api.get('/inventory/adjustments', { params }),
  getDocuments: (params) => api.get('/inventory/documents', { params }),
  getDocumentById: (id) => api.get(`/inventory/documents/${id}`),
  createDocument: (payload) => api.post('/inventory/documents', payload),
  approveDocument: (id) => api.post(`/inventory/documents/${id}/approve`),
  cancelDocument: (id, payload) => api.post(`/inventory/documents/${id}/cancel`, payload),
  updateThreshold: (payload) => api.put('/inventory/threshold', payload),
  adjustStock: (payload) => api.post('/inventory/adjust', payload),
  exportCsv: (params) => api.get('/inventory/export', { params, responseType: 'blob' }),
};

export default inventoryService;
