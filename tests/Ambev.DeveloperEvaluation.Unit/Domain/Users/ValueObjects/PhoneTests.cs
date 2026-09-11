using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Users.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData("")]
    [InlineData("(11) 98765-4321")]
    [InlineData("+55 11 98765-4321")]
    [InlineData("+0511987654321")]
    [InlineData("+5")]
    public void Constructing_A_Phone_From_An_Invalid_Value_Throws(string value)
    {
        // Act
        var act = () => new Phone(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("+5511987654321")]
    [InlineData("5511987654321")]
    public void Constructing_A_Phone_From_A_Valid_International_Number_Keeps_The_Value(string value)
    {
        // Act
        var phone = new Phone(value);

        // Assert
        phone.Value.Should().Be(value);
    }

    [Fact]
    public void Two_Phones_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new Phone("+5511987654321");

        // Act
        var b = new Phone("+5511987654321");

        // Assert
        a.Should().Be(b);
    }
}
