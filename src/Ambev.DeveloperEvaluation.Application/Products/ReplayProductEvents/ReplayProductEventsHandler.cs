using Ambev.DeveloperEvaluation.Application.Ports;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ReplayProductEvents;

public sealed class ReplayProductEventsHandler(IProductEventReplay replay)
    : IRequestHandler<ReplayProductEventsCommand, long>
{
    public Task<long> Handle(ReplayProductEventsCommand request, CancellationToken cancellationToken) =>
        replay.EnqueueSnapshotsAsync(cancellationToken);
}
