using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using SharpDX.XInput;

namespace SpeedWheelController.Models
{
    public class SpeedWheel : BaseModel
    {
        // Each of the thumbstick axis members is a signed value between -32768 and 32767 describing the position of the thumbstick.
        // A value of 0 is centered. Negative values signify down or to the left.
        // Positive values signify up or to the right.

        private int steeringMaximumValue = 32_767;

        private int steeringMinimumValue = -32_768;

        private int steeringValue = 0;

        private int accelerationMaximumValue = 255;

        private int accelerationMinimumValue = 0;

        private int accelerationValue = 0;

        private int brakingMaximumValue = 255;

        private int brakingMinimumValue = 0;

        private int brakingValue = 0;

        private int motorSpeedMaximumValue = 65_535;

        private int motorSpeedMinimumValue = 0;

        private int leftMotorSpeedValue = 0;

        private int rightMotorSpeedValue = 0;

        private double steeringGrowth = 1.0;

        private double? steeringLeft90Value = null;

        private double? steeringRight90Value = null;

        private double? steeringCenteredValue = null;

        private string message = string.Empty;

        private bool calibrated = false;

        private bool aButtonWasReleased = true;

        private IXbox360Controller? virtualController;

        private DispatcherTimer timer = new DispatcherTimer();

        private Controller? physicalController;
        private State? previousState;

        public TimeSpan PollInterval => TimeSpan.FromMilliseconds(10);

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

        public int SteeringMaximumValue
        {
            get
            {
                return this.steeringMaximumValue;
            }
            set
            {
                if (value == this.steeringMaximumValue)
                {
                    return;
                }

                this.steeringMaximumValue = value;
                base.OnPropertyChanged(nameof(this.SteeringMaximumValue));
            }
        }

        public int SteeringMinimumValue
        {
            get
            {
                return this.steeringMinimumValue;
            }
            set
            {
                if (value == this.steeringMinimumValue)
                {
                    return;
                }

                this.steeringMinimumValue = value;
                base.OnPropertyChanged(nameof(this.SteeringMinimumValue));
            }
        }

        public double? SteeringLeft90Value
        {
            get
            {
                return this.steeringLeft90Value;
            }
            set
            {
                if (value == this.steeringLeft90Value)
                {
                    return;
                }

                this.steeringLeft90Value = value;
                base.OnPropertyChanged(nameof(this.SteeringLeft90Value));
            }
        }

        public double? SteeringRight90Value
        {
            get
            {
                return this.steeringRight90Value;
            }
            set
            {
                if (value == this.steeringRight90Value)
                {
                    return;
                }

                this.steeringRight90Value = value;
                base.OnPropertyChanged(nameof(this.SteeringRight90Value));
            }
        }

        public double? SteeringCenteredValue
        {
            get
            {
                return this.steeringCenteredValue;
            }
            set
            {
                if (value == this.steeringCenteredValue)
                {
                    return;
                }

                this.steeringCenteredValue = value;
                base.OnPropertyChanged(nameof(this.SteeringCenteredValue));
            }
        }

        public DoubleCollection SteeringTicks
        {
            get
            {
                return new DoubleCollection
                {
                    this.SteeringMinimumValue,
                    this.SteeringLeft90Value ?? 0,
                    this.SteeringCenteredValue ?? 0,
                    this.SteeringRight90Value ?? 0,
                    this.SteeringMaximumValue
                };
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

        public int AccelerationMaximumValue
        {
            get
            {
                return this.accelerationMaximumValue;
            }
            set
            {
                if (value == this.accelerationMaximumValue)
                {
                    return;
                }

                this.accelerationMaximumValue = value;
                base.OnPropertyChanged(nameof(this.AccelerationMaximumValue));
            }
        }

        public int AccelerationMinimumValue
        {
            get
            {
                return this.accelerationMinimumValue;
            }
            set
            {
                if (value == this.accelerationMinimumValue)
                {
                    return;
                }

                this.accelerationMinimumValue = value;
                base.OnPropertyChanged(nameof(this.AccelerationMinimumValue));
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

        public int BrakingMaximumValue
        {
            get
            {
                return this.brakingMaximumValue;
            }
            set
            {
                if (value == this.brakingMaximumValue)
                {
                    return;
                }

                this.brakingMaximumValue = value;
                base.OnPropertyChanged(nameof(this.BrakingMaximumValue));
            }
        }

        public int BrakingMinimumValue
        {
            get
            {
                return this.brakingMinimumValue;
            }
            set
            {
                if (value == this.brakingMinimumValue)
                {
                    return;
                }

                this.brakingMinimumValue = value;
                base.OnPropertyChanged(nameof(this.BrakingMinimumValue));
            }
        }

        public int MotorSpeedMaximumValue => this.motorSpeedMaximumValue;

        public int MotorSpeedMinimumValue => this.motorSpeedMinimumValue;

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
            
            this.timer.Interval = this.PollInterval;
            this.timer.Tick += new EventHandler(this.ConfigureSpeedWheel);
            this.timer.Start();
        }

        private void ConfigureSpeedWheel(object? sender, EventArgs e)
        {
            if (this.virtualController == null && this.GetControllers().Any(x => x.IsConnected))
            {
                this.Message = "Please disconnect all controllers before starting SpeedWheel emulation.";
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
            this.RightMotorSpeedValue = (ushort)(((decimal)e.SmallMotor / 255m) * this.motorSpeedMaximumValue);
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

                    // Each of the thumbstick axis members is a signed value between -32768 and 32767 describing the position of the thumbstick.
                    // A value of 0 is centered. Negative values signify down or to the left.
                    // Positive values signify up or to the right.
                    // The constants XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE or XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE can be used as a positive and negative value to filter a thumbstick input.
                    // Left thumbstick x-axis value.

                    double steeringIn = (double)(state?.Gamepad.LeftThumbX ?? 0);
                    double steeringOut = steeringIn;

                    if (this.calibrated)
                    {
                        steeringIn = steeringIn - (double?)this.SteeringCenteredValue ?? 0.0;
                        if (steeringIn > 0)
                        {
                            steeringOut = Math.Min(steeringIn / (double)(this.SteeringRight90Value ?? this.SteeringMaximumValue), 1.0) * this.SteeringMaximumValue;
                        }
                        if (steeringIn < 0)
                        {
                            steeringOut = -1 * (Math.Max(-steeringIn / (double)(this.SteeringLeft90Value ?? this.SteeringMinimumValue), -1.0) * this.SteeringMinimumValue);
                        }
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

                    // calibration
                    if (!this.IsButtonPressed(state, GamepadButtonFlags.A))
                    {
                        this.aButtonWasReleased = true;
                    }

                    if (!this.calibrated && this.aButtonWasReleased)
                    {
                        this.Calibrate(this.IsButtonPressed(state, GamepadButtonFlags.A));
                    }
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
                this.Calibrate();
            }
        }

        private void Calibrate(bool confirmButtonPressed = false)
        {
            if (this.calibrated)
            {
                return;
            }

            if (this.SteeringLeft90Value == null)
            {
                this.Message = "Please turn wheel 90 degrees to the left and press A button.";
                if (confirmButtonPressed)
                {
                    this.SteeringLeft90Value = this.SteeringValue;
                }
            }
            else if (this.SteeringRight90Value == null)
            {
                this.Message = "Please turn wheel 90 degrees to the right and press A button.";
                if (confirmButtonPressed)
                {
                    this.SteeringRight90Value = this.SteeringValue;
                }
            }
            else if (this.SteeringCenteredValue == null)
            {
                this.Message = "Please turn wheel to center and press A button.";
                if (confirmButtonPressed)
                {
                    this.SteeringCenteredValue = this.SteeringValue;
                    this.Message = "SpeedWheel connected as Xbox 360 controller and calibrated!";
                    this.calibrated = true;
                }
            }

            if (confirmButtonPressed)
            {
                this.aButtonWasReleased = false;
            }
        }
    }
}
