using SpeedWheelController.Models;

namespace SpeedWheelController.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public SpeedWheel SpeedWheel { get; }

        public MainWindowViewModel()
        {
            this.SpeedWheel = new SpeedWheel();
        }
    }
}
