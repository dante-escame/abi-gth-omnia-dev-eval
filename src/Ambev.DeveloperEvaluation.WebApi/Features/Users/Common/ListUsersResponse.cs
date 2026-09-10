using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Users.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.Common;

public sealed record ListUsersResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<UserResponse> Data,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("currentPage")] int CurrentPage,
    [property: JsonPropertyName("totalPages")] int TotalPages)
{
    public static ListUsersResponse From(PagedResult<UserResult> page) => new(
        page.Data.Select(UserResponse.From).ToList(),
        page.TotalItems,
        page.CurrentPage,
        page.TotalPages);
}
