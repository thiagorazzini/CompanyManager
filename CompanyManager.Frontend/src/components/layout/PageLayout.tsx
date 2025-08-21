import React from 'react';
import { Outlet } from 'react-router-dom';

const PageLayout: React.FC = () => {
    return (
        <div className="page-layout">
            <header className="page-header">
                <h1>Company Manager</h1>
            </header>
            <main className="page-main">
                <Outlet />
            </main>
            <footer className="page-footer">
                <p>&copy; 2024 Company Manager</p>
            </footer>
        </div>
    );
};

export default PageLayout;
