using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Users.Events;
using Ambev.DeveloperEvaluation.Unit.Domain;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateUserHandlerTests
{
    private static readonly string ValidHash = new BCryptPasswordHasher().HashPassword("Passw0rd@1");

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ValidHash);
        _handler = new CreateUserHandler(_userRepository, _passwordHasher);
    }

    [Fact(DisplayName = "Given valid user data When creating user Then returns the created user")]
    public async Task Handle_ValidRequest_ReturnsCreatedUser()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(command.Email);
        result.Value.Username.Should().Be(command.Username);
        result.Value.Name.FirstName.Should().Be(command.Name.FirstName);
        result.Value.Address.City.Should().Be(command.Address.City);
        result.Value.Password.Should().Be(ValidHash);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given user creation request When handling Then password is hashed")]
    public async Task Handle_ValidRequest_HashesPassword()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(command.Password);
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Password.Value == ValidHash),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a created user When handling Then the registration event is raised")]
    public async Task Handle_ValidRequest_RaisesRegisteredEvent()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.DomainEvents.OfType<UserRegisteredDomainEvent>().Count() == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a taken email When creating user Then returns a conflict")]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(UserTestData.RegisterValidUser());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be("Users.DuplicateEmail");
    }

    [Fact(DisplayName = "Given a taken username When creating user Then returns a conflict")]
    public async Task Handle_DuplicateUsername_ReturnsConflict()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        _userRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(UserTestData.RegisterValidUser());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be("Users.DuplicateUsername");
    }

    [Theory(DisplayName = "Given an elevated role and no admin caller When creating user Then returns role not allowed")]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Admin)]
    public async Task Handle_ElevatedRoleWithoutAdmin_ReturnsRoleNotAllowed(UserRole role)
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        command.Role = role;

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be("Users.RoleNotAllowed");
    }

    [Fact(DisplayName = "Given an admin caller When creating an admin Then succeeds")]
    public async Task Handle_ElevatedRoleWithAdminCaller_Succeeds()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        command.Role = UserRole.Admin;
        command.CallerRole = UserRole.Admin;

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(UserRole.Admin);
    }
}
