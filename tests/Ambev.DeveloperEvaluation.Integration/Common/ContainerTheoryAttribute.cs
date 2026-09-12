using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Common;

public sealed class ContainerTheoryAttribute : TheoryAttribute
{
    public ContainerTheoryAttribute()
    {
        if (!ContainerRuntime.IsAvailable)
            Skip = ContainerRuntime.SkipReason;
    }
}
