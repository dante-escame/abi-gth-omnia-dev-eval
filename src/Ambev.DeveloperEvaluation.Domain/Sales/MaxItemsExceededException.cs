namespace Ambev.DeveloperEvaluation.Domain.Sales;

public class MaxItemsExceededException(int attempted, int maximum)
    : DomainException($"A sale line cannot hold more than {maximum} identical items, {attempted} were requested.")
{
    public int Attempted { get; } = attempted;

    public int Maximum { get; } = maximum;
}
