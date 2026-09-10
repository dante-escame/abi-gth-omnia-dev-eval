namespace Ambev.DeveloperEvaluation.Application.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
