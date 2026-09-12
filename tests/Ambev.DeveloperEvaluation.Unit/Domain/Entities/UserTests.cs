using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Users.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class UserTests
{
    [Fact(DisplayName = "User status should change to Active when activated")]
    public void Given_SuspendedUser_When_Activated_Then_StatusShouldBeActive()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Suspended;

        user.Activate();

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact(DisplayName = "User status should change to Inactive when deactivated")]
    public void Given_ActiveUser_When_Deactivated_Then_StatusShouldBeInactive()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        user.Deactivate();

        Assert.Equal(UserStatus.Inactive, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact(DisplayName = "User status should change to Suspended when suspended")]
    public void Given_ActiveUser_When_Suspended_Then_StatusShouldBeSuspended()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;

        user.Suspend();

        Assert.Equal(UserStatus.Suspended, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact(DisplayName = "Registering a user raises exactly one UserRegisteredDomainEvent")]
    public void Given_RegisteredUser_When_Created_Then_RaisesSingleEvent()
    {
        var user = UserTestData.RegisterValidUser();

        var raised = Assert.Single(user.DomainEvents);
        var registered = Assert.IsType<UserRegisteredDomainEvent>(raised);
        Assert.Equal(user.Id, registered.UserId);
        Assert.Equal(user.Email.Value, registered.Email);
        Assert.Equal(user.Username.Value, registered.Username);
        Assert.Equal(user.Role, registered.Role);
        Assert.NotEqual(default, registered.OccurredOnUtc);
    }

    [Fact(DisplayName = "Clearing domain events empties the collection")]
    public void Given_RegisteredUser_When_EventsCleared_Then_CollectionIsEmpty()
    {
        var user = UserTestData.RegisterValidUser();

        user.ClearDomainEvents();

        Assert.Empty(user.DomainEvents);
    }

    [Fact(DisplayName = "Validation should pass for valid user data")]
    public void Given_ValidUserData_When_Validated_Then_ShouldReturnValid()
    {
        var user = UserTestData.GenerateValidUser();

        var result = user.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validation should fail when status and role are sentinels")]
    public void Given_SentinelStatusAndRole_When_Validated_Then_ShouldReturnInvalid()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Unknown;
        user.Role = UserRole.None;

        var result = user.Validate();

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
