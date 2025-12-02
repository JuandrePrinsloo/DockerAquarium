namespace DockerAquarium.Presentation.Models;

using System.ComponentModel;
using System.Windows.Media;

/// <summary>
/// Visual representation of a container in the aquarium.
/// </summary>
public class FishViewModel : INotifyPropertyChanged
{
    private double _positionX;
    private double _positionY;
    private double _size;
    private Color _color;
    private double _speedX;
    private double _speedY;
    private string _containerName = string.Empty;
    private string _containerId = string.Empty;
    private string _healthStatus = string.Empty;
    private string _imageName = string.Empty;
    private string _state = string.Empty;
    private double _cpuPercentage;
    private double _memoryPercentage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ContainerId
    {
        get => _containerId;
        set
        {
            if (_containerId != value)
            {
                _containerId = value;
                OnPropertyChanged(nameof(ContainerId));
            }
        }
    }

    public string ContainerName
    {
        get => _containerName;
        set
        {
            if (_containerName != value)
            {
                _containerName = value;
                OnPropertyChanged(nameof(ContainerName));
            }
        }
    }

    public double PositionX
    {
        get => _positionX;
        set
        {
            if (Math.Abs(_positionX - value) > 0.01)
            {
                _positionX = value;
                OnPropertyChanged(nameof(PositionX));
            }
        }
    }

    public double PositionY
    {
        get => _positionY;
        set
        {
            if (Math.Abs(_positionY - value) > 0.01)
            {
                _positionY = value;
                OnPropertyChanged(nameof(PositionY));
            }
        }
    }

    public double Size
    {
        get => _size;
        set
        {
            if (Math.Abs(_size - value) > 0.01)
            {
                _size = Math.Max(15, Math.Min(60, value)); // Clamp between 15 and 60
                OnPropertyChanged(nameof(Size));
            }
        }
    }

    public Color FishColor
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged(nameof(FishColor));
            }
        }
    }

    public double SpeedX
    {
        get => _speedX;
        set
        {
            if (Math.Abs(_speedX - value) > 0.01)
            {
                _speedX = value;
                OnPropertyChanged(nameof(SpeedX));
            }
        }
    }

    public double SpeedY
    {
        get => _speedY;
        set
        {
            if (Math.Abs(_speedY - value) > 0.01)
            {
                _speedY = value;
                OnPropertyChanged(nameof(SpeedY));
            }
        }
    }

    public string HealthStatus
    {
        get => _healthStatus;
        set
        {
            if (_healthStatus != value)
            {
                _healthStatus = value;
                OnPropertyChanged(nameof(HealthStatus));
            }
        }
    }

    public string ImageName
    {
        get => _imageName;
        set
        {
            if (_imageName != value)
            {
                _imageName = value;
                OnPropertyChanged(nameof(ImageName));
            }
        }
    }

    public string State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }

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

    public double MemoryPercentage
    {
        get => _memoryPercentage;
        set
        {
            if (Math.Abs(_memoryPercentage - value) > 0.01)
            {
                _memoryPercentage = value;
                OnPropertyChanged(nameof(MemoryPercentage));
            }
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

