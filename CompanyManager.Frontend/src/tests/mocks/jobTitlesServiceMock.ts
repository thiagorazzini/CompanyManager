import { jest } from '@jest/globals';

export const mockJobTitlesService = {
    getJobTitles: jest.fn(),
    getJobTitleById: jest.fn(),
};

// Mock das respostas padrão
export const mockJobTitleData = {
    jobTitle: {
        id: '1',
        name: 'Desenvolvedor',
        description: 'Desenvolvedor de software'
    },
    jobTitles: [
        {
            id: '1',
            name: 'Desenvolvedor',
            description: 'Desenvolvedor de software'
        },
        {
            id: '2',
            name: 'Analista',
            description: 'Analista de sistemas'
        },
        {
            id: '3',
            name: 'Gerente',
            description: 'Gerente de projeto'
        },
        {
            id: '4',
            name: 'Designer',
            description: 'Designer de interface'
        }
    ]
};

// Configurar mocks padrão
mockJobTitlesService.getJobTitles.mockResolvedValue(mockJobTitleData.jobTitles);
mockJobTitlesService.getJobTitleById.mockResolvedValue(mockJobTitleData.jobTitle);

export default mockJobTitlesService;
