namespace Ambev.DeveloperEvaluation.Integration.Common;

public static class ContainerRuntime
{
    public const string SkipReason = "No container runtime is reachable on this machine.";

    public static bool IsAvailable { get; } = Detect();

    private static bool Detect()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST")))
            return true;

        if (OperatingSystem.IsWindows())
            return Directory.Exists(@"\\.\pipe\") && File.Exists(@"\\.\pipe\docker_engine");

        return File.Exists("/var/run/docker.sock")
            || File.Exists($"{Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR")}/podman/podman.sock");
    }
}
