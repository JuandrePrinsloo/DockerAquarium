namespace DockerAquarium.Presentation.ViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DockerAquarium.Domain.Interfaces;
using DockerAquarium.Domain.Models;
using DockerAquarium.Presentation.Models;

/// <summary>
/// View model for the aquarium display.
/// Handles UI logic and container-to-fish mapping.
/// </summary>
public class AquariumViewModel : INotifyPropertyChanged
{
    private readonly IContainerMonitoringService _monitoringService;
    private readonly Random _random = new();
    private bool _isMonitoring;
    private ObservableCollection<FishViewModel> _fish = [];
    private Dictionary<string, FishViewModel> _containerToFishMap = [];
    private double _aquariumWidth = 1000;
    private double _aquariumHeight = 600;
    private CancellationTokenSource? _animationTokenSource;
    private Task? _animationTask;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AquariumViewModel(IContainerMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
        _monitoringService.ContainerListChanged += OnContainerListChanged;
        _monitoringService.ContainerMetricsUpdated += OnContainerMetricsUpdated;

        StartCommand = new RelayCommand(() => StartMonitoring());
        StopCommand = new RelayCommand(() => StopMonitoring());
        RefreshCommand = new RelayCommand(() => RefreshContainers());
    }

    public ObservableCollection<FishViewModel> Fish => _fish;

    public bool IsMonitoring
    {
        get => _isMonitoring;
        set
        {
            if (_isMonitoring != value)
            {
                _isMonitoring = value;
                OnPropertyChanged(nameof(IsMonitoring));
            }
        }
    }

    public double AquariumWidth
    {
        get => _aquariumWidth;
        set
        {
            if (Math.Abs(_aquariumWidth - value) > 0.01)
            {
                _aquariumWidth = value;
                OnPropertyChanged(nameof(AquariumWidth));
            }
        }
    }

    public double AquariumHeight
    {
        get => _aquariumHeight;
        set
        {
            if (Math.Abs(_aquariumHeight - value) > 0.01)
            {
                _aquariumHeight = value;
                OnPropertyChanged(nameof(AquariumHeight));
            }
        }
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RefreshCommand { get; }

    private void OnContainerListChanged(object? sender, ContainerListChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            SyncFishWithContainers(e.Containers);
        });
    }

    private void OnContainerMetricsUpdated(object? sender, ContainerMetricsUpdatedEventArgs e)
    {
        if (_containerToFishMap.TryGetValue(e.ContainerId, out var fish))
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateFishFromMetrics(fish, e.Metrics);
            });
        }
    }

    private void SyncFishWithContainers(IReadOnlyList<ContainerInfo> containers)
    {
        // Remove fish for containers that no longer exist
        var currentContainerIds = containers.Select(c => c.ContainerId).ToHashSet();
        var fishToRemove = _containerToFishMap
            .Where(kvp => !currentContainerIds.Contains(kvp.Key))
            .ToList();

        foreach (var (containerId, fish) in fishToRemove)
        {
            _fish.Remove(fish);
            _containerToFishMap.Remove(containerId);
        }

        // Add fish for new containers
        foreach (var container in containers)
        {
            if (!_containerToFishMap.ContainsKey(container.ContainerId))
            {
                var newFish = CreateFishFromContainer(container);
                _fish.Add(newFish);
                _containerToFishMap[container.ContainerId] = newFish;
            }
        }
    }

    private FishViewModel CreateFishFromContainer(ContainerInfo container)
    {
        var fish = new FishViewModel
        {
            ContainerId = container.ContainerId,
            ContainerName = container.ContainerName,
            ImageName = container.ImageName,
            State = container.State,
            PositionX = _random.NextDouble() * _aquariumWidth,
            PositionY = _random.NextDouble() * _aquariumHeight,
            Size = 25,
            SpeedX = (_random.NextDouble() - 0.5) * 3,
            SpeedY = (_random.NextDouble() - 0.5) * 2,
            FishColor = GetColorForHealth(container.HealthStatus),
            HealthStatus = container.HealthStatus.ToString()
        };

        return fish;
    }

    private void UpdateFishFromMetrics(FishViewModel fish, ContainerMetrics metrics)
    {
        // Size represents memory usage
        fish.Size = 15 + (metrics.MemoryPercentage / 100 * 45);

        // Speed represents activity level
        var activityLevel = Math.Min((metrics.CpuPercentage + metrics.MemoryPercentage) / 200 * 3, 3);
        fish.SpeedX = (_random.NextDouble() - 0.5) * activityLevel;
        fish.SpeedY = (_random.NextDouble() - 0.5) * (activityLevel * 0.8);

        // Update CPU and Memory percentages for tooltip
        fish.CpuPercentage = metrics.CpuPercentage;
        fish.MemoryPercentage = metrics.MemoryPercentage;
    }

    private void StartMonitoring()
    {
        IsMonitoring = true;
        _ = _monitoringService.StartMonitoringAsync(1500);

        StartAnimation();
    }

    private void StopMonitoring()
    {
        IsMonitoring = false;
        _ = _monitoringService.StopMonitoringAsync();

        StopAnimation();
    }

    private void RefreshContainers()
    {
        // Force a refresh of the container list
        var containers = _monitoringService.GetCurrentContainers();
        SyncFishWithContainers(containers);
    }

    private void StartAnimation()
    {
        if (_animationTask != null && !_animationTask.IsCompleted)
            return;

        _animationTokenSource = new CancellationTokenSource();
        _animationTask = AnimationLoop(_animationTokenSource.Token);
    }

    private void StopAnimation()
    {
        _animationTokenSource?.Cancel();
    }

    private async Task AnimationLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var fish in _fish)
                {
                    UpdateFishPosition(fish);
                }
            });

            await Task.Delay(30, cancellationToken).ConfigureAwait(false);
        }
    }

    private void UpdateFishPosition(FishViewModel fish)
    {
        var newX = fish.PositionX + fish.SpeedX;
        var newY = fish.PositionY + fish.SpeedY;

        // Bounce off walls
        if (newX < 0 || newX > _aquariumWidth - fish.Size)
        {
            fish.SpeedX = -fish.SpeedX;
        }
        else
        {
            fish.PositionX = newX;
        }

        if (newY < 0 || newY > _aquariumHeight - fish.Size)
        {
            fish.SpeedY = -fish.SpeedY;
        }
        else
        {
            fish.PositionY = newY;
        }
    }

    private Color GetColorForHealth(ContainerHealthStatus health) => health switch
    {
        ContainerHealthStatus.Healthy => Color.FromRgb(0, 200, 100),
        ContainerHealthStatus.Warning => Color.FromRgb(255, 200, 0),
        ContainerHealthStatus.Critical => Color.FromRgb(255, 50, 50),
        ContainerHealthStatus.Stopped => Color.FromRgb(150, 150, 150),
        _ => Color.FromRgb(100, 100, 200)
    };

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Simple relay command implementation for MVVM.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _action;

    public RelayCommand(Action action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _action();
}

