using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Users.ValueObjects;

public class PhoneTests
{
    [Theory(DisplayName = "Invalid phone is rejected on construction")]
    [InlineData("")]
    [InlineData("11987654321")]
    [InlineData("+5511987654321")]
    [InlineData("(11) 98765-432")]
    [InlineData("(1) 98765-4321")]
    public void Given_InvalidValue_When_Constructed_Then_Throws(string value)
    {
        var act = () => new Phone(value);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Valid Brazilian phone is accepted")]
    [InlineData("(11) 98765-4321")]
    [InlineData("(21) 3456-7890")]
    public void Given_ValidValue_When_Constructed_Then_KeepsValue(string value)
    {
        new Phone(value).Value.Should().Be(value);
    }

    [Fact(DisplayName = "Phones with the same value are structurally equal")]
    public void Given_SameValue_When_Compared_Then_Equal()
    {
        new Phone("(11) 98765-4321").Should().Be(new Phone("(11) 98765-4321"));
    }
}
