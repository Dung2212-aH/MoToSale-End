import React, { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../services/api';

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
        const storedUser = localStorage.getItem('user');
        const token = localStorage.getItem('token');
        if (storedUser && token) {
            setUser(JSON.parse(storedUser));
        }
        setLoading(false);
    }, []);

    const login = async (email, password) => {
        try {
            const response = await authApi.login(email, password);
            const payload = response.data;
            const userData = payload.user || {};
            const roles = userData.roles || [];
            const role = roles[0] || 'User';

            const authUser = {
                id: userData.id,
                username: userData.email || email,
                name: userData.hoTen || userData.email || email,
                email: userData.email || email,
                phone: userData.soDienThoai || '',
                role,
                roles,
                isActive: (userData.trangThai || 'Active') === 'Active',
            };

            localStorage.setItem('token', payload.token);
            localStorage.setItem('user', JSON.stringify(authUser));
            setUser(authUser);

            return { success: true };
        } catch (error) {
            const message = error.response?.data?.message || 'Dang nhap that bai';
            return { success: false, message };
        }
    };

    const logout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        setUser(null);
    };

    const isAdmin = () => {
        return user?.role === 'Admin' || user?.roles?.includes('Admin');
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
