using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ReplayProductEvents;

public sealed record ReplayProductEventsCommand : IRequest<long>;
