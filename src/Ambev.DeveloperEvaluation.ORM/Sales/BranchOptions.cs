namespace Ambev.DeveloperEvaluation.ORM.Sales;

public sealed class BranchOptions
{
    public const string SectionName = "Branches";

    public List<BranchEntry> Entries { get; set; } = [];
}

public sealed class BranchEntry
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
