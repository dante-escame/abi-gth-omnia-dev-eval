using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class RatingTests
{
    [Theory(DisplayName = "Rate outside the zero to five range is rejected")]
    [InlineData(-0.1)]
    [InlineData(5.1)]
    [InlineData(10)]
    public void Given_RateOutOfRange_When_Constructed_Then_Throws(decimal rate)
    {
        var act = () => new Rating(rate, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Negative count is rejected")]
    public void Given_NegativeCount_When_Constructed_Then_Throws()
    {
        var act = () => new Rating(4.5m, -1);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Rate at the range bounds is accepted")]
    [InlineData(0)]
    [InlineData(2.5)]
    [InlineData(5)]
    public void Given_RateInRange_When_Constructed_Then_KeepsValues(decimal rate)
    {
        var rating = new Rating(rate, 42);

        rating.Rate.Should().Be(rate);
        rating.Count.Should().Be(42);
    }

    [Fact(DisplayName = "None is an unrated value")]
    public void Given_None_When_Read_Then_HasNoVotes()
    {
        Rating.None.Rate.Should().Be(0m);
        Rating.None.Count.Should().Be(0);
    }

    [Fact(DisplayName = "Ratings with the same rate and count are structurally equal")]
    public void Given_SameValues_When_Compared_Then_Equal()
    {
        var a = new Rating(4.7m, 500);
        var b = new Rating(4.7m, 500);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Ratings differing in count are not equal")]
    public void Given_DifferentCount_When_Compared_Then_NotEqual()
    {
        new Rating(4.7m, 500).Should().NotBe(new Rating(4.7m, 501));
    }
}
