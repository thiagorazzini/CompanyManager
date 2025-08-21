using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Interfaces;

namespace CompanyManager.Application.Queries
{
    public class GetJobTitleByIdQueryHandler : IGetJobTitleByIdQueryHandler
    {
        private readonly IJobTitleRepository _jobTitleRepository;

        public GetJobTitleByIdQueryHandler(IJobTitleRepository jobTitleRepository)
        {
            _jobTitleRepository = jobTitleRepository;
        }

        public async Task<JobTitleResponse?> Handle(GetJobTitleByIdRequest request, CancellationToken cancellationToken)
        {
            var jobTitle = await _jobTitleRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (jobTitle == null)
                return null;

            return new JobTitleResponse
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name,
                HierarchyLevel = jobTitle.HierarchyLevel,
                Description = jobTitle.Description,
                IsActive = jobTitle.IsActive,
                CreatedAt = jobTitle.CreatedAt,
                UpdatedAt = jobTitle.UpdatedAt,
                EmployeeCount = jobTitle.Employees?.Count ?? 0
            };
        }
    }
}









