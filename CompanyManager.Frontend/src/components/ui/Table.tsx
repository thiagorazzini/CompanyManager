import React from 'react';

export interface TableColumn<T> {
    key: keyof T | string;
    header: string;
    render?: (value: any, item: T) => React.ReactNode;
    className?: string;
}

export interface TableProps<T> {
    data: T[];
    columns: TableColumn<T>[];
    onEdit?: (item: T) => void;
    onDelete?: (item: T) => void;
    isLoading?: boolean;
    emptyMessage?: string;
    className?: string;
}

function Table<T extends { id: string | number }>({
    data,
    columns,
    onEdit,
    onDelete,
    isLoading = false,
    emptyMessage = 'Nenhum registro encontrado',
    className = '',
}: TableProps<T>) {
    if (isLoading) {
        return (
            <div className="flex justify-center items-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    if (data.length === 0) {
        return (
            <div className="text-center py-8">
                <div className="text-gray-400 mb-4">
                    <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                </div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                    {emptyMessage}
                </h3>
            </div>
        );
    }

    return (
        <div className={`overflow-x-auto ${className}`}>
            <table className="min-w-full bg-white border border-gray-200 rounded-lg overflow-hidden">
                <thead className="bg-blue-800 text-white">
                    <tr>
                        {columns.map((column) => (
                            <th
                                key={String(column.key)}
                                className={`px-6 py-3 text-left text-xs font-medium uppercase tracking-wider ${column.className || ''}`}
                            >
                                {column.header}
                            </th>
                        ))}
                        {(onEdit || onDelete) && (
                            <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">
                                Ações
                            </th>
                        )}
                    </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                    {data.map((item, index) => (
                        <tr
                            key={item.id}
                            className={`${index % 2 === 0 ? 'bg-white' : 'bg-blue-50'} hover:bg-blue-100 transition-colors`}
                        >
                            {columns.map((column) => (
                                <td
                                    key={String(column.key)}
                                    className={`px-6 py-4 whitespace-nowrap text-sm text-gray-900 ${column.className || ''}`}
                                >
                                    {column.render
                                        ? column.render(item[column.key as keyof T], item)
                                        : String(item[column.key as keyof T] || '')}
                                </td>
                            ))}
                            {(onEdit || onDelete) && (
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                    <div className="flex space-x-2">
                                        {onEdit && (
                                            <button
                                                onClick={() => onEdit(item)}
                                                className="text-blue-600 hover:text-blue-900 font-medium text-sm px-3 py-1 rounded border border-blue-600 hover:bg-blue-50 transition-colors"
                                            >
                                                Editar
                                            </button>
                                        )}
                                        {onDelete && (
                                            <button
                                                onClick={() => onDelete(item)}
                                                className="text-red-600 hover:text-red-900 font-medium text-sm px-3 py-1 rounded border border-red-600 hover:bg-red-50 transition-colors"
                                            >
                                                Deletar
                                            </button>
                                        )}
                                    </div>
                                </td>
                            )}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default Table;


