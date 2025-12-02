namespace DockerAquarium.Domain.Interfaces;

using DockerAquarium.Domain.Models;

/// <summary>
/// Provides access to Docker API operations.
/// Follows the Dependency Inversion Principle.
/// </summary>
public interface IDockerApiClient
{
    /// <summary>
    /// Gets all containers currently available in Docker.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of container information</returns>
    Task<IReadOnlyList<ContainerInfo>> GetAllContainersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets real-time metrics for a specific container.
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Container metrics or null if not available</returns>
    Task<ContainerMetrics?> GetContainerMetricsAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a stopped container.
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a running container.
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running container.
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PauseContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpauses a paused container.
    /// </summary>
    /// <param name="containerId">Container identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UnpauseContainerAsync(string containerId, CancellationToken cancellationToken = default);
}

