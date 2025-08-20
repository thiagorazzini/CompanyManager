using AutoMapper;
using CompanyManager.Application.DTOs;

namespace CompanyManager.Application.Mapping.Profiles
{
    public class GetByIdProfile : Profile
    {
        public GetByIdProfile()
        {
            CreateMap<GetEmployeeByIdRequest, Guid>().ConvertUsing(src => src.Id);
            CreateMap<GetDepartmentByIdRequest, Guid>().ConvertUsing(src => src.Id);
        }
    }
}
