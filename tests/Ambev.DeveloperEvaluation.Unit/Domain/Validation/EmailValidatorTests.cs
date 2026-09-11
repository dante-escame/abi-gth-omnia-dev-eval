using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class EmailValidatorTests
{
    private readonly EmailValidator _validator = new();

    [Fact]
    public void A_Valid_Email_Passes_Validation()
    {
        // Arrange
        string email = UserTestData.GenerateValidEmail();

        // Act
        var result = _validator.TestValidate(email);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void An_Empty_Email_Fails_Validation()
    {
        // Arrange
        string email = string.Empty;

        // Act
        var result = _validator.TestValidate(email);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("The email address cannot be empty.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@.com")]
    [InlineData("user@domain.")]
    public void An_Invalid_Email_Format_Fails_Validation(string email)
    {
        // Act
        var result = _validator.TestValidate(email);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("The provided email address is not valid.");
    }

    [Fact]
    public void An_Email_Longer_Than_100_Characters_Fails_Validation()
    {
        // Arrange
        string email = $"{"a".PadLeft(90, 'a')}@example.com";

        // Act
        var result = _validator.TestValidate(email);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("The email address cannot be longer than 100 characters.");
    }
}
