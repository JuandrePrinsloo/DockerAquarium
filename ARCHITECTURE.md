# Docker Aquarium - Architecture & API Documentation

## Table of Contents
1. [System Architecture](#system-architecture)
2. [Domain Models](#domain-models)
3. [Service Interfaces](#service-interfaces)
4. [Component Interactions](#component-interactions)
5. [Data Flow](#data-flow)
6. [Extending the System](#extending-the-system)

---

## System Architecture

### Layered Architecture

The application follows a **4-layer clean architecture** pattern:

```
┌─────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER                                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ UI (XAML)            ViewModel         ValueConverters│  │
│  │ MainWindow.xaml      AquariumVM        ColorToBrush   │  │
│  │                      FishViewModel     NotBool        │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
                            ↑↓
┌──────────────────────────────────────────────────────────────┐
│  APPLICATION LAYER                                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Services             Commands                         │  │
│  │ ContainerMonitoring  StartMonitoring                 │  │
│  │ HealthEvaluator      StopMonitoring                  │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
                            ↑↓
┌──────────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER                                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ External Integrations                               │  │
│  │ DockerApiClient (Mock Implementation)               │  │
│  │ Ready for Docker.DotNet integration                 │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
                            ↑↓
┌──────────────────────────────────────────────────────────────┐
│  DOMAIN LAYER (Pure Business Logic)                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Models              Interfaces                        │  │
│  │ ContainerInfo       IDockerApiClient                 │  │
│  │ ContainerMetrics    IContainerMonitoringService      │  │
│  │ ContainerHealth     IHealthEvaluator                 │  │
│  │ HealthStatus                                         │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## Domain Models

### ContainerHealthStatus (Enum)

Enumeration for container health states.

```csharp
public enum ContainerHealthStatus
{
    Healthy,    // Container running normally, good resources
    Warning,    // Container has elevated resource usage
    Critical,   // Container has critical resource issues
    Stopped     // Container is not running
}
```

### ContainerMetrics (Record)

Immutable metrics snapshot for a container.

```csharp
public record ContainerMetrics
{
    public double CpuPercentage { get; init; }           // CPU usage %
    public ulong MemoryUsageBytes { get; init; }         // Current memory
    public ulong MemoryLimitBytes { get; init; }         // Memory limit
    public ulong NetworkInputBytes { get; init; }        // Network RX
    public ulong NetworkOutputBytes { get; init; }       // Network TX
    public DateTime CollectedAt { get; init; }           // Timestamp
    
    public double MemoryPercentage => 
        MemoryLimitBytes > 0 
            ? (MemoryUsageBytes / (double)MemoryLimitBytes) * 100 
            : 0;
            
    public ulong TotalNetworkBytes => 
        NetworkInputBytes + NetworkOutputBytes;
}
```

### ContainerInfo (Class)

Core domain entity representing a Docker container.

```csharp
public class ContainerInfo
{
    public required string ContainerId { get; set; }
    public required string ContainerName { get; set; }
    public required string ImageName { get; set; }
    public required string State { get; set; }
    public ContainerHealthStatus HealthStatus { get; set; }
    public ContainerMetrics? CurrentMetrics { get; set; }
    
    public bool IsRunning => 
        State.Equals("running", StringComparison.OrdinalIgnoreCase);
    
    public double SizeFactor => CurrentMetrics?.MemoryPercentage ?? 0;
    
    public double ActivityLevel
    {
        get
        {
            if (CurrentMetrics == null) return 0;
            var cpuWeight = CurrentMetrics.CpuPercentage * 0.6;
            var networkWeight = 
                Math.Min(CurrentMetrics.TotalNetworkBytes / 1_000_000.0, 100) * 0.4;
            return Math.Min((cpuWeight + networkWeight) / 100 * 100, 100);
        }
    }
}
```

---

## Service Interfaces

### IDockerApiClient

Abstraction for Docker API operations.

```csharp
public interface IDockerApiClient
{
    /// Gets all containers (running and stopped)
    Task<IReadOnlyList<ContainerInfo>> GetAllContainersAsync(
        CancellationToken cancellationToken = default);
    
    /// Gets real-time metrics for a container
    Task<ContainerMetrics?> GetContainerMetricsAsync(
        string containerId, 
        CancellationToken cancellationToken = default);
    
    /// Starts a stopped container
    Task StartContainerAsync(
        string containerId, 
        CancellationToken cancellationToken = default);
    
    /// Stops a running container
    Task StopContainerAsync(
        string containerId, 
        CancellationToken cancellationToken = default);
    
    /// Pauses a running container
    Task PauseContainerAsync(
        string containerId, 
        CancellationToken cancellationToken = default);
    
    /// Unpauses a paused container
    Task UnpauseContainerAsync(
        string containerId, 
        CancellationToken cancellationToken = default);
}
```

### IContainerMonitoringService

Service for monitoring containers in real-time.

```csharp
public interface IContainerMonitoringService
{
    /// Starts monitoring with specified update interval
    Task StartMonitoringAsync(
        int updateIntervalMilliseconds = 2000, 
        CancellationToken cancellationToken = default);
    
    /// Stops monitoring
    Task StopMonitoringAsync();
    
    /// Gets current list of monitored containers
    IReadOnlyList<ContainerInfo> GetCurrentContainers();
    
    /// Raised when container list changes
    event EventHandler<ContainerListChangedEventArgs>? ContainerListChanged;
    
    /// Raised when metrics are updated
    event EventHandler<ContainerMetricsUpdatedEventArgs>? ContainerMetricsUpdated;
}

public class ContainerListChangedEventArgs : EventArgs
{
    public required IReadOnlyList<ContainerInfo> Containers { get; init; }
}

public class ContainerMetricsUpdatedEventArgs : EventArgs
{
    public required string ContainerId { get; init; }
    public required ContainerMetrics Metrics { get; init; }
}
```

### IHealthEvaluator

Service for evaluating container health.

```csharp
public interface IHealthEvaluator
{
    /// Evaluates health status based on container state and metrics
    ContainerHealthStatus EvaluateHealth(ContainerInfo container);
}
```

---

## Component Interactions

### Startup Flow

```
App.xaml.cs
    ↓
OnStartup()
    ↓
ConfigureServices() - Register all dependencies
    ↓
Build IServiceProvider
    ↓
Create MainWindow & AquariumViewModel
    ↓
Set DataContext
    ↓
Show MainWindow
```

### Monitoring Flow

```
User clicks "Start Monitoring"
    ↓
AquariumViewModel.StartMonitoring()
    ↓
IContainerMonitoringService.StartMonitoringAsync()
    ↓
MonitoringLoop() starts on background thread
    ↓
Every 1500ms:
    ├─ GetAllContainersAsync() - Get container list
    ├─ For each container: GetContainerMetricsAsync()
    ├─ EvaluateHealth() for each container
    └─ Raise events to update UI
    ↓
UI updates via event handlers
    ↓
AquariumViewModel syncs FishViewModel collection
    ↓
MainWindow updates canvas binding
    ↓
Animation loop moves fish continuously
```

### Health Evaluation Logic

```
Container received
    ↓
Is running?
    ├─ No → Status = Stopped
    └─ Yes
        ↓
        Metrics available?
        ├─ No → Status = Warning
        └─ Yes
            ↓
            Check thresholds:
            ├─ CPU >= 95% OR Memory >= 95% → Critical
            ├─ CPU >= 80% OR Memory >= 85% → Warning
            └─ All healthy → Healthy
```

---

## Data Flow

### Real-Time Updates

```
Docker API / Mock Client
        ↓
ContainerMetrics
        ↓
IContainerMonitoringService (thread-safe collection)
        ↓
Events (ContainerMetricsUpdatedEventArgs)
        ↓
Application.Current.Dispatcher.Invoke()
        ↓
AquariumViewModel updates FishViewModel
        ↓
WPF Binding updates UI
        ↓
Animation loop moves fish
        ↓
Visual update displayed
```

### Color Determination

```
ContainerHealthStatus
    ├─ Healthy → #00c864 (Green)
    ├─ Warning → #ffc800 (Yellow)
    ├─ Critical → #ff3232 (Red)
    └─ Stopped → #969696 (Gray)
        ↓
Color → Brush (via ColorToBrushConverter)
        ↓
Fill property binding
        ↓
Ellipse rendered with color
```

---

## Extending the System

### Adding a New Container Action

1. **Add to IDockerApiClient interface**:
```csharp
Task RestartContainerAsync(string containerId, CancellationToken cancellationToken = default);
```

2. **Implement in DockerApiClient**:
```csharp
public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken = default)
{
    // Implementation here
}
```

3. **Add to AquariumViewModel**:
```csharp
public ICommand RestartCommand { get; }

private void RestartContainer(ContainerInfo container)
{
    _dockerApiClient.RestartContainerAsync(container.ContainerId);
}
```

4. **Add UI button to MainWindow.xaml**:
```xaml
<Button Command="{Binding RestartCommand}" Content="Restart Selected"/>
```

### Adding a New Health Rule

1. **Modify HealthEvaluator.cs**:
```csharp
public ContainerHealthStatus EvaluateHealth(ContainerInfo container)
{
    // ... existing code ...
    
    // Add new rule
    if (metrics.DiskUsagePercentage > 90)
    {
        return ContainerHealthStatus.Critical;
    }
    
    // ... rest of logic ...
}
```

2. **Add new metric to ContainerMetrics**:
```csharp
public record ContainerMetrics
{
    // ... existing properties ...
    public double DiskUsagePercentage { get; init; }
}
```

### Adding Metrics Display

1. **Add property to FishViewModel**:
```csharp
private double _cpuPercentage;

public double CpuPercentage
{
    get => _cpuPercentage;
    set
    {
        if (Math.Abs(_cpuPercentage - value) > 0.01)
        {
            _cpuPercentage = value;
            OnPropertyChanged(nameof(CpuPercentage));
        }
    }
}
```

2. **Update from AquariumViewModel**:
```csharp
private void UpdateFishFromMetrics(FishViewModel fish, ContainerMetrics metrics)
{
    // ... existing code ...
    fish.CpuPercentage = metrics.CpuPercentage;
}
```

3. **Bind in MainWindow.xaml**:
```xaml
<Ellipse.ToolTip>
    <ToolTip>
        <StackPanel>
            <!-- ... existing content ... -->
            <TextBlock Text="{Binding CpuPercentage, StringFormat='CPU: {0:F1}%'}"/>
        </StackPanel>
    </ToolTip>
</Ellipse.ToolTip>
```

---

## SOLID Principles in Practice

### Single Responsibility
- `ContainerMonitoringService` - Only monitors containers
- `HealthEvaluator` - Only evaluates health
- `DockerApiClient` - Only communicates with Docker
- `AquariumViewModel` - Only manages aquarium logic

### Open/Closed
- New container actions can be added to `IDockerApiClient` without modifying existing ones
- New health rules can be added to `HealthEvaluator` without modifying existing rules
- New converters can be added without modifying existing ones

### Liskov Substitution
- `DockerApiClient` can be replaced with `RealDockerApiClient` seamlessly
- `HealthEvaluator` can be replaced with `CustomHealthEvaluator`
- All implementations satisfy their interface contracts

### Interface Segregation
- `IDockerApiClient` is focused on API operations
- `IContainerMonitoringService` is focused on monitoring
- `IHealthEvaluator` is focused on health evaluation
- Each interface is minimal and cohesive

### Dependency Inversion
- `AquariumViewModel` depends on `IContainerMonitoringService`, not concrete implementation
- `ContainerMonitoringService` depends on `IDockerApiClient`, not concrete implementation
- All dependencies are injected via constructor

---

## Configuration

### Dependency Injection Setup (App.xaml.cs)

```csharp
private static void ConfigureServices(ServiceCollection services)
{
    // Logging
    services.AddLogging(builder => builder.AddConsole());
    
    // Domain services
    services.AddSingleton<IDockerApiClient, DockerApiClient>();
    services.AddSingleton<IHealthEvaluator, HealthEvaluator>();
    services.AddSingleton<IContainerMonitoringService, ContainerMonitoringService>();
    
    // Presentation
    services.AddSingleton<AquariumViewModel>();
    services.AddSingleton<MainWindow>();
}
```

---

## Performance Considerations

1. **Immutable Records** - `ContainerMetrics` uses `record` for efficient memory
2. **Thread-Safe Collections** - Aquarium uses locks for container list access
3. **Async/Await** - Non-blocking operations throughout
4. **Efficient Binding** - Only changed properties notify UI
5. **Throttled Updates** - 1500ms monitoring interval prevents excessive updates
6. **Smooth Animation** - 30 FPS animation loop optimized

---

## Testing Approach

The system is designed for testability:

```csharp
// Easy to test services independently
var mockDockerClient = new MockDockerApiClient();
var healthEvaluator = new HealthEvaluator();
var service = new ContainerMonitoringService(mockDockerClient, healthEvaluator);

// Services don't depend on WPF or UI
// Can be tested in console or xUnit projects
```

---

## Security Considerations

- **No hardcoded credentials** - Ready for credential injection
- **CancellationToken support** - Allows safe cancellation
- **Input validation** - Container IDs validated before use
- **Error handling** - No sensitive info in error messages
- **Logging** - Structured logging for audit trails

---

This architecture provides a solid foundation for professional C# development with clear separation of concerns, SOLID principles, and extensibility.

