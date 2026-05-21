import React from 'react';
import Sidebar from './Sidebar';
import Navbar from './Navbar';
import Footer from './Footer';

const MainLayout = ({ children }) => {
  return (
    <div className="wrapper">
      <Navbar />
      <Sidebar />
      {children}
      <Footer />
    </div>
  );
};

export default MainLayout;
