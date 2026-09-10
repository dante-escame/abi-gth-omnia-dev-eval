using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Users.ValueObjects;

public class EmailTests
{
    [Theory(DisplayName = "Invalid email is rejected on construction")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nolocal.com")]
    public void Given_InvalidValue_When_Constructed_Then_Throws(string value)
    {
        var act = () => new Email(value);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Valid email is accepted")]
    [InlineData("user@example.com")]
    [InlineData("first.last+tag@sub.example.co")]
    public void Given_ValidValue_When_Constructed_Then_KeepsValue(string value)
    {
        new Email(value).Value.Should().Be(value);
    }

    [Fact(DisplayName = "Emails with the same value are structurally equal")]
    public void Given_SameValue_When_Compared_Then_Equal()
    {
        var a = new Email("user@example.com");
        var b = new Email("user@example.com");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Emails with different values are not equal")]
    public void Given_DifferentValue_When_Compared_Then_NotEqual()
    {
        new Email("a@example.com").Should().NotBe(new Email("b@example.com"));
    }
}
