using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ChaosHouse.Core.ViewModels;
using ChaosHouse.Core.Persistence;

namespace ChaosHouse.Wpf
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            services.AddSingleton<IWorldStorage, MultiFormatWorldStorage>();
            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = new MainWindow(
                _serviceProvider.GetRequiredService<MainViewModel>());

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
