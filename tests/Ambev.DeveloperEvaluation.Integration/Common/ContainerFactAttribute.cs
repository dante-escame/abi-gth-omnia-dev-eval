using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Common;

public sealed class ContainerFactAttribute : FactAttribute
{
    public ContainerFactAttribute()
    {
        if (!ContainerRuntime.IsAvailable)
            Skip = ContainerRuntime.SkipReason;
    }
}
