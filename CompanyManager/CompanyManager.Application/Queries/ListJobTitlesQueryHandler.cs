using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Common;
using CompanyManager.Domain.Interfaces;

namespace CompanyManager.Application.Queries
{
    public class ListJobTitlesQueryHandler : IListJobTitlesQueryHandler
    {
        private readonly IJobTitleRepository _jobTitleRepository;

        public ListJobTitlesQueryHandler(IJobTitleRepository jobTitleRepository)
        {
            _jobTitleRepository = jobTitleRepository;
        }

        public async Task<PageResult<JobTitleResponse>> Handle(ListJobTitlesRequest request, CancellationToken cancellationToken)
        {
            var jobTitles = await _jobTitleRepository.GetAllAsync(cancellationToken);
            
            // Filtrar por IsActive se especificado
            if (request.IsActive.HasValue)
            {
                jobTitles = jobTitles.Where(jt => jt.IsActive == request.IsActive.Value);
            }

            // Filtrar por Name se especificado
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                jobTitles = jobTitles.Where(jt => jt.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por HierarchyLevel se especificado
            if (request.HierarchyLevel.HasValue)
            {
                jobTitles = jobTitles.Where(jt => jt.HierarchyLevel == request.HierarchyLevel.Value);
            }

            // Ordenar por HierarchyLevel
            var orderedJobTitles = jobTitles.OrderBy(jt => jt.HierarchyLevel).ToList();

            // Aplicar paginação
            var totalCount = orderedJobTitles.Count;
            var pagedJobTitles = orderedJobTitles
                .Skip(request.Offset)
                .Take(request.Take)
                .ToList();

            // Mapear para Response
            var jobTitleResponses = pagedJobTitles.Select(jt => new JobTitleResponse
            {
                Id = jt.Id,
                Name = jt.Name,
                HierarchyLevel = jt.HierarchyLevel,
                Description = jt.Description,
                IsActive = jt.IsActive,
                CreatedAt = jt.CreatedAt,
                UpdatedAt = jt.UpdatedAt,
                EmployeeCount = jt.Employees?.Count ?? 0
            }).ToList();

            return new PageResult<JobTitleResponse>(
                jobTitleResponses,
                totalCount,
                request.PageSafe,
                request.PageSizeSafe
            );
        }
    }
}
