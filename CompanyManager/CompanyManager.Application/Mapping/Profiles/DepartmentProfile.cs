using AutoMapper;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Mapping.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            // CreateDepartmentRequest → CreateDepartmentCommand
            CreateMap<CreateDepartmentRequest, CreateDepartmentCommand>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.Name) ? string.Empty : src.Name.Trim()));

            // UpdateDepartmentRequest → UpdateDepartmentCommand
            CreateMap<UpdateDepartmentRequest, UpdateDepartmentCommand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NewName, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.NewName) ? string.Empty : src.NewName.Trim()));
        }
    }
}
