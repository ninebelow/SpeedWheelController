using System.Windows;
using MahApps.Metro.Controls;
using SpeedWheelController.ViewModels;

namespace SpeedWheelController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
    }
}
