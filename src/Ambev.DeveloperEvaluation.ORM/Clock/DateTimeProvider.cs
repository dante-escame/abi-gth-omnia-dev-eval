using Ambev.DeveloperEvaluation.Application.Clock;

namespace Ambev.DeveloperEvaluation.ORM.Clock;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
