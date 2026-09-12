using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public static class CreateUserHandlerTestData
{
    private static readonly Faker<CreateUserCommand> CreateUserHandlerFaker = new Faker<CreateUserCommand>()
        .RuleFor(u => u.Username, f => f.Internet.UserName().PadRight(3, 'x'))
        .RuleFor(u => u.Password, f => $"Test@{f.Random.Number(100, 999)}")
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.Phone, f => $"+55{f.Random.Number(11, 99)}{f.Random.Number(100000000, 999999999)}")
        .RuleFor(u => u.Name, f => new PersonNameInput(f.Name.FirstName(), f.Name.LastName()))
        .RuleFor(u => u.Address, f => new AddressInput(
            f.Address.City(),
            f.Address.StreetName(),
            f.Random.Int(1, 9999),
            f.Address.ZipCode(),
            new GeolocationInput(f.Address.Latitude().ToString(), f.Address.Longitude().ToString())))
        .RuleFor(u => u.Status, f => f.PickRandom(UserStatus.Active, UserStatus.Suspended))
        .RuleFor(u => u.Role, _ => UserRole.Customer);

    public static CreateUserCommand GenerateValidCommand() => CreateUserHandlerFaker.Generate();
}
