using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public sealed class CancelSaleItemHandler(ISaleRepository saleRepository)
    : IRequestHandler<CancelSaleItemCommand, Result<SaleResult>>
{
    public async Task<Result<SaleResult>> Handle(
        CancelSaleItemCommand command,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.SaleId, cancellationToken: cancellationToken);

        if (sale is null || !SaleVisibility.IsVisibleTo(sale, command.Caller))
            return Result.Failure<SaleResult>(SaleErrors.NotFound(command.SaleId));

        if (sale.Status == SaleStatus.Cancelled)
            return Result.Failure<SaleResult>(SaleErrors.AlreadyCancelled(command.SaleId));

        var item = sale.Items.FirstOrDefault(candidate => candidate.Id == command.ItemId);

        if (item is null)
            return Result.Failure<SaleResult>(SaleErrors.ItemNotFound(command.SaleId, command.ItemId));

        if (!item.IsActive)
            return Result.Failure<SaleResult>(SaleErrors.ItemAlreadyCancelled(command.ItemId));

        sale.CancelItem(command.ItemId);

        await saleRepository.UpdateAsync(sale, cancellationToken);

        return Result.Success(SaleResult.From(sale));
    }
}
