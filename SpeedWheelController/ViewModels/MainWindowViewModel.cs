using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
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
            this.SpeedWheel.LimitTo180Degrees = true;
        }

        public ICommand CloseCommand => new RelayCommand(Application.Current.MainWindow.Close);

        public ICommand ConnectCommand
        {
            get
            {
                return new RelayCommand(this.SpeedWheel.Initialize, () => !this.SpeedWheel.IsVirtualControllerConnected);
            }
        }

        public ICommand DisconnectCommand
        {
            get
            {
                return new RelayCommand(this.SpeedWheel.Disconnect, () => this.SpeedWheel.IsPhysicalControllerConnected);
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                return new RelayCommand(() => MessageBox.Show("SpeedWheel trademark of Microsoft. Thanks to Nefarius, etc.", "About SpeedWheel Controller", MessageBoxButton.OK, MessageBoxImage.Information));
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            this.SpeedWheel.Dispose();
        }
    }
}
