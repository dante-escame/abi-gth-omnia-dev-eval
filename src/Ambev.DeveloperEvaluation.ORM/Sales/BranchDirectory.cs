using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Microsoft.Extensions.Options;

namespace Ambev.DeveloperEvaluation.ORM.Sales;

public sealed class BranchDirectory : IBranchDirectory
{
    private readonly IReadOnlyDictionary<Guid, BranchRef> _branches;

    public BranchDirectory(IOptions<BranchOptions> options)
    {
        _branches = options.Value.Entries
            .Where(entry => entry.Id != Guid.Empty && !string.IsNullOrWhiteSpace(entry.Name))
            .GroupBy(entry => entry.Id)
            .ToDictionary(group => group.Key, group => new BranchRef(group.Key, group.First().Name));
    }

    public BranchRef? Find(Guid branchId) =>
        _branches.TryGetValue(branchId, out var branch) ? branch : null;
}
