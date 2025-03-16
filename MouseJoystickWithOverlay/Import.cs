using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class Import
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out int pid);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out POINT point);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);
    }
}
