using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.Common;

public sealed record GeolocationResponse(
    [property: JsonPropertyName("lat")] string Lat,
    [property: JsonPropertyName("long")] string Long);

public sealed record AddressResponse(
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("street")] string Street,
    [property: JsonPropertyName("number")] int Number,
    [property: JsonPropertyName("zipcode")] string ZipCode,
    [property: JsonPropertyName("geolocation")] GeolocationResponse Geolocation);

public sealed record PersonNameResponse(
    [property: JsonPropertyName("firstname")] string FirstName,
    [property: JsonPropertyName("lastname")] string LastName);

public sealed record UserResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("name")] PersonNameResponse Name,
    [property: JsonPropertyName("address")] AddressResponse Address,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("status")] UserStatus Status,
    [property: JsonPropertyName("role")] UserRole Role)
{
    public static UserResponse From(UserResult user) => new(
        user.Id,
        user.Email,
        user.Username,
        user.Password,
        new PersonNameResponse(user.Name.FirstName, user.Name.LastName),
        new AddressResponse(
            user.Address.City,
            user.Address.Street,
            user.Address.Number,
            user.Address.ZipCode,
            new GeolocationResponse(user.Address.Geolocation.Lat, user.Address.Geolocation.Long)),
        user.Phone,
        user.Status,
        user.Role);
}
