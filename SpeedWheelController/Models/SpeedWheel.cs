using System;
using System.Linq;
using System.Windows.Threading;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using SharpDX.XInput;
using SpeedWheelController.Utilities;

namespace SpeedWheelController.Models
{
    public class SpeedWheel : BaseModel, IDisposable
    {
        private int steering = 0;
        private int acceleration = 0;
        private int braking = 0;
        private int leftMotorSpeed = 0;
        private int rightMotorSpeed = 0;
        private string message = string.Empty;
        private IXbox360Controller? virtualController;
        private Controller? physicalController;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly TimeSpan pollInterval = TimeSpan.FromMilliseconds(10);

        public SpeedWheelConstants Constants => new SpeedWheelConstants();

        public State? ControllerState { get; private set; }

        public State? PreviousState { get; private set; }

        public bool LimitTo180Degrees { get; set; }

        public int Steering
        {
            get => this.steering;
            set => this.SetProperty(ref this.steering, value);
        }

        public int Acceleration
        {
            get => this.acceleration;
            set => this.SetProperty(ref this.acceleration, value);
        }

        public int Braking
        {
            get => this.braking;
            set => this.SetProperty(ref this.braking, value);
        }

        public int LeftMotorSpeed
        {
            get => this.leftMotorSpeed;
            set
            {
                this.SetProperty(ref this.leftMotorSpeed, value);
                this.SetVibration();
            }
        }

        public int RightMotorSpeed
        {
            get => this.rightMotorSpeed;
            set
            {
                this.SetProperty(ref this.rightMotorSpeed, value);
                this.SetVibration();
            }
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public bool IsVirtualControllerConnected
        {
            get
            {
                return this.virtualController != null;
            }
        }

        public bool IsPhysicalControllerConnected
        {
            get
            {
                return this.physicalController != null;
            }
        }

        public SpeedWheel()
        {
            this.Initialize();
        }

        public void Dispose()
        {
            ControllerUtilities.TurnOffAllControllers();
            this.virtualController?.Disconnect();
        }

        public void Initialize()
        {
            this.Message = "Starting!";
            this.timer.Interval = this.pollInterval;
            this.timer.Tick += new EventHandler(this.ConfigureSpeedWheel);
            this.timer.Start();
        }

        private void ConfigureSpeedWheel(object? sender, EventArgs e)
        {
            if (this.virtualController == null && ControllerUtilities.GetControllers().Any(x => x.IsConnected))
            {
                this.Message = "Disconnecting all controllers before starting SpeedWheel emulation.";
                ControllerUtilities.TurnOffAllControllers();
            }
            else if (this.virtualController == null)
            {
                this.Message = "Creating virtual xbox 360 controller.";
                try
                {
                    ViGEmClient client = new ViGEmClient();
                    this.virtualController = client.CreateXbox360Controller();
                    this.virtualController.FeedbackReceived += this.FeedbackReceived;
                    this.virtualController.AutoSubmitReport = false;
                    this.virtualController.Connect();
                }
                catch
                {
                    this.Message = "Could not create virtual controller. Is ViGemBus installed?";
                    return;
                }
            }
            else if (this.virtualController.UserIndex > -1)
            {
                this.Message = "Virtual Xbox 360 controller created.";
                this.timer.Tick -= this.ConfigureSpeedWheel;
                this.timer.Stop();
                this.timer.Tick += new EventHandler(this.PollSpeedWheel);
                this.timer.Start();
            }
        }

        private void PollSpeedWheel(object? sender, EventArgs e)
        {
            if (this.physicalController == null || !this.physicalController.IsConnected)
            {
                this.ConnectController();
                return;
            }

            if (this.virtualController == null)
            {
                this.ConnectController();
                return;
            }

            try
            {
                this.ControllerState = this.physicalController?.GetState();
                if (this.PreviousState?.PacketNumber != this.ControllerState?.PacketNumber)
                {
                    this.SyncControllerState();
                    this.PreviousState = this.ControllerState;
                }
            }
            catch
            {
                this.Message = "Controller disconnected!";
                this.physicalController = null;
            }
        }

        public void ConnectController()
        {
            this.physicalController = ControllerUtilities.GetControllers().FirstOrDefault(x => (int)x.UserIndex != (int)(this.virtualController?.UserIndex ?? int.MaxValue) && x.IsConnected);
            base.OnPropertyChanged(nameof(this.IsPhysicalControllerConnected));
            if (this.physicalController == null)
            {
                this.Message = "Please connect your SpeedWheel now.";
            }
            else
            {
                var batteryInfo = this.physicalController.GetBatteryInformation(BatteryDeviceType.Gamepad);
                this.Message = $"SpeedWheel connected as an Xbox 360 controller... Ready to race! (Battery level: {batteryInfo.BatteryLevel})";
            }
        }

        public void Disconnect()
        {
            this.timer.Tick -= new EventHandler(this.ConfigureSpeedWheel);
            this.timer.Stop();

            ControllerUtilities.TurnOffAllControllers();
            this.physicalController = null;
            this.virtualController?.Disconnect();
            this.virtualController = null;
            base.OnPropertyChanged(nameof(this.IsPhysicalControllerConnected));
            base.OnPropertyChanged(nameof(this.IsVirtualControllerConnected));

            this.Message = "SpeedWheel has been disconnected.";
        }

        private void SyncControllerState()
        {
            this.HandleFakeShoulderButtonPress();
            this.Steering = this.GetSteering();
            this.Acceleration = this.ControllerState?.Gamepad.RightTrigger ?? 0;
            this.Braking = this.ControllerState?.Gamepad.LeftTrigger ?? 0;

            if (this.virtualController == null)
            {
                return;
            }

            this.virtualController.SetAxisValue(Xbox360Axis.LeftThumbX, (short)this.Steering);
            this.virtualController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)this.Acceleration);
            this.virtualController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)this.Braking);

            this.virtualController.SetAxisValue(Xbox360Axis.LeftThumbY, this.ControllerState?.Gamepad.LeftThumbY ?? 0);
            this.virtualController.SetAxisValue(Xbox360Axis.RightThumbX, this.ControllerState?.Gamepad.RightThumbX ?? 0);
            this.virtualController.SetAxisValue(Xbox360Axis.RightThumbY, this.ControllerState?.Gamepad.RightThumbY ?? 0);

            this.virtualController.SetButtonState(Xbox360Button.A, this.IsButtonPressed(GamepadButtonFlags.A));
            this.virtualController.SetButtonState(Xbox360Button.B, this.IsButtonPressed(GamepadButtonFlags.B));
            this.virtualController.SetButtonState(Xbox360Button.X, this.IsButtonPressed(GamepadButtonFlags.X));
            this.virtualController.SetButtonState(Xbox360Button.Y, this.IsButtonPressed(GamepadButtonFlags.Y));
            this.virtualController.SetButtonState(Xbox360Button.Left, this.IsButtonPressed(GamepadButtonFlags.DPadLeft));
            this.virtualController.SetButtonState(Xbox360Button.Right, this.IsButtonPressed(GamepadButtonFlags.DPadRight));
            this.virtualController.SetButtonState(Xbox360Button.Up, this.IsButtonPressed(GamepadButtonFlags.DPadUp));
            this.virtualController.SetButtonState(Xbox360Button.Down, this.IsButtonPressed(GamepadButtonFlags.DPadDown));
            this.virtualController.SetButtonState(Xbox360Button.Start, this.IsButtonPressed(GamepadButtonFlags.Start));
            this.virtualController.SetButtonState(Xbox360Button.Back, this.IsButtonPressed(GamepadButtonFlags.Back));
            this.virtualController.SetButtonState(Xbox360Button.LeftShoulder, this.IsButtonPressed(GamepadButtonFlags.LeftShoulder));
            this.virtualController.SetButtonState(Xbox360Button.RightShoulder, this.IsButtonPressed(GamepadButtonFlags.RightShoulder));

            this.virtualController.SubmitReport();
        }

        private void HandleFakeShoulderButtonPress()
        {
            if (this.IsButtonPressed(GamepadButtonFlags.DPadRight) && this.IsButtonPressed(GamepadButtonFlags.X))
            {
                this.virtualController?.SetButtonState(Xbox360Button.RightShoulder, this.ControllerState?.Gamepad.RightTrigger > 50);
                this.virtualController?.SetButtonState(Xbox360Button.LeftShoulder, this.ControllerState?.Gamepad.LeftTrigger > 50);
                this.virtualController?.SubmitReport();
                return;
            }
            else
            {
                this.virtualController?.SetButtonState(Xbox360Button.RightShoulder, false);
                this.virtualController?.SetButtonState(Xbox360Button.LeftShoulder, false);
            }
        }

        private int GetSteering()
        {
            double steeringIn = (double)(this.ControllerState?.Gamepad.LeftThumbX ?? 0);
            double steeringOut = steeringIn;

            if (this.LimitTo180Degrees)
            {
                if (steeringIn > 0)
                {
                    steeringOut = Math.Min(steeringIn, this.Constants.NinetyDegrees) / this.Constants.NinetyDegrees * this.Constants.SteeringMaximum;
                }
                else if (steeringIn < 0)
                {
                    steeringOut = Math.Min(-steeringIn, this.Constants.NinetyDegrees) / this.Constants.NinetyDegrees * this.Constants.SteeringMinimum;
                }
            }

            return (int)steeringOut;
        }

        private bool IsButtonPressed(GamepadButtonFlags buttonFlag)
        {
            if (this.ControllerState == null)
            {
                return false;
            }

            return (this.ControllerState.Value.Gamepad.Buttons & buttonFlag) == buttonFlag;
        }

        private void FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            if (this.physicalController == null)
            {
                return;
            }

            this.LeftMotorSpeed = (ushort)(((decimal)e.LargeMotor / 255m) * this.Constants.MotorSpeedMaximum);
            this.RightMotorSpeed = (ushort)(((decimal)e.SmallMotor / 255m) * this.Constants.MotorSpeedMaximum);
        }

        private void SetVibration()
        {
            if (this.physicalController == null)
            {
                return;
            }

            Vibration vibration = new Vibration()
            {
                LeftMotorSpeed = (ushort)this.LeftMotorSpeed,
                RightMotorSpeed = (ushort)this.RightMotorSpeed
            };
            this.physicalController.SetVibration(vibration);
        }
    }
}
