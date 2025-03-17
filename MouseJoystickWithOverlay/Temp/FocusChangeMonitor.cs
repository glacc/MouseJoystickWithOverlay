using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseJoystickWithOverlay
{
    internal class FocusChangeMonitor
    {
        public static bool abortFlag = false;

        public static void FocusChangeMonitorThread()
        {
            EventWaitHandle focusEvent = new EventWaitHandle
            (
                false, EventResetMode.AutoReset, "Global\\OnMouseJoystickFocusChangeEvent"
            );

            IntPtr focusedHwndOld = IntPtr.Zero;

            while (!abortFlag)
            {
                focusEvent.WaitOne();

                IntPtr focusedHwnd = Hook.GetFocusedHwnd();

                if (focusedHwnd != focusedHwndOld)
                {
                    Console.WriteLine($"{focusedHwndOld} -> {focusedHwnd}");

                    if (focusedHwnd != IntPtr.Zero)
                    {
                        int pid;
                        Win32.GetWindowThreadProcessId(focusedHwnd, out pid);

                        if (pid != 0)
                            Console.WriteLine($"{Process.GetProcessById(pid).ProcessName}");
                    }
                }

                focusedHwndOld = focusedHwnd;
            }
        }
    }
}
