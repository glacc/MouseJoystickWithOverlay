using SharpGen.Runtime;
using Vortice.DirectInput;

namespace MouseJoystickWithOverlay
{
    internal class InputHandling
    {
        static IDirectInput8 directInput = DInput.DirectInput8Create();

        static List<IDirectInputDevice8> keyboards = new List<IDirectInputDevice8>();
        static List<IDirectInputDevice8> mice = new List<IDirectInputDevice8>();

        public static bool lmbPressed = false;
        public static bool rmbPressed = false;

        public static HashSet<Key> pressedKeys = new HashSet<Key>();

        static InputHandling()
        {
            List<DeviceInstance> keyboardInstances = new List<DeviceInstance>(directInput.GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly));
            List<DeviceInstance> mouseInstances = new List<DeviceInstance>(directInput.GetDevices(DeviceClass.Pointer, DeviceEnumerationFlags.AttachedOnly));

            foreach (DeviceInstance keyboard in keyboardInstances)
            {
                IDirectInputDevice8 inputDevice = directInput.CreateDevice(keyboard.InstanceGuid);

                inputDevice.Properties.BufferSize = 128;

                if (inputDevice.SetDataFormat<RawKeyboardState>().Success)
                    keyboards.Add(inputDevice);
            }

            foreach (DeviceInstance mouse in mouseInstances)
            {
                IDirectInputDevice8 inputDevice = directInput.CreateDevice(mouse.InstanceGuid);

                inputDevice.Properties.BufferSize = 128;

                if (inputDevice.SetDataFormat<RawMouseState>().Success)
                    mice.Add(inputDevice);
            }
        }

        static bool PollDevice(IDirectInputDevice8 device)
        {
            Result result;

            result = device.Poll();

            if (result.Failure)
            {
                result = device.Acquire();

                if (result.Failure)
                    return false;
            }

            return true;
        }

        public static (int deltaX, int deltaY) PollMouseInput()
        {
            int deltaX = 0;
            int deltaY = 0;

            lmbPressed = false;
            rmbPressed = false;

            foreach (IDirectInputDevice8 mouse in mice)
            {
                if (!PollDevice(mouse))
                    continue;

                try
                {
                    MouseState state = mouse.GetCurrentState<MouseState, RawMouseState, MouseUpdate>();

                    deltaX += state.X;
                    deltaY += state.Y;

                    lmbPressed = lmbPressed || state.Buttons[0];
                    rmbPressed = rmbPressed || state.Buttons[1];
                }
                catch
                { }
            }

            return (deltaX, deltaY);
        }

        public static HashSet<Key> PollKeyboardInput()
        {
            HashSet<Key> pressedKeysTemp = new HashSet<Key>();

            foreach (IDirectInputDevice8 keyboard in keyboards)
            {
                if (!PollDevice(keyboard))
                    continue;

                try
                {
                    KeyboardState state = keyboard.GetCurrentState<KeyboardState, RawKeyboardState, KeyboardUpdate>();

                    foreach (Key key in state.PressedKeys)
                        pressedKeysTemp.Add(key);
                }
                catch
                { }
            }

            pressedKeys = pressedKeysTemp;

            return pressedKeysTemp;
        }
    }
}
