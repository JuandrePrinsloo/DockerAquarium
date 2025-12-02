namespace DockerAquarium.Domain.Models;

/// <summary>
/// Represents a Docker container in the aquarium.
/// Acts as the core domain entity.
/// </summary>
public class ContainerInfo
{
    /// <summary>
    /// Unique identifier for the container
    /// </summary>
    public required string ContainerId { get; set; }

    /// <summary>
    /// Container name for display
    /// </summary>
    public required string ContainerName { get; set; }

    /// <summary>
    /// Image name/tag
    /// </summary>
    public required string ImageName { get; set; }

    /// <summary>
    /// Current state of the container
    /// </summary>
    public required string State { get; set; }

    /// <summary>
    /// Current health status
    /// </summary>
    public ContainerHealthStatus HealthStatus { get; set; }

    /// <summary>
    /// Latest metrics for this container
    /// </summary>
    public ContainerMetrics? CurrentMetrics { get; set; }

    /// <summary>
    /// Whether the container is running
    /// </summary>
    public bool IsRunning => State.Equals("running", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the display size factor based on memory usage (0-100).
    /// Used for visualizing fish size in the aquarium.
    /// </summary>
    public double SizeFactor => CurrentMetrics?.MemoryPercentage ?? 0;

    /// <summary>
    /// Gets the activity level based on CPU and network usage (0-100).
    /// Used for visualizing fish speed in the aquarium.
    /// </summary>
    public double ActivityLevel
    {
        get
        {
            if (CurrentMetrics == null)
                return 0;

            var cpuWeight = CurrentMetrics.CpuPercentage * 0.6;
            var networkWeight = Math.Min(CurrentMetrics.TotalNetworkBytes / 1_000_000.0, 100) * 0.4;
            return Math.Min((cpuWeight + networkWeight) / 100 * 100, 100);
        }
    }
}

