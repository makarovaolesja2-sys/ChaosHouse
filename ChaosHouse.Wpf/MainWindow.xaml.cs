using System;
using System.Windows;
using Microsoft.Win32;
using ChaosHouse.Core.ViewModels;

namespace ChaosHouse.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private async void SaveWorld_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Salvesta kaosemaailm",
                Filter = "Kõik toetatud failid (*.json;*.xml)|*.json;*.xml|JSON-fail (*.json)|*.json|XML-fail (*.xml)|*.xml",
                DefaultExt = ".json",
                AddExtension = true,
                FileName = "chaos-house.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                await _viewModel.SaveWorldAsync(dialog.FileName);

                MessageBox.Show(
                    "Maailm salvestati edukalt!",
                    "Salvestamine",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Maailma salvestamine ebaõnnestus: {ex.Message}",
                    "Salvestamise viga",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private async void LoadWorld_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Laadi kaosemaailm",
                Filter = "Kõik toetatud failid (*.json;*.xml)|*.json;*.xml|JSON-fail (*.json)|*.json|XML-fail (*.xml)|*.xml",
                DefaultExt = ".json",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                await _viewModel.LoadWorldAsync(dialog.FileName);

                MessageBox.Show(
                    "Maailm laaditi edukalt!",
                    "Laadimine",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Maailma laadimine ebaõnnestus: {ex.Message}",
                    "Laadimise viga",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void SaveLocalDb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.SaveWorldAsync("ChaosHouseDb.db");

                MessageBox.Show(
                    "Maailm salvestati LocalDB andmebaasi!",
                    "LocalDB salvestamine",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"LocalDB salvestamine ebaõnnestus: {ex.Message}",
                    "LocalDB viga",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void LoadLocalDb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadWorldAsync("ChaosHouseDb.db");

                MessageBox.Show(
                    "Maailm laaditi LocalDB andmebaasist!",
                    "LocalDB laadimine",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"LocalDB laadimine ebaõnnestus: {ex.Message}",
                    "LocalDB viga",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
