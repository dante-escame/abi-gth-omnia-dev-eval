using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales.Services;

public interface IDiscountPolicy
{
    DiscountRate Resolve(Quantity quantity);
}
