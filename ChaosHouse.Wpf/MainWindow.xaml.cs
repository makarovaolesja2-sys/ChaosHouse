
using System;
using System.Collections.ObjectModel;
using System.Windows;
using ChaosHouse.Core;

namespace ChaosHouse.Wpf
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<HouseMember> HouseMembers { get; set; }

        private int _counter = 1;

        public MainWindow()
        {
            InitializeComponent();

            HouseMembers = new ObservableCollection<HouseMember>
            {
                new TalkingFridge(
                    Guid.NewGuid().ToString("N")[..5], "Sigmund"),

                new Ghost(
                    Guid.NewGuid().ToString("N")[..5], "Kasper"),

                new Inventor(
                    Guid.NewGuid().ToString("N")[..5], "Tesla")
            };

            MembersListBox.ItemsSource = HouseMembers;
        }

        private void AddFridge_Click(object sender, RoutedEventArgs e)
        {
            HouseMembers.Add(new TalkingFridge(
                Guid.NewGuid().ToString("N")[..5],
                $"Külmkapp-{_counter++}"));

            Log("Majja saabus uus tark külmkapp.");
        }

        private void AddGhost_Click(object sender, RoutedEventArgs e)
        {
            HouseMembers.Add(new Ghost(
                Guid.NewGuid().ToString("N")[..5],
                $"Kummitus-{_counter++}"));

            Log("Majja kolis uus kummitus.");
        }

        private void AddInventor_Click(object sender, RoutedEventArgs e)
        {
            HouseMembers.Add(new Inventor(
                Guid.NewGuid().ToString("N")[..5],
                $"Leiutaja-{_counter++}"));

            Log("Majja saabus ekstsentriline leiutaja.");
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (MembersListBox.SelectedItem is HouseMember selected)
            {
                HouseMembers.Remove(selected);
                Log($"Elanik {selected.Name} lahkus Kaosemajast.");
            }
            else
            {
                MessageBox.Show(
                    "Vali kõigepealt elanik, keda soovid eemaldada.",
                    "Tähelepanu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MembersListBox.SelectedItem is not HouseMember selectedMember)
            {
                MessageBox.Show(
                    "Vali kõigepealt vasakult elanik!",
                    "Tähelepanu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                string resultMessage =
                    selectedMember.CrazyAction(InputTextBox.Text);

                Log(resultMessage);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Vigane sisend",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Log(string message)
        {
            LogTextBox.AppendText(
                $"[{DateTime.Now.ToLongTimeString()}] {message}{Environment.NewLine}");

            LogTextBox.ScrollToEnd();
        }
    }
}

