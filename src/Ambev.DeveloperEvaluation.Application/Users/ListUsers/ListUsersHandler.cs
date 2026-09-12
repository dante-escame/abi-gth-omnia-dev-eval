using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public sealed class ListUsersHandler(IUserRepository userRepository, IPagedQueryExecutor executor)
    : IRequestHandler<ListUsersQuery, PagedResult<UserResult>>
{
    public async Task<PagedResult<UserResult>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = userRepository.Query()
            .ApplyFilters(request.List, UserListFields.Map)
            .ApplyOrdering(request.List, UserListFields.Map);

        var page = await executor.ToPagedResultAsync(query, request.List.Page, request.List.Size, cancellationToken);

        return page.Map(UserResult.From);
    }
}
