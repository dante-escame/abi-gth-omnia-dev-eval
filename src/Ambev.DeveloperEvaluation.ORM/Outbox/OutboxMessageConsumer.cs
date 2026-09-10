namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public class OutboxMessageConsumer
{
    public Guid OutboxMessageId { get; set; }

    public string HandlerName { get; set; } = string.Empty;
}
