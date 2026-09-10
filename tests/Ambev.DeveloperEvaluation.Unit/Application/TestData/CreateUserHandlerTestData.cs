using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public static class CreateUserHandlerTestData
{
    private static readonly Faker<CreateUserCommand> createUserHandlerFaker = new Faker<CreateUserCommand>()
        .RuleFor(u => u.Username, f => f.Internet.UserName())
        .RuleFor(u => u.Password, f => $"Test@{f.Random.Number(100, 999)}")
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.Phone, f => $"+55{f.Random.Number(11, 99)}{f.Random.Number(100000000, 999999999)}")
        .RuleFor(u => u.Status, f => f.PickRandom(UserStatus.Active, UserStatus.Suspended))
        .RuleFor(u => u.Role, f => f.PickRandom(UserRole.Customer, UserRole.Admin));

    public static CreateUserCommand GenerateValidCommand() => createUserHandlerFaker.Generate();
}
