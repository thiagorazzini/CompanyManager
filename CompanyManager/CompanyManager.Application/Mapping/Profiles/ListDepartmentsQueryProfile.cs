using AutoMapper;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Common;

namespace CompanyManager.Application.Mapping.Profiles
{
    public sealed record ListDepartmentsQueryInput(DepartmentFilter Filter, PageRequest Page);

    public class ListDepartmentsQueryProfile : Profile
    {
        public ListDepartmentsQueryProfile()
        {
            CreateMap<ListDepartmentsRequest, ListDepartmentsQueryInput>()
                .ConstructUsing(src => CreateQueryInput(src));
        }

        private static ListDepartmentsQueryInput CreateQueryInput(ListDepartmentsRequest src)
        {
            var filter = new DepartmentFilter(
                string.IsNullOrWhiteSpace(src.NameContains) ? null : src.NameContains.Trim()
            );

            var page = new PageRequest(
                Math.Max(src.Page, 1),
                Math.Clamp(src.PageSize, 1, 100)
            );

            return new ListDepartmentsQueryInput(filter, page);
        }
    }
}
