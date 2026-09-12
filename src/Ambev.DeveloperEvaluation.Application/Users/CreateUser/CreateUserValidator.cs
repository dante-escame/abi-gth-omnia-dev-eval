using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(user => user.Email).SetValidator(new EmailValidator());
        RuleFor(user => user.Username).NotEmpty().Length(3, 50);
        RuleFor(user => user.Password).SetValidator(new PasswordValidator());
        RuleFor(user => user.Phone).SetValidator(new PhoneValidator());
        RuleFor(user => user.Status).NotEqual(UserStatus.Unknown);
        RuleFor(user => user.Role).NotEqual(UserRole.None);
        RuleFor(user => user.Name.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(user => user.Name.LastName).NotEmpty().MaximumLength(100);
        RuleFor(user => user.Address.City).NotEmpty().MaximumLength(100);
        RuleFor(user => user.Address.Street).NotEmpty().MaximumLength(200);
        RuleFor(user => user.Address.Number).GreaterThanOrEqualTo(0);
        RuleFor(user => user.Address.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(user => user.Address.Geolocation.Lat).NotEmpty();
        RuleFor(user => user.Address.Geolocation.Long).NotEmpty();
    }
}
