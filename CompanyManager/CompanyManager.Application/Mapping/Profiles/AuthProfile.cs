using AutoMapper;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Mapping.Profiles
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // AuthenticateRequest → AuthenticateCommand
            CreateMap<AuthenticateRequest, AuthenticateCommand>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.Email) ? string.Empty : src.Email.Trim().ToLowerInvariant()))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => 
                    src.Password ?? string.Empty));

            // RefreshTokenRequest → RefreshTokenCommand
            CreateMap<RefreshTokenRequest, RefreshTokenCommand>()
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.RefreshToken) ? string.Empty : src.RefreshToken.Trim()));

            // ChangePasswordRequest → ChangePasswordCommand
            CreateMap<ChangePasswordRequest, ChangePasswordCommand>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.Email) ? string.Empty : src.Email.Trim().ToLowerInvariant()))
                .ForMember(dest => dest.CurrentPassword, opt => opt.MapFrom(src => 
                    src.CurrentPassword ?? string.Empty))
                .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => 
                    src.NewPassword ?? string.Empty))
                .ForMember(dest => dest.ConfirmNewPassword, opt => opt.MapFrom(src => 
                    src.ConfirmNewPassword ?? string.Empty));
        }
    }
}
