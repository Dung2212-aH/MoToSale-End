import api from './api';

const inventoryService = {
  getAll: (params) => api.get('/inventory', { params }),
  sync: () => api.post('/inventory/sync'),
};

export default inventoryService;
