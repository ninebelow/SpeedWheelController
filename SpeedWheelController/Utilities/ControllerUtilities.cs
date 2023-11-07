using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.XInput;

namespace SpeedWheelController.Utilities
{
    public static partial class ControllerUtilities
    {
        [LibraryImport("XInput1_3.dll", EntryPoint = "#103")]
        private static partial int FnOff(int i);

        public static void TurnOffAllControllers()
        {
            for (int i = 0; i < 4; i++)
            {
                _ = FnOff(i);
            }
        }

        public static IEnumerable<Controller> GetControllers()
        {
            return new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
        }
    }
}
