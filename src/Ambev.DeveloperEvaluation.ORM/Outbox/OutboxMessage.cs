namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Error { get; set; }
}
