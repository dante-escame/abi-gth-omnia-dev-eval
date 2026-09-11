using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales.Services;

public interface ISaleNumberGenerator
{
    Task<SaleNumber> NextAsync(CancellationToken cancellationToken = default);
}
