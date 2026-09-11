using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Users.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Activating_A_Suspended_User_Makes_The_User_Active()
    {
        // Arrange
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Suspended;

        // Act
        user.Activate();

        // Assert
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Deactivating_An_Active_User_Makes_The_User_Inactive()
    {
        // Arrange
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        // Act
        user.Deactivate();

        // Assert
        Assert.Equal(UserStatus.Inactive, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Suspending_An_Active_User_Makes_The_User_Suspended()
    {
        // Arrange
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        // Act
        user.Suspend();

        // Assert
        Assert.Equal(UserStatus.Suspended, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Registering_A_User_Raises_Exactly_One_Registration_Event()
    {
        // Act
        var user = UserTestData.RegisterValidUser();

        // Assert
        var raised = Assert.Single(user.DomainEvents);
        var registered = Assert.IsType<UserRegisteredDomainEvent>(raised);
        Assert.Equal(user.Id, registered.UserId);
        Assert.Equal(user.Email.Value, registered.Email);
        Assert.Equal(user.Username.Value, registered.Username);
        Assert.Equal(user.Role, registered.Role);
        Assert.NotEqual(default, registered.OccurredOnUtc);
    }

    [Fact]
    public void Clearing_The_Domain_Events_Empties_The_Collection()
    {
        // Arrange
        var user = UserTestData.RegisterValidUser();

        // Act
        user.ClearDomainEvents();

        // Assert
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    public void Validating_A_User_Built_From_Valid_Data_Succeeds()
    {
        // Arrange
        var user = UserTestData.GenerateValidUser();

        // Act
        var result = user.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validating_A_User_With_Sentinel_Status_And_Role_Fails()
    {
        // Arrange
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Unknown;
        user.Role = UserRole.None;

        // Act
        var result = user.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
