using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Users.Events;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class User : AggregateRoot, IUser
{
    public Username Username { get; set; } = null!;

    public Email Email { get; set; } = null!;

    public Phone Phone { get; set; } = null!;

    public PasswordHash Password { get; set; } = null!;

    public PersonName Name { get; set; } = null!;

    public Address Address { get; set; } = null!;

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    string IUser.Id => Id.ToString();

    string IUser.Username => Username.Value;

    string IUser.Role => Role.ToString();

    public User()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public static User Register(
        Username username,
        Email email,
        Phone phone,
        PasswordHash password,
        PersonName name,
        Address address,
        UserRole role,
        UserStatus status)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            Phone = phone,
            Password = password,
            Name = name,
            Address = address,
            Role = role,
            Status = status
        };

        user.Raise(new UserRegisteredDomainEvent(user.Id, email.Value, username.Value, role));

        return user;
    }

    public ValidationResultDetail Validate()
    {
        var validator = new UserValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
}
