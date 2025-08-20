using AutoMapper;
using CompanyManager.Application.DTOs;
using System.Globalization;

namespace CompanyManager.Application.Mapping.Profiles
{
    public class EmployeeCreateProfile : Profile
    {
        public EmployeeCreateProfile()
        {
            CreateMap<CreateEmployeeRequest, CreateEmployeeRequest>()
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
                .ForMember(dest => dest.PhoneNumbers, opt => opt.MapFrom(src => 
                    src.PhoneNumbers == null ? Array.Empty<string>() : 
                    src.PhoneNumbers.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray()))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => ValidateAndTrimDateOfBirth(src.DateOfBirth)))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
        }

        private static string ValidateAndTrimDateOfBirth(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
                throw new ArgumentException("DateOfBirth is required.");
            
            return dateOfBirth.Trim();
        }
    }
}
