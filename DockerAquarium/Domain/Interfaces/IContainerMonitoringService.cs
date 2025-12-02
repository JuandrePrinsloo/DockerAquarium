namespace DockerAquarium.Domain.Interfaces;

using DockerAquarium.Domain.Models;

/// <summary>
/// Service for monitoring Docker containers in real-time.
/// Follows the Single Responsibility Principle.
/// </summary>
public interface IContainerMonitoringService
{
    /// <summary>
    /// Starts monitoring containers for metrics updates.
    /// </summary>
    /// <param name="updateIntervalMilliseconds">How often to update metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartMonitoringAsync(int updateIntervalMilliseconds = 2000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops monitoring containers.
    /// </summary>
    Task StopMonitoringAsync();

    /// <summary>
    /// Gets the current list of monitored containers.
    /// </summary>
    /// <returns>Read-only list of container information</returns>
    IReadOnlyList<ContainerInfo> GetCurrentContainers();

    /// <summary>
    /// Raised when the container list changes.
    /// </summary>
    event EventHandler<ContainerListChangedEventArgs>? ContainerListChanged;

    /// <summary>
    /// Raised when container metrics are updated.
    /// </summary>
    event EventHandler<ContainerMetricsUpdatedEventArgs>? ContainerMetricsUpdated;
}

/// <summary>
/// Event arguments for container list changes.
/// </summary>
public class ContainerListChangedEventArgs : EventArgs
{
    public required IReadOnlyList<ContainerInfo> Containers { get; init; }
}

/// <summary>
/// Event arguments for container metrics updates.
/// </summary>
public class ContainerMetricsUpdatedEventArgs : EventArgs
{
    public required string ContainerId { get; init; }
    public required ContainerMetrics Metrics { get; init; }
}

