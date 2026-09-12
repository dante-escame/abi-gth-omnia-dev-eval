using Ambev.DeveloperEvaluation.Application.Ports;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.Replication;

public sealed class UpsertProductTitleHandler(IProductTitles productTitles)
    : IRequestHandler<UpsertProductTitleCommand>
{
    public Task Handle(UpsertProductTitleCommand request, CancellationToken cancellationToken) =>
        productTitles.UpsertAsync(request.ProductId, request.Title, request.OccurredOnUtc, cancellationToken);
}
