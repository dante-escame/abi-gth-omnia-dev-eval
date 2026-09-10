using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

public class GetUserProfile : Profile
{
    public GetUserProfile()
    {
        CreateMap<User, GetUserResult>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Value))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone.Value))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name == null ? string.Empty : s.Name.FirstName + " " + s.Name.LastName));
    }
}
