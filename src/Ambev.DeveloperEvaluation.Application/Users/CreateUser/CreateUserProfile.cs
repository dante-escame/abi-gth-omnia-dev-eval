using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserProfile : Profile
{
    public CreateUserProfile()
    {
        CreateMap<CreateUserCommand, User>()
            .ForMember(d => d.Email, o => o.MapFrom(s => new Email(s.Email)))
            .ForMember(d => d.Username, o => o.MapFrom(s => new Username(s.Username)))
            .ForMember(d => d.Phone, o => o.MapFrom(s => new Phone(s.Phone)))
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.Name, o => o.Ignore())
            .ForMember(d => d.Address, o => o.Ignore());

        CreateMap<User, CreateUserResult>();
    }
}
