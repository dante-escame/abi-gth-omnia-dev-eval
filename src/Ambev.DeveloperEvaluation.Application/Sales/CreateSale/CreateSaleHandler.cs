using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public sealed class CreateSaleHandler(
    ISaleRepository saleRepository,
    ICartRepository cartRepository,
    IProductReader productReader,
    IBranchDirectory branchDirectory,
    ISaleNumberGenerator saleNumbers,
    IDiscountPolicy discountPolicy,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSaleCommand, Result<SaleResult>>
{
    public async Task<Result<SaleResult>> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var branch = branchDirectory.Find(command.BranchId);

        if (branch is null)
            return Result.Failure<SaleResult>(SaleErrors.UnknownBranch(command.BranchId));

        var cart = await cartRepository.GetByIdAsync(command.CartId, cancellationToken);

        if (cart is null || !CartLines.IsVisibleTo(cart, command.Caller))
            return Result.Failure<SaleResult>(SaleErrors.UnknownCart(command.CartId));

        if (cart.Status == CartStatus.CheckedOut)
            return Result.Failure<SaleResult>(SaleErrors.CartAlreadyCheckedOut(command.CartId));

        if (cart.Items.Count == 0)
            return Result.Failure<SaleResult>(SaleErrors.EmptyCart(command.CartId));

        var snapshots = await productReader.GetManyAsync(
            cart.Items.Select(item => item.Product.Id).Distinct().ToList(),
            cancellationToken);

        var unknown = cart.Items.FirstOrDefault(item => !snapshots.ContainsKey(item.Product.Id));

        if (unknown is not null)
            return Result.Failure<SaleResult>(SaleErrors.UnknownProduct(unknown.Product.Id));

        var customer = new CustomerRef(command.Caller.UserId, command.Caller.Name);

        await using var scope = await unitOfWork.BeginAsync(cancellationToken);

        Sale sale;

        try
        {
            var lines = BuildLines(cart, snapshots);
            var number = await saleNumbers.NextAsync(cancellationToken);

            sale = Sale.Create(number, customer, branch, cart.Id, lines, discountPolicy);
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

        cart.MarkCheckedOut(sale.Id);

        await saleRepository.CreateAsync(sale, cancellationToken);
        await cartRepository.UpdateAsync(cart, cancellationToken);
        await scope.CommitAsync(cancellationToken);

        return Result.Success(SaleResult.From(sale));
    }

    private static List<SaleLine> BuildLines(Cart cart, IReadOnlyDictionary<Guid, ProductSnapshot> snapshots)
    {
        var lines = new List<SaleLine>();

        foreach (var item in cart.Items)
        {
            var snapshot = snapshots[item.Product.Id];

            lines.Add(new SaleLine(
                new ProductRef(snapshot.Id, snapshot.Title),
                new Quantity(item.Quantity.Value),
                new Money(snapshot.Price)));
        }

        return lines;
    }
}
