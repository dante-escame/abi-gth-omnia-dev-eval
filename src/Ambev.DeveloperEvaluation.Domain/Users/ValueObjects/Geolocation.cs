using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;

public sealed class Geolocation : ValueObject
{
    public string Lat { get; }

    public string Long { get; }

    public Geolocation(string lat, string @long)
    {
        if (string.IsNullOrWhiteSpace(lat))
            throw new DomainException("Latitude is required.");
        if (string.IsNullOrWhiteSpace(@long))
            throw new DomainException("Longitude is required.");

        Lat = lat;
        Long = @long;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Lat;
        yield return Long;
    }
}
