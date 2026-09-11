using Ambev.DeveloperEvaluation.Application.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ambev.DeveloperEvaluation.ORM;

public sealed class UnitOfWork(DefaultContext context) : IUnitOfWork
{
    public async Task<IAtomicScope> BeginAsync(CancellationToken cancellationToken = default) =>
        new AtomicScope(await context.Database.BeginTransactionAsync(cancellationToken));

    private sealed class AtomicScope(IDbContextTransaction transaction) : IAtomicScope
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            transaction.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
