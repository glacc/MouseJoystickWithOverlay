// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.Diagnostics;
using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    // Combinations not binded by default in IL-2 Sturmovik
    // RAlt + N, RAlt + M

    [SupportedOSPlatform("Windows")]
    internal class FocusedHwndChecker
    {
        static IntPtr m_focusedTargetHwnd = IntPtr.Zero;
        static public IntPtr focusedTargetHwnd
        {
            get => m_focusedTargetHwnd;
            private set => m_focusedTargetHwnd = value;
        }

        public static void Update()
        {
            Process[] processes = Process.GetProcessesByName("il-2");

            if (processes.Length > 0)
            {
                IntPtr currentlyFocusedHwnd = Import.GetForegroundWindow();

                if (currentlyFocusedHwnd != IntPtr.Zero)
                {
                    int threadPidOfHwnd;

                    IntPtr resultGetWindowThreadPid = Import.GetWindowThreadProcessId(currentlyFocusedHwnd, out threadPidOfHwnd);

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
        }
    }
}
