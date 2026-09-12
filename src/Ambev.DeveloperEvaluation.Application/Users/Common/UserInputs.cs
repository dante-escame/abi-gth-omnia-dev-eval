namespace Ambev.DeveloperEvaluation.Application.Users.Common;

public sealed record GeolocationInput(string Lat, string Long)
{
    public static GeolocationInput Empty => new(string.Empty, string.Empty);
}

public sealed record AddressInput(string City, string Street, int Number, string ZipCode, GeolocationInput Geolocation)
{
    public static AddressInput Empty => new(string.Empty, string.Empty, 0, string.Empty, GeolocationInput.Empty);
}

public sealed record PersonNameInput(string FirstName, string LastName)
{
    public static PersonNameInput Empty => new(string.Empty, string.Empty);
}
