using System.ComponentModel;
using System.Windows;
using SpeedWheelController.Models;

namespace SpeedWheelController.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public SpeedWheel SpeedWheel { get; }

        public MainWindowViewModel()
        {
            Application.Current.MainWindow.Closing += this.MainWindow_Closing;
            this.SpeedWheel = new SpeedWheel();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            this.SpeedWheel.Dispose();
        }
    }
}
