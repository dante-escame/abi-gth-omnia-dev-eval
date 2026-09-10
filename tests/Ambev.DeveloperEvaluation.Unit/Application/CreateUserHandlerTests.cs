using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateUserHandlerTests
{
    private static readonly string ValidHash = new BCryptPasswordHasher().HashPassword("Passw0rd@1");

    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _handler = new CreateUserHandler(_userRepository, _mapper, _passwordHasher);
    }

    private static User MappedUser(CreateUserCommand command) => new()
    {
        Id = Guid.NewGuid(),
        Username = new Username(command.Username),
        Email = new Email(command.Email),
        Phone = new Phone(command.Phone),
        Status = command.Status,
        Role = command.Role
    };

    [Fact(DisplayName = "Given valid user data When creating user Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        var user = MappedUser(command);
        var result = new CreateUserResult { Id = user.Id };

        _mapper.Map<User>(command).Returns(user);
        _mapper.Map<CreateUserResult>(user).Returns(result);
        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ValidHash);

        var createUserResult = await _handler.Handle(command, CancellationToken.None);

        createUserResult.Should().NotBeNull();
        createUserResult.Id.Should().Be(user.Id);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid user data When creating user Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var command = new CreateUserCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given user creation request When handling Then password is hashed")]
    public async Task Handle_ValidRequest_HashesPassword()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        var originalPassword = command.Password;
        var user = MappedUser(command);

        _mapper.Map<User>(command).Returns(user);
        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(originalPassword).Returns(ValidHash);

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(originalPassword);
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Password.Value == ValidHash),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid command When handling Then maps command to user entity")]
    public async Task Handle_ValidRequest_MapsCommandToUser()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        var user = MappedUser(command);

        _mapper.Map<User>(command).Returns(user);
        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ValidHash);

        await _handler.Handle(command, CancellationToken.None);

        _mapper.Received(1).Map<User>(Arg.Is<CreateUserCommand>(c =>
            c.Username == command.Username &&
            c.Email == command.Email &&
            c.Phone == command.Phone &&
            c.Status == command.Status &&
            c.Role == command.Role));
    }
}
