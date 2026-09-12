using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class BranchRefTests
{
    [Fact]
    public void Constructing_A_Branch_Reference_Without_An_Id_Throws()
    {
        // Act
        var act = () => new BranchRef(Guid.Empty, SaleTestData.BranchName());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructing_A_Branch_Reference_With_A_Blank_Name_Throws(string name)
    {
        // Act
        var act = () => new BranchRef(SaleTestData.Id(), name);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Branch_Reference_Trims_The_Name()
    {
        // Act
        var branch = new BranchRef(SaleTestData.Id(), "  Downtown Store  ");

        // Assert
        branch.Name.Should().Be("Downtown Store");
    }

    [Fact]
    public void Two_Branch_References_Compare_By_Id_And_Name()
    {
        // Arrange
        var id = SaleTestData.Id();

        // Act
        var branch = new BranchRef(id, "Downtown Store");

        // Assert
        branch.Should().Be(new BranchRef(id, "Downtown Store"));
        branch.Should().NotBe(new BranchRef(id, "Airport Store"));
        branch.Should().NotBe(new BranchRef(SaleTestData.Id(), "Downtown Store"));
    }
}
