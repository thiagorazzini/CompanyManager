import React from 'react';

// Input Component
export interface InputProps {
    label: string;
    name: string;
    type?: 'text' | 'email' | 'password' | 'tel' | 'date';
    value: string;
    onChange: (value: string) => void;
    error?: string;
    required?: boolean;
    placeholder?: string;
    className?: string;
}

export const Input: React.FC<InputProps> = ({
    label,
    name,
    type = 'text',
    value,
    onChange,
    error,
    required = false,
    placeholder,
    className = '',
}) => {
    return (
        <div className={className}>
            <label htmlFor={name} className="block text-sm font-medium text-gray-700 mb-1">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            <input
                id={name}
                name={name}
                type={type}
                value={value}
                onChange={(e) => onChange(e.target.value)}
                placeholder={placeholder}
                className={`
          w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500
          ${error
                        ? 'border-red-300 focus:ring-red-500 focus:border-red-500'
                        : 'border-gray-300'
                    }
        `}
            />
            {error && (
                <p className="mt-1 text-sm text-red-600">{error}</p>
            )}
        </div>
    );
};

// Select Component
export interface SelectOption {
    value: string;
    label: string;
}

export interface SelectProps {
    label: string;
    name: string;
    value: string;
    onChange: (value: string) => void;
    options: SelectOption[];
    error?: string;
    required?: boolean;
    placeholder?: string;
    className?: string;
}

export const Select: React.FC<SelectProps> = ({
    label,
    name,
    value,
    onChange,
    options,
    error,
    required = false,
    placeholder = 'Select an option',
    className = '',
}) => {
    return (
        <div className={className}>
            <label htmlFor={name} className="block text-sm font-medium text-gray-700 mb-1">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            <select
                id={name}
                name={name}
                value={value}
                onChange={(e) => onChange(e.target.value)}
                className={`
          w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500
          ${error
                        ? 'border-red-300 focus:ring-red-500 focus:border-red-500'
                        : 'border-gray-300'
                    }
        `}
            >
                <option value="">{placeholder}</option>
                {options.map((option) => (
                    <option key={option.value} value={option.value}>
                        {option.label}
                    </option>
                ))}
            </select>
            {error && (
                <p className="mt-1 text-sm text-red-600">{error}</p>
            )}
        </div>
    );
};

// Form Container Component
export interface FormProps {
    children: React.ReactNode;
    onSubmit: (e: React.FormEvent) => void;
    className?: string;
}

export const Form: React.FC<FormProps> = ({
    children,
    onSubmit,
    className = '',
}) => {
    return (
        <form onSubmit={onSubmit} className={`space-y-6 ${className}`}>
            {children}
        </form>
    );
};

// Form Row Component for horizontal layout
export interface FormRowProps {
    children: React.ReactNode;
    className?: string;
}

export const FormRow: React.FC<FormRowProps> = ({
    children,
    className = '',
}) => {
    return (
        <div className={`grid grid-cols-1 md:grid-cols-2 gap-6 ${className}`}>
            {children}
        </div>
    );
};

// Form Actions Component
export interface FormActionsProps {
    children: React.ReactNode;
    className?: string;
    justify?: 'start' | 'end' | 'between' | 'around' | 'center';
}

export const FormActions: React.FC<FormActionsProps> = ({
    children,
    className = '',
    justify = 'end',
}) => {
    const justifyClass = {
        start: 'justify-start',
        end: 'justify-end',
        between: 'justify-between',
        around: 'justify-around',
        center: 'justify-center'
    }[justify];

    return (
        <div className={`flex ${justifyClass} space-x-4 pt-6 border-t border-gray-200 ${className}`}>
            {children}
        </div>
    );
};


