using AutoMapper;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Common;

namespace CompanyManager.Application.Mapping.Profiles
{
    public sealed record ListEmployeesQueryInput(EmployeeFilter Filter, PageRequest Page);

    public class ListEmployeesQueryProfile : Profile
    {
        public ListEmployeesQueryProfile()
        {
            CreateMap<ListEmployeesRequest, ListEmployeesQueryInput>()
                .ConstructUsing(src => CreateQueryInput(src));
        }

        private static ListEmployeesQueryInput CreateQueryInput(ListEmployeesRequest src)
        {
            var filter = new EmployeeFilter(
                NameOrEmail: string.IsNullOrWhiteSpace(src.NameContains) ? null : src.NameContains.Trim(),
                DepartmentId: src.DepartmentId
            );

            var page = new PageRequest(
                Math.Max(src.Page, 1),
                Math.Clamp(src.PageSize, 1, 100)
            );

            return new ListEmployeesQueryInput(filter, page);
        }
    }
}
