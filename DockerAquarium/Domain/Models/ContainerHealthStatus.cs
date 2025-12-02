namespace DockerAquarium.Domain.Models;

/// <summary>
/// Enumeration for container health states.
/// </summary>
public enum ContainerHealthStatus
{
    /// <summary>
    /// Container is running healthily
    /// </summary>
    Healthy,

    /// <summary>
    /// Container is running but with warnings
    /// </summary>
    Warning,

    /// <summary>
    /// Container is in a critical state
    /// </summary>
    Critical,

    /// <summary>
    /// Container is not running
    /// </summary>
    Stopped
}

