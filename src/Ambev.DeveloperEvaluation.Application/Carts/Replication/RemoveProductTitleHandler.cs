using Ambev.DeveloperEvaluation.Application.Ports;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.Replication;

public sealed class RemoveProductTitleHandler(IProductTitles productTitles)
    : IRequestHandler<RemoveProductTitleCommand>
{
    public Task Handle(RemoveProductTitleCommand request, CancellationToken cancellationToken) =>
        productTitles.RemoveAsync(request.ProductId, request.OccurredOnUtc, cancellationToken);
}
