namespace Ambev.DeveloperEvaluation.Application.Carts.Common;

public sealed record CartLineInput(Guid ProductId, int Quantity);
