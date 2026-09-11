using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Users.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nolocal.com")]
    public void Constructing_An_Email_From_An_Invalid_Value_Throws(string value)
    {
        // Act
        var act = () => new Email(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("first.last+tag@sub.example.co")]
    public void Constructing_An_Email_From_A_Valid_Value_Keeps_The_Value(string value)
    {
        // Act
        var email = new Email(value);

        // Assert
        email.Value.Should().Be(value);
    }

    [Fact]
    public void Two_Emails_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new Email("user@example.com");
        var b = new Email("user@example.com");

        // Act
        bool equal = a == b;

        // Assert
        a.Should().Be(b);
        equal.Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Emails_With_Different_Values_Are_Not_Equal()
    {
        // Arrange
        var a = new Email("a@example.com");
        var b = new Email("b@example.com");

        // Act
        bool equal = a == b;

        // Assert
        a.Should().NotBe(b);
        equal.Should().BeFalse();
    }
}
