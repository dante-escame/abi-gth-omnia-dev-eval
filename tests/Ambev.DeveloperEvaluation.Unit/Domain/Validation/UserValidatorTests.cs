using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact(DisplayName = "Valid user should pass all validation rules")]
    public void Given_ValidUser_When_Validated_Then_ShouldNotHaveErrors()
    {
        var user = UserTestData.GenerateValidUser();

        var result = _validator.TestValidate(user);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Unknown status should fail validation")]
    public void Given_UnknownStatus_When_Validated_Then_ShouldHaveError()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Unknown;

        var result = _validator.TestValidate(user);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact(DisplayName = "None role should fail validation")]
    public void Given_NoneRole_When_Validated_Then_ShouldHaveError()
    {
        var user = UserTestData.GenerateValidUser();
        user.Role = UserRole.None;

        var result = _validator.TestValidate(user);

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }
}
