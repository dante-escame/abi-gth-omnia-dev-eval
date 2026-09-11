namespace Ambev.DeveloperEvaluation.Application.Products.Common;

public sealed record RatingInput(decimal Rate, int Count)
{
    public static RatingInput Empty => new(0m, 0);
}
