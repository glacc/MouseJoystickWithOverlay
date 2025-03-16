using SharpGen.Runtime;
using Vortice.DirectInput;

namespace MouseJoystickWithOverlay
{
    internal class MouseInputHandling
    {
        static IDirectInput8 directInput = DInput.DirectInput8Create();

        static Guid pointerDeviceGuid = Guid.Empty;

        static IDirectInputDevice8? mouse = null;

        public static bool lmbPressed = false;
        public static bool rmbPressed = false;

        static MouseInputHandling()
        {
            List<DeviceInstance> devices = new List<DeviceInstance>(directInput.GetDevices(DeviceClass.Pointer, DeviceEnumerationFlags.AttachedOnly));

            if (devices.Count > 0)
                pointerDeviceGuid = devices[0].InstanceGuid;

            IDirectInputDevice8 inputDevice = directInput.CreateDevice(pointerDeviceGuid);

            inputDevice.Properties.BufferSize = 128;

            if (inputDevice.SetDataFormat<RawMouseState>().Success)
                mouse = inputDevice;
        }

        public static void Init()
        {

        }

        public static (int deltaX, int deltaY) Poll()
        {
            int deltaX = 0;
            int deltaY = 0;

            if (mouse == null)
                goto ReturnDirectly;

            Result result;
            
            result = mouse.Poll();

            if (result.Failure)
            {
                result = mouse.Acquire();

                if (result.Failure)
                    goto ReturnDirectly;
            }

            try
            {
                MouseState state = mouse.GetCurrentState<MouseState, RawMouseState, MouseUpdate>();

                deltaX = state.X;
                deltaY = state.Y;

                lmbPressed = state.Buttons[0];
                rmbPressed = state.Buttons[1];

                /*
                if (deltaX != 0 || deltaY != 0)
                    Console.WriteLine($"Mouse X:{deltaX}, Y:{deltaY}");
                */
            }
            catch
            { }

        ReturnDirectly:
            return (deltaX, deltaY);
        }
    }
}
