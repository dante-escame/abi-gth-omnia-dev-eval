using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Specifications.TestData;

public static class ActiveUserSpecificationTestData
{
    public static User GenerateUser(UserStatus status)
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = status;
        return user;
    }
}
