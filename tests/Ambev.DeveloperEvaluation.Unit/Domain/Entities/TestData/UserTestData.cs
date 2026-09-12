using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class UserTestData
{
    private static readonly string BcryptHash = new BCryptPasswordHasher().HashPassword("Passw0rd@1");

    private static readonly Faker<User> UserFaker = new Faker<User>()
        .RuleFor(u => u.Username, f => new Username(GenerateValidUsername()))
        .RuleFor(u => u.Password, f => new PasswordHash(GenerateBcryptHash()))
        .RuleFor(u => u.Email, f => new Email(f.Internet.Email()))
        .RuleFor(u => u.Phone, f => new Phone(GenerateValidPhone()))
        .RuleFor(u => u.Name, f => new PersonName(f.Name.FirstName(), f.Name.LastName()))
        .RuleFor(u => u.Address, f => GenerateValidAddress())
        .RuleFor(u => u.Status, f => f.PickRandom(UserStatus.Active, UserStatus.Suspended))
        .RuleFor(u => u.Role, f => f.PickRandom(UserRole.Customer, UserRole.Admin));

    public static User GenerateValidUser() => UserFaker.Generate();

    public static User RegisterValidUser()
    {
        var faker = new Faker();
        return User.Register(
            new Username(GenerateValidUsername()),
            new Email(faker.Internet.Email()),
            new Phone(GenerateValidPhone()),
            new PasswordHash(GenerateBcryptHash()),
            new PersonName(faker.Name.FirstName(), faker.Name.LastName()),
            GenerateValidAddress(),
            UserRole.Customer,
            UserStatus.Active);
    }

    public static string GenerateValidEmail() => new Faker().Internet.Email();

    public static string GenerateValidPassword() => $"Test@{new Faker().Random.Number(100, 999)}";

    public static string GenerateBcryptHash() => BcryptHash;

    public static string GenerateValidPhone()
    {
        var faker = new Faker();
        return $"+55{faker.Random.Number(11, 99)}{faker.Random.Number(100000000, 999999999)}";
    }

    public static string GenerateValidUsername()
    {
        string? candidate = new Faker().Internet.UserName();
        if (candidate.Length < 3)
            candidate = candidate.PadRight(3, 'x');
        return candidate.Length > 50 ? candidate[..50] : candidate;
    }

    public static Address GenerateValidAddress()
    {
        var faker = new Faker();
        return new Address(
            faker.Address.City(),
            faker.Address.StreetName(),
            faker.Random.Int(1, 9999),
            faker.Address.ZipCode(),
            new Geolocation(faker.Address.Latitude().ToString(), faker.Address.Longitude().ToString()));
    }

    public static string GenerateInvalidEmail() => new Faker().Lorem.Word();

    public static string GenerateInvalidPassword() => new Faker().Lorem.Word();

    public static string GenerateInvalidPhone() => new Faker().Random.AlphaNumeric(5);

    public static string GenerateLongUsername() => new Faker().Random.String2(51);
}
