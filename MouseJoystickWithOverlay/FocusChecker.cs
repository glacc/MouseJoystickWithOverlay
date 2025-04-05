// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;

namespace MouseJoystickWithOverlay
{
    // Combinations not binded by default in IL-2 Sturmovik
    // RAlt + N, RAlt + M

    [SupportedOSPlatform("Windows")]
    internal class FocusChecker
    {
        public static Mutex mutex = new Mutex();

        static IntPtr m_focusedTargetHwnd = IntPtr.Zero;
        public static IntPtr focusedTargetHwnd
        {
            get => m_focusedTargetHwnd;
            private set => m_focusedTargetHwnd = value;
        }

        static IntPtr m_lastFocusedTargetHwnd = focusedTargetHwnd;
        public static IntPtr lastFocusedTargetHwnd
        {
            get => m_lastFocusedTargetHwnd;
            private set => m_lastFocusedTargetHwnd = value;
        }

        public static Win32.RECT m_winRect;
        public static Win32.RECT? winRect
        {
            get => (focusedTargetHwnd != IntPtr.Zero) ? m_winRect : null;
            private set
            {
                if (value != null)
                    m_winRect = (Win32.RECT)value;
            }
        }

        public static void Update()
        {
            mutex.WaitOne();

            string[] processNames = ["il-2", "il2fb", "JoystickWizard", "Launcher64"];

            List<Process> processes = new List<Process>();
            foreach (string processName in processNames)
            {
                Process[] processesWithTheName = Process.GetProcessesByName(processName);
                processes.AddRange(processesWithTheName);
            }

            if (processes.Count > 0)
            {
                IntPtr currentlyFocusedHwnd = Win32.GetForegroundWindow();

                if (currentlyFocusedHwnd != IntPtr.Zero)
                {
                    int threadPidOfHwnd;

                    IntPtr resultGetWindowThreadPid = Win32.GetWindowThreadProcessId(currentlyFocusedHwnd, out threadPidOfHwnd);

                    if (resultGetWindowThreadPid != IntPtr.Zero && threadPidOfHwnd != 0)
                    {
                        bool isTarget = false;

                        foreach (Process process in processes)
                        {
                            if (process.Id == threadPidOfHwnd)
                            {
                                isTarget = true;
                                break;
                            }
                        }

                        if (isTarget)
                            focusedTargetHwnd = currentlyFocusedHwnd;
                        else
                            focusedTargetHwnd = IntPtr.Zero;
                    }
                }
            }
            else
                focusedTargetHwnd = IntPtr.Zero;

            if (focusedTargetHwnd != IntPtr.Zero)
            {
                Win32.RECT rectWindow;
                if (Win32.GetWindowRect(focusedTargetHwnd, out rectWindow))
                    winRect = rectWindow;
            }

            lastFocusedTargetHwnd = focusedTargetHwnd;

            mutex.ReleaseMutex();
        }
    }
}
