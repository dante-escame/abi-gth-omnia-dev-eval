namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface IProductEventReplay
{
    Task<long> EnqueueSnapshotsAsync(CancellationToken cancellationToken = default);
}
