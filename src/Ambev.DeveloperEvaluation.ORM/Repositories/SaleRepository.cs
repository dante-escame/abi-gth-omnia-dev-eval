using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.ORM.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository(DefaultContext context) : ISaleRepository
{
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await context.Sales.AddAsync(sale, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(
        Guid id,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var sales = includeDeleted ? context.Sales.IgnoreQueryFilters() : context.Sales;

        return await sales
            .Include(SaleConfiguration.ItemsNavigation)
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        context.Sales.Update(sale);
        await context.SaveChangesAsync(cancellationToken);
        return sale;
    }
}
