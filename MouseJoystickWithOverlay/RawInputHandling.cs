// https://github.com/mfakane/rawinput-sharp/tree/master/RawInput.Sharp.SimpleExample

using Linearstar.Windows.RawInput;
using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class RawInputHandling
    {
        static Thread? seperateThread = null;

        public static RawInputReceiverWindow window = new RawInputReceiverWindow();

        public static EventHandler<RawInputEventArgs>? onInput = null;

        static void WinFormThread()
        {
            /*
            RawInputDevice[] devices = RawInputDevice.GetDevices();

            IEnumerable<RawInputMouse> mice = devices.OfType<RawInputMouse>();
            foreach (RawInputMouse mouse in mice)
                Console.WriteLine($"{mouse.DeviceType} {mouse.VendorId:X4}:{mouse.ProductId:X4} {mouse.ProductName}, {mouse.ManufacturerName}");
            */

            window.Input += (sender, e) =>
            {
                // /*
                // Catch your input here!
                RawInputData data = e.Data;

                Console.WriteLine(data);
                // */

                onInput?.Invoke(null, e);
            };

            try
            {
                // Register the HidUsageAndPage to watch any device.
                RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard,
                    RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, window.Handle);
                RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse,
                    RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, window.Handle);

                Application.Run();
            }
            finally
            {
                RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
                RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
            }

            return;
        }

        public static void Start()
        {
            seperateThread = new Thread(WinFormThread);
            seperateThread.SetApartmentState(ApartmentState.STA);
            seperateThread.Start();
        }
    }
}
