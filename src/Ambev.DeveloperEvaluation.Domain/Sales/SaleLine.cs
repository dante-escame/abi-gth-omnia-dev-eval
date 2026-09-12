using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales;

public sealed record SaleLine(ProductRef Product, Quantity Quantity, Money UnitPrice);

public sealed record SaleLineChange(Guid ProductId, Quantity Quantity);
