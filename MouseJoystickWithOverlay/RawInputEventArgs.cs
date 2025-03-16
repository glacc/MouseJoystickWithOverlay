// https://github.com/mfakane/rawinput-sharp/tree/master/RawInput.Sharp.SimpleExample

using Linearstar.Windows.RawInput;

namespace MouseJoystickWithOverlay
{
    internal class RawInputEventArgs : EventArgs
    {
        public RawInputEventArgs(RawInputData data)
        {
            Data = data;
        }

        public RawInputData Data { get; }
    }
}
