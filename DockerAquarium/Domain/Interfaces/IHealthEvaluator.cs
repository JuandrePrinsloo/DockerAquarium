namespace DockerAquarium.Domain.Interfaces;

using DockerAquarium.Domain.Models;

/// <summary>
/// Service for evaluating container health status based on metrics.
/// Follows the Single Responsibility Principle.
/// </summary>
public interface IHealthEvaluator
{
    /// <summary>
    /// Evaluates the health status of a container based on its current state and metrics.
    /// </summary>
    /// <param name="container">Container information</param>
    /// <returns>Health status evaluation</returns>
    ContainerHealthStatus EvaluateHealth(ContainerInfo container);
}

