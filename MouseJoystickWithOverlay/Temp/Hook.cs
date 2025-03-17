using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MouseJoystickWithOverlay
{
    internal class Hook
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void HookCallback(Int64 wParam, Int64 lParam);

        [DllImport("Hook.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RegisterHook();

        [DllImport("Hook.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UnregisterHook();

        [DllImport("Hook.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetFocusedHwnd();
    }
}
