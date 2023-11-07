using System;
using System.Collections.Generic;
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
        private int steeringValue = 0;

        private int accelerationValue = 0;
        
        private int brakingValue = 0;

        private int leftMotorSpeedValue = 0;

        private int rightMotorSpeedValue = 0;

        private string message = string.Empty;

        private IXbox360Controller? virtualController;

        private DispatcherTimer timer = new DispatcherTimer();

        private Controller? physicalController;

        private State? previousState;

        private int ninetyDegrees = 25_000;

        private TimeSpan pollInterval = TimeSpan.FromMilliseconds(10);

        public int SteeringMaximumValue => 32_767;

        public int SteeringMinimumValue => -32_768;

        public int AccelerationMaximumValue => 255;

        public int AccelerationMinimumValue => 0;

        public int BrakingMaximumValue => 255;

        public int BrakingMinimumValue => 0;

        public int MotorSpeedMaximumValue => 65_535;

        public int MotorSpeedMinimumValue => 0;

        public int SteeringValue
        {
            get
            {
                return this.steeringValue;
            }
            set
            {
                if (value == this.steeringValue)
                {
                    return;
                }

                this.steeringValue = value;
                base.OnPropertyChanged(nameof(this.SteeringValue));
            }
        }

        public int AccelerationValue
        {
            get
            {
                return this.accelerationValue;
            }
            set
            {
                if (value == this.accelerationValue)
                {
                    return;
                }

                this.accelerationValue = value;
                base.OnPropertyChanged(nameof(this.AccelerationValue));
            }
        }

        public int BrakingValue
        {
            get
            {
                return this.brakingValue;
            }
            set
            {
                if (value == this.brakingValue)
                {
                    return;
                }

                this.brakingValue = value;
                base.OnPropertyChanged(nameof(this.BrakingValue));
            }
        }

        public int LeftMotorSpeedValue
        {
            get
            {
                return this.leftMotorSpeedValue;
            }

            set
            {
                this.leftMotorSpeedValue = value;
                this.SetVibration();
                base.OnPropertyChanged(nameof(this.LeftMotorSpeedValue));
            }
        }

        public int RightMotorSpeedValue
        {
            get
            {
                return this.rightMotorSpeedValue;
            }

            set
            {
                this.rightMotorSpeedValue = value;
                this.SetVibration();
                base.OnPropertyChanged(nameof(this.RightMotorSpeedValue));
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                base.OnPropertyChanged(nameof(this.Message));
            }
        }

        public SpeedWheel()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Message = "Starting!";
            
            this.timer.Interval = this.pollInterval;
            this.timer.Tick += new EventHandler(this.ConfigureSpeedWheel);
            this.timer.Start();
        }

        private void ConfigureSpeedWheel(object? sender, EventArgs e)
        {
            if (this.virtualController == null && this.GetControllers().Any(x => x.IsConnected))
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
                    this.virtualController.FeedbackReceived += this.VirtualController_FeedbackReceived;
                    this.virtualController.AutoSubmitReport = false;
                    this.virtualController.Connect();
                }
                catch
                {
                    this.Message = "Could not create virtual controller. Is ViGemBus installed?";
                    return;
                }
            }
            else if(this.virtualController.UserIndex >  -1)
            {
                this.Message = "Virtual Xbox 360 controller created.";
                this.timer.Tick -= this.ConfigureSpeedWheel;
                this.timer.Stop();
                this.timer.Tick += new EventHandler(this.PollSpeedWheel);
                this.timer.Start();
            }
        }

        private void VirtualController_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            if (this.physicalController == null)
            {
                return;
            }

            this.LeftMotorSpeedValue = (ushort)(((decimal)e.LargeMotor / 255m) * this.MotorSpeedMaximumValue);
            this.RightMotorSpeedValue = (ushort)(((decimal)e.SmallMotor / 255m) * this.MotorSpeedMaximumValue);
        }

        private void SetVibration()
        {
            if (this.physicalController == null)
            {
                return;
            }

            Vibration vibration = new Vibration();
            vibration.LeftMotorSpeed = (ushort)this.LeftMotorSpeedValue;
            vibration.RightMotorSpeed = (ushort)this.RightMotorSpeedValue;
            this.physicalController.SetVibration(vibration);
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
                var state = this.physicalController?.GetState();
                if (this.previousState?.PacketNumber != state?.PacketNumber)
                {
                    // fake shoulder button press
                    if (this.IsButtonPressed(state, GamepadButtonFlags.DPadRight) && this.IsButtonPressed(state, GamepadButtonFlags.X))
                    {
                        this.virtualController?.SetButtonState(Xbox360Button.RightShoulder, state?.Gamepad.RightTrigger > 50);
                        this.virtualController?.SetButtonState(Xbox360Button.LeftShoulder, state?.Gamepad.LeftTrigger > 50);
                        this.virtualController?.SubmitReport();
                        return;
                    }
                    else
                    {
                        this.virtualController?.SetButtonState(Xbox360Button.RightShoulder, false);
                        this.virtualController?.SetButtonState(Xbox360Button.LeftShoulder, false);
                    }

                    double steeringIn = (double)(state?.Gamepad.LeftThumbX ?? 0);
                    double steeringOut = steeringIn;
                  
                    if (steeringIn > 0)
                    {
                        steeringOut = Math.Min(steeringIn, this.ninetyDegrees) / this.ninetyDegrees * this.SteeringMaximumValue;
                    }
                    else if (steeringIn < 0)
                    {
                        steeringOut = Math.Min(-steeringIn, this.ninetyDegrees) / this.ninetyDegrees * this.SteeringMinimumValue;
                    }

                    this.SteeringValue = (int)steeringOut;
                    this.AccelerationValue = state?.Gamepad.RightTrigger ?? 0;
                    this.BrakingValue = state?.Gamepad.LeftTrigger ?? 0;

                    if (this.virtualController == null)
                    {
                        return;
                    }

                    this.virtualController.SetAxisValue(Xbox360Axis.LeftThumbX, (short)this.SteeringValue);
                    this.virtualController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)this.AccelerationValue);
                    this.virtualController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)this.BrakingValue);

                    this.virtualController.SetAxisValue(Xbox360Axis.LeftThumbY, state?.Gamepad.LeftThumbY ?? 0);
                    this.virtualController.SetAxisValue(Xbox360Axis.RightThumbX, state?.Gamepad.RightThumbX ?? 0);
                    this.virtualController.SetAxisValue(Xbox360Axis.RightThumbY, state?.Gamepad.RightThumbY ?? 0);

                    this.virtualController.SetButtonState(Xbox360Button.A, this.IsButtonPressed(state, GamepadButtonFlags.A));
                    this.virtualController.SetButtonState(Xbox360Button.B, this.IsButtonPressed(state, GamepadButtonFlags.B));
                    this.virtualController.SetButtonState(Xbox360Button.X, this.IsButtonPressed(state, GamepadButtonFlags.X));
                    this.virtualController.SetButtonState(Xbox360Button.Y, this.IsButtonPressed(state, GamepadButtonFlags.Y));
                    this.virtualController.SetButtonState(Xbox360Button.Left, this.IsButtonPressed(state, GamepadButtonFlags.DPadLeft));
                    this.virtualController.SetButtonState(Xbox360Button.Right, this.IsButtonPressed(state, GamepadButtonFlags.DPadRight));
                    this.virtualController.SetButtonState(Xbox360Button.Up, this.IsButtonPressed(state, GamepadButtonFlags.DPadUp));
                    this.virtualController.SetButtonState(Xbox360Button.Down, this.IsButtonPressed(state, GamepadButtonFlags.DPadDown));
                    this.virtualController.SetButtonState(Xbox360Button.Start, this.IsButtonPressed(state, GamepadButtonFlags.Start));
                    this.virtualController.SetButtonState(Xbox360Button.Back, this.IsButtonPressed(state, GamepadButtonFlags.Back));
                    this.virtualController.SetButtonState(Xbox360Button.LeftShoulder, this.IsButtonPressed(state, GamepadButtonFlags.LeftShoulder));
                    this.virtualController.SetButtonState(Xbox360Button.RightShoulder, this.IsButtonPressed(state, GamepadButtonFlags.RightShoulder));

                    this.virtualController.SubmitReport();

                    this.previousState = state;
                }
            }
            catch
            {
                this.Message = "Controller disconnected!";
                this.physicalController = null;
            }
        }

        private bool IsButtonPressed(State? state, GamepadButtonFlags buttonFlag)
        {
            if (state == null)
            {
                return false;
            }

            return (state.Value.Gamepad.Buttons & buttonFlag) == buttonFlag;
        }

        private IEnumerable<Controller> GetControllers()
        {
            return new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
        }

        private void ConnectController()
        {
            this.physicalController = this.GetControllers().FirstOrDefault(x => (int)x.UserIndex != (int)(this.virtualController?.UserIndex ?? int.MaxValue) && x.IsConnected);
            if (this.physicalController == null)
            {
                this.Message = "Please connect your SpeedWheel now";
            }
            else
            {
                this.Message = "SpeedWheel connected as an Xbox 360 controller... ready to race!";
            }
        }

        public void Dispose()
        {
            ControllerUtilities.TurnOffAllControllers();
            this.virtualController?.Disconnect();
        }
    }
}
