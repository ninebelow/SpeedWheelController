using System.Runtime.InteropServices;

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
    }
}
