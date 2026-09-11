using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed class UpdateSaleHandler(ISaleRepository saleRepository, IDiscountPolicy discountPolicy)
    : IRequestHandler<UpdateSaleCommand, Result<SaleResult>>
{
    public async Task<Result<SaleResult>> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (sale is null || !SaleVisibility.IsVisibleTo(sale, command.Caller))
            return Result.Failure<SaleResult>(SaleErrors.NotFound(command.Id));

        if (sale.Status == SaleStatus.Cancelled)
            return Result.Failure<SaleResult>(SaleErrors.AlreadyCancelled(command.Id));

        if (command.Items.Count == 0)
            return Result.Failure<SaleResult>(SaleErrors.EmptySale(command.Id));

        var unknown = command.Items.FirstOrDefault(line =>
            sale.Items.All(item => !item.IsActive || item.Product.Id != line.ProductId));

        if (unknown is not null)
            return Result.Failure<SaleResult>(SaleErrors.UnknownItem(unknown.ProductId));

        try
        {
            var changes = command.Items
                .Select(line => new SaleLineChange(line.ProductId, new Quantity(line.Quantity)))
                .ToList();

            sale.ModifyItems(changes, discountPolicy);
        }
        catch (MaxItemsExceededException exception)
        {
            return Result.Failure<SaleResult>(
                SaleErrors.QuantityExceeded(exception.Attempted, exception.Maximum));
        }
        catch (DomainException exception)
        {
            return Result.Failure<SaleResult>(SaleErrors.Rejected(exception.Message));
        }

        await saleRepository.UpdateAsync(sale, cancellationToken);

        return Result.Success(SaleResult.From(sale));
    }
}
