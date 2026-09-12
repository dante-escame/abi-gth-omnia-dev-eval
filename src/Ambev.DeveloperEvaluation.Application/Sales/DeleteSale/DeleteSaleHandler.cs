using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public sealed class DeleteSaleHandler(ISaleRepository saleRepository)
    : IRequestHandler<DeleteSaleCommand, Result>
{
    public async Task<Result> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.Id, includeDeleted: true, cancellationToken);

        bool invisibleOrNullSale = sale is null || !SaleVisibility.IsVisibleTo(sale, command.Caller);
        if (invisibleOrNullSale)
            return Result.Failure(SaleErrors.NotFound(command.Id));

        if (sale!.IsDeleted)
            return Result.Success();

        sale.Delete();

        await saleRepository.UpdateAsync(sale, cancellationToken);

        return Result.Success();
    }
}
