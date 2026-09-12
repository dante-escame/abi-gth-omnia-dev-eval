using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Sales;

public sealed class SaleNumberGenerator(DefaultContext context) : ISaleNumberGenerator
{
    public const string SequenceName = "sale_number_seq";

    private const string NextValueSql = $"SELECT nextval('{SequenceName}') AS \"Value\"";

    public async Task<SaleNumber> NextAsync(CancellationToken cancellationToken = default)
    {
        long next = await context.Database
            .SqlQueryRaw<long>(NextValueSql)
            .SingleAsync(cancellationToken);

        return SaleNumber.FromSequence(next);
    }
}
