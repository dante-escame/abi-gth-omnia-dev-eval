using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

public sealed class AuthenticateUserProfile : Profile
{
    public AuthenticateUserProfile()
    {
        CreateMap<User, AuthenticateUserResult>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username.Value))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Value))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
