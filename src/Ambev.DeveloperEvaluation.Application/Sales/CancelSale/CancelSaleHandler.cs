using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public sealed class CancelSaleHandler(ISaleRepository saleRepository)
    : IRequestHandler<CancelSaleCommand, Result<SaleResult>>
{
    public async Task<Result<SaleResult>> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (sale is null || !SaleVisibility.IsVisibleTo(sale, command.Caller))
            return Result.Failure<SaleResult>(SaleErrors.NotFound(command.Id));

        if (sale.Status == SaleStatus.Cancelled)
            return Result.Success(SaleResult.From(sale));

        sale.Cancel(command.Reason);

        await saleRepository.UpdateAsync(sale, cancellationToken);

        return Result.Success(SaleResult.From(sale));
    }
}
