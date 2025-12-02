namespace DockerAquarium.Application.Services;

using DockerAquarium.Domain.Interfaces;
using DockerAquarium.Domain.Models;

/// <summary>
/// Evaluates container health status based on metrics and state.
/// Single Responsibility: determining health status only.
/// </summary>
public class HealthEvaluator : IHealthEvaluator
{
    // Thresholds for health evaluation
    private const double HighCpuThreshold = 80.0;
    private const double HighMemoryThreshold = 85.0;
    private const double CriticalCpuThreshold = 95.0;
    private const double CriticalMemoryThreshold = 95.0;

    public ContainerHealthStatus EvaluateHealth(ContainerInfo container)
    {
        // Stopped containers are not healthy
        if (!container.IsRunning)
        {
            return ContainerHealthStatus.Stopped;
        }

        // No metrics available
        if (container.CurrentMetrics == null)
        {
            return ContainerHealthStatus.Warning;
        }

        var metrics = container.CurrentMetrics;

        // Check for critical conditions
        if (metrics.CpuPercentage >= CriticalCpuThreshold || 
            metrics.MemoryPercentage >= CriticalMemoryThreshold)
        {
            return ContainerHealthStatus.Critical;
        }

        // Check for warning conditions
        if (metrics.CpuPercentage >= HighCpuThreshold || 
            metrics.MemoryPercentage >= HighMemoryThreshold)
        {
            return ContainerHealthStatus.Warning;
        }

        // Container is healthy
        return ContainerHealthStatus.Healthy;
    }
}

