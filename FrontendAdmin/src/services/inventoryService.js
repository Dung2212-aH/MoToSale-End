import api from './api';

const inventoryService = {
  getAll: (params) => api.get('/inventory', { params }),
  sync: () => api.post('/inventory/sync'),
  getHolds: (params) => api.get('/inventory/holds', { params }),
  getAdjustments: (params) => api.get('/inventory/adjustments', { params }),
  updateThreshold: (payload) => api.put('/inventory/threshold', payload),
  adjustStock: (payload) => api.post('/inventory/adjust', payload),
  exportCsv: (params) => api.get('/inventory/export', { params, responseType: 'blob' }),
};

export default inventoryService;
