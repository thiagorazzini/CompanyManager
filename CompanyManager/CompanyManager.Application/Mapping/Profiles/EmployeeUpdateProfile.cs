using AutoMapper;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Mapping.Profiles
{
    public class EmployeeUpdateProfile : Profile
    {
        public EmployeeUpdateProfile()
        {
            CreateMap<UpdateEmployeeRequest, UpdateEmployeeCommand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.FirstName) ? string.Empty : src.FirstName.Trim()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.LastName) ? string.Empty : src.LastName.Trim()))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.JobTitle) ? string.Empty : src.JobTitle.Trim()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.Email) ? string.Empty : src.Email.Trim().ToLowerInvariant()))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.DocumentNumber) ? string.Empty : src.DocumentNumber.Trim()))
                .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => 
                    src.PhoneNumbers == null ? Array.Empty<string>() : 
                    src.PhoneNumbers.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray()))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));
        }
    }
}
