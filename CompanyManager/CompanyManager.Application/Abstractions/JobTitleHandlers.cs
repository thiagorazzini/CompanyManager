using CompanyManager.Application.DTOs;
using CompanyManager.Application.Common;

namespace CompanyManager.Application.Abstractions
{
    public interface IGetJobTitleByIdQueryHandler
    {
        Task<JobTitleResponse?> Handle(GetJobTitleByIdRequest request, CancellationToken cancellationToken);
    }

    public interface IListJobTitlesQueryHandler
    {
        Task<PageResult<JobTitleResponse>> Handle(ListJobTitlesRequest request, CancellationToken cancellationToken);
    }
}
