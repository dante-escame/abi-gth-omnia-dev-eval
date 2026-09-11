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

    [Fact]
    public async Task Creating_A_User_From_Valid_Data_Returns_The_Created_User()
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(command.Email);
        result.Value.Username.Should().Be(command.Username);
        result.Value.Name.FirstName.Should().Be(command.Name.FirstName);
        result.Value.Address.City.Should().Be(command.Address.City);
        result.Value.Password.Should().Be(ValidHash);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creating_A_User_Stores_The_Password_Hashed()
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).HashPassword(command.Password);
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Password.Value == ValidHash),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creating_A_User_Raises_The_Registration_Event()
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.DomainEvents.OfType<UserRegisteredDomainEvent>().Count() == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creating_A_User_With_An_Email_That_Is_Taken_Returns_A_Conflict()
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(UserTestData.RegisterValidUser());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be("Users.DuplicateEmail");
    }

    [Fact]
    public async Task Creating_A_User_With_A_Username_That_Is_Taken_Returns_A_Conflict()
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        _userRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(UserTestData.RegisterValidUser());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be("Users.DuplicateUsername");
    }

    [Theory]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Admin)]
    public async Task Creating_A_User_With_An_Elevated_Role_And_No_Admin_Caller_Is_Refused(UserRole role)
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        command.Role = role;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be("Users.RoleNotAllowed");
    }

    [Fact]
    public async Task Creating_An_Admin_As_An_Admin_Caller_Succeeds()
    {
        // Arrange
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        command.Role = UserRole.Admin;
        command.CallerRole = UserRole.Admin;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(UserRole.Admin);
    }
}
