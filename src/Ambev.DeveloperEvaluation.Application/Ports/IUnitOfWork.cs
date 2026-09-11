namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface IUnitOfWork
{
    Task<IAtomicScope> BeginAsync(CancellationToken cancellationToken = default);
}

public interface IAtomicScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
