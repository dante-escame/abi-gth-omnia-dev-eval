using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales.Services;

public interface IBranchDirectory
{
    BranchRef? Find(Guid branchId);
}
