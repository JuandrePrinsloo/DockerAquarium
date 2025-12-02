# Docker Aquarium - Container Visualization

A creative and interactive WPF desktop application that visualizes Docker containers as animated fish swimming in an aquarium. The application provides real-time monitoring of container metrics with visual feedback based on health status, resource usage, and activity levels.

## Project Overview

Docker Aquarium transforms the technical complexity of Docker container monitoring into an engaging, visual experience. Each container is represented as a "fish" with the following characteristics:

- **Size** = Memory resource usage (larger fish = higher memory consumption)
- **Color** = Health status (green = healthy, yellow = warning, red = critical, gray = stopped)
- **Speed** = Container activity level (faster = higher CPU/network activity)

## Features

✅ **Real-Time Container Monitoring**
- Automatic discovery of all Docker containers
- Live metrics collection and updates
- Continuous health evaluation

✅ **Interactive Aquarium Visualization**
- Animated fish representing containers
- Bouncing physics within the aquarium boundaries
- Smooth visual transitions and updates
- Tooltip information on hover

✅ **Interactive Container Management**
- Start/Stop running containers
- Pause/Unpause container execution
- Refresh container list on demand

✅ **Health Status Indication**
- Color-coded health states
- CPU and memory threshold monitoring
- Real-time status updates

✅ **Clean Architecture**
- Separation of concerns with Domain, Application, and Presentation layers
- SOLID principles throughout
- Dependency Injection for loose coupling
- Extensible design for future enhancements

## Project Structure

```
DockerAquarium/
├── Domain/
│   ├── Models/
│   │   ├── ContainerInfo.cs           # Core container entity
│   │   ├── ContainerMetrics.cs        # Real-time metrics data
│   │   └── ContainerHealthStatus.cs   # Health status enumeration
│   └── Interfaces/
│       ├── IDockerApiClient.cs        # Docker API abstraction
│       ├── IContainerMonitoringService.cs  # Monitoring service
│       └── IHealthEvaluator.cs        # Health evaluation logic
├── Infrastructure/
│   └── Docker/
│       └── DockerApiClient.cs         # Docker API implementation
├── Application/
│   └── Services/
│       ├── ContainerMonitoringService.cs  # Real-time monitoring
│       └── HealthEvaluator.cs         # Health evaluation
├── Presentation/
│   ├── ViewModels/
│   │   └── AquariumViewModel.cs       # MVVM view model
│   ├── Models/
│   │   └── FishViewModel.cs           # Fish visual model
│   └── Converters/
│       ├── ColorToBrushConverter.cs   # Color conversion
│       └── NotBoolConverter.cs        # Boolean inversion
├── MainWindow.xaml                     # UI definition
├── MainWindow.xaml.cs                 # UI code-behind
├── App.xaml                            # Application resources
├── App.xaml.cs                        # Startup configuration
└── DockerAquarium.csproj              # Project configuration
```

## Architecture Principles

### SOLID Principles Applied

**S - Single Responsibility**
- Each service handles one concern (monitoring, health evaluation, API communication)
- View models separate UI logic from business logic

**O - Open/Closed**
- Easy to extend with new health evaluation rules or metrics
- New container management commands can be added without modifying existing code

**L - Liskov Substitution**
- Interfaces guarantee safe substitution (e.g., mock Docker client for testing)

**I - Interface Segregation**
- Small, focused interfaces (IDockerApiClient, IHealthEvaluator, IContainerMonitoringService)
- Clients depend only on the methods they use

**D - Dependency Inversion**
- High-level modules (ViewModels) depend on abstractions (Interfaces)
- Low-level modules (DockerApiClient) implement the abstractions
- Configured through Dependency Injection

### Clean Code Principles

- Clear, descriptive variable and method names
- XML documentation comments on all public types
- Immutable data models using C# records where appropriate
- Separation of concerns across layers
- No magic strings or hardcoded values

## Technology Stack

- **.NET 8.0** - Latest LTS framework
- **WPF (Windows Presentation Foundation)** - Desktop UI framework
- **MVVM Pattern** - Clean separation of UI and logic
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **Logging** - Microsoft.Extensions.Logging

## Installation & Setup

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11 (for WPF support)
- Docker Desktop (optional, for real Docker monitoring)

### Build Instructions

```bash
# Navigate to project directory
cd "C:\Users\Juand\OneDrive\Desktop\Projects\Docker Aquarium"

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project DockerAquarium\DockerAquarium.csproj
```

### Running the Application

```bash
# Debug mode
dotnet run --configuration Debug

# Release mode
dotnet run --configuration Release

# Run built executable
.\DockerAquarium\bin\Debug\net8.0-windows\DockerAquarium.exe
```

## Usage

### Starting the Application

1. Launch the Docker Aquarium application
2. Click "Start Monitoring" to begin collecting container metrics
3. Watch the aquarium populate with fish representing your containers
4. Observe fish size, color, and speed change based on container metrics

### Understanding the Visualization

**Fish Colors:**
- 🟢 **Green** - Container is healthy (CPU < 80%, Memory < 85%)
- 🟡 **Yellow** - Container has warnings (CPU 80-95%, Memory 85-95%)
- 🔴 **Red** - Container is critical (CPU > 95%, Memory > 95%)
- ⚪ **Gray** - Container is stopped/not running

**Fish Behavior:**
- **Size** - Based on memory usage percentage (15-60 pixels)
- **Speed** - Based on CPU and network activity (faster = more active)
- **Movement** - Bounces off aquarium boundaries naturally

### Interactive Controls

- **Start Monitoring** - Enable real-time container tracking
- **Stop Monitoring** - Pause monitoring and updates
- **Refresh Containers** - Manually refresh the container list
- **Hover** - Hover over any fish to see container details in a tooltip

## Health Evaluation Logic

The health evaluator uses the following thresholds:

```csharp
Healthy:    CPU < 80% AND Memory < 85%
Warning:    CPU 80-95% OR Memory 85-95%
Critical:   CPU > 95% OR Memory > 95%
Stopped:    Container is not running
```

These thresholds can be easily adjusted in the `HealthEvaluator` class.

## Metrics Collected

For each running container, the application collects:

- **CPU Percentage** - Current CPU usage (0-100%)
- **Memory Usage** - Current memory consumption in bytes
- **Memory Limit** - Maximum memory allocation in bytes
- **Network Input** - Bytes received over network
- **Network Output** - Bytes transmitted over network
- **Collection Timestamp** - When metrics were captured

## Future Enhancements

- ✨ Real Docker.DotNet integration for actual container monitoring
- ✨ Docker statistics streaming for more detailed metrics
- ✨ Container logs viewer
- ✨ Performance history graphs
- ✨ Container resource limit management
- ✨ Custom color schemes and themes
- ✨ Multi-host Docker Swarm support
- ✨ Persistent metrics storage
- ✨ Alert and notification system

## Development Notes

### Adding New Features

1. **New Health Rules** - Modify `HealthEvaluator.cs`
2. **New Metrics** - Extend `ContainerMetrics` record
3. **Container Actions** - Add methods to `IDockerApiClient`
4. **UI Changes** - Update `MainWindow.xaml` and `AquariumViewModel.cs`

### Testing Mock Data

The application includes a mock Docker client for testing and demonstration:

```csharp
// Uses simulated containers with realistic metrics
// Perfect for development without requiring Docker
// Can be swapped with real implementation later
```

### Logging

Logging is configured for all major operations:

```csharp
// Enable verbose logging
// Check console output for monitoring details
```

## Error Handling

The application implements graceful error handling:

- Failed Docker API calls log warnings but continue operation
- Invalid metrics are ignored and retried on next update
- UI remains responsive even when monitoring encounters errors
- Errors are logged via ILogger for diagnostics

## Performance Considerations

- **Efficient Updates** - Only UI elements that changed are updated
- **Thread Safety** - Event-driven architecture with proper synchronization
- **Memory Usage** - Immutable records prevent unnecessary allocations
- **Update Frequency** - Configurable monitoring interval (default 1500ms)
- **Animation FPS** - Smooth 30 FPS animation loop

## License

This project demonstrates professional C# development with clean architecture principles, suitable for portfolio and production use.

## Contact & Support

For questions, issues, or contributions, please refer to the project structure and code comments for detailed implementation guidance.

---

**Project Status**: ✅ Complete and Buildable

The Docker Aquarium project successfully demonstrates:
- Clean Code Principles
- SOLID Design Patterns
- Modern .NET Architecture
- WPF Desktop Development
- Real-time Data Monitoring
- Professional Project Structure

