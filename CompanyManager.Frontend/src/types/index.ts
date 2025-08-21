// Common types used across the application
export interface ApiResponse<T = any> {
    success: boolean;
    data?: T;
    message?: string;
    errors?: string[];
}

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

export interface User {
    id: string;
    username: string;
    email: string;
    firstName?: string;
    lastName?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface Employee {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    dateOfBirth: string;
    jobTitle: string;
    departmentId: string;
    createdAt: string;
    updatedAt?: string;
}

export interface Department {
    id: string;
    name: string;
    description?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface JobTitle {
    id: string;
    name: string;
    description?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateEmployeeRequest {
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    dateOfBirth: string;
    phoneNumbers: string[];
    jobTitleId: string;
    departmentId: string;
    password: string;
}
