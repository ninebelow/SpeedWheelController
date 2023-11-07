namespace SpeedWheelController.Models
{
    public class SpeedWheelConstants
    {
        public int SteeringMaximum => 32_767;

        public int SteeringMinimum => -32_768;

        public int AccelerationMaximum => 255;

        public int AccelerationMinimum => 0;

        public int BrakingMaximum => 255;

        public int BrakingMinimum => 0;

        public int MotorSpeedMaximum => 65_535;

        public int MotorSpeedMinimum => 0;

        public int NinetyDegrees = 25_000; // close enough
    }
}
