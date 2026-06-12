import api from './api';

const authService = {
  login: (email, password) => api.post('/auth/login', { Email: email, MatKhau: password }),
};

export default authService;
