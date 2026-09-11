using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class CustomerRefTests
{
    [Fact]
    public void Constructing_A_Customer_Reference_Without_An_Id_Throws()
    {
        // Act
        var act = () => new CustomerRef(Guid.Empty, SaleTestData.PersonName());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructing_A_Customer_Reference_With_A_Blank_Name_Throws(string name)
    {
        // Act
        var act = () => new CustomerRef(SaleTestData.Id(), name);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Customer_Reference_Trims_The_Name()
    {
        // Act
        var customer = new CustomerRef(SaleTestData.Id(), "  Ada Lovelace  ");

        // Assert
        customer.Name.Should().Be("Ada Lovelace");
    }

    [Fact]
    public void Two_Customer_References_Compare_By_Id_And_Name()
    {
        // Arrange
        var id = SaleTestData.Id();

        // Act
        var customer = new CustomerRef(id, "Ada Lovelace");

        // Assert
        customer.Should().Be(new CustomerRef(id, "Ada Lovelace"));
        customer.Should().NotBe(new CustomerRef(id, "Grace Hopper"));
        customer.Should().NotBe(new CustomerRef(SaleTestData.Id(), "Ada Lovelace"));
    }
}
