namespace DockerAquarium.Application.Services;

using DockerAquarium.Domain.Interfaces;
using DockerAquarium.Domain.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for monitoring Docker containers in real-time.
/// Handles container discovery and metrics collection.
/// </summary>
public class ContainerMonitoringService : IContainerMonitoringService
{
    private readonly IDockerApiClient _dockerApiClient;
    private readonly IHealthEvaluator _healthEvaluator;
    private readonly ILogger<ContainerMonitoringService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _monitoringTask;
    private List<ContainerInfo> _containers = [];
    private readonly object _lockObject = new();

    public event EventHandler<ContainerListChangedEventArgs>? ContainerListChanged;
    public event EventHandler<ContainerMetricsUpdatedEventArgs>? ContainerMetricsUpdated;

    public ContainerMonitoringService(
        IDockerApiClient dockerApiClient,
        IHealthEvaluator healthEvaluator,
        ILogger<ContainerMonitoringService> logger)
    {
        _dockerApiClient = dockerApiClient;
        _healthEvaluator = healthEvaluator;
        _logger = logger;
    }

    public async Task StartMonitoringAsync(int updateIntervalMilliseconds = 2000, CancellationToken cancellationToken = default)
    {
        if (_monitoringTask != null && !_monitoringTask.IsCompleted)
        {
            _logger.LogWarning("Monitoring is already running");
            return;
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _monitoringTask = MonitoringLoop(updateIntervalMilliseconds, _cancellationTokenSource.Token);

        _logger.LogInformation("Container monitoring started");
    }

    public async Task StopMonitoringAsync()
    {
        if (_cancellationTokenSource == null)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
        if (_monitoringTask != null)
        {
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
        _monitoringTask = null;
        _logger.LogInformation("Container monitoring stopped");
    }

    public IReadOnlyList<ContainerInfo> GetCurrentContainers()
    {
        lock (_lockObject)
        {
            return _containers.AsReadOnly();
        }
    }

    private async Task MonitoringLoop(int updateIntervalMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdateContainers(cancellationToken).ConfigureAwait(false);
                await Task.Delay(updateIntervalMilliseconds, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Monitoring loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in monitoring loop");
        }
    }

    private async Task UpdateContainers(CancellationToken cancellationToken)
    {
        try
        {
            var freshContainers = await _dockerApiClient.GetAllContainersAsync(cancellationToken).ConfigureAwait(false);
            bool containerListChanged = false;

            lock (_lockObject)
            {
                if (freshContainers.Count != _containers.Count ||
                    !freshContainers.Select(c => c.ContainerId).SequenceEqual(_containers.Select(c => c.ContainerId)))
                {
                    _containers = freshContainers.ToList();
                    containerListChanged = true;
                }
            }

            if (containerListChanged)
            {
                OnContainerListChanged(new ContainerListChangedEventArgs { Containers = freshContainers });
            }

            // Fire off metrics collection as a background task without blocking
            _ = UpdateMetricsInBackground(freshContainers, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating containers");
        }
    }

    private async Task UpdateMetricsInBackground(IReadOnlyList<ContainerInfo> containers, CancellationToken cancellationToken)
    {
        try
        {
            var runningContainers = containers.Where(c => c.IsRunning).ToList();
            
            foreach (var container in runningContainers)
            {
                // Each container gets its own timeout
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(1));
                    
                    var metrics = await _dockerApiClient.GetContainerMetricsAsync(container.ContainerId, cts.Token).ConfigureAwait(false);
                    if (metrics != null)
                    {
                        container.CurrentMetrics = metrics;
                        container.HealthStatus = _healthEvaluator.EvaluateHealth(container);
                        OnContainerMetricsUpdated(new ContainerMetricsUpdatedEventArgs
                        {
                            ContainerId = container.ContainerId,
                            Metrics = metrics
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Metrics retrieval timeout for container {ContainerId}", container.ContainerId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error updating metrics for container {ContainerId}", container.ContainerId);
                }
            }

            // Update stopped containers
            foreach (var container in containers.Where(c => !c.IsRunning))
            {
                container.HealthStatus = ContainerHealthStatus.Stopped;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background metrics update");
        }
    }

    protected virtual void OnContainerListChanged(ContainerListChangedEventArgs args)
    {
        ContainerListChanged?.Invoke(this, args);
    }

    protected virtual void OnContainerMetricsUpdated(ContainerMetricsUpdatedEventArgs args)
    {
        ContainerMetricsUpdated?.Invoke(this, args);
    }
}

