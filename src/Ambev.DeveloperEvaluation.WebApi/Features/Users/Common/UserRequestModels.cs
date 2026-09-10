using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Users.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.Common;

public sealed class GeolocationRequest
{
    [JsonPropertyName("lat")]
    public string Lat { get; set; } = string.Empty;

    [JsonPropertyName("long")]
    public string Long { get; set; } = string.Empty;

    public GeolocationInput ToInput() => new(Lat, Long);
}

public sealed class AddressRequest
{
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("zipcode")]
    public string ZipCode { get; set; } = string.Empty;

    [JsonPropertyName("geolocation")]
    public GeolocationRequest Geolocation { get; set; } = new();

    public AddressInput ToInput() => new(City, Street, Number, ZipCode, Geolocation.ToInput());
}

public sealed class PersonNameRequest
{
    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = string.Empty;

    public PersonNameInput ToInput() => new(FirstName, LastName);
}
