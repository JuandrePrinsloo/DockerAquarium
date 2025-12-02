namespace DockerAquarium.Domain.Models;

/// <summary>
/// Represents real-time metrics for a Docker container.
/// Implements Immutable pattern for thread-safety.
/// </summary>
public record ContainerMetrics
{
    /// <summary>
    /// CPU usage percentage (0-100)
    /// </summary>
    public double CpuPercentage { get; init; }

    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public ulong MemoryUsageBytes { get; init; }

    /// <summary>
    /// Memory limit in bytes
    /// </summary>
    public ulong MemoryLimitBytes { get; init; }

    /// <summary>
    /// Network input in bytes
    /// </summary>
    public ulong NetworkInputBytes { get; init; }

    /// <summary>
    /// Network output in bytes
    /// </summary>
    public ulong NetworkOutputBytes { get; init; }

    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    public DateTime CollectedAt { get; init; }

    /// <summary>
    /// Gets memory usage as a percentage of the limit.
    /// </summary>
    public double MemoryPercentage =>
        MemoryLimitBytes > 0 ? (MemoryUsageBytes / (double)MemoryLimitBytes) * 100 : 0;

    /// <summary>
    /// Gets total network activity in bytes.
    /// </summary>
    public ulong TotalNetworkBytes => NetworkInputBytes + NetworkOutputBytes;
}

