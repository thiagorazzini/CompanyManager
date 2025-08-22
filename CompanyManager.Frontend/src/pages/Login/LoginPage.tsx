import { useState } from 'react';
import { useAuth } from '@hooks/useAuth';
import { toast } from 'react-hot-toast';

const LoginPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const { login, isLoading } = useAuth();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!email || !password) {
            toast.error('Por favor, preencha todos os campos');
            return;
        }

        try {
            await login({ email, password });
        } catch (error) {
            // Error is already handled in useAuth hook
        }
    };

    return (
        <div className="min-h-screen bg-white flex items-center justify-center">
            <div className="max-w-md w-full p-6">
                <h1 className="text-3xl font-bold text-black text-center mb-6">
                    Company Manager
                </h1>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-black mb-2">
                            Email
                        </label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-black"
                            placeholder="admin@companymanager.com"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-black mb-2">
                            Password
                        </label>
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-black"
                            placeholder="••••••••"
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={isLoading}
                        className={`w-full py-2 px-4 rounded-md transition-colors ${isLoading
                            ? 'bg-gray-400 cursor-not-allowed'
                            : 'bg-blue-600 hover:bg-blue-700'
                            } text-white`}
                    >
                        {isLoading ? 'Signing in...' : 'Sign In'}
                    </button>
                </form>

                <p className="text-xs text-black text-center mt-4">
                    Credentials: admin@companymanager.com / Admin123!
                </p>
            </div>
        </div>
    );
};

export default LoginPage;
