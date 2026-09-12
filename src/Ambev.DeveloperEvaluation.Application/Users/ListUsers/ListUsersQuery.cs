using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public sealed record ListUsersQuery(ListQuery List) : IRequest<PagedResult<UserResult>>;
