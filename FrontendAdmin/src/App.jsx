import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import React, { useEffect } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './components/MainLayout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import ProductList from './pages/products/ProductList';
import CategoryList from './pages/categories/CategoryList';
import BrandList from './pages/brands/BrandList';
import OrderList from './pages/orders/OrderList';
import OrderDetail from './pages/orders/OrderDetail';
import VoucherList from './pages/vouchers/VoucherList';
import InventoryView from './pages/inventory/InventoryView';
import UserList from './pages/users/UserList';
import ReviewList from './pages/reviews/ReviewList';
import PostList from './pages/posts/PostList';
import FaqList from './pages/faq/FaqList';
import ContactList from './pages/contacts/ContactList';
import ReportsPage from './pages/reports/ReportsPage';

const PublicRoute = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '100vh' }}>
        <div className="spinner-border text-primary" role="status">
          <span className="sr-only">Đang tải...</span>
        </div>
      </div>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return children;
};

const ScrollToTop = () => {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);

  return null;
};

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<PublicRoute><Login /></PublicRoute>} />

      {/* Dashboard */}
      <Route path="/" element={<ProtectedRoute><MainLayout><Dashboard /></MainLayout></ProtectedRoute>} />

      {/* Catalog */}
      <Route path="/products" element={<ProtectedRoute><MainLayout><ProductList /></MainLayout></ProtectedRoute>} />
      <Route path="/categories" element={<ProtectedRoute><MainLayout><CategoryList /></MainLayout></ProtectedRoute>} />
      <Route path="/brands" element={<ProtectedRoute><MainLayout><BrandList /></MainLayout></ProtectedRoute>} />

      {/* Orders & Payments */}
      <Route path="/orders" element={<ProtectedRoute><MainLayout><OrderList /></MainLayout></ProtectedRoute>} />
      <Route path="/orders/:id" element={<ProtectedRoute><MainLayout><OrderDetail /></MainLayout></ProtectedRoute>} />
      <Route path="/vouchers" element={<ProtectedRoute><MainLayout><VoucherList /></MainLayout></ProtectedRoute>} />
      <Route path="/inventory" element={<ProtectedRoute><MainLayout><InventoryView /></MainLayout></ProtectedRoute>} />

      {/* Users & Content */}
      <Route path="/users" element={<ProtectedRoute roles={['Admin']}><MainLayout><UserList /></MainLayout></ProtectedRoute>} />
      <Route path="/reviews" element={<ProtectedRoute><MainLayout><ReviewList /></MainLayout></ProtectedRoute>} />
      <Route path="/posts" element={<ProtectedRoute><MainLayout><PostList /></MainLayout></ProtectedRoute>} />
      <Route path="/faq" element={<ProtectedRoute><MainLayout><FaqList /></MainLayout></ProtectedRoute>} />
      <Route path="/contacts" element={<ProtectedRoute><MainLayout><ContactList /></MainLayout></ProtectedRoute>} />

      {/* Reports */}
      <Route path="/reports" element={<ProtectedRoute><MainLayout><ReportsPage /></MainLayout></ProtectedRoute>} />

      {/* Catch all */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <ScrollToTop />
        <AppRoutes />
      </AuthProvider>
    </Router>
  );
}

export default App;
