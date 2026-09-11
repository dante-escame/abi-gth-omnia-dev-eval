using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using Ambev.DeveloperEvaluation.Unit.Domain.Specifications.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Specifications;

public class ActiveUserSpecificationTests
{
    [Theory]
    [InlineData(UserStatus.Active, true)]
    [InlineData(UserStatus.Inactive, false)]
    [InlineData(UserStatus.Suspended, false)]
    public void Only_An_Active_User_Satisfies_The_Active_User_Specification(UserStatus status, bool expectedResult)
    {
        // Arrange
        var user = ActiveUserSpecificationTestData.GenerateUser(status);
        var specification = new ActiveUserSpecification();

        // Act
        bool result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().Be(expectedResult);
    }
}
