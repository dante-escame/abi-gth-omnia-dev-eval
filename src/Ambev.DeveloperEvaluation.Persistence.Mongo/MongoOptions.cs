namespace Ambev.DeveloperEvaluation.Persistence.Mongo;

public sealed class MongoOptions
{
    public string ConnectionString { get; init; } = string.Empty;

    public string Database { get; init; } = string.Empty;
}
