import api from './api';

const authService = {
  login: (email, password) => api.post('/auth/login', { email, matKhau: password }),
  getMe: () => api.get('/users/me'),
};

export default authService;
