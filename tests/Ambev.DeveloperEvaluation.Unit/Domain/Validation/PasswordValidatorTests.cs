using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class PasswordValidatorTests
{
    private readonly PasswordValidator _validator = new();

    [Fact]
    public void A_Valid_Password_Passes_Validation()
    {
        // Arrange
        string password = UserTestData.GenerateValidPassword();

        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void An_Empty_Password_Fails_Validation()
    {
        // Arrange
        string password = string.Empty;

        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Theory]
    [InlineData("Test@1")]
    [InlineData("Pass#2")]
    public void A_Password_Shorter_Than_The_Minimum_Length_Fails_Validation(string password)
    {
        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void A_Password_Without_An_Uppercase_Letter_Fails_Validation()
    {
        // Arrange
        string password = "password@123";

        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void A_Password_Without_A_Lowercase_Letter_Fails_Validation()
    {
        // Arrange
        string password = "PASSWORD@123";

        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void A_Password_Without_A_Number_Fails_Validation()
    {
        // Arrange
        string password = "Password@ABC";

        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Password must contain at least one number.");
    }

    [Fact]
    public void A_Password_Without_A_Special_Character_Fails_Validation()
    {
        // Arrange
        string password = "Password123";

        // Act
        var result = _validator.TestValidate(password);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Password must contain at least one special character.");
    }
}
