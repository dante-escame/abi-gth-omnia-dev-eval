namespace Ambev.DeveloperEvaluation.Application.Carts.Common;

public sealed class CartListItem
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } = string.Empty;

    public IReadOnlyList<CartLineItem> Items { get; set; } = [];
}

public sealed class CartLineItem
{
    public Guid ProductId { get; set; }

    public string? Title { get; set; }

    public int Quantity { get; set; }
}
