# SpeedWheelController

If you have an Xbox 360 Wireless Speed Wheel Controller, you can use this software to possibly enable some extra compatability and functionality in racing games. For instance, with this program you can use your Wireless Speed Wheel with Forza Motorsports 2023 and the Windows Xbox App. Using this software it will show up in games as a standard Xbox 360 Controller.

![alt text](https://github.com/ninebelow/SpeedWheelController/blob/master/SpeedWheelController/Images/SpeedWheel.png?raw=true)

## How to install

This software would not be possible without the excellent work of Nefarius who has provided the VIGem framework and Legacinator.

### Install dependencies

1) Install [VIGEm framework](https://github.com/ViGEm/ViGEmBus/releases/latest)

2) Download and run [Legacinator](https://github.com/nefarius/Legacinator/releases/latest) to fix any driver problems from legacy outdated drivers (may not be required but shouldn't hurt)

3) Download and install [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) (if you do not already have it)
   
4) Download and install [DirectX End-User Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=35#:~:text=DirectX%20End-User%20Runtime%20Web%20Installer.%20The%20Microsoft%20DirectX,on%20your%20Windows%20OS%20in...%20Learn%20more%20on...microsoft.com) (if you don't already have it)

### Download and run SpeedWheelController

1) Download the [latest alpha release zip file](https://github.com/ninebelow/SpeedWheelController/releases/latest)
2) Unzip to any directory
3) Double-click on the SpeedWheelController.exe to run

## How to use

If all the requirements are installed, the software should start up without any error messages. Of course, using the software assumes that you have a Xbox 360 Speed Wheel Controller setup on your Windows PC. If you have not already done this, it involves the following steps:

1) Plug in the Xbox 360 Wireless Receiver into your PC. It should show up in Device Manager under "Xbox 360 Peripherals" as "Xbox 360 Wireless Receiver for Windows". If it does not you may need to manually choose the appropriate driver for the "unknown" device. 

    a) Right-click on the new "unknown" device and Update Driver

    b) Browse my computer for drivers

    c) Let me pick from a list of available drivers on my computer

    d) Choose Xbox 360 Wireless Receiver for Windows

2) Pair the Xbox 360 Wireless Speed Wheel Controller with the Xbox 360 Wireless Receiver. It may show up in Windows as a Generic HID Game Controller.

3) Run the SpeedWheelController.exe. When you run SpeedWheelController it will disconnect any connected gaming contollers (including your Speed Wheel if connected). It then creates a virtual Xbox 360 controller and will ask you to now connect your Speed Wheel Controller. It does this so that the virtual controller will be the first controller seen by games.

## Features

1) By default under settings, SpeedWheelController will limit the steering to 180 degrees. However, this can be unchecked if you would like to use the full range of steering motion of your Xbox 360 Wireless Speed Wheel.

2) The Xbox 360 Wireless Speed Wheel Controller does not have left and right bumpers like most Xbox 360 controllers. The SpeedWheelController software can emulate the bumpers by holding down simultaneously either the left or right dpad buttons, and either the X or B button, and either the right or left trigger.

3) When you close the SpeedWheelController software it will automatically disconnect your Xbox 360 Wireless Speed Wheel. You can also manually disconnect with the option under the File menu.

4) You can see the steering, accelleration, and braking input in the software. The current battery level is also displayed. Finally, you can also see and test the vibration functionality of the controller.

## Forza Motorsports Settings

These settings seem to work pretty good:

- In SpeedWheelController choose setting to "limit to 180 degrees"
- In Forza:
   - Driving Assists Steering: Normal
   - No steering, acceleration, or deceleration deadzones
   - Vibration Scale: 100
   - Steering Self-Alignment: 100

Similar settings should work with Forza Horizon.
     
## Assetto Corsa Settings

Assetto Corsa seems to recognize the Xbox 360 Wireless Speed Wheel Controller as a regular Xbox Controller all on its own with no need for the SpeedWheelController app. It should work very similar with the SpeedWheelController software running with the "limit to 180 degrees" setting turned off. The following settings have worked well with Assetto Corsa Content Manager in the past:

- Steer left thumb
- Speed Sensitivity: 75%
- Steering Speed: 100%
- Steering gamma: 210%
- Steering filter: 82%
- Steering deadzone: 0%
- Rumble effects: 100%

   



