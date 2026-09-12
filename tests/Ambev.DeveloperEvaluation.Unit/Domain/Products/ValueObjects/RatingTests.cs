using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class RatingTests
{
    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.1)]
    [InlineData(10)]
    public void Constructing_A_Rating_Outside_The_Zero_To_Five_Range_Throws(decimal rate)
    {
        // Act
        var act = () => new Rating(rate, 1);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Rating_With_A_Negative_Count_Throws()
    {
        // Act
        var act = () => new Rating(4.5m, -1);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2.5)]
    [InlineData(5)]
    public void Constructing_A_Rating_Inside_The_Range_Keeps_The_Rate_And_The_Count(decimal rate)
    {
        // Act
        var rating = new Rating(rate, 42);

        // Assert
        rating.Rate.Should().Be(rate);
        rating.Count.Should().Be(42);
    }

    [Fact]
    public void The_None_Rating_Has_No_Votes()
    {
        // Act
        var rating = Rating.None;

        // Assert
        rating.Rate.Should().Be(0m);
        rating.Count.Should().Be(0);
    }

    [Fact]
    public void Two_Ratings_With_The_Same_Rate_And_Count_Are_Equal()
    {
        // Arrange
        var a = new Rating(4.7m, 500);

        // Act
        var b = new Rating(4.7m, 500);

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Ratings_Differing_In_Count_Are_Not_Equal()
    {
        // Arrange
        var a = new Rating(4.7m, 500);

        // Act
        var b = new Rating(4.7m, 501);

        // Assert
        a.Should().NotBe(b);
    }
}
