using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
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

        public ICommand CloseCommand
        {
            get
            {
                return new RelayCommand((_) => Application.Current.MainWindow.Close());
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                return new RelayCommand((_) => MessageBox.Show("SpeedWheel trademark of Microsoft. Thanks to Nefarius, etc.", "About SpeedWheel Controller", MessageBoxButton.OK, MessageBoxImage.Information));
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            this.SpeedWheel.Dispose();
        }
    }
}
