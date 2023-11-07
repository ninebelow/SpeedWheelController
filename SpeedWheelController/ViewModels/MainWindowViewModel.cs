using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using SpeedWheelController.Models;

namespace SpeedWheelController.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public SpeedWheel SpeedWheel { get; set; }

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

        public ICommand ConnectCommand
        {
            get
            {
                return new RelayCommand((_) => this.SpeedWheel.Initialize(), (_) => !this.SpeedWheel.IsVirtualControllerConnected);
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                return new RelayCommand((_) => this.SpeedWheel.Disconnect(), (_) => this.SpeedWheel.IsPhysicalControllerConnected);
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
