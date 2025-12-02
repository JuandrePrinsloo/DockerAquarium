using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DockerAquarium.Application.Services;
using DockerAquarium.Domain.Interfaces;
using DockerAquarium.Infrastructure.Docker;
using DockerAquarium.Presentation.ViewModels;
using Microsoft.Extensions.Logging;

namespace DockerAquarium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Configures dependency injection and application startup.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Create and show main window with view model
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var viewModel = _serviceProvider.GetRequiredService<AquariumViewModel>();
            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Register logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            // Register domain interfaces and implementations
            services.AddSingleton<IDockerApiClient, DockerApiClient>();
            services.AddSingleton<IHealthEvaluator, HealthEvaluator>();
            services.AddSingleton<IContainerMonitoringService, ContainerMonitoringService>();

            // Register presentation layer
            services.AddSingleton<AquariumViewModel>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }
}

