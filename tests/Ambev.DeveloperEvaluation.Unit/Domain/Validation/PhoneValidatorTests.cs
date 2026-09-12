using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class PhoneValidatorTests
{
    [Theory]
    [InlineData("+5511987654321", true)]
    [InlineData("5511987654321", true)]
    [InlineData("+1234567890", true)]
    [InlineData("+0511987654321", false)]
    [InlineData("(11) 98765-4321", false)]
    [InlineData("+55 11 98765-4321", false)]
    [InlineData("+5", false)]
    [InlineData("+551198765432112345", false)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void The_Phone_Validator_Enforces_The_International_Format(string phone, bool expectedResult)
    {
        // Arrange
        var validator = new PhoneValidator();

        // Act
        var result = validator.Validate(phone);

        // Assert
        result.IsValid.Should().Be(expectedResult);
    }
}
