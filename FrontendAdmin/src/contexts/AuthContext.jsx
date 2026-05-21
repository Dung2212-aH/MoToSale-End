import React, { createContext, useContext, useState, useEffect } from 'react';
import authService from '../services/authService';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem('admin_user');
    const token = localStorage.getItem('admin_token');
    if (storedUser && token) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = async (email, password) => {
    try {
      const response = await authService.login(email, password);
      const data = response.data;
      const authUser = data.user || data;

      localStorage.setItem('admin_token', data.token);
      localStorage.setItem('admin_user', JSON.stringify(authUser));
      setUser(authUser);

      return { success: true };
    } catch (error) {
      const resp = error.response;
      let message = 'Đăng nhập thất bại. Vui lòng thử lại.';
      if (resp?.status === 401) {
        message = resp.data?.message || 'Email hoặc mật khẩu không đúng.';
      } else if (resp?.status === 400) {
        message = resp.data?.message || 'Thông tin đăng nhập không hợp lệ.';
      } else if (resp?.data?.message) {
        message = resp.data.message;
      }
      return { success: false, message };
    }
  };

  const logout = () => {
    localStorage.removeItem('admin_token');
    localStorage.removeItem('admin_user');
    setUser(null);
  };

  const isAdmin = () => {
    return user?.role === 'Admin' || user?.roles?.includes('Admin') || user?.Roles?.includes('Admin');
  };

  const value = {
    user,
    login,
    logout,
    isAdmin,
    isAuthenticated: !!user,
    loading,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext;
