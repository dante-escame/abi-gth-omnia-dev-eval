using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.Services;

public class TieredDiscountPolicyTests
{
    private readonly IDiscountPolicy _policy = new TieredDiscountPolicy();

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0.10)]
    [InlineData(5, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(15, 0.20)]
    [InlineData(20, 0.20)]
    public void Every_Quantity_Boundary_Resolves_To_Its_Documented_Rate(int quantity, decimal expected)
    {
        // Act
        var rate = _policy.Resolve(new Quantity(quantity));

        // Assert
        rate.Value.Should().Be(expected);
    }

    [Fact]
    public void Twenty_One_Never_Reaches_The_Policy_Because_The_Quantity_Threw_First()
    {
        // Act
        var act = () => _policy.Resolve(new Quantity(21));

        // Assert
        act.Should().Throw<MaxItemsExceededException>();
    }

    [Fact]
    public void Resolving_Without_A_Quantity_Throws()
    {
        // Act
        var act = () => _policy.Resolve(null!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void The_Policy_Needs_Nothing_Injected_So_A_Test_Builds_It_With_New()
    {
        // Act
        var constructors = typeof(TieredDiscountPolicy).GetConstructors();

        // Assert
        constructors.Should().ContainSingle()
            .Which.GetParameters().Should().BeEmpty();
    }
}
