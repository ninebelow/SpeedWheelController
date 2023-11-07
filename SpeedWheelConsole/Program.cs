using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Serilog;
using Serilog.Core;
using SharpDX.XInput;

namespace SpeedWheelConsole
{
    internal class Program
    {
        private static Controller controller = null;
        private static IXbox360Controller vController = null;
        private static Logger log = null;
        private const int sleepTime = 5;

        static void Main(string[] args)
        {
            log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("speedwheel.log")
                .CreateLogger();


            var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            log.Information("Starting!");

            while (controllers.Any(x => x.IsConnected))
            {
                log.Information("Please disconnect all controllers before starting speedwheel emulation.");
                Thread.Sleep(1000);
            }

            log.Information("Creating virtual xbox 360 controller.");
            try
            {
                ViGEmClient client = new ViGEmClient();
                vController = client.CreateXbox360Controller();
                vController.FeedbackReceived += Controller_FeedbackReceived;
                vController.AutoSubmitReport = false;
                vController.Connect();
                Thread.Sleep(1000);
                log.Information($"Created virtual xbox 360 controller successfully as controller #{vController.UserIndex + 1}.");
            } catch (Exception ex)
            {
                log.Information("Could not create virtual controller. Is ViGemBus installed? Press any key to exit...");
                Console.ReadKey();
                return;
            }
            try
            {
                while (!controllers.Where(x => ((int)x.UserIndex) > vController.UserIndex).Any(x => x.IsConnected))
                {
                    log.Information("Please connect the SpeedWheel now.");
                    Thread.Sleep(1000);
                }

                // Get 1st controller available
                foreach (var selectControler in controllers.Where(x => ((int)x.UserIndex) > vController.UserIndex))
                {
                    if (selectControler.IsConnected)
                    {
                        controller = selectControler;
                        break;
                    }
                }
            } catch(Exception e)
            {
                log.Error(e.Message);
                log.Information("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            if (controller == null)
            {
                log.Information("No XInput controller installed. Preass any key to exit...");
                Console.ReadKey();
                return;
            }

            log.Information("Found a XInput controller available");

            log.Information("Ready to race!");

            // Poll events from joystick
            var previousState = controller.GetState();
            while (controller.IsConnected)
            {
                if (IsKeyPressed(ConsoleKey.Escape))
                {
                    break;
                }

                var state = controller.GetState();
                if (previousState.PacketNumber != state.PacketNumber)
                {
                    log.Debug(state.Gamepad.ToString());

                    // fake shoulder button press
                    if (IsButtonPressed(state, GamepadButtonFlags.Y) && IsButtonPressed(state, GamepadButtonFlags.DPadUp))
                    {
                        vController.SetButtonState(Xbox360Button.RightShoulder, state.Gamepad.RightTrigger > 50);
                        vController.SetButtonState(Xbox360Button.LeftShoulder, state.Gamepad.LeftTrigger > 50);
                        vController.SubmitReport();
                        Thread.Sleep(sleepTime);
                        continue;
                    }
                    else
                    {
                        vController.SetButtonState(Xbox360Button.RightShoulder, false);
                        vController.SetButtonState(Xbox360Button.LeftShoulder, false);
                    }

                    // Each of the thumbstick axis members is a signed value between -32768 and 32767 describing the position of the thumbstick.
                    // A value of 0 is centered. Negative values signify down or to the left.
                    // Positive values signify up or to the right.
                    // The constants XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE or XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE can be used as a positive and negative value to filter a thumbstick input.
                    // Left thumbstick x-axis value.

                    double growth = 1.2;
                    double steeringIn = (double)state.Gamepad.LeftThumbX;
                    double steeringOut = 0;

                    if (steeringIn > 0)
                    {
                        double limit = 32767.0;
                        steeringOut = Math.Pow(steeringIn, growth) / Math.Pow(limit, growth) * limit;
                    }

                    if (steeringIn < 0)
                    {
                        double limit = 32768.0;
                        steeringOut = -1 * Math.Pow(Math.Abs(steeringIn), growth) / Math.Pow(limit, growth) * limit;
                    }

                    log.Information($"{steeringIn} => {steeringOut}");

                    vController.SetAxisValue(Xbox360Axis.LeftThumbX, (short)steeringOut);
                    vController.SetAxisValue(Xbox360Axis.LeftThumbY, state.Gamepad.LeftThumbY);
                    vController.SetAxisValue(Xbox360Axis.RightThumbX, state.Gamepad.RightThumbX);
                    vController.SetAxisValue(Xbox360Axis.RightThumbY, state.Gamepad.RightThumbY);


                    vController.SetButtonState(Xbox360Button.A, IsButtonPressed(state, GamepadButtonFlags.A));
                    vController.SetButtonState(Xbox360Button.B, IsButtonPressed(state, GamepadButtonFlags.B));
                    vController.SetButtonState(Xbox360Button.X, IsButtonPressed(state, GamepadButtonFlags.X));
                    vController.SetButtonState(Xbox360Button.Y, IsButtonPressed(state, GamepadButtonFlags.Y));
                    vController.SetButtonState(Xbox360Button.Left, IsButtonPressed(state, GamepadButtonFlags.DPadLeft));
                    vController.SetButtonState(Xbox360Button.Right, IsButtonPressed(state, GamepadButtonFlags.DPadRight));
                    vController.SetButtonState(Xbox360Button.Up, IsButtonPressed(state, GamepadButtonFlags.DPadUp));
                    vController.SetButtonState(Xbox360Button.Down, IsButtonPressed(state, GamepadButtonFlags.DPadDown));
                    vController.SetButtonState(Xbox360Button.Start, IsButtonPressed(state, GamepadButtonFlags.Start));
                    vController.SetButtonState(Xbox360Button.Back, IsButtonPressed(state, GamepadButtonFlags.Back));
                    vController.SetButtonState(Xbox360Button.LeftShoulder, IsButtonPressed(state, GamepadButtonFlags.LeftShoulder));
                    vController.SetButtonState(Xbox360Button.RightShoulder, IsButtonPressed(state, GamepadButtonFlags.RightShoulder));
                    vController.SetSliderValue(Xbox360Slider.LeftTrigger, state.Gamepad.LeftTrigger);
                    vController.SetSliderValue(Xbox360Slider.RightTrigger, state.Gamepad.RightTrigger);
                    vController.SubmitReport();
                }

                Thread.Sleep(sleepTime);
                previousState = state;
            }

            Debug.WriteLine("Exiting!");
        }

        private static bool IsButtonPressed(State state, GamepadButtonFlags buttonFlag)
        {
            return (state.Gamepad.Buttons & buttonFlag) == buttonFlag;
        }

        public static bool IsKeyPressed(ConsoleKey key)
        {
            return Console.KeyAvailable && Console.ReadKey(true).Key == key;
        }

        private static void Controller_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            if (controller == null)
            {
                return;
            }

            Vibration vibration = new Vibration();
            
            // TODO: Do we really care about left and right on speedwheel? Try to get consistent vibration;
            var speed = (ushort)(((decimal)Math.Max(e.LargeMotor, e.SmallMotor) / 255m) * 65_535m);
            vibration.LeftMotorSpeed = speed;
            vibration.RightMotorSpeed = speed;
            //vibration.LeftMotorSpeed = (ushort)(((decimal)e.LargeMotor / 255m) * 65_535m);
            //vibration.RightMotorSpeed = (ushort)(((decimal)e.SmallMotor / 255m) * 65_535m);
            controller.SetVibration(vibration);
            log.Debug(e.ToString());
            log.Debug($"Large Motor: {e.LargeMotor}");
            log.Debug($"Small Motor: {e.SmallMotor}");
            log.Debug($"Led Number: {e.LedNumber}");
            log.Debug($"Left Motor Speed: {vibration.LeftMotorSpeed}");
            log.Debug($"Right Motor Speed: {vibration.RightMotorSpeed}");
        }
    }
}
