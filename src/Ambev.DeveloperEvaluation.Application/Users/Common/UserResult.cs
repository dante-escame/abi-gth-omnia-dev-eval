using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Users.Common;

public sealed record GeolocationResult(string Lat, string Long);

public sealed record AddressResult(string City, string Street, int Number, string ZipCode, GeolocationResult Geolocation);

public sealed record PersonNameResult(string FirstName, string LastName);

public sealed record UserResult(
    Guid Id,
    string Email,
    string Username,
    string Password,
    PersonNameResult Name,
    AddressResult Address,
    string Phone,
    UserStatus Status,
    UserRole Role)
{
    public static UserResult From(User user) => new(
        user.Id,
        user.Email.Value,
        user.Username.Value,
        user.Password.Value,
        new PersonNameResult(user.Name.FirstName, user.Name.LastName),
        new AddressResult(
            user.Address.City,
            user.Address.Street,
            user.Address.Number,
            user.Address.ZipCode,
            new GeolocationResult(user.Address.Geolocation.Lat, user.Address.Geolocation.Long)),
        user.Phone.Value,
        user.Status,
        user.Role);
}
