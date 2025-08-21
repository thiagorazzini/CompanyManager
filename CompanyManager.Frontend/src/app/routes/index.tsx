import { Routes, Route, Navigate } from 'react-router-dom';
import PageLayout from '@components/layout/PageLayout';
import PrivateRoute from '@components/PrivateRoute';
import LoginPage from '@pages/Login/LoginPage';
import DashboardPage from '@pages/Dashboard/DashboardPage';
import EmployeesListPage from '@pages/Employees/EmployeesListPage';
import EmployeeCreatePage from '@pages/Employees/EmployeeCreatePage';
import EmployeeEditPage from '@pages/Employees/EmployeeEditPage';
import DepartmentsPage from '@pages/Departments/DepartmentsPage';
import DepartmentsCreatePage from '@pages/Departments/DepartmentsCreatePage';
import DepartmentsEditPage from '@pages/Departments/DepartmentsEditPage';

const AppRoutes: React.FC = () => {
    return (
        <Routes>
            {/* Rota pública */}
            <Route path="/login" element={<LoginPage />} />

            {/* Rota raiz - redireciona para dashboard se autenticado, login se não */}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />

            {/* Dashboard - rota protegida */}
            <Route
                path="/dashboard"
                element={
                    <PrivateRoute>
                        <DashboardPage />
                    </PrivateRoute>
                }
            />

            {/* Rotas protegidas */}
            <Route
                path="/employees"
                element={
                    <PrivateRoute>
                        <EmployeesListPage />
                    </PrivateRoute>
                }
            />

            <Route
                path="/employees/create"
                element={
                    <PrivateRoute>
                        <EmployeeCreatePage />
                    </PrivateRoute>
                }
            />

            <Route
                path="/employees/edit/:id"
                element={
                    <PrivateRoute>
                        <EmployeeEditPage />
                    </PrivateRoute>
                }
            />

            {/* Rotas de Departamentos */}
            <Route
                path="/departments"
                element={
                    <PrivateRoute>
                        <DepartmentsPage />
                    </PrivateRoute>
                }
            />

            <Route
                path="/departments/create"
                element={
                    <PrivateRoute>
                        <DepartmentsCreatePage />
                    </PrivateRoute>
                }
            />

            <Route
                path="/departments/edit/:id"
                element={
                    <PrivateRoute>
                        <DepartmentsEditPage />
                    </PrivateRoute>
                }
            />

            <Route
                path="/job-titles"
                element={
                    <PrivateRoute>
                        <PageLayout />
                    </PrivateRoute>
                }
            />

            {/* Rota padrão - redireciona para dashboard */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
    );
};

export default AppRoutes;
