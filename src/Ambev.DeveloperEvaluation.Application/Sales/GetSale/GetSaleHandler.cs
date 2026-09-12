using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public sealed class GetSaleHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetSaleQuery, Result<SaleResult>>
{
    public async Task<Result<SaleResult>> Handle(GetSaleQuery request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

        return sale is null || !SaleVisibility.IsVisibleTo(sale, request.Caller)
            ? Result.Failure<SaleResult>(SaleErrors.NotFound(request.Id))
            : Result.Success(SaleResult.From(sale));
    }
}
