using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class PhoneValidatorTests
{
    [Theory(DisplayName = "Given a phone number When validating Then should enforce the international format")]
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
    public void Given_PhoneNumber_When_Validating_Then_ShouldValidateAccordingToPattern(string phone, bool expectedResult)
    {
        var validator = new PhoneValidator();

        var result = validator.Validate(phone);

        result.IsValid.Should().Be(expectedResult);
    }
}
