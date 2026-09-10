using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;

public sealed class Address : ValueObject
{
    public string City { get; }

    public string Street { get; }

    public int Number { get; }

    public string ZipCode { get; }

    public Geolocation Geolocation { get; }

    public Address(string city, string street, int number, string zipCode, Geolocation geolocation)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street is required.");
        if (number < 0)
            throw new DomainException("Address number cannot be negative.");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new DomainException("Zip code is required.");

        City = city;
        Street = street;
        Number = number;
        ZipCode = zipCode;
        Geolocation = geolocation ?? throw new DomainException("Geolocation is required.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return Street;
        yield return Number;
        yield return ZipCode;
        yield return Geolocation;
    }
}
