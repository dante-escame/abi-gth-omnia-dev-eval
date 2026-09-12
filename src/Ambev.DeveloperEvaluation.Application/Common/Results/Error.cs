namespace Ambev.DeveloperEvaluation.Application.Common.Results;

public sealed record Error(string Type, string Summary, string Detail, ErrorCategory Category)
{
    public static Error Validation(string type, string summary, string detail) =>
        new(type, summary, detail, ErrorCategory.Validation);

    public static Error NotFound(string type, string summary, string detail) =>
        new(type, summary, detail, ErrorCategory.NotFound);

    public static Error Conflict(string type, string summary, string detail) =>
        new(type, summary, detail, ErrorCategory.Conflict);

    public static Error Forbidden(string type, string summary, string detail) =>
        new(type, summary, detail, ErrorCategory.Forbidden);
}
