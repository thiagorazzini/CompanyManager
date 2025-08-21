// Mock para react-hot-toast
const mockToast = {
    success: jest.fn(),
    error: jest.fn(),
    warning: jest.fn(),
    info: jest.fn(),
    dismiss: jest.fn(),
    loading: jest.fn(),
    promise: jest.fn(),
};

export default mockToast;

