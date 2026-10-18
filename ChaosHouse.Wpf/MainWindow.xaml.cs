using System.Windows;
using ChaosHouse.Core.ViewModels;

namespace ChaosHouse.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}


